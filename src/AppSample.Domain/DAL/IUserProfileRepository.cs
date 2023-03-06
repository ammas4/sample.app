using AppSample.Domain.Models.Ups;
using AppSample.Domain.Models.UserProfile;

namespace AppSample.Domain.DAL;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetUserProfile(long msisdn);
    Task<PushPinResultState> SendDstkPushPin(long msisdn, string message);
    Task<PushPinResultState> SendMcPushPin(long msisdn, string message);
    Task SendHistory(HistoryCommand command);
}