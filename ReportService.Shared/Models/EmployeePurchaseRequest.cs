namespace ReportService.Shared.Models;

public class EmployeePurchaseRequest : BaseReportRequest
{
    public EmployeePurchaseRequest()
    {
        ReportType = ReportType.EmployeePurchase;
    }

    public string EmployeeName { get; set; } = string.Empty;
    public string? EmployeeNumber { get; set; }
    public string? Address { get; set; }
    public string? ZipCode { get; set; }
    public string? City { get; set; }
    public string? Date { get; set; }
    public string? DateRange { get; set; }
    public string? DeliveryDate { get; set; }
    public List<EmployeePurchaseItem> Items { get; set; } = new();
    public decimal TotalGross { get; set; }
}

public class EmployeePurchaseItem
{
    public string DeliveryDate { get; set; } = string.Empty;
    public string ArticleNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal GrossPrice { get; set; }
    public decimal TotalGross { get; set; }
}
