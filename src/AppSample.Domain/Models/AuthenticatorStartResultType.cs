using AppSample.Domain.DAL.DTOs;
using AppSample.Domain.Models.ServiceProviders;

namespace AppSample.Domain.Models;

public struct AuthChainStartResult
{
    readonly IReadOnlyCollection<AuthenticatorStartResult>? _startResults;
    public bool IsStarted { get; init; }
    public AuthenticatorStartResult? First { get; private init; } 
    public AuthenticatorStartResult? FirstStarted { get; private init; }

    public IReadOnlyCollection<AuthenticatorStartResult>? StartResults
    {
        init
        {
            if (value == null || value.Count == 0) return;
            
            _startResults = value;
            
            foreach (var startResult in _startResults)
            {
                if (startResult.ResultType != AuthenticatorStartResultType.Started) continue;
                FirstStarted = startResult;
                break;
            }

            First = _startResults.First();
        }
    }
}

public struct AuthenticatorStartResult
{
    public AuthenticatorType Authenticator { get; init; }
    public AuthenticatorStartResultType ResultType { get; init; }
    public string? HheUrl { get; init; }
    public string? OtpNotifyUrl { get; init; }
    public AcrValues StartedLoA { get; init; }
}

public enum AuthenticatorStartResultType
{
    NoValue = 0,
    Started = 1,
    SimCardNotSupport = 2,
    ServiceProviderNotFound = 3,
    MsisdnInvalid = 4,
    UserInfoNotFound = 5,
    NotSent = 6,
    UserBlocked = 7,
    OtpNotifyUrlNotFound = 8,
    LoA3NotSupported = 9,
    LoA4NotSupported = 10,
}