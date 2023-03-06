using System.Text.Json.Serialization;
using AppSample.Domain.Models;

namespace AppSample.Api.Models;

public class InfoPayResponseVm
{
    [JsonPropertyName(MobileConnectParameterNames.ClientName)]
    public string ClientName { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.OrderSum)]
    public string OrderSum { get; set; }

    [JsonPropertyName(MobileConnectParameterNames.ConfirmStatus)]
    public string ConfirmStatus { get; set; }

    public InfoPayResponseVm(PaymentInfoResult paymentInfo)
    {
        ClientName = paymentInfo.ClientName;
        OrderSum = paymentInfo.OrderSum;
        ConfirmStatus = paymentInfo.ConfirmStatus;
    }
}