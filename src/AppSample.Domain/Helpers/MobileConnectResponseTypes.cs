namespace AppSample.Domain.Helpers;

public static class MobileConnectResponseTypes
{
    public static readonly string[] ValidResponseTypes = new[]
    {
        SIAsyncCode,
        SIPolling
    };

    public const string SIAsyncCode = "mc_si_async_code";
    public const string SIPolling = "mc_si_polling";
}