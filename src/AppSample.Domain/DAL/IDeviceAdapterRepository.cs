using AppSample.Domain.Models.DeviceAdapter;

namespace AppSample.Domain.DAL;

public interface IDeviceAdapterRepository
{
    Task<DaCommandResultType> SendPushToMc(long msisdn, string text);
    Task<DaCommandResultType> SendPushToDstk(long msisdn, string text);
    Task<bool> SendSmsAsync(long msisdn, string text);
}