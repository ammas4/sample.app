using System.Diagnostics;
using AppSample.Domain.Models;
using AppSample.Domain.Services;
using Inetlab.SMPP;
using Inetlab.SMPP.Common;
using Inetlab.SMPP.PDU;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AppSample.Infrastructure.DAL
{
    public class SmppOneClientDAL : ISmppDAL
    {
        public SmppOneClientDAL(
            ILogger<SIAuthorizeService> logger,
            IOptions<SmppSettings> smppSettings)
        {
            _logger = logger;
            _smppSettings = smppSettings.Value;
        }

        readonly ILogger<SIAuthorizeService> _logger;
        readonly SmppSettings _smppSettings;


        readonly object _lockObject = new();

        SmppClient _smppClient = null!;
        Task _doConnectionTask = null!;
        bool _isConnectionAlive = false;
        int _isConnecting = 0;
        int _connectVersion = 0;


        public async Task<bool> SendAsync(long ctn, string text)
        {
            return await SendWithRetryAsync(ctn, text);
        }

        async Task<bool> SendWithRetryAsync(long ctn, string text)
        {
            const ushort retryCount = 10;
            const ushort delayBeforeRetryMs = 100;

            int attemptsCounter = 0;
            string? lastResponseStatus = null;
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < retryCount; i++)
            {
                attemptsCounter++;
                if (i > 1)
                {
                    await Task.Delay(i * delayBeforeRetryMs);
                }

                var localConnectVersion = _connectVersion;
                if (!_isConnectionAlive)
                {
                    await ReconnectOrAwaitAsync();
                }

                var response = await SendCoreAsync(ctn, text);

                if (response == null)
                    continue;

                lastResponseStatus = response.Header.Status.ToString("G");

                if (response.Header.Status == CommandStatus.ESME_ROK)
                {
                    _logger.LogInformation($"SMPP send completed, ctn={ctn} text=`{text}` status={lastResponseStatus} attempts={attemptsCounter} elapsed={sw.Elapsed}");
                    return true;
                }

                // Если отправка не прошла (не ROK), то надо сбросить _isConnectionAlive, чтобы началось переподключение
                // Сбросить _isConnectionAlive должен только один поток, остальные просто уходят на следующий круг
                // Это нужно, т.к. может возинкнуть ситуация, когда Reconnect уже выполнился и выставил _isConnectionAlive,
                // а какой-то поток всё ещё выполняет код в этой точке, и сбросит _isConnectionAlive, хотя коннект только что создался
                if (Interlocked.CompareExchange(ref _connectVersion, _connectVersion + 1, localConnectVersion) == localConnectVersion)
                {
                    _isConnectionAlive = false;
                }
            }

            _logger.LogInformation($"SMPP send completed, ctn={ctn} text=`{text}` status={lastResponseStatus} attempts={attemptsCounter} elapsed={sw.Elapsed}");
            return false;
        }

        private async Task ReconnectOrAwaitAsync()
        {
            // Все потоки будут ждать пока первый не закинет ReconnectAsync в ThreadPool
            lock (_lockObject)
            {
                if (

                    // Отсечёт, оставшиеся потоки, когда ReconnectAsync доработает до конца
                    !_isConnectionAlive 

                    // Это условие должно идти после проверки _isConnectionAlive == true, т.к. CompareExchange выставит _isConnecting в true.
                    // Только первый поток пройдёт это условие, остальные сразу перейдут к ожиданию _doConnectionTask
                    && Interlocked.CompareExchange(ref _isConnecting, 1, 0) == 0
                    
                    ) 
                {
                    // Первый поток выполняет ReconnectAsync до первого await.
                    // Затем отправляет вторую часть ReconnectAsync в ThreadPool.
                    _doConnectionTask = ReconnectAsync();
                }
            }

            // Все потоки ожидают окончания второй части ReconnectAsync
            await _doConnectionTask;
            // По окончании второй части ReconnectAsync, все потоки продолжают выполнение отсюда
        }

        private async Task ReconnectAsync()
        {
            try
            {
                if (_smppClient != null)
                    _smppClient.Dispose();

                var isConnected = await ConnectCoreAsync();

                if (!isConnected)
                    return;

                _isConnectionAlive = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMPP reconnect failed");
            }
            finally
            {
                // Важно сбрасывать после _isConnectionAlive = true.
                // Т.к. ReconnectOrAwaitAsync сначала проверяет _isConnectionAlive == false, а только потом _isConnecting == false,
                // если делать присвоение наоборот, то между этими проверками может проскочить другой поток
                _isConnecting = 0;
            }
        }

        private async Task<bool> ConnectCoreAsync()
        {
            _smppClient = new SmppClient();

            var isConnSuccess = await _smppClient.ConnectAsync(_smppSettings.Host, _smppSettings.Port);

            if (!isConnSuccess)
                return false;

            var bindResp = await _smppClient.BindAsync(
                _smppSettings.SystemId, _smppSettings.Password, ConnectionMode.Transmitter);

            if (bindResp.Header.Status != CommandStatus.ESME_ROK)
            {
                await _smppClient.DisconnectAsync();
                return false;
            }
            return true;
        }

        private async Task<SubmitSmResp> SendCoreAsync(long ctn, string text)
        {
            var pdus = SMS
                .ForSubmit()
                .From(_smppSettings.From)
                .To(ctn.ToString())
                .Coding(DataCodings.UCS2)
                .Text(text)
                .Create(_smppClient);

            try
            {
                var responses = await _smppClient.SubmitAsync(pdus);

                return responses?.FirstOrDefault()!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMPP send failed");
            }

            return null!;
        }
    }
}
