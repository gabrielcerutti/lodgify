using Lodgify.Api.Application.Commands;
using Lodgify.Api.Application.Extensions;

namespace Lodgify.Api.Controllers
{
    /// <summary>
    /// For simplicity this controller is handling both commands and queries for all entities, so it's not a true REST API.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CinemaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CinemaController> _logger;

        public CinemaController(IMediator mediator, ILogger<CinemaController> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
        [HttpPost]
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
    }
}