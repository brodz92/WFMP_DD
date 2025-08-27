
# FirstStrike DDU — WPF (.NET 8) Skeleton

A fast, lightweight Windows replacement for the NW.js Dimensions Data Utility.

## Why this stack
- Native WPF UI → faster startup, lower memory, smooth grid virtualisation
- CsvHelper + async I/O → big CSV support
- Polly retries + HttpClient → robust WFD imports
- MVVM with CommunityToolkit

## Structure
- `Ddu.Core` — CSV/service core (no UI), reusable
- `Ddu.Wpf`  — WPF application (UI, DataGrid, commands)

## Build & Run
1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download)
2. Open the solution in Visual Studio 2022 (or run CLI):
   ```bash
   dotnet build FirstStrikeDDU_WPF.sln
   dotnet run --project Ddu.Wpf
   ```
3. In the app:
   - **Open CSV…** to load data into the grid
   - **Import to WFD** (placeholder endpoint) — replace the path in `ViewModels/MainViewModel.cs` with your real WFD API endpoint(s)

## Configure WFD
Edit the fields on the toolbar (Base URL, Tenant, Client Id/Secret, App Key, Username, Password). The sample code authenticates against `/auth/oauth/token` then sets `Authorization: Bearer` and `appkey` headers.

## Next steps
- Replace `/tas/replaceme/endpoint` with the actual endpoints per import type
- Add per-import validators & mappers (e.g., column → JSON mapping)
- Add paging/progress UI and error export (CSV of failed rows)
- Persist connections (user secrets) in Windows Credential Manager
- Optional: swap DataGrid for a commercial grid (frozen columns, filters, etc.)

## Notes
This is a scaffold; no vendor code included. Keep your tenant credentials secure.
