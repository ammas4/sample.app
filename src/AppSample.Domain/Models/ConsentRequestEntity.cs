namespace AppSample.Domain.Models;

public class ConsentRequestEntity
{
    public long Id { get; set; }

    public Guid PdpConsentRequestId { get; set; }

    public Guid Code { get; set; }

    public int ServiceProviderId { get; set; }

    public string Msisdn { get; set; }

    public string Scope { get; set; }

    public string AcrValues { get; set; }

    public string? SpHitTs { get; set; }

    public string ResponseType { get; set; }

    public string RequestContent { get; set; }

    public bool HasAdvertisingConsent { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ConfirmedAt { get; set; }

}