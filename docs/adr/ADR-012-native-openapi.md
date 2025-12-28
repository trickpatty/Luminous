# ADR-012: Native ASP.NET Core OpenAPI

> **Status:** Accepted
> **Date:** 2025-12-28
> **Deciders:** Luminous Core Team
> **Categories:** Technology, Architecture

## Context

ASP.NET Core 10 introduces native OpenAPI document generation via the `Microsoft.AspNetCore.OpenApi` package. Previously, Swashbuckle was the de facto standard for generating OpenAPI documents in .NET applications.

We needed to choose between:
1. Using Swashbuckle for full OpenAPI generation and Swagger UI
2. Using native ASP.NET Core OpenAPI with Swashbuckle for UI only
3. Using a completely different solution

## Decision Drivers

- **Framework Alignment**: Follow Microsoft's recommended approach for .NET 10
- **Maintainability**: Use supported, actively maintained packages
- **Package Compatibility**: Ensure all packages work together without conflicts
- **Developer Experience**: Maintain good API documentation tooling
- **Future-Proofing**: Choose the approach with best long-term support

## Considered Options

### Option 1: Full Swashbuckle Stack

Use Swashbuckle.AspNetCore for both OpenAPI generation and Swagger UI.

**Pros:**
- Well-established, mature library
- Single package for everything
- Extensive customization options

**Cons:**
- Swashbuckle 10.x requires Microsoft.OpenApi 2.x
- Not the Microsoft-recommended approach for .NET 10+
- Third-party dependency for core functionality

### Option 2: Native OpenAPI + Swashbuckle UI (Selected)

Use `Microsoft.AspNetCore.OpenApi` for document generation, `Swashbuckle.AspNetCore.SwaggerUI` for interactive documentation.

**Pros:**
- Microsoft-recommended approach for .NET 10
- Native framework integration
- Document transformers for customization
- First-party support and updates
- Swashbuckle UI is mature and feature-rich

**Cons:**
- Split responsibility between packages
- New API to learn (document transformers)

### Option 3: Alternative Solutions (NSwag, Scalar, etc.)

Use NSwag, Scalar, or other third-party solutions.

**Pros:**
- Some offer additional features
- Different UI options

**Cons:**
- Less framework integration
- Additional dependencies
- Varying levels of support

## Decision

We will use **native ASP.NET Core OpenAPI** for document generation with **Swashbuckle SwaggerUI** for interactive documentation:

### Package Configuration

```xml
<!-- Directory.Packages.props -->
<PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="10.0.1" />
<PackageVersion Include="Microsoft.OpenApi" Version="2.4.1" />
<PackageVersion Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.1.0" />
```

**Important**: Microsoft.OpenApi 3.x is NOT compatible with ASP.NET Core 10's OpenAPI source generator or Swashbuckle 10.x. Version 2.x must be used.

### Configuration

```csharp
// Program.cs

// Configure OpenAPI with native ASP.NET Core support
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Luminous API";
        document.Info.Version = "v1";
        document.Info.Description = "API for the Luminous Family Hub";
        return Task.CompletedTask;
    });
});

// In development, enable OpenAPI and Swagger UI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Luminous API v1");
        options.DisplayRequestDuration();
    });
}
```

### Document Transformers

Document transformers replace Swashbuckle's document/operation filters:

```csharp
// BearerSecuritySchemeTransformer.cs
internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authSchemes.Any(s => s.Name == "Bearer"))
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter your JWT token."
                }
            };

            document.SecurityRequirements ??= [];
            document.SecurityRequirements.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            });
        }
    }
}
```

## Rationale

1. **Microsoft Recommendation**: .NET 10+ officially recommends the native OpenAPI package.

2. **Package Compatibility**: Microsoft.OpenApi 2.x works with both ASP.NET Core OpenAPI and Swashbuckle UI. Version 3.x is incompatible.

3. **Best of Both**: Native document generation with established Swagger UI provides stability and features.

4. **Future Updates**: First-party OpenAPI support will receive priority framework updates.

## Consequences

### Positive

- Aligned with Microsoft's recommended approach
- Native framework integration
- First-party support and documentation
- Consistent with future .NET versions
- Document transformers provide clean customization

### Negative

- Different API from Swashbuckle (learning curve)
- Fewer third-party extensions (for now)
- Some Swashbuckle-specific features unavailable

### Neutral

- OpenAPI document available at `/openapi/v1.json`
- Swagger UI available at `/swagger`
- Document transformers replace operation/document filters

## Endpoints

| Endpoint | Description |
|----------|-------------|
| `/openapi/v1.json` | OpenAPI 3.0 specification document |
| `/swagger` | Swagger UI (development only) |

## Related Decisions

- [ADR-001: .NET 10 as Backend Platform](./ADR-001-dotnet-backend.md)

## References

- [ASP.NET Core OpenAPI Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi)
- [Microsoft.OpenApi GitHub Repository](https://github.com/microsoft/OpenAPI.NET)
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)
