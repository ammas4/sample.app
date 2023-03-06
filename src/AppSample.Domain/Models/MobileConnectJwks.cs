using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;

namespace AppSample.Domain.Models
{
    public class MobileConnectJwks
	{
		[JsonPropertyName(JsonWebKeyParameterNames.Keys)]
		public List<MobileConnectJwk> Keys { get; set; } = new();
	}

	public class MobileConnectJwk
	{
		public MobileConnectJwk()
		{
			E = "";
			Kty = "";
			Use = "";
			Kid = "";
			Alg = "";
			N = "";
		}

		public MobileConnectJwk(JsonWebKey jsonWebKey)
		{
			E = jsonWebKey.E;
			Kty = jsonWebKey.Kty;
			Use = jsonWebKey.Use;
			Kid = jsonWebKey.Kid;
			Alg = jsonWebKey.Alg;
			N = jsonWebKey.N;
		}

		[JsonPropertyName(JsonWebKeyParameterNames.E)]
		public string E { get; set; }

		[JsonPropertyName(JsonWebKeyParameterNames.Kty)]
		public string Kty { get; set; }

		[JsonPropertyName(JsonWebKeyParameterNames.Use)]
		public string Use { get; set; }

		[JsonPropertyName(JsonWebKeyParameterNames.Kid)]
		public string Kid { get; set; }

		[JsonPropertyName(JsonWebKeyParameterNames.Alg)]
		public string Alg { get; set; }

		[JsonPropertyName(JsonWebKeyParameterNames.N)]
		public string N { get; set; }
	}
}
