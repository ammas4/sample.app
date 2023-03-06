using AppSample.Domain.DAL;
using AppSample.Domain.Models;
using AppSample.Domain.Services;
using Inetlab.SMPP;
using Inetlab.SMPP.Common;
using Inetlab.SMPP.PDU;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppSample.Infrastructure.Repositories
{
    public class SmppRepository : ISmppRepository
    {
        readonly ILogger<SIAuthorizeService> _logger;
        readonly SmppSettings _smppSettings;

        public SmppRepository(
            ILogger<SIAuthorizeService> logger,
            IOptions<SmppSettings> smppSettings)
        {
            _smppSettings = smppSettings.Value;
            _logger = logger;
        }


        /// <param name="ctn">Без кода страны</param>
        /// <param name="text"></param>
        public async Task SendSmsInternalAsync(long ctn, string text)
        {
            _logger.LogInformation($"SendSmsInternalAsync ctn={ctn} text=`{text}`");

            using SmppClient client = new SmppClient();
            await client.ConnectAsync(_smppSettings.Host, _smppSettings.Port);
            
            if (client.Status == ConnectionStatus.Open)
            {
                BindResp bindResp = await client.BindAsync(_smppSettings.SystemId, _smppSettings.Password, ConnectionMode.Transmitter);

                if (bindResp.Header.Status == CommandStatus.ESME_ROK)
                {
                    IList<SubmitSmResp> responses = await client.SubmitAsync(
                        SMS.ForSubmit()
                            .Text(text)
                            .From(_smppSettings.From)
                            .To(ctn.ToString())
                            .Coding(DataCodings.UCS2)
                            .DeliveryReceipt()
                    );

                    _logger.LogInformation($"SendSmsInternalAsync ctn={ctn}. ResponseStatuses={string.Join(",", responses.Select(r => r.Header.Status))}");
                }
                else
                {
                    _logger.LogError($"SendSmsInternalAsync ctn={ctn}. CommandStatus={bindResp.Header.Status}");    
                }
                
                await client.DisconnectAsync();
            }
            else
            {
                _logger.LogError($"SendSmsInternalAsync ctn={ctn}. SmppClient.Status={client.Status}");
            }
        }
    }
}