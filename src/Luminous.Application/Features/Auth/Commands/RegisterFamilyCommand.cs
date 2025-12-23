using FluentValidation;
using Luminous.Application.Common.Exceptions;
using Luminous.Application.Common.Interfaces;
using Luminous.Application.DTOs;
using Luminous.Domain.Entities;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// Command to register a new family with its owner and return authentication tokens.
/// This is the primary signup flow for new users creating a family.
/// </summary>
public sealed record RegisterFamilyCommand : IRequest<FamilyCreationResultDto>
{
    /// <summary>
    /// The name of the family to create.
    /// </summary>
    public string FamilyName { get; init; } = string.Empty;

    /// <summary>
    /// The timezone for the family (IANA timezone ID).
    /// </summary>
    public string Timezone { get; init; } = "UTC";

    /// <summary>
    /// The email address of the family owner.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The display name of the family owner.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;
}

/// <summary>
/// Validator for RegisterFamilyCommand.
/// </summary>
public sealed class RegisterFamilyCommandValidator : AbstractValidator<RegisterFamilyCommand>
{
    public RegisterFamilyCommandValidator()
    {
        RuleFor(x => x.FamilyName)
            .NotEmpty().WithMessage("Family name is required.")
            .MaximumLength(100).WithMessage("Family name must not exceed 100 characters.")
            .Matches(@"^[\w\s\-'\.]+$").WithMessage("Family name contains invalid characters.");

        RuleFor(x => x.Timezone)
            .NotEmpty().WithMessage("Timezone is required.")
            .MaximumLength(50).WithMessage("Timezone must not exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(50).WithMessage("Display name must not exceed 50 characters.")
            .Matches(@"^[\w\s\-'\.]+$").WithMessage("Display name contains invalid characters.");
    }
}

/// <summary>
/// Handler for RegisterFamilyCommand.
/// </summary>
public sealed class RegisterFamilyCommandHandler : IRequestHandler<RegisterFamilyCommand, FamilyCreationResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeService _dateTimeService;

    public RegisterFamilyCommandHandler(
        IUnitOfWork unitOfWork,
        ITokenService tokenService,
        IDateTimeService dateTimeService)
    {
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
        _dateTimeService = dateTimeService;
    }

    public async Task<FamilyCreationResultDto> Handle(RegisterFamilyCommand request, CancellationToken cancellationToken)
    {
        // Check if email is already in use
        if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
        {
            throw new ConflictException($"An account with email '{request.Email}' already exists.");
        }

        // Create the family (tenant)
        var family = Family.Create(request.FamilyName, request.Timezone, request.Email);
        await _unitOfWork.Families.AddAsync(family, cancellationToken);

        // Create the owner user
        var owner = User.CreateOwner(
            family.Id,
            request.Email,
            request.DisplayName);

        // Mark email as verified for now (in production, this would require email verification)
        owner.EmailVerified = true;
        owner.RecordLogin();

        await _unitOfWork.Users.AddAsync(owner, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Generate authentication token
        var authResult = _tokenService.GenerateToken(owner);

        return new FamilyCreationResultDto
        {
            Family = new FamilyDto
            {
                Id = family.Id,
                Name = family.Name,
                Timezone = family.Timezone,
                Settings = new FamilySettingsDto
                {
                    DefaultView = family.Settings.DefaultView,
                    PrivacyModeEnabled = family.Settings.PrivacyModeEnabled,
                    PrivacyModeTimeoutMinutes = (int)family.Settings.PrivacyModeTimeout.TotalMinutes,
                    ShowWeather = family.Settings.ShowWeather,
                    WeatherLocation = family.Settings.WeatherLocation,
                    UseCelsius = family.Settings.UseCelsius,
                    WeekStartDay = family.Settings.WeekStartDay
                },
                Subscription = family.Subscription != null ? new SubscriptionDto
                {
                    Tier = family.Subscription.Tier,
                    StartedAt = family.Subscription.StartedAt,
                    ExpiresAt = family.Subscription.ExpiresAt,
                    IsActive = family.Subscription.IsActive
                } : null,
                CreatedAt = family.CreatedAt,
                MemberCount = 1,
                DeviceCount = 0
            },
            Auth = authResult
        };
    }
}
