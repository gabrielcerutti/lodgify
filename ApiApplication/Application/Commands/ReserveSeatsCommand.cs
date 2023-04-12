namespace Lodgify.Api.Application.Commands;

[DataContract]
public class ReserveSeatsCommand : IRequest<ReserveSeatsDTO>
{
    [DataMember]
    public int ShowtimeId { get; private set; }

    [DataMember]
    public IEnumerable<SeatDTO> Seats { get; private set; }

    public ReserveSeatsCommand(int showtimeId, IEnumerable<SeatDTO> seats)
    {
        ShowtimeId = showtimeId;
        Seats = seats;
    }
}

public record SeatDTO
{
    public short Row { get; set; }
    public short SeatNumber { get; set; }
}

