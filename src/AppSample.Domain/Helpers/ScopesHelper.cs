using System.Collections.Immutable;
using AppSample.Domain.Models;

namespace AppSample.Domain.Helpers;

public static class ScopesHelper
{
    public const string PaymentScope = "mc_pay";
    public const string IdentityFullScope = "mc_identity_full";
    public const string IdentityBasicScope = "mc_identity_basic";
    public const string IdentityNationalIdScope = "mc_identity_nationalid";
    public const string IdentityBasicAddressScope = "mc_identity_basic_address";
    public const string IdentityPhoneNumberScope = "mc_identity_phonenumber";
    public const string PhoneNumberScope = "mc_phonenumber";
    public const string EmailScope = "mc_email";
    public const string KycPlainScope = "mc_kyc_plain";
    public const string AuthnScope = "mc_authn";
    public const string AuthzScope = "mc_authz";
    public const string AtpScope = "mc_atp";

    public static IList<string> SpecialScopes = new List<string>()
    {
        IdentityNationalIdScope, IdentityBasicScope, IdentityBasicAddressScope, IdentityFullScope, IdentityPhoneNumberScope, PhoneNumberScope, KycPlainScope, AtpScope, EmailScope
    };

    static SortedList<int, string> _customMessageScopes = new ()
    {
        {1, IdentityFullScope},
        {2, IdentityBasicScope},
        {3, IdentityBasicAddressScope},
        {4, IdentityPhoneNumberScope},
        {5, AuthnScope},
        {6, AuthzScope},
    };

    public static IList<string> PremiumScopes = new List<string>()
    {
        "mc_identity_nationalid",
        "mc_identity_basic",
        "mc_identity_basic_address",
        "mc_identity_full",
        "mc_identity_phonenumber",
        "mc_atp",
        "mc_email",
        "mc_taxid_rus",
    };

    public static bool IsPremiumScope(string scope) => PremiumScopes.Contains(scope);

    public static bool IsSpecialScope(string scope) => SpecialScopes.Contains(scope);

    public static bool HasAuthzScope(ImmutableHashSet<string> scopes) =>
         scopes.Any(scope => scope == AuthzScope);

    public static bool HasPaymentScope(ImmutableHashSet<string> scopes) =>
        scopes.Any(scope => scope == PaymentScope);

    public static bool HasPaymentScope(string? scope)
    {
        if (scope == null)
            return false;
        var scopes = scope.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToImmutableHashSet();
        return HasPaymentScope(scopes);
    }

    /// <summary>
    /// Search the most relevant message for request scopes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="serviceProviderScopes"></param>
    /// <param name="requestScopes"></param>
    /// <param name="message"></param>
    /// <returns>true, if exist not empty message for request scopes</returns>
    public static bool IsCustomMessageRequired<T>(this List<T> serviceProviderScopes, ICollection<string> requestScopes, out string? message) where T : Scope
    {
        message = null;
        foreach (var scope in _customMessageScopes)
        {
            if (requestScopes.Contains(scope.Value))
            {
                message = serviceProviderScopes.FirstOrDefault(s => s.ScopeName == scope.Value)?.Message;
                if (!string.IsNullOrWhiteSpace(message))
                    return true;
            }
        }
        return false;
    }

    public static bool IsCustomMessageRequired<T>(T scope) where T : Scope
    {
        return _customMessageScopes.ContainsValue(scope.ScopeName);
    }
}