using Scriban;
using Scriban.Runtime;
using ReportService.Shared.Models;

namespace ReportService.Api.Services;

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

    public async Task<string> RenderTemplateAsync(string templateName, object model, TenantConfiguration tenant)
    {
        // Map template names to specific HTML files
        var templateFileName = templateName switch
        {
            "employee-purchase" => "EmployeePurchase.html",
            "hemme-milch" => "HemmeInvoice.html",
            "hemme-lagerbestand" => "Lagerbestand.html",
            _ => "Invoice.html" // Fallback for any other templates
        };

        var templatePath = Path.Combine(
            _environment.ContentRootPath,
            "Templates",
            templateName,
            templateFileName);

        if (!File.Exists(templatePath))
        {
            throw new FileNotFoundException($"Template not found: {templatePath}");
        }

        _logger.LogInformation("Loading template from {TemplatePath}", templatePath);

        var templateContent = await File.ReadAllTextAsync(templatePath);

        // Convert logo to base64 for embedding (tenant-specific)
        var logoPath = Path.Combine(
            _environment.WebRootPath,
            "images",
            tenant.TenantId.ToLower(),
            tenant.LogoFileName);

        string logoBase64 = string.Empty;
        if (File.Exists(logoPath))
        {
            var logoBytes = await File.ReadAllBytesAsync(logoPath);
            logoBase64 = Convert.ToBase64String(logoBytes);

            // Replace various logo references
            templateContent = templateContent
                .Replace("src=\"images/hemme-logo.png\"", $"src=\"data:image/png;base64,{logoBase64}\"")
                .Replace($"src=\"images/{tenant.LogoFileName}\"", $"src=\"data:image/png;base64,{logoBase64}\"");
        }
        else
        {
            _logger.LogWarning("Logo file not found: {LogoPath}", logoPath);
        }

        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            var errors = string.Join(", ", template.Messages.Select(m => m.Message));
            throw new InvalidOperationException($"Template parsing errors: {errors}");
        }

        // Create a TemplateContext with member renaming
        var context = new TemplateContext();
        var scriptObject = new ScriptObject();

        // Import model with member filter to include all properties
        scriptObject.Import(model, renamer: member => member.Name, filter: member => true);

        // Import tenant configuration
        scriptObject["Tenant"] = tenant;

        // Make logo available as base64 for templates
        scriptObject["logo_base64"] = logoBase64;

        // Register custom format functions
        ScribanFormatFunctions.Register(scriptObject);

        context.PushGlobal(scriptObject);

        // Enable automatic member renaming for nested objects
        context.MemberRenamer = member => member.Name;

        var result = await template.RenderAsync(context);
        return result;
    }
}
