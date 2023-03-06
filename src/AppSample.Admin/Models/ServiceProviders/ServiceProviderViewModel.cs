using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Admin.Models.ServiceProviders;

public class ServiceProviderViewModel
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ClientId { get; set; }
    public string? RedirectUrls { get; set; }
    public string? NotificationUrls { get; set; }
    public string? TTL { get; set; }
    public string CreatedAt { get; set; }
    public List<MobileIdMode> Modes { get; set; }

    public bool Active { get; set; }

    public ServiceProviderViewModel(ServiceProviderEntity? item)
    {
        if (item == null)
            return;

        Id = item.Id;
        Name = item.Name;
        ClientId = item.ClientId;
        RedirectUrls = string.Join(", ", item.RedirectUrls);
        NotificationUrls = string.Join(", ", item.NotificationUrls);
        Active = item.Active;
        TTL = item.TTL?.ToString();
        CreatedAt = item.CreatedAt.ToString("yyyy-MM-dd HH:mm");
        Modes = GetModes(item);
    }

    List<MobileIdMode> GetModes(ServiceProviderEntity item)
    {
        var modes = new List<MobileIdMode>();

        if (item.RedirectUrls.Count(x => !string.IsNullOrEmpty(x)) > 0)
        {
            modes.Add(MobileIdMode.DI);
        }

        if (item.NotificationUrls.Count(x => !string.IsNullOrEmpty(x)) > 0)
        {
            modes.Add(MobileIdMode.SI);
        }

        return modes;
    }
}