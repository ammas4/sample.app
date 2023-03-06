using System.Net;
using AppSample.Domain.Interfaces;
using AppSample.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetTools;

namespace AppSample.Domain.Services;

public class IPRangeService : IIPRangeService
{
    readonly ILogger<IPRangeService> _logger;
    readonly Dictionary<byte, Dictionary<byte, List<IPAddressRange>>> _ranges;

    public IPRangeService(IOptions<IdgwSettings> idgwSettings,
        ILogger<IPRangeService> logger)
    {
        _logger = logger;
        _ranges = ParseRanges(idgwSettings.Value.BeelineIpRange);
    }

    public bool IsBeelineIp(IPAddress ipAddress)
    {
        var addressBytes = ipAddress.GetAddressBytes();

        return _ranges.ContainsKey(addressBytes[0])
            && _ranges[addressBytes[0]].ContainsKey(addressBytes[1])
            & _ranges[addressBytes[0]][addressBytes[1]].Any(x => x.Contains(ipAddress));
    }

    Dictionary<byte, Dictionary<byte, List<IPAddressRange>>> ParseRanges(string allRangesString)
    {
        var ranges = new Dictionary<byte, Dictionary<byte, List<IPAddressRange>>>();
        var rangeStrings = allRangesString.Split(",");

        foreach (var rangeString in rangeStrings)
        {
            var rangeParts = rangeString.Split(".");
            if (rangeParts.Length != 4
                || !byte.TryParse(rangeParts[0], out var firstByte)
                || !byte.TryParse(rangeParts[1], out var secondByte)
                || !IPAddressRange.TryParse(rangeString, out var range))
            {
                _logger.LogError("Invalid IP range string: {rangeString}", rangeString);
                continue;
            }

            if (!ranges.ContainsKey(firstByte))
            {
                ranges.Add(firstByte, new Dictionary<byte, List<IPAddressRange>>());
            }

            if (!ranges[firstByte].ContainsKey(secondByte))
            {
                ranges[firstByte].Add(secondByte, new List<IPAddressRange>());
            }

            ranges[firstByte][secondByte].Add(range);
        }

        return ranges;
    }
}
