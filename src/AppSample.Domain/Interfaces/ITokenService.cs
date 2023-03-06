using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Interfaces;

public interface ITokenService
{
    Task<CreateTokensResult> CreateTokensAsync(CreateTokensCommand command, AuthenticatorType authenticatorType);
}