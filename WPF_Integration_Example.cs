// ==============================================================================
// WPF Integration Beispiel für ReportService
// ==============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace ReportService.WpfClient
{
    // ==============================================================================
    // 1. MODELS (sollten idealerweise aus ReportService.Shared referenziert werden)
    // ==============================================================================

    public class EmployeePurchaseRequest
    {
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
    // 2. SERVICE INTERFACE
    // ==============================================================================

    public interface IReportServiceClient
    {
        Task<byte[]> GenerateEmployeePurchasePdfAsync(EmployeePurchaseRequest request);
        Task<string> GenerateAndSavePdfAsync(EmployeePurchaseRequest request, string filePath);
        Task<string> GenerateAndOpenPdfAsync(EmployeePurchaseRequest request, string fileName);
    }

    // ==============================================================================
    // 3. SERVICE IMPLEMENTATION
    // ==============================================================================

    public class ReportServiceClient : IReportServiceClient
    {
        private readonly HttpClient _httpClient;

        public ReportServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<byte[]> GenerateEmployeePurchasePdfAsync(EmployeePurchaseRequest request)
        {
            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/reports/employee-purchase", content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string> GenerateAndSavePdfAsync(EmployeePurchaseRequest request, string filePath)
        {
            var pdfData = await GenerateEmployeePurchasePdfAsync(request);
            await File.WriteAllBytesAsync(filePath, pdfData);
            return filePath;
        }

        public async Task<string> GenerateAndOpenPdfAsync(EmployeePurchaseRequest request, string fileName)
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Mitarbeiter-Einkäufe"
            );

            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, fileName);
            await GenerateAndSavePdfAsync(request, filePath);

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
    // 4. APP.XAML.CS - Dependency Injection Setup
    // ==============================================================================

    public partial class App : Application
    {
        private ServiceProvider? _serviceProvider;

        public App()
        {
            // Dependency Injection Container aufsetzen
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Konfiguration laden
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // HttpClient mit IHttpClientFactory
            services.AddHttpClient<IReportServiceClient, ReportServiceClient>(client =>
            {
                var baseUrl = configuration["ReportService:BaseUrl"] ?? "http://localhost:5254";
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromMinutes(2);
            });

            // ViewModels registrieren
            services.AddTransient<MainViewModel>();

            // MainWindow registrieren
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider?.GetService<MainWindow>();
            mainWindow?.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _serviceProvider?.Dispose();
            base.OnExit(e);
        }
    }

    // ==============================================================================
    // 5. VIEWMODEL BEISPIEL
    // ==============================================================================

    public class MainViewModel : ViewModelBase
    {
        private readonly IReportServiceClient _reportService;
        private bool _isGenerating;
        private string _statusMessage = string.Empty;

        public MainViewModel(IReportServiceClient reportService)
        {
            _reportService = reportService;
            GenerateReportCommand = new RelayCommand(
                async () => await GenerateReportAsync(),
                () => !IsGenerating
            );
        }

        public bool IsGenerating
        {
            get => _isGenerating;
            set
            {
                _isGenerating = value;
                OnPropertyChanged();
                GenerateReportCommand.RaiseCanExecuteChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand GenerateReportCommand { get; }

        private async Task GenerateReportAsync()
        {
            try
            {
                IsGenerating = true;
                StatusMessage = "PDF wird generiert...";

                // Beispieldaten - in echter App aus deinen Datenquellen
                var request = new EmployeePurchaseRequest
                {
                    EmployeeName = "Anette Koßmann",
                    EmployeeNumber = "57",
                    Date = DateTime.Now.ToString("dd.MM.yyyy"),
                    Items = new List<EmployeePurchaseItem>
                    {
                        new EmployeePurchaseItem
                        {
                            DeliveryDate = "01.05.26",
                            ArticleNumber = "10001",
                            Name = "H-Milch 3,8% 1L",
                            Quantity = 2,
                            GrossPrice = 1.49m,
                            TotalGross = 2.98m
                        },
                        new EmployeePurchaseItem
                        {
                            DeliveryDate = "02.05.26",
                            ArticleNumber = "10002",
                            Name = "Butter 250g",
                            Quantity = 1,
                            GrossPrice = 2.99m,
                            TotalGross = 2.99m
                        }
                    },
                    TotalGross = 5.97m
                };

                var fileName = $"Einkauf_{request.EmployeeName?.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                var filePath = await _reportService.GenerateAndOpenPdfAsync(request, fileName);

                StatusMessage = $"PDF erfolgreich erstellt: {Path.GetFileName(filePath)}";
            }
            catch (HttpRequestException ex)
            {
                StatusMessage = $"Netzwerkfehler: {ex.Message}";
                MessageBox.Show(
                    $"Fehler beim Verbinden mit dem ReportService:\n{ex.Message}\n\nStelle sicher, dass der ReportService läuft.",
                    "Verbindungsfehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler: {ex.Message}";
                MessageBox.Show(
                    $"Fehler beim Generieren des Berichts:\n{ex.Message}",
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsGenerating = false;
            }
        }
    }

    // ==============================================================================
    // 6. HELPER CLASSES (ViewModelBase & RelayCommand)
    // ==============================================================================

    public class ViewModelBase : System.ComponentModel.INotifyPropertyChanged
    {
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : System.Windows.Input.ICommand
    {
        private readonly Func<Task> _executeAsync;
        private readonly Func<bool>? _canExecute;

        public RelayCommand(Func<Task> executeAsync, Func<bool>? canExecute = null)
        {
            _executeAsync = executeAsync ?? throw new ArgumentNullException(nameof(executeAsync));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object? parameter)
        {
            await _executeAsync();
        }

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    // ==============================================================================
    // 7. MAINWINDOW CODE-BEHIND
    // ==============================================================================

    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}

// ==============================================================================
// 8. APPSETTINGS.JSON (im Projektverzeichnis erstellen)
// ==============================================================================
/*
{
  "ReportService": {
    "BaseUrl": "http://localhost:5254"
  }
}
*/

// ==============================================================================
// 9. MAINWINDOW.XAML BEISPIEL
// ==============================================================================
/*
<Window x:Class="ReportService.WpfClient.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="ReportService Client" Height="300" Width="500">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0"
                   Text="Mitarbeiter-Einkauf Bericht"
                   FontSize="20"
                   FontWeight="Bold"
                   Margin="0,0,0,20"/>

        <Button Grid.Row="1"
                Content="Bericht generieren"
                Command="{Binding GenerateReportCommand}"
                IsEnabled="{Binding IsGenerating, Converter={StaticResource InverseBoolConverter}}"
                Padding="20,10"
                FontSize="14"
                Margin="0,0,0,20"/>

        <TextBlock Grid.Row="2"
                   Text="{Binding StatusMessage}"
                   TextWrapping="Wrap"
                   VerticalAlignment="Top"/>
    </Grid>
</Window>
*/

// ==============================================================================
// 10. BENÖTIGTE NUGET PACKAGES
// ==============================================================================
/*
dotnet add package Microsoft.Extensions.DependencyInjection
dotnet add package Microsoft.Extensions.Configuration
dotnet add package Microsoft.Extensions.Configuration.Json
dotnet add package Microsoft.Extensions.Http
dotnet add package System.Text.Json
*/

// ==============================================================================
// 11. PROJEKT-SETUP
// ==============================================================================
/*
1. Neues WPF-Projekt erstellen:
   dotnet new wpf -n ReportService.WpfClient

2. NuGet-Packages installieren (siehe oben)

3. appsettings.json erstellen und Properties auf "Copy if newer" setzen

4. Optional: Projektreference auf ReportService.Shared hinzufügen:
   dotnet add reference ..\ReportService.Shared\ReportService.Shared.csproj

5. App.xaml anpassen:
   - StartupUri entfernen (wird in App.xaml.cs gehandhabt)

6. Projekt builden und starten
*/
