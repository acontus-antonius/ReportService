// ==============================================================================
// Multi-Tenant & Multi-Report-Type Design für ReportService
// ==============================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReportService.Shared.Models
{
    // ==============================================================================
    // 1. MANDANTEN-KONFIGURATION
    // ==============================================================================

    public class TenantConfiguration
    {
        public string TenantId { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Fax { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string TaxNumber { get; set; } = string.Empty;
        public string VatId { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string IBAN { get; set; } = string.Empty;
        public string BIC { get; set; } = string.Empty;
        public string LogoFileName { get; set; } = string.Empty; // z.B. "hemme-logo.png"
        public Dictionary<string, string> CustomSettings { get; set; } = new();
    }

    // ==============================================================================
    // 2. BELEGART-ENUM
    // ==============================================================================

    public enum ReportType
    {
        EmployeePurchase,      // Mitarbeiter-Einkauf
        HemmeInvoice,          // Hemme-Rechnung
        EmployeeWithdrawal,    // Mitarbeiter-Entnahme
        DeliveryNote,          // Lieferschein
        CreditNote,            // Gutschrift
        MonthlyStatement       // Monatsabrechnung
        // Weitere Belegarten hier hinzufügen
    }

    // ==============================================================================
    // 3. BASIS REQUEST MIT MANDANT & BELEGART
    // ==============================================================================

    public abstract class BaseReportRequest
    {
        public string TenantId { get; set; } = string.Empty;
        public ReportType ReportType { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.Now;
    }

    // ==============================================================================
    // 4. ERWEITERTE EMPLOYEE PURCHASE REQUEST
    // ==============================================================================

    public class EmployeePurchaseRequest : BaseReportRequest
    {
        public EmployeePurchaseRequest()
        {
            ReportType = ReportType.EmployeePurchase;
        }

        public string EmployeeName { get; set; } = string.Empty;
        public string? EmployeeNumber { get; set; }
        public string? Date { get; set; }
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

    // ==============================================================================
    // 5. HEMME INVOICE REQUEST
    // ==============================================================================

    public class HemmeInvoiceRequest : BaseReportRequest
    {
        public HemmeInvoiceRequest()
        {
            ReportType = ReportType.HemmeInvoice;
        }

        public string InvoiceNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string CustomerZip { get; set; } = string.Empty;
        public string CustomerCity { get; set; } = string.Empty;
        public string InvoiceDate { get; set; } = string.Empty;
        public string DeliveryDate { get; set; } = string.Empty;
        public List<InvoiceItem> Items { get; set; } = new();
        public decimal SubTotal { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class InvoiceItem
    {
        public int Position { get; set; }
        public string ArticleNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public string Unit { get; set; } = "Stk";
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

namespace ReportService.Api.Services
{
    // ==============================================================================
    // 6. TENANT SERVICE INTERFACE
    // ==============================================================================

    public interface ITenantService
    {
        Task<TenantConfiguration> GetTenantConfigurationAsync(string tenantId);
        Task<List<TenantConfiguration>> GetAllTenantsAsync();
        Task<string> GetTenantLogoPathAsync(string tenantId);
    }

    // ==============================================================================
    // 7. TENANT SERVICE IMPLEMENTATION
    // ==============================================================================

    public class TenantService : ITenantService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly Dictionary<string, TenantConfiguration> _tenants;

        public TenantService(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
            _tenants = LoadTenantConfigurations();
        }

        private Dictionary<string, TenantConfiguration> LoadTenantConfigurations()
        {
            // Hier kannst du aus Datenbank, Config-Datei oder appsettings.json laden
            return new Dictionary<string, TenantConfiguration>
            {
                ["hemme"] = new TenantConfiguration
                {
                    TenantId = "hemme",
                    CompanyName = "Hemme Milch GmbH & Co. KG",
                    Street = "Heideweg 4",
                    ZipCode = "16278",
                    City = "Angermünde/OT Schmargendorf",
                    Phone = "03331 25 25 25",
                    Fax = "03331 25 25 26",
                    Email = "info@hemme-uckermark.de",
                    Website = "www.hemme-uckermark.de",
                    TaxNumber = "062/171/09270",
                    VatId = "DE269646421",
                    BankName = "Sparkasse Uckermark",
                    IBAN = "DE34 1705 6060 3624 014918",
                    BIC = "WELADED1UMP",
                    LogoFileName = "hemme-logo.png"
                },
                // Weitere Mandanten hier hinzufügen
                ["firma2"] = new TenantConfiguration
                {
                    TenantId = "firma2",
                    CompanyName = "Beispiel GmbH",
                    Street = "Musterstraße 1",
                    ZipCode = "12345",
                    City = "Musterstadt",
                    Phone = "012345 67890",
                    Email = "info@beispiel.de",
                    Website = "www.beispiel.de",
                    LogoFileName = "firma2-logo.png"
                }
            };
        }

        public Task<TenantConfiguration> GetTenantConfigurationAsync(string tenantId)
        {
            if (!_tenants.TryGetValue(tenantId, out var config))
            {
                throw new ArgumentException($"Tenant '{tenantId}' not found");
            }
            return Task.FromResult(config);
        }

        public Task<List<TenantConfiguration>> GetAllTenantsAsync()
        {
            return Task.FromResult(new List<TenantConfiguration>(_tenants.Values));
        }

        public Task<string> GetTenantLogoPathAsync(string tenantId)
        {
            var config = _tenants[tenantId];
            var logoPath = Path.Combine(_environment.WebRootPath, "images", tenantId, config.LogoFileName);
            return Task.FromResult(logoPath);
        }
    }

    // ==============================================================================
    // 8. ERWEITERTE TEMPLATE SERVICE INTERFACE
    // ==============================================================================

    public interface ITemplateService
    {
        Task<string> RenderTemplateAsync(string templateName, object model, TenantConfiguration tenant);
    }

    // ==============================================================================
    // 9. ERWEITERTE SCRIBAN TEMPLATE SERVICE
    // ==============================================================================

    public class ScribanTemplateService : ITemplateService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ScribanTemplateService> _logger;

        public ScribanTemplateService(
            IWebHostEnvironment environment,
            ILogger<ScribanTemplateService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> RenderTemplateAsync(
            string templateName,
            object model,
            TenantConfiguration tenant)
        {
            // Template-Pfad: Templates/{templateName}/Invoice.html
            var templatePath = Path.Combine(
                _environment.ContentRootPath,
                "Templates",
                templateName,
                "Invoice.html");

            _logger.LogInformation("Loading template from {TemplatePath}", templatePath);

            var templateContent = await File.ReadAllTextAsync(templatePath);

            // Logo für den Mandanten laden
            var logoPath = Path.Combine(
                _environment.WebRootPath,
                "images",
                tenant.TenantId,
                tenant.LogoFileName);

            if (File.Exists(logoPath))
            {
                var logoBytes = await File.ReadAllBytesAsync(logoPath);
                var logoBase64 = Convert.ToBase64String(logoBytes);

                // Logo ersetzen - unterstützt verschiedene Pfade
                templateContent = templateContent
                    .Replace($"src=\"images/{tenant.LogoFileName}\"",
                        $"src=\"data:image/png;base64,{logoBase64}\"")
                    .Replace("src=\"images/hemme-logo.png\"",
                        $"src=\"data:image/png;base64,{logoBase64}\"");
            }

            var template = Template.Parse(templateContent);
            var context = new TemplateContext();
            var scriptObject = new ScriptObject();

            // Model importieren
            scriptObject.Import(model, renamer: member => member.Name, filter: member => true);

            // Tenant-Daten importieren
            scriptObject["Tenant"] = tenant;

            // Custom Functions registrieren
            ScribanFormatFunctions.Register(scriptObject);

            context.PushGlobal(scriptObject);
            context.MemberRenamer = member => member.Name;

            var result = await template.RenderAsync(context);
            return result;
        }
    }

    // ==============================================================================
    // 10. ERWEITERTE PDF SERVICE INTERFACE
    // ==============================================================================

    public interface IPdfService
    {
        Task<byte[]> GeneratePdfFromTemplateAsync(
            string templateName,
            object model,
            string tenantId);
    }

    // ==============================================================================
    // 11. ERWEITERTE PUPPETEER PDF SERVICE
    // ==============================================================================

    public class PuppeteerPdfService : IPdfService
    {
        private readonly ILogger<PuppeteerPdfService> _logger;
        private readonly ITemplateService _templateService;
        private readonly ITenantService _tenantService;
        private IBrowser? _browser;

        public PuppeteerPdfService(
            ILogger<PuppeteerPdfService> logger,
            ITemplateService templateService,
            ITenantService tenantService)
        {
            _logger = logger;
            _templateService = templateService;
            _tenantService = tenantService;
        }

        public async Task<byte[]> GeneratePdfFromTemplateAsync(
            string templateName,
            object model,
            string tenantId)
        {
            await EnsureBrowserInitializedAsync();

            var page = await _browser!.NewPageAsync();
            try
            {
                _logger.LogInformation(
                    "Rendering template {TemplateName} for tenant {TenantId}",
                    templateName,
                    tenantId);

                // Tenant-Konfiguration laden
                var tenant = await _tenantService.GetTenantConfigurationAsync(tenantId);

                // Template mit Tenant-Daten rendern
                var html = await _templateService.RenderTemplateAsync(templateName, model, tenant);

                await page.SetContentAsync(html, new NavigationOptions
                {
                    WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
                });

                // Seitennummerierung injizieren
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

                _logger.LogInformation(
                    "PDF generated successfully for tenant {TenantId}, size: {Size} bytes",
                    tenantId,
                    pdfData.Length);

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

            _logger.LogInformation("Downloading Chromium browser...");
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            _logger.LogInformation("Launching browser...");
            _browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            });

            _logger.LogInformation("Browser launched successfully");
        }
    }

    // ==============================================================================
    // 12. ERWEITERTE CONTROLLER
    // ==============================================================================

    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly IPdfService _pdfService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(
            IPdfService pdfService,
            ILogger<ReportsController> logger)
        {
            _pdfService = pdfService;
            _logger = logger;
        }

        [HttpPost("employee-purchase")]
        public async Task<IActionResult> GenerateEmployeePurchase(
            [FromBody] EmployeePurchaseRequest request)
        {
            _logger.LogInformation(
                "Generating employee purchase report for {EmployeeName} (Tenant: {TenantId})",
                request.EmployeeName,
                request.TenantId);

            var pdfData = await _pdfService.GeneratePdfFromTemplateAsync(
                "employee-purchase",
                request,
                request.TenantId);

            var fileName = $"Einkauf_Mitarbeiter_{request.EmployeeName}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfData, "application/pdf", fileName);
        }

        [HttpPost("hemme-invoice")]
        public async Task<IActionResult> GenerateHemmeInvoice(
            [FromBody] HemmeInvoiceRequest request)
        {
            _logger.LogInformation(
                "Generating Hemme invoice {InvoiceNumber} for {CustomerName} (Tenant: {TenantId})",
                request.InvoiceNumber,
                request.CustomerName,
                request.TenantId);

            var pdfData = await _pdfService.GeneratePdfFromTemplateAsync(
                "hemme-invoice",
                request,
                request.TenantId);

            var fileName = $"Rechnung_{request.InvoiceNumber}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfData, "application/pdf", fileName);
        }

        // Generische Methode für alle Belegarten
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateReport(
            [FromBody] BaseReportRequest request)
        {
            var templateName = GetTemplateName(request.ReportType);

            _logger.LogInformation(
                "Generating report type {ReportType} for tenant {TenantId}",
                request.ReportType,
                request.TenantId);

            var pdfData = await _pdfService.GeneratePdfFromTemplateAsync(
                templateName,
                request,
                request.TenantId);

            var fileName = $"{request.ReportType}_{request.TenantId}_{DateTime.Now:yyyyMMdd}.pdf";
            return File(pdfData, "application/pdf", fileName);
        }

        private string GetTemplateName(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.EmployeePurchase => "employee-purchase",
                ReportType.HemmeInvoice => "hemme-invoice",
                ReportType.EmployeeWithdrawal => "employee-withdrawal",
                ReportType.DeliveryNote => "delivery-note",
                ReportType.CreditNote => "credit-note",
                ReportType.MonthlyStatement => "monthly-statement",
                _ => throw new ArgumentException($"Unknown report type: {reportType}")
            };
        }
    }
}

// ==============================================================================
// 13. PROGRAM.CS REGISTRIERUNG
// ==============================================================================
/*
var builder = WebApplication.CreateBuilder(args);

// Services registrieren
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Report Services
builder.Services.AddSingleton<ITenantService, TenantService>();
builder.Services.AddSingleton<ITemplateService, ScribanTemplateService>();
builder.Services.AddSingleton<IPdfService, PuppeteerPdfService>();

var app = builder.Build();
*/

// ==============================================================================
// 14. VERZEICHNISSTRUKTUR
// ==============================================================================
/*
ReportService.Api/
├── wwwroot/
│   └── images/
│       ├── hemme/
│       │   └── hemme-logo.png
│       └── firma2/
│           └── firma2-logo.png
├── Templates/
│   ├── employee-purchase/
│   │   └── Invoice.html
│   ├── hemme-invoice/
│   │   └── Invoice.html
│   └── delivery-note/
│       └── Invoice.html
└── appsettings.json
*/

// ==============================================================================
// 15. APPSETTINGS.JSON MIT TENANT-KONFIGURATION
// ==============================================================================
/*
{
  "Tenants": {
    "hemme": {
      "CompanyName": "Hemme Milch GmbH & Co. KG",
      "Address": {
        "Street": "Heideweg 4",
        "ZipCode": "16278",
        "City": "Angermünde/OT Schmargendorf"
      },
      "Contact": {
        "Phone": "03331 25 25 25",
        "Fax": "03331 25 25 26",
        "Email": "info@hemme-uckermark.de",
        "Website": "www.hemme-uckermark.de"
      },
      "Tax": {
        "TaxNumber": "062/171/09270",
        "VatId": "DE269646421"
      },
      "Banking": {
        "BankName": "Sparkasse Uckermark",
        "IBAN": "DE34 1705 6060 3624 014918",
        "BIC": "WELADED1UMP"
      },
      "Logo": "hemme-logo.png"
    }
  }
}
*/

// ==============================================================================
// 16. TEMPLATE MIT TENANT-VARIABLEN (Scriban)
// ==============================================================================
/*
<div class="company-info">
    <strong>{{ Tenant.CompanyName }}</strong> |
    {{ Tenant.Street }} |
    {{ Tenant.ZipCode }} {{ Tenant.City }}
</div>

<div class="footer-company-info">
    <p><strong>{{ Tenant.CompanyName }}</strong> | {{ Tenant.Street }} | {{ Tenant.ZipCode }} {{ Tenant.City }}</p>
    <p>Telefon: {{ Tenant.Phone }} | Telefax: {{ Tenant.Fax }}</p>
    <p>eMail: {{ Tenant.Email }} | {{ Tenant.Website }}</p>
    <p>Steuernummer: {{ Tenant.TaxNumber }} | USt-ID: {{ Tenant.VatId }}</p>
    <p>{{ Tenant.BankName }} IBAN: {{ Tenant.IBAN }} BIC: {{ Tenant.BIC }}</p>
</div>
*/

// ==============================================================================
// 17. WPF CLIENT USAGE
// ==============================================================================
/*
var request = new EmployeePurchaseRequest
{
    TenantId = "hemme",  // <- Wichtig!
    EmployeeName = "Anette Koßmann",
    EmployeeNumber = "57",
    Date = DateTime.Now.ToString("dd.MM.yyyy"),
    Items = new List<EmployeePurchaseItem> { ... },
    TotalGross = 15.50m
};

var pdfData = await reportService.GenerateEmployeePurchasePdfAsync(request);
*/
