namespace AppSample.Domain.Models;

public class McTokenResult
{
    public string? AccessToken { get; init; }
    public string? TokenType { get; init; }
    public int ExpiresIn { get; init; }
    public string? Scope { get; init; }
    public string? CorrelationId { get; init; }
    public string? IdToken { get; init; }
}