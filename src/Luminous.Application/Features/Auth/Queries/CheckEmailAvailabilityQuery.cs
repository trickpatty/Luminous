using FluentValidation;
using Luminous.Domain.Interfaces;
using MediatR;

namespace Luminous.Application.Features.Auth.Commands;

/// <summary>
/// DTO for email availability check result.
/// </summary>
public sealed record EmailAvailabilityDto
{
    /// <summary>
    /// Gets whether the email is available for registration.
    /// </summary>
    public bool IsAvailable { get; init; }

    /// <summary>
    /// Gets a message about the availability status.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Query to check if an email is available for registration.
/// </summary>
public sealed record CheckEmailAvailabilityQuery : IRequest<EmailAvailabilityDto>
{
    /// <summary>
    /// The email to check.
    /// </summary>
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Validator for CheckEmailAvailabilityQuery.
/// </summary>
public sealed class CheckEmailAvailabilityQueryValidator : AbstractValidator<CheckEmailAvailabilityQuery>
{
    public CheckEmailAvailabilityQueryValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}

/// <summary>
/// Handler for CheckEmailAvailabilityQuery.
/// </summary>
public sealed class CheckEmailAvailabilityQueryHandler : IRequestHandler<CheckEmailAvailabilityQuery, EmailAvailabilityDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckEmailAvailabilityQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<EmailAvailabilityDto> Handle(CheckEmailAvailabilityQuery request, CancellationToken cancellationToken)
    {
        var exists = await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken);

        return new EmailAvailabilityDto
        {
            IsAvailable = !exists,
            Message = exists
                ? "This email is already registered."
                : "This email is available."
        };
    }
}
