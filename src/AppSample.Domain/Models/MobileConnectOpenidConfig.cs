using System.Text.Json.Serialization;
using AppSample.CoreTools.Helpers;
using AppSample.Domain.Helpers;

namespace AppSample.Domain.Models
{
    public class MobileConnectOpenidConfig
    {
		readonly string _basePath;
		readonly string _issuer;

		public MobileConnectOpenidConfig(IdgwSettings settings)
		{
			_basePath = settings.BasePath;
			_issuer = settings.TokenIssuer;
		}

        string FullUrl(string path)
        {
            return UrlHelper.Combine(_basePath, path);
        }

		[JsonPropertyName("authorization_endpoint")] public string AuthorizationEndpoint => FullUrl("/authorize");
		[JsonPropertyName("end_session_endpoint")] public string EndSessionEndpoint => FullUrl("/endsession");
		[JsonPropertyName("introspection_endpoint")] public string IntrospectionEndpoint => FullUrl("/introspect");
		[JsonPropertyName("issuer")] public string Issuer => _issuer;
		[JsonPropertyName("jwks_uri")] public string JwksUri => FullUrl("/jwks");
		[JsonPropertyName("op_policy_uri")] public string OpPolicyUri => FullUrl("/about");
		[JsonPropertyName("op_tos_uri")] public string OpTosUri => FullUrl("/about");
		[JsonPropertyName("premiuminfo_endpoint")] public string PremiumInfoEndpoint => FullUrl("/premiuminfo");
		[JsonPropertyName("registration_endpoint")] public string RegistrationEndpoint => FullUrl("/register");
		[JsonPropertyName("revocation_endpoint")] public string RevocationEndpoint => FullUrl("/revoke");
		[JsonPropertyName("service_documentation")] public string ServiceDocumentation => FullUrl("/about");
		[JsonPropertyName("si_authorization_endpoint")] public string SIAuthorizationEndpoint => FullUrl("/si-authorize");
		[JsonPropertyName("signdocument_endpoint")] public string Signdocument_endpoint => FullUrl("/sign-document");
		[JsonPropertyName("token_endpoint")] public string TokenEndpoint => FullUrl("/mc-token");
		[JsonPropertyName("userinfo_endpoint")] public string UserinfoEndpoint => FullUrl("/mc-userinfo");
		[JsonPropertyName("verifydocument_endpoint")] public string VerifydocumentEndpoint => FullUrl("/verify-document");

        [JsonPropertyName("acr_values_supported")]
		public string[] AcrValuesSupported => new[] { "2", "3", "4" };

		[JsonPropertyName("claim_types_supported")]
		public string[] ClaimTypesSupported => new[] { "normal" };

		[JsonPropertyName("claims_locales_supported")]
		public bool ClaimsLocalesSupported => false;

		[JsonPropertyName("claims_parameter_supported")]
		public bool ClaimsParameterSupported => true;

