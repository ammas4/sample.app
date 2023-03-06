using AppSample.CoreTools.Logging;
using Microsoft.AspNetCore.Http;

namespace AppSample.CoreTools.Settings;

public class LogRequestResponseSettings : BaseSettings, ILogRequestResponseSettings
{
    public string[]? IndexedScopeHeaders { get; set; }
    public PathString[]? RootPath { get; set; }
    public PathString[]? ExcludePath { get; set; }
    public PathString[]? SkipResponseBody { get; set; }

    public int UnmaskedPartLength { get; set; }

    public MaskingStrategy MaskingType { get; set; }
    public byte PercentageOfMasking { get; set; }
    public string[]? MaskValues { get; set; }
}