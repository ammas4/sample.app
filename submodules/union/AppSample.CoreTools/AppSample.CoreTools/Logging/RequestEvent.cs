namespace AppSample.CoreTools.Logging;

/// <summary>
/// 
/// </summary>
public class RequestEvent
{
    public DateTimeOffset CreateDate { get; set; }
    public object TransactionId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}