		[JsonPropertyName("claims_supported")]
		public string[] ClaimsSupported => new[] { "abroad_trips_avg_6m", "abroad_trips_cnt_6m", "abroad_trips_cnt_m", "acr", "acr_values", "address", "age_cat", "amr", "apartment", "application", "at_hash", "auth_time", "auto_owners_probvk_used_ind", "azp", "binding_message", "birthdate", "business_owners_ind", "children_ind", "city", "client_name", "context", "countries_travel", "countries_travel_cnt", "countries_travel_time", "country", "dacha_ind", "document_type", "email", "email_verified", "employment_type", "ext_roam_trip_ind", "facebook_used_ind", "family_name", "family_status", "football_fan_ind", "future_parents_14d_ind", "gender", "gender_ind", "given_name", "home_city", "home_district", "home_region", "houseno_or_housename", "income_cat", "instagram_used_ind", "interest_in_auto_service_14d_ind", "interest_in_autocredits_14d_ind", "interest_in_autoinsur_14d_ind", "interest_in_beauty_salon_7d_ind", "interest_in_beauty_salon_m_ind", "interest_in_business_resource_7d_ind", "interest_in_business_resource_m_ind", "interest_in_buying_car_14d_ind", "interest_in_buying_foreign_property_14d_ind", "interest_in_buying_land_country_property_7d_ind", "interest_in_buying_land_country_property_m_ind", "interest_in_buying_property_7d_ind", "interest_in_buying_property_m_ind", "interest_in_care_products_7d_ind", "interest_in_care_products_m_ind", "interest_in_carsharing_7d_ind", "interest_in_carsharing_m_ind", "interest_in_cashback_discounts_7d_ind", "interest_in_cashback_discounts_m_ind", "interest_in_clothes_shoes_7d_ind", "interest_in_clothes_shoes_m_ind", "interest_in_consumer_credits_14d_ind", "interest_in_credit_cards_14d_ind", "interest_in_debit_cards_14d_ind", "interest_in_delivery_food_14d_ind", "interest_in_deposits_14d_ind", "interest_in_dop_education_14d_ind", "interest_in_entertainment_7d_ind", "interest_in_entertainment_m_ind", "interest_in_exhibitions_museums_7d_ind", "interest_in_exhibitions_museums_m_ind", "interest_in_first_education_14d_ind", "interest_in_investicii_14d_ind", "interest_in_kasko_14d_ind", "interest_in_learn_lang_7d_ind", "interest_in_learn_lang_m_ind", "interest_in_lifeinsur_14d_ind", "interest_in_microcredit_14d_ind", "interest_in_mortgages_14d_ind", "interest_in_online_cinema_7d_ind", "interest_in_online_cinema_m_ind", "interest_in_property_insur_7d_ind", "interest_in_property_insur_m_ind", "interest_in_property_rent_14d_ind", "interest_in_remittances_14d_ind", "interest_in_renovation_7d_ind", "interest_in_renovation_m_ind", "interest_in_restaurants_7d_ind", "interest_in_restaurants_m_ind", "interest_in_sport_clubs_7d_ind", "interest_in_sport_clubs_m_ind", "interest_in_sport_goods_7d_ind", "interest_in_sport_goods_m_ind", "interest_in_supermarkets", "interest_in_theaters_7d_ind", "interest_in_theaters_m_ind", "interest_in_travel_7d_ind", "interest_in_travel_m_ind", "interest_in_vzr_7d_ind", "interest_in_vzr_m_ind", "job_city", "job_district", "job_region", "job_search_14d_ind", "life_time", "locale", "locality", "middle_name", "mortgage_fact_14d_ind", "name", "national_identifier", "national_identifier_authority", "national_identifier_authority_code", "national_identifier_date", "nickname", "nonce", "notification_uri", "ok_used_ind", "onnet_roam_trip_ind", "os", "parent_0_1_ind", "parent_0_3_ind", "parent_3_6_ind", "parent_school_child_ind", "pet_owner_ind", "phone_number", "phone_number_alternate", "phone_number_verified", "picture", "postal_code", "preferred_username", "priority_search_system", "profile", "property_owners_ind", "redirect_uri", "region", "regions_travel", "regions_travel_cnt", "regions_travel_time", "repeated_rest_ind", "rus_trips_avg_6m", "rus_trips_cnt_6m", "rus_trips_cnt_m", "scope", "sex", "skype_used_ind", "sport_fan_ind", "state", "street", "street_address", "sub", "subway_use_cntazs_card_ind", "subway_use_ind", "tax_id", "telegram_used_ind", "title", "town", "typedevice", "updated_at", "viber_used_ind", "website", "weekend_city", "weekend_district", "weekend_region", "whatsapp_used_ind", "zoneinfo" };

		[JsonPropertyName("code_challenge_methods_supported")]
		public string[] CodeChallengeMethodsSupported => new[] { "plain", "S256" };

		[JsonPropertyName("grant_types_supported")]
		public string[] GrantTypesSupported => new[]
		{
			"authorization_code",
			"client_credentials",
			"implicit",
			"password",
			"refresh_token",
			"urn:ietf:params:oauth:grant-type:device_code",
			"urn:ietf:params:oauth:grant-type:jwt-bearer",
			"urn:ietf:params:oauth:grant_type:redelegate",
			"urn:openid:params:mc:grant_type:server_initiated"
		};

