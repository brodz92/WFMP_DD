using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ddu.Core.Models;
using Ddu.Core.Services;
using Ddu.Wpf.Models;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace Ddu.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly CsvService _csv = new();
    private readonly WfdApiClient _api;
    private readonly PeopleImportService _people = new();
    private readonly PayCodeImportService _paycodes = new();
    private readonly ShiftTemplateImportService _shifts = new();

    public ObservableCollection<CsvRecord> Rows { get; } = new();

    public Array ImportTypes { get; } = Enum.GetValues(typeof(ImportType));

    [ObservableProperty] private ImportType selectedImportType = ImportType.People;

    [ObservableProperty] private string status = "Ready";
    [ObservableProperty] private string progressText = "";
    [ObservableProperty] private string baseUrl = "https://api.au.kronos.com";
    [ObservableProperty] private string tenant = "";
    [ObservableProperty] private string clientId = "";
    [ObservableProperty] private string clientSecret = "";
    [ObservableProperty] private string appKey = "";
    [ObservableProperty] private string username = "";
    [ObservableProperty] private string password = "";

    public MainViewModel()
    {
        var http = new HttpClient(){ BaseAddress = new Uri(BaseUrl) };
        _api = new WfdApiClient(http);
    }

    [RelayCommand]
    private async Task OpenCsv()
    {
        var dlg = new OpenFileDialog{ Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*" };
        if (dlg.ShowDialog() == true)
        {
            Status = "Loading…";
            Rows.Clear();
            var list = await _csv.LoadAsync(dlg.FileName);
            foreach (var r in list) Rows.Add(r);
            Status = $"Loaded {Rows.Count:N0} rows.";
        }
    }

    [RelayCommand]
    private async Task SaveCsv()
    {
        var dlg = new SaveFileDialog{ Filter = "CSV files (*.csv)|*.csv", FileName = "export.csv" };
        if (dlg.ShowDialog() == true)
        {
            Status = "Saving…";
            await _csv.SaveAsync(dlg.FileName, Rows);
            Status = "Saved.";
        }
    }

    [RelayCommand]
    private async Task Import()
    {
        try
        {
            Status = "Authenticating…";
            var conn = new WfdConnection(BaseUrl, ClientId, ClientSecret, AppKey, Username, Password, Tenant);
            await _api.AuthenticateAsync(conn);

            Status = $"Importing ({SelectedImportType})…";
            (int ok, int failed, List<CsvRecord> failures, List<string> messages) result = SelectedImportType switch
            {
                ImportType.People => await _people.ImportAsync(Rows, _api, Tenant),
                ImportType.PayCodes => await _paycodes.ImportAsync(Rows, _api, Tenant),
                ImportType.ShiftTemplates => await _shifts.ImportAsync(Rows, _api, Tenant),
                _ => (0,0,new(),new())
            };

            Status = $"Done. OK: {result.ok}, Failed: {result.failed}";
            if (result.failed > 0)
            {
                // Offer to save failures as CSV
                var dlg = new SaveFileDialog{ Filter = "CSV files (*.csv)|*.csv", FileName = $"{SelectedImportType}_failures.csv" };
                if (dlg.ShowDialog() == true)
                {
                    await _csv.SaveAsync(dlg.FileName, result.failures);
                    ProgressText = $"Saved failures to {dlg.FileName}";
                }
                else
                {
                    ProgressText = string.Join(" | ", result.messages.Take(3));
                }
            }
            else
            {
                ProgressText = "All rows imported successfully.";
            }
        }
        catch (Exception ex)
        {
            Status = "Error during import.";
            ProgressText = ex.Message;
        }
    }
}