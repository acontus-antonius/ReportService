using ReportService.Shared.Models;

namespace ReportService.Api.Services;

public interface ITenantService
{
    Task<TenantConfiguration> GetTenantConfigurationAsync(string tenantId);
    Task<List<TenantConfiguration>> GetAllTenantsAsync();
    Task<string> GetTenantLogoPathAsync(string tenantId);
}
