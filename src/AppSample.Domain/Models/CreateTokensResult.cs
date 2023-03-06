namespace AppSample.Domain.Models;

public class CreateTokensResult
{
    public string AccessToken { get; init; }
    public string IdToken { get; init; }
    public int ExpiresIn { get; init; }
    public string TokenType { get; init; }
}