using Scriban.Runtime;
using System.Globalization;

namespace ReportService.Api.Services;

public static class ScribanFormatFunctions
{
    public static string FormatGermanDecimal(decimal value)
    {
        // Deutsches Format: 1.234,56
        return value.ToString("N2", new CultureInfo("de-DE"));
    }

    public static string FormatQuantity(decimal value)
    {
        // Nur Nachkommastellen zeigen wenn != 0
        if (value == Math.Floor(value))
        {
            return value.ToString("0", CultureInfo.InvariantCulture);
        }
        return value.ToString("0.###", new CultureInfo("de-DE"));
    }

    public static void Register(ScriptObject scriptObject)
    {
        scriptObject.Import(typeof(ScribanFormatFunctions), renamer: member => member.Name);
    }
}
