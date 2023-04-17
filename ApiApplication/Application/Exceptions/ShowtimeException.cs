using System.ComponentModel;

namespace Showtime.Api.Application.Exceptions;

/// <summary>
/// Exception type for domain exceptions
/// </summary>
public class ShowtimeException : Exception
{
    public string[] Errors { get; set; } = Array.Empty<string>();

    public ShowtimeException()
    { }

    public ShowtimeException(string message)
        : base(message)
    { }

    public ShowtimeException(string message, string[] errors)
    : base(message)
    { 
        Errors = errors;
    }

    public ShowtimeException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
