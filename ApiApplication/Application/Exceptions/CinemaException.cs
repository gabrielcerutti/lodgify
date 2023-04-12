namespace Lodgify.Api.Application.Exceptions;

/// <summary>
/// Exception type for domain exceptions
/// </summary>
public class CinemaException : Exception
{
    public CinemaException()
    { }

    public CinemaException(string message)
        : base(message)
    { }

    public CinemaException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
