namespace AppSample.Domain.Interfaces;

public interface IGlobalUrlHelper
{
    string GetUssdAskUserTextUrl();
    string GetHheRequestUrl(Guid interactionId);
    string GetHheEnrichmentUrl(Guid interactionId);
    string GetMcPushAnswerUrl();
    string GetMcPushPinAnswerUrl();
    string GetDstkMcPushAnswerUrl();
    string GetDstkPushPinAnswerUrl();
}