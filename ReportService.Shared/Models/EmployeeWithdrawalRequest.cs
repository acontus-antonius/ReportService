namespace ReportService.Shared.Models;

public class EmployeeWithdrawalRequest
{
    public string EmployeeName { get; set; } = string.Empty;
    public List<WithdrawalItem> Items { get; set; } = new();
}

public class WithdrawalItem
{
    public string ArticleNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
