using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Models.Confirmation;

public class ConfirmCodeResult
{
    public bool IsSuccessful { get; set; }
    public AuthorizationRequestDto? AuthorizationRequest { get; set; }
    public ServiceProviderEntity? ServiceProvider { get; set; }
}