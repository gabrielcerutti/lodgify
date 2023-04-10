using Lodgify.Api.Application.Commands;
using Lodgify.Api.Application.Extensions;

namespace Lodgify.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShowtimeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ShowtimeController> _logger;

        public ShowtimeController(IMediator mediator, ILogger<ShowtimeController> logger)
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
    }
}