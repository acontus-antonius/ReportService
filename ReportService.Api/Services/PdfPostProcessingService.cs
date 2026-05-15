using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using PdfSharpCore.Drawing;
using ReportService.Shared.Models;

namespace ReportService.Api.Services;

public interface IPdfPostProcessingService
{
    byte[] AddSummaryAndFooter(byte[] pdfData, LagerbestandRequest request, int unterMindestbestand, decimal gesamtBestand);
}

public class PdfPostProcessingService : IPdfPostProcessingService
{
    private readonly ILogger<PdfPostProcessingService> _logger;

    public PdfPostProcessingService(ILogger<PdfPostProcessingService> logger)
    {
        _logger = logger;
    }

    public byte[] AddSummaryAndFooter(byte[] pdfData, LagerbestandRequest request, int unterMindestbestand, decimal gesamtBestand)
    {
        try
        {
            using var inputStream = new MemoryStream(pdfData);
            using var outputStream = new MemoryStream();

            var document = PdfReader.Open(inputStream, PdfDocumentOpenMode.Modify);
            var totalPages = document.PageCount;

            // Add footer to all pages
            for (int i = 0; i < totalPages; i++)
            {
                var page = document.Pages[i];
                using var gfx = XGraphics.FromPdfPage(page);

                DrawFooter(gfx, page, i + 1, totalPages);
            }

            // Add summary box to last page
            if (totalPages > 0)
            {
                var lastPage = document.Pages[totalPages - 1];
                using var gfx = XGraphics.FromPdfPage(lastPage);

                DrawSummaryBox(gfx, lastPage, request.Items.Count, unterMindestbestand, gesamtBestand);
            }

            document.Save(outputStream, false);
            _logger.LogInformation("PDF post-processing completed successfully");

            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PDF post-processing");
            throw;
        }
    }

    private void DrawFooter(XGraphics gfx, PdfPage page, int pageNumber, int totalPages)
    {
        var font = new XFont("Arial", 8);
        var grayBrush = new XSolidBrush(XColor.FromArgb(102, 102, 102));
        var grayPen = new XPen(XColor.FromArgb(221, 221, 221), 1);

        // Convert mm to points (1mm = 2.83465 points)
        var leftMargin = 15 * 2.83465;
        var rightMargin = 15 * 2.83465;
        var bottomMargin = 3 * 2.83465; // 3mm from bottom

        // Draw horizontal line
        var lineY = page.Height - bottomMargin - 15; // 15 points above bottom for text
        gfx.DrawLine(grayPen, leftMargin, lineY, page.Width - rightMargin, lineY);

        // Draw footer text
        var footerY = lineY + 12; // 12 points below line
        gfx.DrawString("Hemme Milch GmbH • Lagerbestandsbericht", font, grayBrush,
            new XPoint(leftMargin, footerY));

        // Draw page number (right-aligned)
        var pageText = $"Seite {pageNumber} von {totalPages}";
        var pageTextSize = gfx.MeasureString(pageText, font);
        gfx.DrawString(pageText, font, grayBrush,
            new XPoint(page.Width - rightMargin - pageTextSize.Width, footerY));
    }

    private void DrawSummaryBox(XGraphics gfx, PdfPage page, int artikelGesamt, int unterMindest, decimal gesamtBestand)
    {
        var titleFont = new XFont("Arial", 8, XFontStyle.Bold);
        var valueFont = new XFont("Arial", 12, XFontStyle.Bold);
        var labelFont = new XFont("Arial", 7);

        var blueBrush = new XSolidBrush(XColor.FromArgb(0, 102, 178)); // #0066B2
        var lightBlueBrush = new XSolidBrush(XColor.FromArgb(227, 242, 253)); // #e3f2fd
        var bluePen = new XPen(XColor.FromArgb(0, 102, 178), 2);
        var grayBrush = new XSolidBrush(XColor.FromArgb(102, 102, 102));

        // Convert mm to points
        var leftMargin = 15 * 2.83465;
        var rightMargin = 15 * 2.83465;
        var footerHeight = 27; // Height needed for footer (line + text with 12pt spacing)

        var boxWidth = page.Width - leftMargin - rightMargin;
        var boxHeight = 60;
        var boxX = leftMargin;
        var boxY = page.Height - (3 * 2.83465) - footerHeight - boxHeight - 5; // 5 points above footer

        // Draw box background
        gfx.DrawRoundedRectangle(bluePen, lightBlueBrush, boxX, boxY, boxWidth, boxHeight, 3, 3);

        // Draw title
        var titleY = boxY + 15;
        gfx.DrawString("Zusammenfassung", titleFont, blueBrush, new XPoint(boxX + 12, titleY));

        // Draw stats
        var statsY = titleY + 20;
        var colWidth = boxWidth / 3;

        // Artikel gesamt
        DrawStat(gfx, artikelGesamt.ToString(), "Artikel gesamt",
            boxX + colWidth * 0.5, statsY, valueFont, labelFont, blueBrush, grayBrush);

        // Unter Mindestbestand
        DrawStat(gfx, unterMindest.ToString(), "Unter Mindestbestand",
            boxX + colWidth * 1.5, statsY, valueFont, labelFont, blueBrush, grayBrush);

        // Gesamtbestand
        DrawStat(gfx, gesamtBestand.ToString("F1"), "Gesamtbestand",
            boxX + colWidth * 2.5, statsY, valueFont, labelFont, blueBrush, grayBrush);
    }

    private void DrawStat(XGraphics gfx, string value, string label, double centerX, double y,
        XFont valueFont, XFont labelFont, XBrush valueBrush, XBrush labelBrush)
    {
        // Draw value (centered)
        var valueSize = gfx.MeasureString(value, valueFont);
        gfx.DrawString(value, valueFont, valueBrush,
            new XPoint(centerX - valueSize.Width / 2, y));

        // Draw label (centered, below value)
        var labelSize = gfx.MeasureString(label, labelFont);
        gfx.DrawString(label, labelFont, labelBrush,
            new XPoint(centerX - labelSize.Width / 2, y + 18));
    }
}
