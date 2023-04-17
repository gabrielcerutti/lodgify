namespace Showtime.Api.Application.Commands;

using Showtime.Api.Application.Exceptions;
using Showtime.Api.Database.Entities;
using Showtime.Api.Database.Repositories.Abstractions;

public class ReserveSeatsCommandHandler : IRequestHandler<ReserveSeatsCommand, ReserveSeatsDTO>
{    
    private readonly ITicketsRepository _ticketsRepository;
    private readonly IAuditoriumsRepository _auditoriumsRepository;
    private readonly IShowtimesRepository _showtimesRepository;
    private readonly ILogger<CreateShowtimeCommandHandler> _logger;

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
            throw new ShowtimeException($"Showtime with id {message.ShowtimeId} does not exist");
        }
        var auditorium = await _auditoriumsRepository.GetAsync(showtimeWithTickets.AuditoriumId, cancellationToken);

        _logger.LogInformation("Reserving seats for showtime: {@Id}", showtimeWithTickets.Id);

        // Check if the seats are contiguous
        if (!AreSeatsContiguous(message.Seats.ToList()))
        {
            throw new ShowtimeException("Seats are not contiguous");
        }

        //Check if all se   ats exist in the auditorium
        var seatsExistInAuditorium = SeatsExistInAuditorium(auditorium, message.Seats);
        if (!seatsExistInAuditorium.Valid)
        {
            throw new ShowtimeException("Some of the selected seats dont's exist in the auditorium", seatsExistInAuditorium.Errors.ToArray());
        }

        // Check if the seats are available
        var seatsAreAvailable = SeatsAreAvailable(showtimeWithTickets, message.Seats);
        if (!seatsAreAvailable.Valid)
        {
            throw new ShowtimeException("Some of the selected seats are not available", seatsAreAvailable.Errors.ToArray());
        }

        // Reserve the seats
        _logger.LogInformation("Everything is Ok! Proceeding with reservation");
        var seats = auditorium.Seats.Where(s => message.Seats.Any(x => x.Row == s.Row && x.SeatNumber == s.SeatNumber)).ToList();
        var ticket = await _ticketsRepository.CreateAsync(showtimeWithTickets, seats, cancellationToken);
        var showtimeWithMovie = await _showtimesRepository.GetWithMoviesByIdAsync(showtimeWithTickets.Id, cancellationToken);
        return new ReserveSeatsDTO 
        { 
            ReserveId = ticket.Id,
            Movie = showtimeWithMovie.Movie.Title,
            NumberOfSeats = seats.Count,
            AuditoriumId = ticket.Showtime.AuditoriumId, 
            SessionDate = ticket.Showtime.SessionDate,
        };            
    }

    #region Private Methods

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

        _logger.LogInformation("Seats are contiguous!");
        // All seats are contiguous
        return true;
    }

    private ValidationResult SeatsExistInAuditorium(AuditoriumEntity auditorium, IEnumerable<SeatDTO> seats)
    {
        _logger.LogInformation("Checking if seats exist in the auditorium");
        var result = new ValidationResult();
        var auditoriumSeats = auditorium.Seats.ToList();
        foreach (var seat in seats)
        {
            var seatExists = auditoriumSeats.Exists(s => s.Row == seat.Row && s.SeatNumber == seat.SeatNumber);
            if (!seatExists)
            {
                var error = $"Seat Row:{seat.Row} Number:{seat.SeatNumber} does not exist in auditorium";
                _logger.LogInformation(error);
                result.Valid = false;
                result.Errors.Add(error);
            }
        }
        _logger.LogInformation("Seats exist in the auditorium!");
        return result;
    }

    private ValidationResult SeatsAreAvailable(ShowtimeEntity showtime, IEnumerable<SeatDTO> seats)
    {
        _logger.LogInformation("Checking if seats are available");
        var result = new ValidationResult();
        foreach (var ticket in showtime.Tickets)
        {
            var ticketSeats = ticket.Seats.ToList();
            foreach (var seat in seats)
            {
                var error = string.Empty;
                var ticketSeat = ticketSeats.SingleOrDefault(s => s.Row == seat.Row && s.SeatNumber == seat.SeatNumber);
                if (ticketSeat != null) // The ticketSeat was already buyed or reserved
                {
                    if (ticket.Paid == false) // The ticketSeat was reserved
                    {
                        // Check if the ticketSeat was reserved for more than 10 minutes
                        if (DateTime.UtcNow - ticket.CreatedTime > TimeSpan.FromMinutes(10))
                        {
                            // The ticket was reserved for more than 10 minutes
                            // Remove the ticket and continue the process
                            showtime.Tickets.Remove(ticket);
                            continue;
                        }
                        else
                        {
                            // The ticketSeat was reserved for less than 10 minutes
                            error = $"Seat Row:{seat.Row} Number:{seat.SeatNumber} is not available";
                            _logger.LogInformation(error);
                            result.Valid = false;
                            result.Errors.Add(error);
                        }
                    }
                    // Seat was already buyed
                    error = $"Seat Row:{seat.Row} Number:{seat.SeatNumber} is not available";
                    _logger.LogInformation(error);
                    result.Valid = false;
                    result.Errors.Add(error);
                }
            }
        }
        _logger.LogInformation("Seats are available!");
        return result;
    }

    private record ValidationResult
    {
        public bool Valid { get; set; } = true;
        public IList<string> Errors { get; set; } = new List<string>();
    }
    
    #endregion
}

public record ReserveSeatsDTO
{
    public Guid ReserveId { get; set; }
    public string Movie { get; set; }
    public int NumberOfSeats { get; set; }
    public int AuditoriumId { get; set; }
    public DateTime SessionDate { get; set; }
}
