namespace AppSample.Admin.Models.Statistics;

public class StatisticsStatViewModel
{
    public List<object>? Items { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int TotalItemCount { get; set; }
    public long? SelectedCtn { get; set; }
    public DateTime? BeginDate { get; set; }
    public DateTime? EndDate { get; set; }

    public List<int>? SelectedProviderIds { get; set; }
    public List<int>? ExceptedProviderIds { get; set; }
    public List<int>? SelectedOperatorsIds { get; set; }

    public string? SelectedDateGrouping { get; set; }
    public string? SelectedSPGrouping { get; set; }
    public string? SelectedQueryTypes { get; set; }
    public string? SelectedStatusType { get; set; }
}