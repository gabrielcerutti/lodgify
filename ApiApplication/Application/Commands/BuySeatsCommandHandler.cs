namespace Showtime.Api.Application.Commands;

using Showtime.Api.Application.Exceptions;
using Showtime.Api.Database.Repositories.Abstractions;

public class BuySeatsCommandHandler : IRequestHandler<BuySeatsCommand, BuySeatsDTO>
{    
    private readonly IShowtimesRepository _showtimesRepository;
    private readonly ITicketsRepository _ticketsRepository;
    private readonly ILogger<CreateShowtimeCommandHandler> _logger;

    public BuySeatsCommandHandler(ITicketsRepository ticketsRepository, IShowtimesRepository showtimesRepository, ILogger<CreateShowtimeCommandHandler> logger)
    {
        _ticketsRepository = ticketsRepository ?? throw new ArgumentNullException(nameof(ticketsRepository));
        _showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BuySeatsDTO> Handle(BuySeatsCommand message, CancellationToken cancellationToken)
    {        
        var ticket = await _ticketsRepository.GetAsync(message.ReserveId, cancellationToken) ?? throw new ShowtimeException($"Reserve with id {message.ReserveId} does not exist");
        var showTime = await _showtimesRepository.GetWithMoviesByIdAsync(ticket.ShowtimeId, cancellationToken);

        // Check if reserve is not already bought
        if (ticket.Paid)
        {
            throw new ShowtimeException($"Reserve with id {message.ReserveId} was already bought");
        }

        // Check if reserve is not expired (10 minutes)
        if (DateTime.UtcNow - ticket.CreatedTime > TimeSpan.FromMinutes(10))
        {
            throw new ShowtimeException($"Reserve with id {message.ReserveId} is expired");
        }

        // Make payment
        ticket = await _ticketsRepository.ConfirmPaymentAsync(ticket, cancellationToken);

        _logger.LogInformation("Buying Tickets for reserve {ReserveId} - Movie: {@Movie}", ticket.Id, showTime.Movie.Title);

        return new BuySeatsDTO
        {
            Message = "Payment was successful, enjoy the movie!",
            Movie = showTime.Movie.Title,
            SessionDate = showTime.SessionDate,
            AuditoriumId = showTime.AuditoriumId,
            Seats = ticket.Seats.Select(s => new SeatDTO(s.Row, s.SeatNumber)).ToList(),
        };
    }
}

public record BuySeatsDTO
{
    public IList<SeatDTO> Seats { get; set; }
    public string Movie { get; set; }
    public DateTime SessionDate { get; set; }
    public int AuditoriumId { get; set; }
    public string Message { get; set; }
}
