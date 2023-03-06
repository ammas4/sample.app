using System.ServiceModel;
using Beeline.MobileID.CoreTools.Helpers;
using Beeline.MobileID.CoreTools.Logging;
using Beeline.MobileID.RegionDb.Client.Connected_Services.RegionDb;
using Beeline.MobileID.RegionDb.Client.Settings;
using Microsoft.Extensions.Logging;
using RegionDb;

namespace Beeline.MobileID.RegionDb.Client;

public class RegionDbClient :  IRegionDbClient
{
    private readonly ILogger<RegionDbClient> _logger;
    private readonly IRequestResponseLogger _requestResponseLogger;
    private readonly IRegionDbSettings _settings;
    private readonly string _clientName;

    public RegionDbClient(ILogger<RegionDbClient> logger, IRequestResponseLogger requestResponseLogger, IRegionDbSettings regionSettings)
    {
        _settings = regionSettings;
        _logger = logger;
        _requestResponseLogger = requestResponseLogger;
        _clientName = LogHelper.GetClientName(this);
    }

    public async Task<RegionOperatorInfo?> GetInfoByMsisdnAsync(long msisdn)
    {
        var phoneNumber = MsisdnHelper.GetCtnFromRussianNumber(msisdn);

        var remoteAddress = new EndpointAddress(_settings.Url);
        var binding = new BasicHttpBinding
        {
            MaxBufferSize = int.MaxValue,
            ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max,
            MaxReceivedMessageSize = int.MaxValue,
            AllowCookies = true
        };

        using var service = new RegionContractClient(binding, remoteAddress);
        service.Endpoint.EndpointBehaviors.Add(new ApiLoggingEndpointBehavior(_requestResponseLogger, _clientName));
        
        try
        {
            var tResult = await service.GetUserOperatorInfoAsync(phoneNumber);
                
            return new RegionOperatorInfo
            {
                FederalOperatorCode = tResult.FederalOperatorCode,
                FederalOperatorName = tResult.FederalOperatorName,
                FederalRegionCode = tResult.FederalRegionCode,
                FederalRegionName = tResult.FederalRegionName,
                OperatorName = tResult.OperatorName,
                PhoneNumber = tResult.PhoneNumber,
                RegionName = tResult.RegionName,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка загрузки данных из RegionDb");
            return null;
        }
    }
}