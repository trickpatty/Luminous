namespace Luminous.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when a request is invalid due to client-side errors.
/// Returns HTTP 400 Bad Request.
/// </summary>
public class BadRequestException : Exception
{
    /// <summary>
    /// Error code for programmatic error handling.
    /// </summary>
    public string? ErrorCode { get; }

    public BadRequestException()
        : base("The request is invalid.")
    {
    }

    public BadRequestException(string message)
        : base(message)
    {
    }

    public BadRequestException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
