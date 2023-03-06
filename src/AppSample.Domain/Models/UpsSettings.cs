using AppSample.CoreTools.Settings;

namespace AppSample.Domain.Models;

public class UpsSettings : BaseSettings
{
    /// <summary>
    /// Признак отправки данных о результате процесса аутентификации
    /// </summary>
    public bool? ReportAboutAuthResult { get; set; }
}