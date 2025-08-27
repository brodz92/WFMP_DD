
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ddu.Core.Models;
using Ddu.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace Ddu.Wpf.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly CsvService _csv = new();
    private readonly WfdApiClient _api;

    public ObservableCollection<CsvRecord> Rows { get; } = new();

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

            // Example: send each row to a placeholder endpoint (replace with your actual WFD path)
            // Use batching for speed; here is a small demo that simulates posting 4 at a time.
            Status = "Importing…";
            var batchSize = 4;
            var tasks = new List<Task>();
            int done = 0;
            using var sem = new SemaphoreSlim(batchSize);
            foreach (var row in Rows)
            {
                await sem.WaitAsync();
                tasks.Add(Task.Run(async () => {
                    try
                    {
                        var payload = new { tenantId = Tenant, data = row };
                        var resp = await _api.PostJsonAsync($"/tas/replaceme/endpoint", payload);
                        resp.EnsureSuccessStatusCode();
                        var current = Interlocked.Increment(ref done);
                        ProgressText = $"{current}/{Rows.Count}";
                    }
                    finally { sem.Release(); }
                }));
            }
            await Task.WhenAll(tasks);
            Status = "Import complete.";
        }
        catch (Exception ex)
        {
            Status = "Error during import.";
            ProgressText = ex.Message;
        }
    }
}
