using AppSample.Domain.Helpers;

namespace AppSample.Domain.Models;

public class DiAuthorizationCommand
{
    public string? State { get; init; }
    public string? ClientId { get; init; }
    public string? RedirectUri { get; init; }
    public string? Scope { get; init; }
    public string? ResponseType { get; init; }
    public string? AcrValues { get; init; }
    public string? Nonce { get; init; }
    public string? Version { get; init; }
    public string? LoginHint { get; init; }
    public string? Display { get; init; }
    public string? ClientName { get; init; }
    public string? Context { get; init; }
    public string? BindingMessage { get; init; }
    public string? CorrelationId { get; init; }
    public string? OrderSum { get; init; }
    public bool? AutodetectSourceIp { get; init; }

    public Func<IdgwAuthMode, string, string?, string> ConfirmationUrlBuilder { get; init; }

    public bool HasSpecialScope()
    {
        if (string.IsNullOrEmpty(Scope)) return false;
        var scopes = Scope!.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        return (scopes.Any(x => ScopesHelper.IsSpecialScope(x)));
    }
}