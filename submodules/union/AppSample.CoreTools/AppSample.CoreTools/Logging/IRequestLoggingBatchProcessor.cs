namespace AppSample.CoreTools.Logging;

public interface IRequestLoggingBatchProcessor
{
    Task ProcessBatchSave(List<RequestEvent>? list);
}