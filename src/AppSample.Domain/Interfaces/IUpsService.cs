using AppSample.Domain.DAL.DTOs;

namespace AppSample.Domain.Interfaces;

public interface IUpsService
{
    Task ReportAboutAuthResult(bool wasSuccess, AuthorizationRequestDto authReqDto);
}