namespace AppSample.Domain.Models;

public record ConsentSmsRequest(long Msisdn, string ServiceProviderName, List<Scope>? ServiceProviderScopes, ICollection<string> RequestScopes, string? Context, string? BindingMessage, bool IsAuthzScopeSelected);