		[JsonPropertyName("id_token_encryption_alg_values_supported")]
		public string[] IdTokenEncryptionAlgValuesSupported => new[] { "RSA-OAEP", "RSA-OAEP-256", "RSA1_5" };

		[JsonPropertyName("id_token_encryption_enc_values_supported")]
		public string[] IdTokenEncryptionEncValuesSupported => new[] { "A256CBC+HS512", "A256GCM", "A192GCM", "A128GCM", "A128CBC-HS256", "A192CBC-HS384", "A256CBC-HS512", "A128CBC+HS256" };

		[JsonPropertyName("id_token_signing_alg_values_supported")]
		public string[] IdTokenSigningAlgValuesSupported => new[] { "ES256", "ES384", "ES512", "HS256", "HS384", "HS512", "PS256", "PS384", "PS512", "RS256", "RS384", "RS512" };

		[JsonPropertyName("login_hint_methods_supported")]
		public string[] LoginHintMethodsSupported => new[] { "MSISDN", "ENCR_MSISDN", "PCR" };

		[JsonPropertyName("login_hint_types_supported")]
		public string[] LoginHintTypesSupported => new[] { "MSISDN", "ENCR_MSISDN", "PCR" };

		[JsonPropertyName("mc_amr_values_supported")]
		public string[] McAmrValuesSupported => new[] { "SIM_OK","SIM_PIN","USSD_OK","USSD_PIN","SMS_URL_OK","OTP_OK","SEAM_OK_OK","SIM_APP_OK","UMB","SEAM_OK" };

		[JsonPropertyName("mc_claims_parameter_supported")]
		public bool McClaimsParameterSupported => true;

		[JsonPropertyName("mc_hash_algs_supported")]
		public string[] McHashAlgsSupported => new[] { "SHA-256" };

		[JsonPropertyName("mc_version")]
		public string[] McVersion => new[]
		{
			"mc_v1.1",
			"mc_v1.2",
			"mc_v2.0",
			"mc_di_r2_v2.3",
			"mc_si_r2_v1.0"
		};

