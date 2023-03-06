using AppSample.Domain.Models.ServiceProviders;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Models;

[ModelBinder(BinderType = typeof(BasicAuthHeaderBinder))]
public class BasicAuthHeaderInfo : AuthInfo
{
}