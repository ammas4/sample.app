namespace AppSample.CoreTools.Helpers.BatchProcessor;

/// <summary>
/// Ошибка при обработке в BatchProcessor
/// </summary>
public class BatchProcessorException : Exception
{
    public BatchProcessorException(string message) : base(message)
    {
    }
    public BatchProcessorException(string message, Exception innerException) : base(message, innerException)
    {
    }
}