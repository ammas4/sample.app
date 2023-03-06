using System.ComponentModel.DataAnnotations;
using AppSample.Admin.Helpers;
using AppSample.Domain.Helpers;
using AppSample.Domain.Models;
using AppSample.Domain.Models.ServiceProviders;
using AppSample.Domain.Models.Constants;

namespace AppSample.Admin.Models.ServiceProviders;

public class ServiceProviderAddEditViewModel
{
    public int SelectedTab { get; set; }
    
    public int? Id { get; set; }

    [Display(Name = @"Сервис-провайдер доступен")]
    public bool Active { get; set; } 

    [Display(Name = @"Название")]
    [Required(ErrorMessage = "Необходимо заполнить поле название")]
    public string? Name { get; set; }

    [Display(Name = @"Client ID")]
    [Required(ErrorMessage = "Необходимо заполнить поле ClientId")]
    public string? ClientId { get; set; }

    [Display(Name = @"Client secret")]
    [Required(ErrorMessage = "Необходимо заполнить поле ClientSecret")]
    public string? ClientSecret { get; set; }

    [Display(Name = @"TTL")]
    [ValidEmptyOrPositiveInteger]
    public string? TTL { get; set; }

    [Display(Name = @"Notification URLs")]
    [ValidEmptyOrUriList]
    public string? NotificationUrls { get; set; }

    [Display(Name = @"Redirect URLs")]
    [ValidEmptyOrUriList]
    public string? RedirectUrls { get; set; }

    [Display(Name = @"Auth Page URL")]
    [ValidEmptyOrUri]
    public string? AuthPageUrl { get; set; }

    [Display(Name = @"Redirect Timeout Seconds")]
    [ValidEmptyOrPositiveInteger]
    public string? RedirectTimeoutSeconds { get; set; }

    [Display(Name = @"OTP Notify URL")]
    [ValidEmptyOrUri]
    public string? OtpNotifyUrl { get; set; }

    [Display(Name = @"Sig Jwks content")]
    public string? JwksContent { get; set; }

    [Display(Name = @"Sig Jwks URL")]
    [ValidEmptyOrUri]
    public string? JwksUrl { get; set; }

    [Display(Name = @"Sig Jwks source")]
    public JwksSourceType JwksSourceType { get; set; }

    [Display(Name = @"Enc Jwks content")]
    public string? JwksEncContent { get; set; }

    [Display(Name = @"Enc Jwks URL")]
    [ValidEmptyOrUri]
    public string? JwksEncUrl { get; set; }

    [Display(Name = @"Enc Jwks source")]
    public JwksSourceType JwksEncSourceType { get; set; }

    [Display(Name = @"Scopes")]
    public List<ScopeViewModel>? Scopes { get; set; }

    [Display(Name = @"DocTypes")]
    public List<DocTypeViewModel> DocTypes { get; set; }

    [Display(Name = @"Метод шифрования")]
    [ValidAuthMode]
    public string? EncryptionMethod { get; set; }

    [Display(Name = @"Алгоритм шифрования")]
    [ValidAuthMode]
    public string? EncryptionAlgorithm { get; set; }

    public string? SelectedAvailableFlags { get; set; }

    public string? SelectedRequiredFlags { get; set; }

    public List<AuthenticatorViewModel>? Authenticators { get; set; }

    public ServiceProviderAddEditViewModel()
    {
        Scopes = GetInitScopes();
        DocTypes = GetInitDocTypes();
        Authenticators = new List<AuthenticatorViewModel> { };
    }

    static List<DocTypeViewModel> GetInitDocTypes()
    {
        return Enum
            .GetNames(typeof(DocumentTypes))
            .Select(docType => new DocTypeViewModel() { DocType = docType })
            .ToList();
    }

