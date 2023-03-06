namespace AppSample.Domain.Models;

public class DiStatusInfo
{
    public CheckConfirmationStatus Status { get; init; }
    public string? Code { get; init; }
    public int? TimerRemaining { get; init; }
}