		[JsonConverter(typeof(KeyValuePairJsonConverter))]
		[JsonPropertyName("mobile_connect_version_supported")]
		public KeyValuePair<string, string>[] MobileConnectVersionSupported => new[]
        {
			new KeyValuePair<string, string>("openid mc_atp", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_authn", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_authn", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_authn", "mc_v1.1"),
			new KeyValuePair<string, string>("openid mc_authn", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_authn", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_authz", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_authz", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_authz", "mc_v1.1"),
			new KeyValuePair<string, string>("openid mc_authz", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_authz", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_bigdata", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_bigdata", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_bigdata", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_bigdata", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_click_auth", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_click_auth", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_click_auth", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_clickless_auth", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_clickless_auth", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_clickless_auth", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_email", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_email", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_email", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_email", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_identity_basic", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_identity_basic", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_identity_basic", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_identity_basic", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_identity_basic_address", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_identity_basic_address", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_identity_basic_address", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_identity_basic_address", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_identity_full", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_identity_full", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_identity_full", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_identity_full", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_identity_nationalid", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_identity_nationalid", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_identity_nationalid", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_identity_nationalid", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_identity_phonenumber", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_identity_phonenumber", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_identity_phonenumber", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_identity_phonenumber", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_identity_signup", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_identity_signup", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_identity_signup", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_identity_signup", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_kyc_hashed", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_kyc_hashed", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_kyc_plain", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_kyc_plain", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_ru_hhe", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_ru_hhe", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_ru_hhe", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_ru_hhe", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_scoring", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_scoring", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_scoring", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_scoring", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_sign_document", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_sign_document", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_sign_document", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_sign_document", "mc_v2.0"),
			new KeyValuePair<string, string>("openid mc_taxid_rus", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid mc_taxid_rus", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid mc_taxid_rus", "mc_v1.2"),
			new KeyValuePair<string, string>("openid mc_taxid_rus", "mc_v2.0"),
			new KeyValuePair<string, string>("openid offline_access", "mc_di_r2_v2.3"),
			new KeyValuePair<string, string>("openid offline_access", "mc_si_r2_v1.0"),
			new KeyValuePair<string, string>("openid offline_access", "mc_v1.1"),
			new KeyValuePair<string, string>("openid offline_access", "mc_v1.2"),
			new KeyValuePair<string, string>("openid offline_access", "mc_v2.0"),
			new KeyValuePair<string, string>("openid", "mc_v1.1")
		};

		[JsonPropertyName("request_object_encryption_alg_values_supported")]
		public string[] RequestObjectEncryptionAlgValuesSupported => new[] { "RSA-OAEP", "RSA-OAEP-256", "RSA1_5" };

		[JsonPropertyName("request_object_encryption_enc_values_supported")]
		public string[] RequestObjectEncryptionEncValuesSupported => new[] { "A256CBC+HS512", "A256GCM", "A192GCM", "A128GCM", "A128CBC-HS256", "A192CBC-HS384", "A256CBC-HS512", "A128CBC+HS256" };

		[JsonPropertyName("request_object_signing_alg_values_supported")]
		public string[] RequestObjectSigningAlgValuesSupported => new[] { "ES256", "ES384", "ES512", "HS256", "HS384", "HS512", "PS256", "PS384", "PS512", "RS256", "RS384", "RS512" };

		[JsonPropertyName("request_parameter_supported")]
		public bool RequestParameterSupported => true;

		[JsonPropertyName("request_uri_parameter_supported")]
		public bool RequestUriParameterSupported => false;

		[JsonPropertyName("require_request_uri_registration")]
		public bool RequireRequestUriRegistration => false;

		[JsonPropertyName("response_types_supported")]
		public string[] ResponseTypesSupported => new[] { "code", "token", "id_token", "mc_si_async_code" };

		[JsonPropertyName("scopes_supported")]
		public string[] ScopesSupported => new[]
		{
			// Скоупы с агрегатора
			"mc_authn",
			"mc_bigdata",
			"mc_email",
			"mc_identity_basic",
			"mc_identity_basic_address",
			"mc_identity_full",
			"mc_identity_nationalid",
			"mc_identity_phonenumber",
			"mc_identity_signup",
			"mc_kyc_hashed",
			"mc_kyc_plain",
			"openid",

			// Дополнительные скоупы с IDGW
			"mc_atp",
			"mc_authz",
			"mc_sign_document",
			"mc_taxid_rus",
			"offline_access"
		};

		[JsonPropertyName("subject_types_supported")]
		public string[] SubjectTypesSupported => new[] { "public", "pairwise" };

		[JsonPropertyName("token_endpoint_auth_methods_supported")]
		public string[] TokenEndpointAuthMethodsSupported => new[] { "client_secret_post", "client_secret_basic", "client_secret_jwt", "private_key_jwt", "none" };

		[JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
		public string[] TokenEndpointAuthSigningAlgValuesSupported => new[] { "ES256", "ES384", "ES512", "HS256", "HS384", "HS512", "PS256", "PS384", "PS512", "RS256", "RS384", "RS512" };

		[JsonPropertyName("ui_locales_supported")]
		public string[] UiLocalesSupported => new[] { "ru-RU", "en-GB", "uz-UZ", "kz-KZ", "uk-UA", "ka-GE" };

		[JsonPropertyName("userinfo_encryption_alg_values_supported")]
		public string[] UserinfoEncryptionAlgValuesSupported => new[] { "RSA-OAEP", "RSA-OAEP-256", "RSA1_5" };

		[JsonPropertyName("userinfo_encryption_enc_values_supported")]
		public string[] UserinfoEncryptionEncValuesSupported => new[] { "A256CBC+HS512", "A256GCM", "A192GCM", "A128GCM", "A128CBC-HS256", "A192CBC-HS384", "A256CBC-HS512", "A128CBC+HS256" };

		[JsonPropertyName("userinfo_signing_alg_values_supported")]
		public string[] UserinfoSigningAlgValuesSupported => new[] { "ES256", "ES384", "ES512", "HS256", "HS384", "HS512", "PS256", "PS384", "PS512", "RS256", "RS384", "RS512" };
    }
}
