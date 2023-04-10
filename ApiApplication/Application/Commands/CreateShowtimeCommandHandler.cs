namespace Lodgify.Api.Application.Commands;
using Lodgify.Api.Database.Entities;
using Lodgify.Api.Database.Repositories.Abstractions;
using Lodgify.Api.Infrastructure;

// Regular CommandHandler
public class CreateShowtimeCommandHandler : IRequestHandler<CreateShowtimeCommand, ShowtimeDTO>
{
    private readonly IMoviesApi _moviesApi;
    private readonly IShowtimesRepository _showtimesRepository;
    private readonly ILogger<CreateShowtimeCommandHandler> _logger;

    // Using DI to inject infrastructure persistence Repositories
    public CreateShowtimeCommandHandler(IMoviesApi moviesApi, IShowtimesRepository showtimesRepository, ILogger<CreateShowtimeCommandHandler> logger)
    {
        _moviesApi = moviesApi ?? throw new ArgumentNullException(nameof(moviesApi));
        _showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ShowtimeDTO> Handle(CreateShowtimeCommand message, CancellationToken cancellationToken)
    {        
        var movie = await _moviesApi.GetByIdAsync(message.MovieId);
        var show = new MovieEntity
        {
            Title = movie.Title,
            ImdbId = movie.Id,
            ReleaseDate = new DateTime(int.Parse(movie.Year), 1, 1),
            Stars = movie.Rank,
        };
        var showtime = new ShowtimeEntity(message.Id, show, message.SessionDate, message.AuditoriumId);

        _logger.LogInformation("----- Creating Showtime - Movie: {@Movie}", movie);

        showtime = await _showtimesRepository.CreateShowtime(showtime, cancellationToken);

        return ShowtimeDTO.FromShowtime(showtime);
    }
}

public record ShowtimeDTO
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public string MovieTitle { get; set; }
    public DateTime SessionDate { get; set; }
    public int AuditoriumId { get; set; }

    public static ShowtimeDTO FromShowtime(ShowtimeEntity showtime)
    {
        return new ShowtimeDTO()
        {
            Id = showtime.Id,
            MovieId = showtime.Movie.Id,
            MovieTitle = showtime.Movie.Title,
            SessionDate = showtime.SessionDate,
            AuditoriumId = showtime.AuditoriumId,
        };
    }

}
