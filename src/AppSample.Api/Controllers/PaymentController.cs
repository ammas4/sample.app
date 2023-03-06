using AppSample.CoreTools.Contracts;
using AppSample.CoreTools.Exceptions;
using AppSample.CoreTools.Extensions;
using AppSample.CoreTools.Helpers;
using AppSample.Api.Models;
using AppSample.Domain.Models;
using AppSample.Domain.Services;
using AppSample.Domain.Services.Authenticators;
using Microsoft.AspNetCore.Mvc;

namespace AppSample.Api.Controllers
{
    /// <summary>
    /// Контроллер для методов платежного сценария
    /// </summary>
    [ApiController]
    [Route("api/payment")]
    public class PaymentController : ControllerBase
    {
        readonly ISmsUrlAuthenticator _smsUrlAuthenticator;

        public PaymentController(ISmsUrlAuthenticator smsUrlAuthenticator)
        {
            _smsUrlAuthenticator = smsUrlAuthenticator;
        }
        
        /// <summary>
        /// Метод получения информации о платеже по коду из смс
        /// </summary>
        /// <returns>Информация о платеже, необходимая для отображения на странице подтверждения оплаты</returns>
        /// <param name="code">Сокращенный уникальный код авторизации</param>
        [HttpGet("info")]
        public async Task<IActionResult> PaymentInfo(string? code)
        {
            if (string.IsNullOrWhiteSpace(code)
                || StringHelper.TryParseShortGuid(code, out Guid key) == false)
                return new UnifiedException(OAuth2Error.InvalidRequest, "Empty or invalid code").FormResponse();

            var info = await _smsUrlAuthenticator.GetRequestInfoByConfirmCodeAsync(key);

            return info.ToObjectResult(result => new InfoPayResponseVm(result));
        }

        /// <summary>
        /// Метод подтверждает оплату. Платежный сценарий
        /// </summary>
        /// <param name="request">Тело запроса на подтверждение оплаты</param>
        /// <returns>200 при успешном подтверждении</returns>
        [HttpPost("confirm")]
        public async Task<IActionResult> ConfirmPayment([FromBody]ConfirmPayRequestVm request)
        {
            if (string.IsNullOrWhiteSpace(request.Code)
                || StringHelper.TryParseShortGuid(request.Code, out Guid key) == false)
                return new UnifiedException(OAuth2Error.InvalidRequest, "Empty or invalid code").FormResponse();

            if (string.IsNullOrWhiteSpace(request.Reason)
               || Enum.TryParse(request.Reason, ignoreCase:true, out ConfirmationReason reason) == false)
                return new UnifiedException(OAuth2Error.InvalidRequest, "Empty or invalid confirmation reason").FormResponse();

            var confirmationResult = await _smsUrlAuthenticator.ProcessConfirmCodeAsync(key, reason);
            return confirmationResult.ToObjectResult(_ => Ok());
        }
    }
}
