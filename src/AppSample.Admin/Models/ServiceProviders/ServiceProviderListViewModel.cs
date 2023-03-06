using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AppSample.Admin.Models.ServiceProviders;

public class ServiceProviderListViewModel : ListViewModel<ServiceProviderViewModel>
{
    public static List<SelectListItem> SortTypes { get; } = new()
    {
        new SelectListItem("Создано (по возрастанию)", ServiceProviderListSortType.RegisteredAtAsc.ToString()),
        new SelectListItem("Создано (по убыванию)", ServiceProviderListSortType.RegisteredAtDesc.ToString()),
        new SelectListItem("Название (по возрастанию)", ServiceProviderListSortType.ClientNameAsc.ToString()),
        new SelectListItem("Название (по убыванию)", ServiceProviderListSortType.ClientNameDesc.ToString())
    };

    public static List<SelectListItem> Modes { get; } = new()
    {
        new SelectListItem("DI", MobileIdMode.DI.ToString()),
        new SelectListItem("SI", MobileIdMode.SI.ToString()),
    };

    public static List<SelectListItem> Statuses { get; } = new()
    {
        new SelectListItem(ServiceProviderStatus.Active.ToString(), ServiceProviderStatus.Active.ToString()),
        new SelectListItem(ServiceProviderStatus.Disabled.ToString(), ServiceProviderStatus.Disabled.ToString()),
        new SelectListItem(ServiceProviderStatus.Deleted.ToString(), ServiceProviderStatus.Deleted.ToString()),
    };

    public ServiceProviderListSortType? SortType { get; set; }
    public string? Name { get; set; }
    public MobileIdMode? Mode { get; set; }
    public ServiceProviderStatus? Status { get; set; }

    public void Initialize(IEnumerable<ServiceProviderEntity> serviceProviders)
    {
        SortType ??= ServiceProviderListSortType.RegisteredAtDesc;

        if (Status != null)
        {
            switch (Status.Value)
            {
                case ServiceProviderStatus.Active:
                    serviceProviders = serviceProviders.Where(x => x.Active);
                    break;
                case ServiceProviderStatus.Disabled:
                    serviceProviders = serviceProviders.Where(x => !x.Active);
                    break;
                case ServiceProviderStatus.Deleted:
                    serviceProviders = serviceProviders.Where(x => x.Deleted);
                    break;
            }
        }

        if (Status != ServiceProviderStatus.Deleted)
        {
            serviceProviders = serviceProviders.Where(x => x.Deleted == false);
        }

        var items = serviceProviders.Select(x => new ServiceProviderViewModel(x));

        if (!string.IsNullOrEmpty(Name))
        {
            items = items.Where(x => x.Name != null && x.Name.Contains(Name, StringComparison.InvariantCultureIgnoreCase));
        }

        if (Mode != null)
        {
            items = items.Where(x => x.Modes.Contains(Mode.Value));
        }

        switch (SortType)
        {
            case ServiceProviderListSortType.RegisteredAtAsc:
                items = items.OrderBy(x => x.CreatedAt);
                break;
            case ServiceProviderListSortType.RegisteredAtDesc:
                items = items.OrderByDescending(x => x.CreatedAt);
                break;
            case ServiceProviderListSortType.ClientNameAsc:
                items = items.OrderBy(x => x.Name);
                break;
            case ServiceProviderListSortType.ClientNameDesc:
                items = items.OrderByDescending(x => x.Name);
                break;
        }

        Items = items.ToList();
    }
}