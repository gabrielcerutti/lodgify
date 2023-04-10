using Lodgify.Api.Database.Entities;

namespace Lodgify.Api.Application.Commands;

[DataContract]
public class CreateShowtimeCommand : IRequest<ShowtimeDTO>
{

    [DataMember]
    public int Id { get; private set; }

    [DataMember]
    public string MovieId { get; private set; }

    [DataMember]
    public DateTime SessionDate { get; private set; }

    [DataMember]
    public int AuditoriumId { get; private set; }

    public CreateShowtimeCommand(int id, string movieId, DateTime sessionDate, int auditoriumId)
    {
        Id = id;
        MovieId = movieId;
        SessionDate = sessionDate;
        AuditoriumId = auditoriumId;
    }
}

