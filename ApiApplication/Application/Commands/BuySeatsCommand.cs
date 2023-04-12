namespace Lodgify.Api.Application.Commands;

[DataContract]
public class BuySeatsCommand : IRequest<BuySeatsDTO>
{
    [DataMember]
    public Guid ReserveId { get; private set; }

    public BuySeatsCommand(Guid reserveId)
    {
        ReserveId = reserveId;
    }
}

