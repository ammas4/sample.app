using AppSample.Domain.Models;
using AppSample.Domain.Services;

namespace AppSample.Domain.DAL;

public interface IUssdRepository
{
    Task<(bool isOk, UssdFailure failureCode)> AskUserForConsistAsync(string msisdn);
}