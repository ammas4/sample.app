using AppSample.CoreTools.DapperContrib;

namespace AppSample.Domain.DAL.Entities;

public class EventDTO
{
    public long Id { get; init; }
    public DateTime Date { get; init; }
    public int? ServiceProviderId { get; init; }
    public int? OperatorId { get; init; }
    public long? Msisdn { get; init; }

    public short? ResponseCode { get; init; }
}