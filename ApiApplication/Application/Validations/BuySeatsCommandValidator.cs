namespace Showtime.Api.Application.Validations;

using FluentValidation;
using Showtime.Api.Application.Commands;

public class BuySeatsCommandValidator : AbstractValidator<BuySeatsCommand>
{
    public BuySeatsCommandValidator(ILogger<BuySeatsCommandValidator> logger)
    {
        RuleFor(command => command.ReserveId).NotEmpty();        

        logger.LogTrace("----- INSTANCE CREATED - {ClassName}", GetType().Name);
    }
}
