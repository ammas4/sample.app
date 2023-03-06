using AppSample.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Nest;

namespace AppSample.Api.Models;

public class DiAuthorizationRequestVm
{
    [FromQuery(Name = OpenIdConnectParameterNames.State)]
    public string? State { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.ClientId)]
    public string ClientId { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.RedirectUri)]
    public string? RedirectUri { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.Scope)]
    public string? Scope { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.ResponseType)]
    public string? ResponseType { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.AcrValues)]
    public string? AcrValues { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.Nonce)]
    public string? Nonce { get; init; }

    [FromQuery(Name = MobileConnectParameterNames.Version)]
    public string? Version { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.LoginHint)]
    public string? LoginHint { get; init; }

    [FromQuery(Name = OpenIdConnectParameterNames.Display)]
    public string? Display { get; init; }

    [FromQuery(Name = MobileConnectParameterNames.ClientName)]
    public string? ClientName { get; init; }

    [FromQuery(Name = MobileConnectParameterNames.Context)]
    public string? Context { get; init; }

    [FromQuery(Name = MobileConnectParameterNames.BindingMessage)]
    public string? BindingMessage { get; init; }

    [FromQuery(Name = MobileConnectParameterNames.CorrelationId)]
    public string? CorrelationId { get; init; }

    [FromQuery(Name = MobileConnectParameterNames.OrderSum)]
    public string? OrderSum { get; init; }

    [FromQuery(Name = MobileConnectParameterNames.AutodetectSourceIp)]
    public bool? AutodetectSourceIp { get; init; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="confirmationUrlBuilder"></param>
    /// <returns></returns>
    public DiAuthorizationCommand ToCommand(Func<IdgwAuthMode, string, string?, string> confirmationUrlBuilder)
    {
        var command = new DiAuthorizationCommand()
        {
            State = State,
            ClientId = ClientId,
            RedirectUri = RedirectUri,
            Scope = Scope,
            ResponseType = ResponseType,
            AcrValues = AcrValues,
            Nonce = Nonce,
            Version = Version,
            LoginHint = LoginHint,
            Display = Display,
            ClientName = ClientName,
            Context = Context,
            BindingMessage = BindingMessage,
            CorrelationId = CorrelationId,
            OrderSum = OrderSum,
            AutodetectSourceIp = AutodetectSourceIp,
            ConfirmationUrlBuilder = confirmationUrlBuilder
        };
        return command;
    }
}