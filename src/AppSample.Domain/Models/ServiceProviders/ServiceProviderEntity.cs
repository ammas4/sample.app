using AppSample.Domain.Models.Constants;

namespace AppSample.Domain.Models.ServiceProviders;

public class ServiceProviderEntity
{
    AuthenticatorType? _authType;

    public int Id { get; set; }
    public string? Name { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public int? TTL { get; set; }
    public List<string> RedirectUrls { get; set; }
    public List<string> NotificationUrls { get; set; }
    public string? AuthPageUrl { get; set; }
    public int? RedirectTimeoutSeconds { get; init; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string? OperatorClientName { get; set; }
    public bool UseWhiteList { get; set; }
    public bool Active { get; set; }
    public bool Deleted { get; set; }
    public AuthenticatorChain AuthenticatorChain { get; set; }
    public AuthenticatorType Type
    {
        get
        {
            if (_authType == null)
            {
                var firstAuthenticator = AuthenticatorChain.GetFirst;
                _authType = firstAuthenticator != null ?
                    firstAuthenticator.Type :
                    AuthTypeOld ?? AuthenticatorType.NoValue;
            }
            return _authType.Value;
        }
    }
    public AuthenticatorType? AuthTypeOld { get; init; }

    //public AuthenticatorChain AuthenticatorChain => new(Authenticators);

    //AuthenticatorEntity[] Authenticators => AuthMode switch
    //{
    //    IdgwAuthMode.Ussd => new AuthenticatorEntity[]
    //        { new() { Id = 1, OrderLevel1 = 1, OrderLevel2 = 1, Type = AuthenticatorType.Ussd } },
    //    IdgwAuthMode.SmsWithUrl => new AuthenticatorEntity[]
    //        { new() { Id = 1, OrderLevel1 = 1, OrderLevel2 = 1, Type = AuthenticatorType.SmsWithUrl } },
    //    IdgwAuthMode.SmsOTP => new AuthenticatorEntity[]
    //        { new() { Id = 1, OrderLevel1 = 1, OrderLevel2 = 1, Type = AuthenticatorType.SmsOtp } },
    //    _ => throw new ArgumentOutOfRangeException()
    //};

    ///// <summary>
    ///// для теста
    ///// </summary>
    //readonly AuthenticatorEntity[] _authenticators =
    //{
    //    new()
    //    {
    //        Id = 1, OrderLevel1 = 1, OrderLevel2 = 1, Timeout = TimeSpan.FromSeconds(10),
    //        Type = AuthenticatorType.Ussd
    //    },
    //    new()
    //    {
    //        Id = 2, OrderLevel1 = 1, OrderLevel2 = 2, Timeout = TimeSpan.FromSeconds(10),
    //        Type = AuthenticatorType.Ussd
    //    },
    //    new()
    //    {
    //        Id = 3, OrderLevel1 = 1, OrderLevel2 = 3, Timeout = TimeSpan.FromSeconds(10),
    //        Type = AuthenticatorType.SmsOtp
    //    },
    //    new()
    //    {
    //        Id = 4, OrderLevel1 = 2, OrderLevel2 = 1, Timeout = TimeSpan.FromSeconds(60),
    //        Type = AuthenticatorType.SmsWithUrl
    //    },
    //    new()
    //    {
    //        Id = 5, OrderLevel1 = 2, OrderLevel2 = 2, Timeout = TimeSpan.FromSeconds(10),
    //        Type = AuthenticatorType.SmsWithUrl
    //    },
    //    new()
    //    {
    //        Id = 6, OrderLevel1 = 2, OrderLevel2 = 3, Timeout = TimeSpan.FromSeconds(10),
    //        Type = AuthenticatorType.SmsOtp
    //    },
    //    new()
    //    {
    //        Id = 7, OrderLevel1 = 3, OrderLevel2 = 1, Timeout = TimeSpan.FromSeconds(10),
    //        Type = AuthenticatorType.Ussd
    //    },
    //    new()
    //    {
    //        Id = 8, OrderLevel1 = 3, OrderLevel2 = 2, Timeout = TimeSpan.FromSeconds(60),
    //        Type = AuthenticatorType.Ussd
    //    },
    //};

    public string? OtpNotifyUrl { get; set; }
    public string? JwksContent { get; set; }
    public string? JwksUrl { get; set; }
    public string? JwksEncContent { get; init; }
    public string? JwksEncUrl { get; set; }

    public List<Scope>? Scopes { get; set; }
    public List<DocumentTypes>? DocTypes { get; set; }

    public JweMethodEncryption EncryptionMethod { get; set; }

    public JweAlgorithm EncryptionAlgorithm { get; set; }


    public ServiceProviderEntity()
    {
        Scopes = InitScopes();
    }

    public static List<Scope> InitScopes()
    {
        var scopes = new List<Scope>(13);

        string scopeNameOpenId = "openid";
        var openIdScope = new Scope() { ScopeName = scopeNameOpenId, IsEnabled = true, IsLoa2Enabled = true, IsLoa3Enabled = true, IsLoa4Enabled = true };
        openIdScope.Claims = new List<Claim>();
        scopes.Add(openIdScope);

        string scopeNameMcAuthn = "mc_authn";
        var mcAuthnScope = new Scope() { ScopeName = scopeNameMcAuthn };
        mcAuthnScope.Claims = new List<Claim>();
        scopes.Add(mcAuthnScope);

        string scopeNameMcClickAuth = "mc_click_auth";
        var clickAuthScope = new Scope() { ScopeName = scopeNameMcClickAuth };
        clickAuthScope.Claims = new List<Claim>();
        scopes.Add(clickAuthScope);

        string scopeNameMcClicklessAuth = "mc_clickless_auth";
        var clicklessAuthScope = new Scope() { ScopeName = scopeNameMcClicklessAuth };
        clicklessAuthScope.Claims = new List<Claim>();
        scopes.Add(clicklessAuthScope);

        string scopeNameMcAuthz = "mc_authz";
        var mcAuthzScope = new Scope() { ScopeName = scopeNameMcAuthz };
        mcAuthzScope.Claims = new List<Claim>();
        scopes.Add(mcAuthzScope);

        string scopeNameMcPay = "mc_pay";
        var mcPayScope = new Scope() { ScopeName = scopeNameMcPay };
        mcPayScope.Claims = new List<Claim>();
        scopes.Add(mcPayScope);

        string scopeNameMcKycPlain = "mc_kyc_plain";
        var mcKycPlainScope = new Scope() { ScopeName = scopeNameMcKycPlain };
        mcKycPlainScope.Claims = new List<Claim>();
        scopes.Add(mcKycPlainScope);

        string scopeNameMcAtp = "mc_atp";
        var mcAtpScope = new Scope() { ScopeName = scopeNameMcAtp };
        mcAtpScope.Claims = new List<Claim>();
        scopes.Add(mcAtpScope);

        string scopeNameNationalId = "mc_identity_nationalid";
        var mcIdentityNationalidScope = new Scope() { ScopeName = scopeNameNationalId };
        mcIdentityNationalidScope.Claims = new List<Claim>(22)
        {
            new Claim(Claims.TITLE),
            new Claim(Claims.GIVEN_NAME),
            new Claim(Claims.FAMILY_NAME),
            new Claim(Claims.MIDDLE_NAME),
            new Claim(Claims.STREET_ADDRESS),
            new Claim(Claims.CITY),
            new Claim(Claims.STATE),
            new Claim(Claims.POSTAL_CODE),
            new Claim(Claims.COUNTRY),
            new Claim(Claims.ADDRESS),
            new Claim(Claims.EMAILI),
            new Claim(Claims.BIRTHDATE),
            new Claim(Claims.NATIONAL_IDENTIFIER),
            new Claim(Claims.NATIONAL_IDENTIFIER_AUTHORITY),
            new Claim(Claims.NATIONAL_IDENTIFIER_DATE),
            new Claim(Claims.DOCUMENT_TYPE),
            new Claim(Claims.SEX),
            new Claim(Claims.STREET),
            new Claim(Claims.APARTMENT),
            new Claim(Claims.HOUSENO_OR_HOUSENAME),
            new Claim(Claims.NATIONAL_IDENTIFIER_AUTHORITY_CODE),
            new Claim(Claims.BIRTHPLACE)
        };
        scopes.Add(mcIdentityNationalidScope);

        string scopePhoneNumber = "mc_identity_phonenumber";
        var mcIdentityPhonenumberScope = new Scope() { ScopeName = scopePhoneNumber };
        mcIdentityPhonenumberScope.Claims = new List<Claim>(1)
        {
            new Claim(Claims.PHONE_NUMBER)
        };
        scopes.Add(mcIdentityPhonenumberScope);

        string scopeNameEmail = "mc_email";
        var mcIdentityEmailScope = new Scope() { ScopeName = scopeNameEmail };
        mcIdentityEmailScope.Claims = new List<Claim>(1)
        {
            new Claim(Claims.EMAIL),
        };
        scopes.Add(mcIdentityEmailScope);

        string scopeNameBasic = "mc_identity_basic";
        var mcIdentityBasicScope = new Scope() { ScopeName = scopeNameBasic };
        mcIdentityBasicScope.Claims = new List<Claim>(6)
        {
            new Claim(Claims.GIVEN_NAME_B),
            new Claim(Claims.FAMILY_NAME_B),
            new Claim(Claims.MIDDLE_NAME_B),
            new Claim(Claims.SEX_B),
            new Claim(Claims.BIRTHDATE_B),
            new Claim(Claims.EMAIL_B)
        };
        scopes.Add(mcIdentityBasicScope);

        string scopeNameBasicAddress = "mc_identity_basic_address";
        var mcIdentityBasicAddressScope = new Scope() { ScopeName = scopeNameBasicAddress };
        mcIdentityBasicAddressScope.Claims = new List<Claim>(1)
        {
            new Claim(Claims.GIVEN_NAME_BA),
        };
        scopes.Add(mcIdentityBasicAddressScope);

        string scopeNameFull = "mc_identity_full";
        var mcIdentityFullScope = new Scope() { ScopeName = scopeNameFull };
        mcIdentityFullScope.Claims = new List<Claim>(20)
        {
            new Claim(Claims.GIVEN_NAME),
            new Claim(Claims.FAMILY_NAME),
            new Claim(Claims.MIDDLE_NAME),
            new Claim(Claims.CITY),
            new Claim(Claims.STATE),
            new Claim(Claims.POSTAL_CODE),
            new Claim(Claims.COUNTRY),
            new Claim(Claims.ADDRESS),
            new Claim(Claims.EMAILI),
            new Claim(Claims.BIRTHDATE),
            new Claim(Claims.NATIONAL_IDENTIFIER),
            new Claim(Claims.NATIONAL_IDENTIFIER_AUTHORITY),
            new Claim(Claims.NATIONAL_IDENTIFIER_DATE),
            new Claim(Claims.DOCUMENT_TYPE),
            new Claim(Claims.SEX),
            new Claim(Claims.STREET),
            new Claim(Claims.APARTMENT),
            new Claim(Claims.HOUSENO_OR_HOUSENAME),
            new Claim(Claims.NATIONAL_IDENTIFIER_AUTHORITY_CODE),
            new Claim(Claims.BIRTHPLACE)
        };
        scopes.Add(mcIdentityFullScope);


        //string scopeNameBigData = "mc_identity_bigdata";
        //var mcIdentityBigDataScope = new Scope() { ScopeName = scopeNameBigData };
        //mcIdentityBigDataScope.Claims = new Claim[117]
        //{
        //    new Claim(scopeNameBigData,Claims.GENDER_IND),
        //    new Claim(scopeNameBigData, Claims.STRINGAGE_CAT),
        //    new Claim(scopeNameBigData, Claims.TYPEDEVICE),
        //    new Claim(scopeNameBigData, Claims.OS),
        //    new Claim(scopeNameBigData, Claims.INCOME_CAT),
        //    new Claim(scopeNameBigData, Claims.FAMILY_STATUS),
        //    new Claim(scopeNameBigData, Claims.CHILDREN_IND),
        //    new Claim(scopeNameBigData, Claims.HOME_CITY),
        //    new Claim(scopeNameBigData, Claims.HOME_REGION),
        //    new Claim(scopeNameBigData, Claims.HOME_DISTRICT),
        //    new Claim(scopeNameBigData, Claims.JOB_CITY),
        //    new Claim(scopeNameBigData, Claims.JOB_REGION),
        //    new Claim(scopeNameBigData, Claims.JOB_DISTRICT),
        //    new Claim(scopeNameBigData, Claims.WEEKEND_CITY),
        //    new Claim(scopeNameBigData, Claims.WEEKEND_REGION),
        //    new Claim(scopeNameBigData, Claims.WEEKEND_DISTRICT),
        //    new Claim(scopeNameBigData, Claims.RUS_TRIPS_CNT_M),
        //    new Claim(scopeNameBigData, Claims.RUS_TRIPS_CNT_6M),
        //    new Claim(scopeNameBigData, Claims.RUS_TRIPS_AVG_6M),
        //    new Claim(scopeNameBigData, Claims.REGIONS_TRAVEL_CNT),
        //    new Claim(scopeNameBigData, Claims.REGIONS_TRAVEL),
        //    new Claim(scopeNameBigData, Claims.REGIONS_TRAVEL_TIME),
        //    new Claim(scopeNameBigData, Claims.ABROAD_TRIPS_CNT_M),
        //    new Claim(scopeNameBigData, Claims.ABROAD_TRIPS_CNT_6M),
        //    new Claim(scopeNameBigData, Claims.ABROAD_TRIPS_AVG_6M),
        //    new Claim(scopeNameBigData, Claims.COUNTRIES_TRAVEL_CNT),
        //    new Claim(scopeNameBigData, Claims.COUNTRIES_TRAVEL),
        //    new Claim(scopeNameBigData, Claims.COUNTRIES_TRAVEL_TIME),
        //    new Claim(scopeNameBigData, Claims.SUBWAY_USE_IND),
        //    new Claim(scopeNameBigData, Claims.SUBWAY_USE_CNTAZS_CARD_IND),
        //    new Claim(scopeNameBigData, Claims.AUTO_OWNERS_PROBVK_USED_IND),
        //    new Claim(scopeNameBigData, Claims.OK_USED_IND),
        //    new Claim(scopeNameBigData, Claims.FACEBOOK_USED_IND),
        //    new Claim(scopeNameBigData, Claims.INSTAGRAM_USED_IND),
        //    new Claim(scopeNameBigData, Claims.WHATSAPP_USED_IND),
        //    new Claim(scopeNameBigData, Claims.VIBER_USED_IND),
        //    new Claim(scopeNameBigData, Claims.TELEGRAM_USED_IND),
        //    new Claim(scopeNameBigData, Claims.SKYPE_USED_IND),
        //    new Claim(scopeNameBigData, Claims.EXT_ROAM_TRIP_IND),
        //    new Claim(scopeNameBigData, Claims.ONNET_ROAM_TRIP_IND),
        //    new Claim(scopeNameBigData, Claims.REPEATED_REST_IND),
        //    new Claim(scopeNameBigData, Claims.PARENT_0_1_IND),
        //    new Claim(scopeNameBigData, Claims.PARENT_0_3_IND),
        //    new Claim(scopeNameBigData, Claims.PARENT_3_6_IND),
        //    new Claim(scopeNameBigData, Claims.PARENT_SCHOOL_CHILD_IND),
        //    new Claim(scopeNameBigData, Claims.PET_OWNER_IND),
        //    new Claim(scopeNameBigData, Claims.DACHA_IND),
        //    new Claim(scopeNameBigData, Claims.BUSINESS_OWNERS_IND),
        //    new Claim(scopeNameBigData, Claims.FUTURE_PARENTS_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_ONLINE_CINEMA_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_ONLINE_CINEMA_M_IND),
        //    new Claim(scopeNameBigData, Claims.EMPLOYMENT_TYPE),
        //    new Claim(scopeNameBigData, Claims.LIFE_TIME),
        //    new Claim(scopeNameBigData, Claims.PRIORITY_SEARCH_SYSTEM),
        //    new Claim(scopeNameBigData, Claims.PROPERTY_OWNERS_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_CAR_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CREDIT_CARDS_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_DEBIT_CARDS_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_REMITTANCES_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_INVESTICII_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_MICROCREDIT_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CONSUMER_CREDITS_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_DEPOSITS_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_AUTOCREDITS_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CASHBACK_DISCOUNTS_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CASHBACK_DISCOUNTS_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData,Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUYING_PROPERTY_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_RESTAURANTS_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_RESTAURANTS_M_IND ),
        //    new Claim(scopeNameBigData, Claims.NTEREST_IN_TRAVEL_7D_IND) ,
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_TRAVEL_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_EXHIBITIONS_MUSEUMS_7D_INd),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_EXHIBITIONS_MUSEUMS_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_THEATERS_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_THEATERS_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CARSHARING_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CARSHARING_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_LIFEINSUR_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_AUTOINSUR_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_KASKO_14D_IND),
        //    new Claim(scopeNameBigData, Claims.JOB_SEARCH_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUSINESS_RESOURCE_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BUSINESS_RESOURCE_M_IND),
        //    new Claim(scopeNameBigData, Claims.MORTGAGE_FACT_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_ENTERTAINMENT_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_ENTERTAINMENT_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_SUPERMARKETS),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_VZR_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_VZR_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_PROPERTY_INSUR_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_PROPERTY_INSUR_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_LEARN_LANG_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_LEARN_LANG_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CARE_PRODUCTS_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_CARE_PRODUCTS_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BEAUTY_SALON_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_BEAUTY_SALON_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_SPORT_CLUBS_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_SPORT_CLUBS_M_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_SPORT_GOODS_7D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_SPORT_GOODS_M_IND),
        //    new Claim(scopeNameBigData, Claims.SPORT_FAN_IND),
        //    new Claim(scopeNameBigData, Claims.FOOTBALL_FAN_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_FIRST_EDUCATION_14D_IND),
        //    new Claim(scopeNameBigData, Claims.INTEREST_IN_DOP_EDUCATION_14D_IND)
        //};
        //scopes[6] = mcIdentityBigDataScope;

        //string scopeNameTaxidRus = "mc_identity_taxid_rus";
        //var mcIdentityTaxidRusScope = new Scope() { ScopeName = scopeNameTaxidRus };
        //mcIdentityTaxidRusScope.Claims = new Claim[1]
        //{
        //    new Claim(scopeNameTaxidRus, Claims.TAX_ID),
        //};
        //scopes[7] = mcIdentityTaxidRusScope;

        return scopes;
    }
}