    private static List<ScopeViewModel> GetInitScopes()
    {
        var scopes = new List<ScopeViewModel>(13);

        const string scopeNameOpenId = "openid";
        var openIdScope = new ScopeViewModel { ScopeName = scopeNameOpenId, IsRequired = true, IsEnabled = true, IsLoa2Enabled = true, IsLoa3Enabled = true, IsLoa4Enabled = true,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(openIdScope);

        const string scopeNameMcAuthn = "mc_authn";
        var mcAuthnScope = new ScopeViewModel
        {
            ScopeName = scopeNameMcAuthn,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(mcAuthnScope);

        const string scopeNameMcClickAuth = "mc_click_auth";
        var clickAuthScope = new ScopeViewModel
        {
            ScopeName = scopeNameMcClickAuth,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(clickAuthScope);

        var scopeNameMcClicklessAuth = "mc_clickless_auth";
        var clicklessAuthScope = new ScopeViewModel
        {
            ScopeName = scopeNameMcClicklessAuth,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(clicklessAuthScope);

        var scopeNameMcAuthz = "mc_authz";
        var mcAuthzScope = new ScopeViewModel
        {
            ScopeName = scopeNameMcAuthz,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(mcAuthzScope);

        const string scopeNameMcPay = "mc_pay";
        var mcPayScope = new ScopeViewModel
        {
            ScopeName = scopeNameMcPay,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(mcPayScope);

        const string scopeNameMcKycPlain = "mc_kyc_plain";
        var mcKycPlainScope = new ScopeViewModel
        {
            ScopeName = scopeNameMcKycPlain,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(mcKycPlainScope);

        const string scopeNameMcAtp = "mc_atp";
        var mcAtpScope = new ScopeViewModel
        {
            ScopeName = scopeNameMcAtp,
            Claims = new List<ClaimViewModel>()
        };
        scopes.Add(mcAtpScope);

        const string scopeNameNationalId = "mc_identity_nationalid";
        var mcIdentityNationalIdScope = new ScopeViewModel
        {
            ScopeName = scopeNameNationalId,
            Claims = new List<ClaimViewModel>(22)
            {
                new(scopeNameNationalId, Claims.TITLE),
                new(scopeNameNationalId, Claims.GIVEN_NAME),
                new(scopeNameNationalId, Claims.FAMILY_NAME),
                new(scopeNameNationalId, Claims.MIDDLE_NAME),
                new(scopeNameNationalId, Claims.STREET_ADDRESS),
                new(scopeNameNationalId, Claims.CITY),
                new(scopeNameNationalId, Claims.STATE),
                new(scopeNameNationalId, Claims.POSTAL_CODE),
                new(scopeNameNationalId, Claims.COUNTRY),
                new(scopeNameNationalId, Claims.ADDRESS),
                new(scopeNameNationalId, Claims.EMAILI),
                new(scopeNameNationalId, Claims.BIRTHDATE),
                new(scopeNameNationalId, Claims.NATIONAL_IDENTIFIER),
                new(scopeNameNationalId, Claims.NATIONAL_IDENTIFIER_AUTHORITY),
                new(scopeNameNationalId, Claims.NATIONAL_IDENTIFIER_DATE),
                new(scopeNameNationalId, Claims.DOCUMENT_TYPE),
                new(scopeNameNationalId, Claims.SEX),
                new(scopeNameNationalId, Claims.STREET),
                new(scopeNameNationalId, Claims.APARTMENT),
                new(scopeNameNationalId, Claims.HOUSENO_OR_HOUSENAME),
                new(scopeNameNationalId, Claims.NATIONAL_IDENTIFIER_AUTHORITY_CODE),
                new(scopeNameNationalId, Claims.BIRTHPLACE)
            }
        };
        scopes.Add(mcIdentityNationalIdScope);

        const string scopePhoneNumber = "mc_identity_phonenumber";
        var mcIdentityPhoneNumberScope = new ScopeViewModel
        {
            ScopeName = scopePhoneNumber,
            Claims = new List<ClaimViewModel>(1)
            {
                new(scopePhoneNumber, Claims.PHONE_NUMBER)
            }
        };
        scopes.Add(mcIdentityPhoneNumberScope);

        const string scopeNameEmail = "mc_email";
        var mcIdentityEmailScope = new ScopeViewModel
        {
            ScopeName = scopeNameEmail,
            Claims = new List<ClaimViewModel>(1)
            {
                new(scopeNameEmail, Claims.EMAIL),
            }
        };
        scopes.Add(mcIdentityEmailScope);

        const string scopeNameBasic = "mc_identity_basic";
        var mcIdentityBasicScope = new ScopeViewModel
        {
            ScopeName = scopeNameBasic,
            Claims = new List<ClaimViewModel>(6)
            {
                new(scopeNameBasic, Claims.GIVEN_NAME_B),
                new(scopeNameBasic, Claims.FAMILY_NAME_B),
                new(scopeNameBasic, Claims.MIDDLE_NAME_B),
                new(scopeNameBasic, Claims.SEX_B),
                new(scopeNameBasic, Claims.BIRTHDATE_B),
                new(scopeNameBasic, Claims.EMAIL_B)
            }
        };
        scopes.Add(mcIdentityBasicScope);

        const string scopeNameBasicAddress = "mc_identity_basic_address";
        var mcIdentityBasicAddressScope = new ScopeViewModel
        {
            ScopeName = scopeNameBasicAddress,
            Claims = new List<ClaimViewModel>(1)
            {
                new(scopeNameBasicAddress, Claims.GIVEN_NAME_BA),
            }
        };
        scopes.Add(mcIdentityBasicAddressScope);

        const string scopeNameFull = "mc_identity_full";
        var mcIdentityFullScope = new ScopeViewModel
        {
            ScopeName = scopeNameFull,
            Claims = new List<ClaimViewModel>(20)
            {
                new(scopeNameFull, Claims.GIVEN_NAME),
                new(scopeNameFull, Claims.FAMILY_NAME),
                new(scopeNameFull, Claims.MIDDLE_NAME),
                new(scopeNameFull, Claims.CITY),
                new(scopeNameFull, Claims.STATE),
                new(scopeNameFull, Claims.POSTAL_CODE),
                new(scopeNameFull, Claims.COUNTRY),
                new(scopeNameFull, Claims.ADDRESS),
                new(scopeNameFull, Claims.EMAILI),
                new(scopeNameFull, Claims.BIRTHDATE),
                new(scopeNameFull, Claims.NATIONAL_IDENTIFIER),
                new(scopeNameFull, Claims.NATIONAL_IDENTIFIER_AUTHORITY),
                new(scopeNameFull, Claims.NATIONAL_IDENTIFIER_DATE),
                new(scopeNameFull, Claims.DOCUMENT_TYPE),
                new(scopeNameFull, Claims.SEX),
                new(scopeNameFull, Claims.STREET),
                new(scopeNameFull, Claims.APARTMENT),
                new(scopeNameFull, Claims.HOUSENO_OR_HOUSENAME),
                new(scopeNameFull, Claims.NATIONAL_IDENTIFIER_AUTHORITY_CODE),
                new(scopeNameFull, Claims.BIRTHPLACE)
            }
        };
        scopes.Add(mcIdentityFullScope);
    
        return scopes;
    }

    public ServiceProviderAddEditViewModel(ServiceProviderEntity? item)
    {
        if (item == null)
            return;

        Id = item.Id;
        Name = item.Name;
        ClientId = item.ClientId;
        ClientSecret = item.ClientSecret;
        NotificationUrls = StringsHelper.JoinList(item.NotificationUrls);
        RedirectUrls = StringsHelper.JoinList(item.RedirectUrls);
        AuthPageUrl = item.AuthPageUrl;
        RedirectTimeoutSeconds = item.RedirectTimeoutSeconds.ToString();
        TTL = item.TTL?.ToString();
        Active = item.Active;
        OtpNotifyUrl = item.OtpNotifyUrl;
        JwksContent = item.JwksContent;
        JwksUrl = item.JwksUrl;
        JwksSourceType = string.IsNullOrEmpty(item.JwksContent) ? JwksSourceType.Url : JwksSourceType.Content;
        JwksEncContent = item.JwksEncContent;
        JwksEncUrl = item.JwksEncUrl;
        JwksEncSourceType = string.IsNullOrEmpty(item.JwksEncContent) ? JwksSourceType.Url : JwksSourceType.Content;
        Scopes = ConvertToViewModel(item.Scopes);
        DocTypes = item.DocTypes == null ? GetInitDocTypes() : ConvertToDocTypesViewModel(item.DocTypes);
        EncryptionMethod = item.EncryptionMethod.ToString("D");
        EncryptionAlgorithm = item.EncryptionAlgorithm.ToString("D");
        Authenticators = ConvertToViewModel(item.AuthenticatorChain);
    }

    static List<ScopeViewModel> ConvertToViewModel(List<Scope>? scopes)
    {
        var viewScopes = GetInitScopes();

        if (scopes == null)
        {
            return viewScopes;
        }
        
        foreach (var scope in scopes)
        {
            var selectedViewScope = viewScopes.FirstOrDefault(viewScope => viewScope.ScopeName == scope.ScopeName);

            if (selectedViewScope == null)
            {
                continue;
            }

            selectedViewScope.IsEnabled = selectedViewScope.IsRequired || scope.IsEnabled;
            selectedViewScope.IsLoa2Enabled = selectedViewScope.IsRequired || scope.IsLoa2Enabled;
            selectedViewScope.IsLoa3Enabled = selectedViewScope.IsRequired || scope.IsLoa3Enabled;
            selectedViewScope.IsLoa4Enabled = selectedViewScope.IsRequired || scope.IsLoa4Enabled;
            selectedViewScope.Message = scope.Message;

            if (scope.Claims?.Any() != true)
            {
                continue;
            }

            foreach (var claim in scope.Claims)
            {
                var selectedViewClaim =
                    selectedViewScope.Claims.FirstOrDefault(c => c.ClaimName == claim.ClaimName);
                
                if (selectedViewClaim == null)
                {
                    continue;
                }
                
                selectedViewClaim.AvailableFlag = claim.AvailableFlag;
                selectedViewClaim.RequiredFlag = claim.RequiredFlag;
            }
        }

        return viewScopes;
    }

    static List<AuthenticatorViewModel> ConvertToViewModel(AuthenticatorChain authenticatorChain)
    {
        List<AuthenticatorViewModel> result = new();

        foreach (var authenticator in authenticatorChain)
        {
            result.Add(new AuthenticatorViewModel
            {
                Id = authenticator.Id,
                ServiceProviderId = authenticator.ServiceProviderId,
                Type = authenticator.Type,
                OrderLevel1 = authenticator.OrderLevel1,
                OrderLevel2 = authenticator.OrderLevel2,
                NextChainStartDelay = authenticator.NextChainStartDelay
            });

        }

        return result;
    }

    private static List<Scope> ConvertToScope(List<ScopeViewModel>? viewScopes)
    {
        var scopes = new List<Scope>();

        if (viewScopes == null)
        {
            return scopes;
        }

        scopes.AddRange(
            from viewScope in viewScopes
            let claims = viewScope.Claims
                .Select(viewClaim => new Claim(viewClaim.ClaimName, viewClaim.AvailableFlag, viewClaim.RequiredFlag))
                .ToList()
            select new Scope
            {
                ScopeName = viewScope.ScopeName,
                Claims = claims,
                IsEnabled = viewScope.IsEnabled,
                IsLoa2Enabled = viewScope.IsLoa2Enabled,
                IsLoa3Enabled = viewScope.IsLoa3Enabled,
                IsLoa4Enabled = viewScope.IsLoa4Enabled,
                Message = viewScope.Message,
            });

        return scopes;
    }

    static List<DocTypeViewModel> ConvertToDocTypesViewModel(List<DocumentTypes> docTypes)
    {
        var docTypesViewModel = GetInitDocTypes();

        foreach (var docTypeViewModel in docTypesViewModel
                     .Where(docTypeViewModel => docTypes.Exists(d => d.ToString() == docTypeViewModel.DocType)))
        {
            docTypeViewModel.IsRequired = true;
        }

        return docTypesViewModel;
    }

    static List<DocumentTypes> ConvertToDocumentTypes(List<DocTypeViewModel> viewModels)
    {
        var documentTypes = new List<DocumentTypes>();

        foreach (var viewModel in viewModels.Where(d => d.IsRequired))
        {
            if (Enum.TryParse(viewModel.DocType, out DocumentTypes documentType))
            {
                documentTypes.Add(documentType);
            }
        }

        return documentTypes;
    }

    public ServiceProviderEntity ToEntity()
    {
        var item = new ServiceProviderEntity
        {
            Id = Id ?? default,
            Name = Name,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            NotificationUrls = StringsHelper.SplitList(NotificationUrls),
            RedirectUrls = StringsHelper.SplitList(RedirectUrls),
            AuthPageUrl = AuthPageUrl,
            RedirectTimeoutSeconds = int.TryParse(RedirectTimeoutSeconds, out var timeout)? timeout : null,
            TTL = int.TryParse(TTL, out var ttl) ? ttl : null,
            Active = Active,
            OtpNotifyUrl = OtpNotifyUrl,
            JwksContent = JwksSourceType == JwksSourceType.Content ? JwksContent : null,
            JwksUrl = JwksSourceType == JwksSourceType.Content ? null : JwksUrl,
            JwksEncContent = JwksEncSourceType == JwksSourceType.Content ? JwksEncContent : null,
            JwksEncUrl = JwksEncSourceType == JwksSourceType.Content ? null : JwksEncUrl,
            Scopes = GetSelectedAvailableScopeClaims(),
            DocTypes = ConvertToDocumentTypes(DocTypes),
            EncryptionMethod = Enum.Parse<JweMethodEncryption>(EncryptionMethod),
            EncryptionAlgorithm = Enum.Parse<JweAlgorithm>(EncryptionAlgorithm),
            AuthenticatorChain = new AuthenticatorChain(GetFromViewModel(Authenticators, Id ?? default))
        };

        return item;
    }

    public ServiceProviderEntity ToItem()
    {
        var serviceProviderEntity = ToEntity();
        
        // Верификация значений цепочки аутентификаторов
        var hasDuplicateOrders = serviceProviderEntity.AuthenticatorChain
            .GroupBy(c => new { c.OrderLevel1, c.OrderLevel2 })
            .Any(g => g.Count() > 1);

        if (hasDuplicateOrders)
        {
            throw new ArgumentException("Значения позиций цепочек аутентификаторов должны быть уникальными");
        }

        return serviceProviderEntity;
    }

    static AuthenticatorEntity[] GetFromViewModel(List<AuthenticatorViewModel> authenticators, int serviceProviderId) =>
        authenticators.Where(a => a != null).Select(i => new AuthenticatorEntity
        {
            Id = i.Id,
            NextChainStartDelay = i.NextChainStartDelay.Value,
            OrderLevel1 = i.OrderLevel1,
            OrderLevel2 = i.OrderLevel2,
            ServiceProviderId = serviceProviderId,
            Type = i.Type
        }).ToArray();

    public class AvailableScope
    {
        public string FullClaimName { get; set; }

        public bool AvailableFlag { get; set; }
    }

    public class RequiredScope
    {
        public string FullClaimName { get; set; }

        public bool RequiredFlag { get; set; }
    }

    private List<Scope>? GetSelectedAvailableScopeClaims()
    {
        Scopes?.ForEach(scope => scope.Claims = scope.Claims.Where(s => s.AvailableFlag).ToList());
        return ConvertToScope(Scopes);
    }
}