using Showtime.Api.Application.Commands;
using Showtime.Api.Application.Extensions;
using Showtime.Api.Database.Repositories.Abstractions;

namespace Showtime.Api.Controllers
{
    /// <summary>
    /// For simplicity this controller is handling both commands and queries for all entities, so it's not a true REST API.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ShowtimeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IShowtimesRepository _showtimeRepository;
        private readonly ITicketsRepository _ticketsRepository;
        private readonly ILogger<ShowtimeController> _logger;

        public ShowtimeController(IMediator mediator, IShowtimesRepository showtimeRepository, ITicketsRepository ticketsRepository, ILogger<ShowtimeController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _showtimeRepository = showtimeRepository ?? throw new ArgumentNullException(nameof(showtimeRepository));
            _ticketsRepository = ticketsRepository ?? throw new ArgumentNullException(nameof(ticketsRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Route("create")]
        [HttpPost]
        public async Task<ActionResult<ShowtimeDTO>> CreateShowtimeAsync([FromBody] CreateShowtimeCommand createShowtimeCommand)
        {
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                createShowtimeCommand.GetGenericTypeName(),
                nameof(createShowtimeCommand.MovieId),
                createShowtimeCommand.MovieId,
                createShowtimeCommand);

            return await _mediator.Send(createShowtimeCommand);
        }

        [Route("reserve")]
        [HttpPut]
        public async Task<ActionResult<ReserveSeatsDTO>> ReserveSeats([FromBody] ReserveSeatsCommand reserveSeatsCommand)
        {
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                reserveSeatsCommand.GetGenericTypeName(),
                nameof(reserveSeatsCommand.ShowtimeId),
                reserveSeatsCommand.ShowtimeId,
                reserveSeatsCommand);

            return await _mediator.Send(reserveSeatsCommand);
        }

        [Route("buy")]
        [HttpPut]
        public async Task<ActionResult<BuySeatsDTO>> BuySeats([FromBody] BuySeatsCommand buySeatsCommand)
        {
            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                buySeatsCommand.GetGenericTypeName(),
                nameof(buySeatsCommand.ReserveId),
                buySeatsCommand.ReserveId,
                buySeatsCommand);

            return await _mediator.Send(buySeatsCommand);
        }

        [Route("buy/{reserveId:Guid}")]
        [HttpGet]
        public async Task<ActionResult<BuySeatsDTO>> GetBuySeats(Guid reserveId)
        {
            var buySeatsCommand = new BuySeatsCommand(reserveId);

            _logger.LogInformation(
                "----- Sending command: {CommandName} - {IdProperty}: {CommandId} ({@Command})",
                buySeatsCommand.GetGenericTypeName(),
                nameof(buySeatsCommand.ReserveId),
                buySeatsCommand.ReserveId,
                buySeatsCommand);

            return await _mediator.Send(buySeatsCommand);
        }

        [Route("{showtimeId:int}")]
        [HttpGet]
        public async Task<ActionResult> GetShowtimeById(int showtimeId, CancellationToken cancellationToken)
        {
            var showtime = await _showtimeRepository.GetWithMoviesByIdAsync(showtimeId, cancellationToken);
            var tickets = await _ticketsRepository.GetEnrichedAsync(showtimeId, cancellationToken);
            var showtimeDto = new
            {
                showtime.Id,
                Movie = showtime.Movie.Title,
                showtime.SessionDate,
                showtime.AuditoriumId,
                SeatsSold = tickets.Where(t => t.Paid == true).Select(t => t.Seats.Count).SingleOrDefault(),
                SeatsReserved = tickets.Where(t => t.Paid == false && DateTime.UtcNow - t.CreatedTime < TimeSpan.FromMinutes(10)).Select(t => t.Seats.Count).SingleOrDefault(),
                SeatsReservationExpired = tickets.Where(t => t.Paid == false && DateTime.UtcNow - t.CreatedTime > TimeSpan.FromMinutes(10)).Select(t => t.Seats.Count).SingleOrDefault(),
            };
            return Ok(showtimeDto);
        }
    }
}