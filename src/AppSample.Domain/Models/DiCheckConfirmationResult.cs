namespace AppSample.Domain.Models;

public class DiCheckConfirmationResult
{
    public CheckConfirmationStatus Status { get; init; }
    public string? RequestRedirectUrl { get; init; }
    public string? State { get; init; }
    public string? Code { get; init; }
    public string? CorrelationId { get; init; }
    public int? TimerRemaining { get; init; }
}