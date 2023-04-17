namespace Showtime.Api.Application.Commands;
using Showtime.Api.Database.Entities;
using Showtime.Api.Database.Repositories.Abstractions;
using Showtime.Api.Infrastructure;

public class CreateShowtimeCommandHandler : IRequestHandler<CreateShowtimeCommand, ShowtimeDTO>
{
    private readonly IMoviesApi _moviesApi;
    private readonly IShowtimesRepository _showtimesRepository;
    private readonly ILogger<CreateShowtimeCommandHandler> _logger;

    public CreateShowtimeCommandHandler(IMoviesApi moviesApi, IShowtimesRepository showtimesRepository, ILogger<CreateShowtimeCommandHandler> logger)
    {
        _moviesApi = moviesApi ?? throw new ArgumentNullException(nameof(moviesApi));
        _showtimesRepository = showtimesRepository ?? throw new ArgumentNullException(nameof(showtimesRepository));        
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ShowtimeDTO> Handle(CreateShowtimeCommand message, CancellationToken cancellationToken)
    {        
        // Validate if showtime already exists
        //TODO: We should validate if the showtime exists in the database for the given movie, auditorium and session date

        var movieDetails = await _moviesApi.GetByIdAsync(message.MovieId);
        var showtime = new ShowtimeEntity
        {
            SessionDate = message.SessionDate,
            AuditoriumId = message.AuditoriumId,
            Movie = new MovieEntity
            {
                Title = movieDetails.Title,
                ImdbId = movieDetails.Id,
                ReleaseDate = new DateTime(int.Parse(movieDetails.Year), 1, 1),
                Stars = movieDetails.Crew,
            },
        };

        _logger.LogInformation("Creating Showtime - Movie: {@Movie}", movieDetails.Title);

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
