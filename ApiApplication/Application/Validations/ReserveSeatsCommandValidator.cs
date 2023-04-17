namespace Showtime.Api.Application.Validations;

using FluentValidation;
using Showtime.Api.Application.Commands;

public class ReserveSeatsCommandValidator : AbstractValidator<ReserveSeatsCommand>
{
    public ReserveSeatsCommandValidator(ILogger<ReserveSeatsCommandValidator> logger)
    {
        RuleFor(command => command.ShowtimeId).NotEmpty();
        RuleFor(command => command.Seats).NotEmpty();
        RuleFor(command => command.Seats).Must(ContainSeats).WithMessage("No seats found");        

        logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
    }

    private bool ContainSeats(IEnumerable<SeatDTO> seats)
    {
        return seats.Any();
    }
}
