namespace ReportService.Shared.Models;

public abstract class BaseReportRequest
{
    public string TenantId { get; set; } = string.Empty;
    public ReportType ReportType { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.Now;
}
