using Microsoft.AspNetCore.Http;

namespace AppSample.CoreTools.Logging;

public interface ILogRequestResponseSettings
{
    string[]? IndexedScopeHeaders { get; }
    PathString[]? RootPath { get; }
    PathString[]? ExcludePath { get; }
    PathString[]? SkipResponseBody { get; }
    public int UnmaskedPartLength { get; }
    public MaskingStrategy MaskingType { get; }
    public byte PercentageOfMasking { get; }
    public string[]? MaskValues { get; }
}