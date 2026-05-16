using PuppeteerSharp;
using PuppeteerSharp.Media;
using ReportService.Shared.Models;
using System.Text.Json;

namespace ReportService.Api.Services;

public class PuppeteerPdfService : IPdfService
{
    private readonly ILogger<PuppeteerPdfService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ITemplateService _templateService;
    private readonly ITenantService _tenantService;
    private IBrowser? _browser;

    public PuppeteerPdfService(
        ILogger<PuppeteerPdfService> logger,
        IConfiguration configuration,
        ITemplateService templateService,
        ITenantService tenantService)
    {
        _logger = logger;
        _configuration = configuration;
        _templateService = templateService;
        _tenantService = tenantService;
    }

    public async Task<byte[]> GenerateEmployeeWithdrawalPdfAsync(EmployeeWithdrawalRequest request)
    {
        await EnsureBrowserInitializedAsync();

        var page = await _browser!.NewPageAsync();
        try
        {
            // URL der Angular-Anwendung (später aus Configuration)
            var angularUrl = _configuration["Angular:BaseUrl"] ?? "http://localhost:4200";
            var reportId = Guid.NewGuid().ToString();

            // Daten als Query-Parameter oder später über API-Endpoint
            var dataJson = JsonSerializer.Serialize(request);
            var encodedData = Uri.EscapeDataString(dataJson);

            var url = $"{angularUrl}/reports/employee-withdrawal?data={encodedData}";

            _logger.LogInformation("Navigating to {Url}", url);

            await page.GoToAsync(url, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 },
                Timeout = 30000
            });

            // Warten auf Report-Rendering
            await page.WaitForSelectorAsync(".report-container", new WaitForSelectorOptions
            {
                Timeout = 10000
            });

            var pdfData = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                MarginOptions = new MarginOptions
                {
                    Top = "10mm",
                    Right = "10mm",
                    Bottom = "10mm",
                    Left = "10mm"
                }
            });

            _logger.LogInformation("PDF generated successfully, size: {Size} bytes", pdfData.Length);

            return pdfData;
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    public async Task<byte[]> GeneratePdfFromTemplateAsync(string templateName, object model, string tenantId)
    {
        await EnsureBrowserInitializedAsync();

        var page = await _browser!.NewPageAsync();
        try
        {
            _logger.LogInformation("Rendering template {TemplateName} for tenant {TenantId}", templateName, tenantId);

            // Load tenant configuration
            var tenant = await _tenantService.GetTenantConfigurationAsync(tenantId);

            // Render template with tenant data
            var html = await _templateService.RenderTemplateAsync(templateName, model, tenant);

            // Debug: Save HTML to file
            var debugPath = Path.Combine(Path.GetTempPath(), $"debug_{templateName}_{tenantId}_{DateTime.Now:yyyyMMdd_HHmmss}.html");
            await File.WriteAllTextAsync(debugPath, html);
            _logger.LogInformation("Debug HTML saved to {DebugPath}", debugPath);

            await page.SetContentAsync(html, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            // Inject page number script before PDF generation
            await page.EvaluateExpressionAsync(@"
                const pageNumbers = document.querySelectorAll('.footer-page-number');
                let pageNum = 1;
                pageNumbers.forEach(el => {
                    el.textContent = 'Seite ' + pageNum;
                });
            ");

            var pdfData = await page.PdfDataAsync(new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true,
                DisplayHeaderFooter = false,
                MarginOptions = new MarginOptions
                {
                    Top = "10mm",
                    Right = "10mm",
                    Bottom = "10mm",
                    Left = "10mm"
                }
            });

            _logger.LogInformation("PDF generated successfully for tenant {TenantId}, size: {Size} bytes", tenantId, pdfData.Length);

            return pdfData;
        }
        finally
        {
            await page.CloseAsync();
        }
    }

    private async Task EnsureBrowserInitializedAsync()
    {
        if (_browser != null) return;

        var chromiumExecutablePath = _configuration["Puppeteer:ExecutablePath"]
            ?? Environment.GetEnvironmentVariable("PUPPETEER_EXECUTABLE_PATH");

        if (string.IsNullOrWhiteSpace(chromiumExecutablePath))
        {
            _logger.LogInformation("Downloading Chromium browser...");

            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
        }
        else
        {
            _logger.LogInformation("Using Chromium browser at {ExecutablePath}", chromiumExecutablePath);
        }

        _logger.LogInformation("Launching browser...");

        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            ExecutablePath = chromiumExecutablePath,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage" }
        });

        _logger.LogInformation("Browser launched successfully");
    }
}
