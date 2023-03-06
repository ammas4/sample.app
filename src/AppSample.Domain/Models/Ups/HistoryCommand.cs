namespace AppSample.Domain.Models.Ups;

public class HistoryCommand 
{
    public long Msisdn { get; init; }
    public AuthStatusEnum AuthStatus { get; init; }
    public string? SpName { get; init; }
    public DateTime AuthTime { get; init; }
    public bool? PinActivate { get; init; }
    public string? Pin { get; init; }
    public bool? Blocked { get; init; }
}