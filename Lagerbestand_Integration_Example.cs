// ==============================================================================
// LAGERBESTAND REPORT - Integration Beispiel für WPF/WinForms
// ==============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReportService.Client
{
    // ==============================================================================
    // 1. MODELS für Lagerbestand-Report
    // ==============================================================================

    public class LagerbestandRequest
    {
        public string TenantId { get; set; } = "hemme";
        public DateTime Buchungsdatum { get; set; }
        public int LagerID { get; set; }
        public string LagerBezeichnung { get; set; } = string.Empty;  // NEU: z.B. "Hauptlager", "Kühlhaus"
        public int Lagerstatus { get; set; }  // 0 = Alle, 1 = Lagerbestand OK, 2 = Nicht ausreichend
        public int Lagerart { get; set; }     // 1 = Warenlager, 2 = Materiallager
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
        public double Mindestmenge { get; set; }
        public bool UnterMindestmenge { get; set; }
        public DateTime? LetzteLieferung { get; set; }
        public DateTime? MHD { get; set; }
        public string? ChargenNr { get; set; }
        public int Status { get; set; }
        public bool IstMateriallager { get; set; }

        // Optional: Chargen-Details
        public List<WarenbestandCharge>? ChargenListe { get; set; }
    }

    public class WarenbestandCharge
    {
        public string ChargenNr { get; set; } = string.Empty;
        public DateTime? MHD { get; set; }
        public int LagerStellplatzID { get; set; }
        public decimal Bestand { get; set; }
        public DateTime? LetzteBewegung { get; set; }
    }

    // ==============================================================================
    // 2. SERVICE ERWEITERUNG
    // ==============================================================================

    public interface ILagerbestandReportService
    {
        Task<byte[]> GenerateLagerbestandPdfAsync(LagerbestandRequest request);
        Task<string> GenerateAndSaveLagerbestandAsync(LagerbestandRequest request, string filePath);
        Task<string> GenerateAndOpenLagerbestandAsync(LagerbestandRequest request, string fileName = null);
    }

    public class LagerbestandReportService : ILagerbestandReportService
    {
        private readonly HttpClient _httpClient;

        public LagerbestandReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<byte[]> GenerateLagerbestandPdfAsync(LagerbestandRequest request)
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reports/hemme-lagerbestand", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> GenerateAndSaveLagerbestandAsync(LagerbestandRequest request, string filePath)
        {
            var pdfData = await GenerateLagerbestandPdfAsync(request);
            await File.WriteAllBytesAsync(filePath, pdfData);
            return filePath;
        }

        public async Task<string> GenerateAndOpenLagerbestandAsync(LagerbestandRequest request, string fileName = null)
        {
            // Standardverzeichnis für Lagerbestand-Reports
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Lagerbestand-Reports"
            );

            Directory.CreateDirectory(directory);

            // Automatischer Dateiname wenn nicht angegeben
            fileName ??= $"Lagerbestand_Lager{request.LagerID}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine(directory, fileName);

            await GenerateAndSaveLagerbestandAsync(request, filePath);

            // PDF öffnen
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = filePath,
                UseShellExecute = true
            });

            return filePath;
        }
    }

    // ==============================================================================
    // 3. VERWENDUNGSBEISPIEL
    // ==============================================================================

    public class LagerbestandExample
    {
        private readonly ILagerbestandReportService _reportService;

        public LagerbestandExample(ILagerbestandReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task GenerateSimpleReportAsync()
        {
            // Einfaches Beispiel mit Mindestdaten
            var request = new LagerbestandRequest
            {
                TenantId = "hemme",
                Buchungsdatum = DateTime.Now,
                LagerID = 1,
                LagerBezeichnung = "Hauptlager",  // NEU: Lagerbezeichnung
                Lagerstatus = 0,  // Alle anzeigen
                Lagerart = 1,     // Warenlager
                Mindestbestand = 100m,
                Items = new List<WarenbestandItem>
                {
                    new WarenbestandItem
                    {
                        ArtikelNr = 1001,
                        Artikelbez = "Vollmilch 3,8% Fett 1L",
                        BestandAktuell = 450.5,
                        InBestellung = 200.0,
                        Kommissioniert = 50.0,
                        Mindestmenge = 300.0,
                        UnterMindestmenge = false,
                        Status = 1
                    },
                    new WarenbestandItem
                    {
                        ArtikelNr = 1002,
                        Artikelbez = "Fettarme Milch 1,5% Fett 1L",
                        BestandAktuell = 85.0,
                        InBestellung = 500.0,
                        Kommissioniert = 15.0,
                        Mindestmenge = 100.0,
                        UnterMindestmenge = true,  // Warnung!
                        Status = 2
                    }
                }
            };

            // PDF generieren und öffnen
            var filePath = await _reportService.GenerateAndOpenLagerbestandAsync(request);
            Console.WriteLine($"PDF erstellt: {filePath}");
        }

        public async Task GenerateDetailedReportWithChargenAsync()
        {
            // Detailliertes Beispiel mit Chargen-Informationen
            var request = new LagerbestandRequest
            {
                TenantId = "hemme",
                Buchungsdatum = DateTime.Now,
                LagerID = 1,
                LagerBezeichnung = "Hauptlager",
                Lagerstatus = 2,  // Nur "Nicht ausreichend"
                Lagerart = 1,
                Mindestbestand = 100m,
                Items = new List<WarenbestandItem>
                {
                    new WarenbestandItem
                    {
                        ArtikelNr = 1002,
                        Artikelbez = "Fettarme Milch 1,5% Fett 1L",
                        BestandAktuell = 85.0,
                        InBestellung = 500.0,
                        Kommissioniert = 15.0,
                        Mindestmenge = 100.0,
                        UnterMindestmenge = true,
                        Status = 2,
                        ChargenListe = new List<WarenbestandCharge>
                        {
                            new WarenbestandCharge
                            {
                                ChargenNr = "CH20250109-003",
                                MHD = DateTime.Parse("2025-01-24"),
                                LagerStellplatzID = 14,
                                Bestand = 85.0m,
                                LetzteBewegung = DateTime.Parse("2025-01-13")
                            }
                        }
                    },
                    new WarenbestandItem
                    {
                        ArtikelNr = 3001,
                        Artikelbez = "Butter 250g",
                        BestandAktuell = 55.0,
                        InBestellung = 150.0,
                        Kommissioniert = 10.0,
                        Mindestmenge = 80.0,
                        UnterMindestmenge = true,
                        Status = 2,
                        ChargenListe = new List<WarenbestandCharge>
                        {
                            new WarenbestandCharge
                            {
                                ChargenNr = "CH20250108-020",
                                MHD = DateTime.Parse("2025-02-15"),
                                LagerStellplatzID = 31,
                                Bestand = 55.0m,
                                LetzteBewegung = DateTime.Parse("2025-01-13")
                            }
                        }
                    }
                }
            };

            var fileName = $"Lagerbestand_Kritisch_{DateTime.Now:yyyyMMdd}.pdf";
            var filePath = await _reportService.GenerateAndOpenLagerbestandAsync(request, fileName);
            Console.WriteLine($"PDF erstellt: {filePath}");
        }

        public async Task GenerateFromDatabaseAsync(int lagerId, int filterStatus)
        {
            // Beispiel: Daten aus deiner Datenbank laden
            // var items = await _database.GetLagerbestandAsync(lagerId, filterStatus);

            var request = new LagerbestandRequest
            {
                TenantId = "hemme",
                Buchungsdatum = DateTime.Now,
                LagerID = lagerId,
                LagerBezeichnung = "Hauptlager", // TODO: Aus Datenbank laden
                Lagerstatus = filterStatus,
                Lagerart = 1,
                Mindestbestand = 100m,
                Items = new List<WarenbestandItem>()
                // Items = items.Select(MapToWarenbestandItem).ToList()
            };

            var filePath = await _reportService.GenerateAndOpenLagerbestandAsync(request);
        }
    }

    // ==============================================================================
    // 4. DEPENDENCY INJECTION SETUP (in App.xaml.cs oder Startup)
    // ==============================================================================
    /*
    private void ConfigureServices(IServiceCollection services)
    {
        // ... andere Services ...

        // Lagerbestand-ReportService registrieren
        services.AddHttpClient<ILagerbestandReportService, LagerbestandReportService>(client =>
        {
            var baseUrl = configuration["ReportService:BaseUrl"] ?? "http://localhost:5000";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromMinutes(2);
        });
    }
    */

    // ==============================================================================
    // 5. VERWENDUNG IN VIEWMODEL / CODE-BEHIND
    // ==============================================================================
    /*
    public class LagerViewModel
    {
        private readonly ILagerbestandReportService _reportService;

        public LagerViewModel(ILagerbestandReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task OnGenerateReportButtonClick()
        {
            try
            {
                var request = new LagerbestandRequest
                {
                    TenantId = "hemme",
                    Buchungsdatum = SelectedDate,
                    LagerID = SelectedLagerId,
                    LagerBezeichnung = SelectedLagerName,  // NEU: z.B. "Hauptlager", "Kühlhaus"
                    Lagerstatus = SelectedFilterOption,  // 0=Alle, 1=OK, 2=Kritisch
                    Lagerart = 1,
                    Items = GetCurrentLagerbestand()
                };

                await _reportService.GenerateAndOpenLagerbestandAsync(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Generieren: {ex.Message}");
            }
        }

        private List<WarenbestandItem> GetCurrentLagerbestand()
        {
            // Deine Logik zum Laden der Daten
            return new List<WarenbestandItem>();
        }
    }
    */
}

// ==============================================================================
// FILTER-OPTIONEN ERKLÄRT
// ==============================================================================
/*
Lagerstatus (Filter im Report):
- 0 = Alle Artikel anzeigen
- 1 = Lagerbestand OK (nur Artikel MIT ausreichendem Bestand)
- 2 = Nicht ausreichend (nur Artikel UNTER Mindestbestand)

Lagerart:
- 1 = Warenlager
- 2 = Materiallager

UnterMindestmenge:
- true = Artikel wird gelb markiert mit Warnsymbol ⚠
- false = Artikel wird normal angezeigt mit ✓
*/
