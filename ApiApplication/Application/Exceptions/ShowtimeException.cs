namespace Lodgify.Api.Application.Exceptions;

/// <summary>
/// Exception type for domain exceptions
/// </summary>
public class ShowtimeException : Exception
{
    public ShowtimeException()
    { }

    public ShowtimeException(string message)
        : base(message)
    { }

    public ShowtimeException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
