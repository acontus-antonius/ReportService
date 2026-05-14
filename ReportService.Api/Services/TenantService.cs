using ReportService.Shared.Models;

namespace ReportService.Api.Services;

public class TenantService : ITenantService
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<TenantService> _logger;
    private readonly Dictionary<string, TenantConfiguration> _tenants;

    public TenantService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        ILogger<TenantService> logger)
    {
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
        _tenants = LoadTenantConfigurations();
    }

    private Dictionary<string, TenantConfiguration> LoadTenantConfigurations()
    {
        _logger.LogInformation("Loading tenant configurations");

        // Tenant-Konfigurationen - können später aus Datenbank oder appsettings.json geladen werden
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
            }
        };
    }

    public Task<TenantConfiguration> GetTenantConfigurationAsync(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            _logger.LogWarning("TenantId is null or empty, using default tenant 'hemme'");
            tenantId = "hemme";
        }

        if (!_tenants.TryGetValue(tenantId.ToLower(), out var config))
        {
            _logger.LogError("Tenant '{TenantId}' not found", tenantId);
            throw new ArgumentException($"Tenant '{tenantId}' not found");
        }

        _logger.LogInformation("Retrieved configuration for tenant '{TenantId}'", tenantId);
        return Task.FromResult(config);
    }

    public Task<List<TenantConfiguration>> GetAllTenantsAsync()
    {
        _logger.LogInformation("Retrieved all tenant configurations");
        return Task.FromResult(new List<TenantConfiguration>(_tenants.Values));
    }

    public Task<string> GetTenantLogoPathAsync(string tenantId)
    {
        var config = _tenants[tenantId.ToLower()];
        var logoPath = Path.Combine(
            _environment.WebRootPath,
            "images",
            tenantId.ToLower(),
            config.LogoFileName);

        _logger.LogInformation("Logo path for tenant '{TenantId}': {LogoPath}", tenantId, logoPath);
        return Task.FromResult(logoPath);
    }
}
