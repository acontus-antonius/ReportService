namespace ReportService.Shared.Models;

public class ReportResponse
{
    public byte[] PdfData { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/pdf";
}
