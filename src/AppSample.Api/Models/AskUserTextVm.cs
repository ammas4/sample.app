using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.DAL.DTOs;

namespace AppSample.Api.Models;

public class AskUserTextVm
{
    public IdgwSettings IdgwSettings { get; init; }

    public ServiceProviderEntity? ServiceProvider { get; init; }
    public string Context { get; init; }
    public string BindingMessage { get; init; }
    
}