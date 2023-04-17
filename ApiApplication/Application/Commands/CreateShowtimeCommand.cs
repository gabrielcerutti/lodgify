namespace Showtime.Api.Application.Commands;

[DataContract]
public class CreateShowtimeCommand : IRequest<ShowtimeDTO>
{
    [DataMember]
    public string MovieId { get; private set; }

    [DataMember]
    public DateTime SessionDate { get; private set; }

    [DataMember]
    public int AuditoriumId { get; private set; }

    public CreateShowtimeCommand(string movieId, DateTime sessionDate, int auditoriumId)
    {
        MovieId = movieId;
        SessionDate = sessionDate;
        AuditoriumId = auditoriumId;
    }
}

