namespace ReportService.Shared.Models;

public class LagerbestandRequest : BaseReportRequest
{
    public LagerbestandRequest()
    {
        ReportType = ReportType.HemmeLagerbestand;
    }

    public DateTime Buchungsdatum { get; set; }
    public int LagerID { get; set; }
    public string LagerBezeichnung { get; set; } = string.Empty;
    public int Lagerstatus { get; set; }
    public int Lagerart { get; set; }
    public decimal Mindestbestand { get; set; } = 100m;

    public List<WarenbestandItem> Items { get; set; } = new();
}

public class WarenbestandItem
{
    public int ArtikelNr { get; set; }
    public string Artikelbez { get; set; } = string.Empty;
    public double BestandAktuell { get; set; }
    public double InBestellung { get; set; }
    public double Kommissioniert { get; set; }
    public DateTime? LetzteLieferung { get; set; }
    public DateTime? MHD { get; set; }
    public string? ChargenNr { get; set; }
    public double Mindestmenge { get; set; }
    public bool UnterMindestmenge { get; set; }
    public int Status { get; set; }
    public bool IstMateriallager { get; set; }

    // Detail-Chargen (optional, nur wenn geladen)
    public List<WarenbestandCharge>? ChargenListe { get; set; }

    // Formatted display properties
    public string BestandAktuellFormatted => FormatGermanNumber(BestandAktuell);
    public string InBestellungFormatted => FormatGermanNumber(InBestellung);
    public string KommissioniertFormatted => FormatGermanNumber(Kommissioniert);
    public string MindestmengeFormatted => FormatGermanNumber(Mindestmenge);

    private static string FormatGermanNumber(double value)
    {
        if (value % 1 == 0)
            return value.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
        return value.ToString("G", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
    }
}

public class WarenbestandCharge
{
    public string ChargenNr { get; set; } = string.Empty;
    public DateTime? MHD { get; set; }
    public int LagerStellplatzID { get; set; }
    public decimal Bestand { get; set; }
    public DateTime? LetzteBewegung { get; set; }

    // Formatted display property
    public string BestandFormatted
    {
        get
        {
            if (Bestand % 1 == 0)
                return Bestand.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
            return Bestand.ToString("G", System.Globalization.CultureInfo.GetCultureInfo("de-DE"));
        }
    }
}
