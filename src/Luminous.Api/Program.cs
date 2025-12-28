using System.Text;
using Fido2NetLib;
using Luminous.Api.Configuration;
using Luminous.Api.Middleware;
using Luminous.Api.Services;
using Luminous.Application;
using Luminous.Application.Common.Interfaces;
using Luminous.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services to the container
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add API services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure JWT settings
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection(JwtSettings.SectionName));

// Configure Email settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection(EmailSettings.SectionName));

// Register local JWT token service (for development)
builder.Services.AddScoped<ILocalJwtTokenService, LocalJwtTokenService>();

// Register email template service
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();

// Register email service based on configuration
// - UseDevelopmentMode=true (local dev): Logs emails to console
// - UseDevelopmentMode=false (Azure): Sends emails via Azure Communication Services
var emailSettings = builder.Configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>();
if (emailSettings?.UseDevelopmentMode == true)
{
    builder.Services.AddScoped<IEmailService, DevelopmentEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, AzureEmailService>();
}

// Register distributed cache
// - UseDevelopmentMode=true (local dev): Uses in-memory cache
// - UseDevelopmentMode=false (Azure): Uses Redis for distributed caching across instances
if (emailSettings?.UseDevelopmentMode == true)
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    var redisConnectionString = builder.Configuration.GetValue<string>("Redis:ConnectionString");
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = builder.Configuration.GetValue<string>("Redis:InstanceName") ?? "luminous:";
        });
    }
    else
    {
        // Fallback to memory cache if Redis not configured
        builder.Services.AddDistributedMemoryCache();
    }
}

// Register WebAuthn/FIDO2 service
builder.Services.AddScoped<IWebAuthnService, WebAuthnService>();

// Configure FIDO2
var fido2Config = new Fido2Configuration
{
    ServerDomain = builder.Configuration["Fido2:ServerDomain"] ?? "localhost",
    ServerName = builder.Configuration["Fido2:ServerName"] ?? "Luminous",
    Origins = builder.Configuration.GetSection("Fido2:Origins").Get<HashSet<string>>()
        ?? ["http://localhost:4200", "http://localhost:5000", "https://localhost:5001"]
};
builder.Services.AddSingleton(fido2Config);
builder.Services.AddSingleton<IFido2>(new Fido2(fido2Config));

// Configure authentication
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? new JwtSettings();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        ClockSkew = TimeSpan.FromMinutes(5)
    };

    // For development, allow HTTP
    if (builder.Environment.IsDevelopment())
    {
        options.RequireHttpsMetadata = false;
    }

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException)
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    // Add authorization policies for different roles
    options.AddPolicy("FamilyMember", policy =>
        policy.RequireClaim("family_id"));

    options.AddPolicy("FamilyAdmin", policy =>
        policy.RequireClaim("family_id")
              .RequireRole("Owner", "Admin"));

    options.AddPolicy("FamilyOwner", policy =>
        policy.RequireClaim("family_id")
              .RequireRole("Owner"));
});

// Configure controllers
builder.Services.AddControllers();

// Configure OpenAPI/Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Luminous API",
        Version = "v1",
        Description = "API for the Luminous Family Hub",
        Contact = new OpenApiContact
        {
            Name = "Luminous Team",
            Url = new Uri("https://github.com/trickpatty/Luminous")
        }
    });

    // Add JWT authentication to Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. In development, use POST /api/devauth/token to get a token."
    });

    // Use the new delegate pattern for .NET 10 / OpenApi 3.x
    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = Array.Empty<string>()
        });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Luminous API v1");
        options.DisplayRequestDuration();
    });
}

app.UseSerilogRequestLogging();

// Only redirect to HTTPS in non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

// Add custom middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseTenantValidation();

app.MapControllers();
app.MapHealthChecks("/health");

// Log startup information
Log.Information("Luminous API started in {Environment} mode", app.Environment.EnvironmentName);
if (app.Environment.IsDevelopment())
{
    Log.Information("Development features enabled:");
    Log.Information("  - Swagger UI: http://localhost:5000/swagger");
    Log.Information("  - Dev Auth: POST http://localhost:5000/api/devauth/token");
    Log.Information("  - Health Check: http://localhost:5000/health");
}

app.Run();
