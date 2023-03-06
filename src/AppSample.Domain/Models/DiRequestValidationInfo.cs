using System.Collections.Immutable;

namespace AppSample.Domain.Models;

public class DiRequestValidationInfo
{
    public ImmutableHashSet<string> Scopes { get; init; }
}