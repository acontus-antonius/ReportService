namespace ReportService.Shared.Models;

public class HemmeInvoiceRequest : BaseReportRequest
{
    public HemmeInvoiceRequest()
    {
        ReportType = ReportType.HemmeInvoice;
    }

    public string InvoiceNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? CustomerZip { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerNumber { get; set; }
    public string? DeliveryDate { get; set; }
    public List<InvoiceItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal Total { get; set; }
    public string? PaymentTerms { get; set; }
    public string? BankAccount { get; set; }
}

public class InvoiceItem
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? Unit { get; set; }
    public decimal Price { get; set; }
    public decimal Total { get; set; }
}
