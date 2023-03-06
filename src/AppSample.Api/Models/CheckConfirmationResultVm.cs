using System.Text.Json.Serialization;
using AppSample.CoreTools.Helpers;
using AppSample.Domain.Models;
using Nest;
using AppSample.Domain.DAL.DTOs;

namespace AppSample.Api.Models;

public class CheckConfirmationResultVm
{
    [JsonPropertyName("status")] public string Status { get; set; }

    [JsonPropertyName("redirect_url")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RedirectUrl { get; set; }

    [JsonPropertyName("timer_remaining")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? TimerRemaining { get; set; }

    public CheckConfirmationResultVm(string basePath, DiCheckConfirmationResult source)
    {
        Status = source.Status.ToString("G").ToLowerInvariant();
        TimerRemaining = source.TimerRemaining;

        switch (source.Status)
        {
            case CheckConfirmationStatus.OK:
                var urlBuilder = new UrlBuilder(source.RequestRedirectUrl!);
                urlBuilder.Query["state"] = source.State;
                urlBuilder.Query["code"] = source.Code;
                if (!string.IsNullOrEmpty(source.CorrelationId))
                {
                    urlBuilder.Query["correlation_id"] = source.CorrelationId;
                }

                RedirectUrl = urlBuilder.ToString();
                break;

            case CheckConfirmationStatus.Reject:
                RedirectUrl = UrlHelper.Combine(basePath, "/reject");
                break;

            case CheckConfirmationStatus.Timeout:
                RedirectUrl = UrlHelper.Combine(basePath, "/error");
                break;

            case CheckConfirmationStatus.Wait:
                TimerRemaining = source.TimerRemaining;
                break;
        }
    }
}