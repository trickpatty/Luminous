namespace Luminous.Application.Common.Exceptions;

/// <summary>
/// Exception thrown when there is a conflict with existing data.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException()
        : base("A conflict occurred with existing data.")
    {
    }

    public ConflictException(string message)
        : base(message)
    {
    }

    public ConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
