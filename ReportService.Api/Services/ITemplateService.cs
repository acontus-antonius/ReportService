using ReportService.Shared.Models;

namespace ReportService.Api.Services;

public interface ITemplateService
{
    Task<string> RenderTemplateAsync(string templateName, object model, TenantConfiguration tenant);
}
