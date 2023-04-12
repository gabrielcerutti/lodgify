namespace Lodgify.Api.Application.Commands;

using Lodgify.Api.Application.Exceptions;
using Lodgify.Api.Database.Entities;
using Lodgify.Api.Database.Repositories.Abstractions;

// Regular CommandHandler
public class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatsCommand, ReserveSeatsDTO>
{    
    private readonly ITicketsRepository _ticketsRepository;
    private readonly IAuditoriumsRepository _auditoriumsRepository;
    private readonly IShowtimesRepository _showtimesRepository;
    private readonly ILogger<CreateShowtimeCommandHandler> _logger;

    // Using DI to inject infrastructure persistence Repositories
    public ReserveSeatsCommandHandler(ITicketsRepository ticketsRepository, IAuditoriumsRepository auditoriumsRepository, IShowtimesRepository showtimesRepository, ILogger<CreateShowtimeCommandHandler> logger)
    {
        _ticketsRepository = ticketsRepository ?? throw new ArgumentNullException(nameof(ticketsRepository));
        _auditoriumsRepository = auditoriumsRepository ?? throw new ArgumentNullException(nameof(auditoriumsRepository));
        _showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ReserveSeatsDTO> Handle(ReserveSeatsCommand message, CancellationToken cancellationToken)
    {        
        var showtimeWithTickets = await _showtimesRepository.GetWithTicketsByIdAsync(message.ShowtimeId, cancellationToken);
        if (showtimeWithTickets == null)
        {
            throw new CinemaException($"Showtime with id {message.ShowtimeId} does not exist");
        }
        var auditorium = await _auditoriumsRepository.GetAsync(showtimeWithTickets.AuditoriumId, cancellationToken);

        _logger.LogInformation("----- Reserving seats - Showtime: {@Id}", showtimeWithTickets.Id);

        // Check if the seats are contiguous
        if (!AreSeatsContiguous(message.Seats.ToList()))
        {
            throw new CinemaException("Seats are not contiguous");
        }

        //Check if all se   ats exist in the auditorium
        if (!SeatsExistInAuditorium(auditorium, message.Seats))
        {
            throw new CinemaException($"Some of the requested seats don't exist in the auditorium");
        }

        // Check if the seats are available
        if (!SeatsAreAvailable(showtimeWithTickets, message.Seats))
        {
            throw new CinemaException("Some of the requested seats are not available");
        }

        // Reserve the seats
        var seats = auditorium.Seats.Where(s => message.Seats.Any(x => x.Row == s.Row && x.SeatNumber == s.SeatNumber)).ToList();
        var ticket = await _ticketsRepository.CreateAsync(showtimeWithTickets, seats, cancellationToken);
        var showtimeWithMovie = await _showtimesRepository.GetWithMoviesByIdAsync(showtimeWithTickets.Id, cancellationToken);
        return new ReserveSeatsDTO 
        { 
            Id = ticket.Id, 
            AuditoriumId = ticket.Showtime.AuditoriumId, 
            MovieTitle = showtimeWithMovie.Movie.Title, 
            NumberOfSeats = seats.Count 
        };            
    }

    private bool AreSeatsContiguous(List<SeatDTO> seats)
    {
        _logger.LogInformation("Checking if all requested seats are contiguous");

        if (seats == null || seats.Count == 0)
        {
            // No seats to reserve
            return false;
        }

        // Sort the seats based on row number and ticketSeat number
        seats.Sort((s1, s2) =>
        {
            int rowComparison = s1.Row.CompareTo(s2.Row);
            if (rowComparison != 0)
            {
                return rowComparison;
            }
            return s1.SeatNumber.CompareTo(s2.SeatNumber);
        });

        // Loop through the seats and check for contiguous seats
        for (int i = 1; i < seats.Count; i++)
        {
            SeatDTO previousSeat = seats[i - 1];
            SeatDTO currentSeat = seats[i];

            // Check if the current ticketSeat is contiguous with the previous ticketSeat
            if (currentSeat.Row == previousSeat.Row &&
                currentSeat.SeatNumber == previousSeat.SeatNumber + 1)
            {
                // Contiguous seats
                continue;
            }

            // Seats are not contiguous
            return false;
        }

        // All seats are contiguous
        return true;
    }

    private bool SeatsExistInAuditorium(AuditoriumEntity auditorium, IEnumerable<SeatDTO> seats)
    {
        var auditoriumSeats = auditorium.Seats.ToList();
        foreach (var seat in seats)
        {
            var seatExists = auditoriumSeats.Exists(s => s.Row == seat.Row && s.SeatNumber == seat.SeatNumber);
            if (!seatExists)
            {
                _logger.LogInformation($"Seat Row:{seat.Row} Number:{seat.SeatNumber} does not exist in auditorium");
                return false;
            }
        }
        return true;
    }

    private bool SeatsAreAvailable(ShowtimeEntity showtime, IEnumerable<SeatDTO> seats)
    {
        foreach (var ticket in showtime.Tickets)
        {
            var ticketSeats = ticket.Seats.ToList();
            foreach (var seat in seats)
            {
                var ticketSeat = ticketSeats.Single(s => s.Row == seat.Row && s.SeatNumber == seat.SeatNumber);
                if (ticketSeat != null && ticket.Paid == false) // The ticketSeat was already buyed or reserved
                {
                    // Check if the ticketSeat was reserved for more than 10 minutes
                    if (DateTime.UtcNow - ticket.CreatedTime > TimeSpan.FromMinutes(10))
                    {
                        // The ticketSeat was reserved for more than 10 minutes
                        // Remove the ticket
                        showtime.Tickets.Remove(ticket);
                        return true;
                    }
                    else
                    {
                        // The ticketSeat was reserved for less than 10 minutes
                        _logger.LogInformation($"Seat Row:{seat.Row} SeatNumber:{seat.SeatNumber} is not available");
                        return false;                        
                    }
                }
                // The ticket was already buyed
                return false;
            }
        }
        // No tickets were found
        return true;
    }
}

public record ReserveSeatsDTO
{
    public Guid Id { get; set; }
    public string MovieTitle { get; set; }
    public int NumberOfSeats { get; set; }
    public int AuditoriumId { get; set; }
}
