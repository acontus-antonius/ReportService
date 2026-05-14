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
        var templatePath = Path.Combine(
            _environment.ContentRootPath,
            "Templates",
            templateName,
            "Invoice.html");

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

        if (File.Exists(logoPath))
        {
            var logoBytes = await File.ReadAllBytesAsync(logoPath);
            var logoBase64 = Convert.ToBase64String(logoBytes);

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

        // Register custom format functions
        ScribanFormatFunctions.Register(scriptObject);

        context.PushGlobal(scriptObject);

        // Enable automatic member renaming for nested objects
        context.MemberRenamer = member => member.Name;

        var result = await template.RenderAsync(context);
        return result;
    }
}
