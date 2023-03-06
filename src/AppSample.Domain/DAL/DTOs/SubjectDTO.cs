using AppSample.CoreTools.DapperContrib;

namespace AppSample.Domain.DAL.Entities;

public class SubjectDTO
{
    [Key]
    public long Id { get; set; }

    public string Msisdn { get; set; }
    public int ServiceProviderId { get; set; }
    public Guid SubjectId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
