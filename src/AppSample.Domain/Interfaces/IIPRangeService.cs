using System.Net;

namespace AppSample.Domain.Interfaces;

public interface IIPRangeService
{
    public bool IsBeelineIp(IPAddress ipAddress);
}
