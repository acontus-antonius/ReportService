using ReportService.Shared.Models;

namespace ReportService.Api.Services;

public interface IPdfService
{
    Task<byte[]> GenerateEmployeeWithdrawalPdfAsync(EmployeeWithdrawalRequest request);
    Task<byte[]> GeneratePdfFromTemplateAsync(string templateName, object model, string tenantId);
}
