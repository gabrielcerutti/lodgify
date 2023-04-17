namespace Showtime.Api.Application.Validations;

using FluentValidation;
using Showtime.Api.Application.Commands;

public class CreateShowtimeCommandValidator : AbstractValidator<CreateShowtimeCommand>
{
    public CreateShowtimeCommandValidator(ILogger<CreateShowtimeCommandValidator> logger)
    {
        RuleFor(command => command.MovieId).NotEmpty();
        RuleFor(command => command.AuditoriumId).NotEmpty();
        RuleFor(command => command.SessionDate).NotEmpty().Must(BeValidDate).WithMessage("Please specify a valid date, must be in the future.");

        logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
    }

    private bool BeValidDate(DateTime dateTime)
    {
        return dateTime >= DateTime.UtcNow;
    }
}
