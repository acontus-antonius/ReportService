using Microsoft.AspNetCore.Mvc;
using ReportService.Shared.Models;
using ReportService.Api.Services;

namespace ReportService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IPdfService pdfService, ILogger<ReportsController> logger)
    {
        _pdfService = pdfService;
        _logger = logger;
    }

    [HttpPost("employee-withdrawal")]
    public async Task<IActionResult> GenerateEmployeeWithdrawalReport(
        [FromBody] EmployeeWithdrawalRequest request)
    {
        try
        {
            _logger.LogInformation("Generating employee withdrawal report for {EmployeeName}",
                request.EmployeeName);

            var pdfData = await _pdfService.GenerateEmployeeWithdrawalPdfAsync(request);

            return File(pdfData, "application/pdf",
                $"Mitarbeiterentnahme_{request.EmployeeName}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employee withdrawal report");
            return StatusCode(500, "Error generating PDF report");
        }
    }

    [HttpPost("employee-purchase")]
    public async Task<IActionResult> GenerateEmployeePurchase(
        [FromBody] EmployeePurchaseRequest request)
    {
        try
        {
            _logger.LogInformation("Generating employee purchase report for {EmployeeName} (Tenant: {TenantId})",
                request.EmployeeName, request.TenantId);

            var pdfData = await _pdfService.GeneratePdfFromTemplateAsync(
                "employee-purchase",
                request,
                request.TenantId);

            return File(pdfData, "application/pdf",
                $"Einkauf_Mitarbeiter_{request.EmployeeName}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating employee purchase report");
            return StatusCode(500, "Error generating PDF report");
        }
    }

    [HttpPost("hemme-invoice")]
    public async Task<IActionResult> GenerateHemmeInvoice(
        [FromBody] HemmeInvoiceRequest request)
    {
        try
        {
            _logger.LogInformation("Generating Hemme invoice {InvoiceNumber} (Tenant: {TenantId})",
                request.InvoiceNumber, request.TenantId);

            var pdfData = await _pdfService.GeneratePdfFromTemplateAsync(
                "hemme-milch",
                request,
                request.TenantId);

            return File(pdfData, "application/pdf",
                $"Rechnung_{request.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Hemme invoice");
            return StatusCode(500, "Error generating PDF report");
        }
    }
}
