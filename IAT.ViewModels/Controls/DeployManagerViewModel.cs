using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for the Deploy tab: account status, list of deployed tests on the server,
/// result retrieval/preview, and deployment of the current local test.
/// <para>
/// Connection policy: the WebSocket is started when the tab becomes visible and kept open
/// for the lifetime of the Deploy UI. It is closed when the tab is hidden so other flows
/// can reconfigure transaction handlers cleanly.
/// </para>
/// </summary>
public partial class DeployManagerViewModel : ObservableObject
{
    private readonly ITestDeploymentService _deploymentService;
    private readonly IResultRetrievalService _resultService;
    private readonly IDeletionService _deletionService;
    private readonly IServerReportService _serverReportService;
    private readonly ILocalStorageService _localStorage;
    private readonly TransactionState _transactionState;
    private readonly IDialogService _dialogService;
    private readonly IUserNotificationService _notificationService;
    private readonly IWebSocketService _webSocket;
    private readonly IatTest _currentTest;

    private bool _isActive;
    private int _activationGate; // 0 = idle, 1 = activation in progress

    // ── Account / connection status (bound to top bar) ─────────────────────
    [ObservableProperty] private string accountName = "—";
    [ObservableProperty] private string storageRemaining = "—";
    [ObservableProperty] private string administrationsRemaining = "—";
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private string lastSyncText = "never";
    [ObservableProperty] private bool isRefreshing;

    // ── Deployed tests list ────────────────────────────────────────────────
    public ObservableCollection<DeployedTestItem> DeployedTests { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetrieveResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    private DeployedTestItem? selectedDeployedTest;

    [ObservableProperty] private string searchText = string.Empty;

    /// <summary>
    /// Password for the currently selected deployed IAT. Auto-filled from AppData
    /// when <see cref="ILocalStorageService.TryGetIATPassword"/> finds a stored value.
    /// Editable so the user can supply a password for tests not deployed from this machine.
    /// </summary>
    [ObservableProperty] private string selectedIatPassword = string.Empty;

    /// <summary>
    /// True when the password box was populated from local storage for the current selection.
    /// </summary>
    [ObservableProperty] private bool hasStoredPassword;

    // ── Results preview (right pane) ───────────────────────────────────────
    [ObservableProperty] private string selectedTestTitle = "No test selected";
    [ObservableProperty] private double meanDScore;
    [ObservableProperty] private int sampleSize;
    [ObservableProperty] private double averageRtMs;
    [ObservableProperty] private bool hasResults;

    /// <summary>One tab per survey in the selected test's results + a Summary tab.</summary>
    public ObservableCollection<SurveyResultTab> SurveyTabs { get; } = new();

    [ObservableProperty] private SurveyResultTab? selectedSurveyTab;

    public DeployManagerViewModel(
        ITestDeploymentService deploymentService,
        IResultRetrievalService resultService,
        IDeletionService deletionService,
        IServerReportService serverReportService,
        ILocalStorageService localStorage,
        TransactionState transactionState,
        IDialogService dialogService,
        IUserNotificationService notificationService,
        IWebSocketService webSocketService,
        IatTest currentTest)
    {
        _deploymentService = deploymentService;
        _resultService = resultService;
        _deletionService = deletionService;
        _serverReportService = serverReportService;
        _localStorage = localStorage;
        _transactionState = transactionState;
        _dialogService = dialogService;
        _webSocket = webSocketService;
        _notificationService = notificationService;
        _currentTest = currentTest;
    }

    /// <summary>
    /// Called when the Deploy tab becomes visible. Starts (or reuses) the WebSocket and
    /// loads the server report into the account bar and deployed-tests list.
    /// </summary>
    public async Task OnActivatedAsync()
    {
        if (Interlocked.Exchange(ref _activationGate, 1) == 1)
            return;

        try
        {
            _isActive = true;

            _webSocket.ConnectionStateChanged -= OnConnectionStateChanged;
            _webSocket.ConnectionStateChanged += OnConnectionStateChanged;

            // Keep the socket open for the duration of the Deploy UI.
            _webSocket.Start();
            IsConnected = _webSocket.ConnectionState == WebSocketConnectionState.Connected;

            _transactionState.ServerReportChanged -= OnServerReportChanged;
            _transactionState.ServerReportChanged += OnServerReportChanged;

            // If a report is already present (e.g. previous session), show it immediately
            if (_transactionState.ServerReport.IATReport?.Count > 0 ||
                !string.IsNullOrWhiteSpace(_transactionState.ServerReport.ContactFName))
            {
                ApplyServerReport(_transactionState.ServerReport);
            }

            await RefreshAsync();
        }
        finally
        {
            Interlocked.Exchange(ref _activationGate, 0);
        }
    }

    /// <summary>
    /// Called when the Deploy tab is hidden. Tears down the connection so other
    /// transaction services can rebind handlers without interference.
    /// </summary>
    public async Task OnDeactivatedAsync()
    {
        _isActive = false;
        _webSocket.ConnectionStateChanged -= OnConnectionStateChanged;
        _transactionState.ServerReportChanged -= OnServerReportChanged;
        try
        {
            await _webSocket.CloseSocketAsync();
        }
        catch
        {
            // Best-effort close — never throw out of a visibility handler.
        }

        IsConnected = false;
    }

    private void OnServerReportChanged(ServerReport report)
    {
        // Always marshal collection mutations to the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (!_isActive) return;
            ApplyServerReport(report);
            LastSyncText = "just now";
        });
    }

    private void OnConnectionStateChanged(object? sender, WebSocketConnectionState newState)
    {
        // Marshal is not required for bool property; CommunityToolkit raises on calling thread.
        // Connection events may arrive off the UI thread — property change is still fine for binding
        // as long as WPF dispatcher is used for collection mutations (we do that in ApplyServerReport).
        IsConnected = newState == WebSocketConnectionState.Connected;
    }

    partial void OnSelectedDeployedTestChanged(DeployedTestItem? value)
    {
        if (value is null)
        {
            SelectedTestTitle = "No test selected";
            HasResults = false;
            SurveyTabs.Clear();
            SelectedIatPassword = string.Empty;
            HasStoredPassword = false;
            return;
        }

        // Prefer the password stored under AppData for this IAT name.
        var stored = _localStorage.TryGetIATPassword(value.Name);
        if (!string.IsNullOrEmpty(stored))
        {
            SelectedIatPassword = stored;
            HasStoredPassword = true;
        }
        else
        {
            SelectedIatPassword = string.Empty;
            HasStoredPassword = false;
        }

        LoadPreviewFor(value);
    }

    partial void OnSearchTextChanged(string value)
    {
        // Client-side filter reserved for a CollectionView; list is rebuilt from ServerReport.
    }

    private void LoadPreviewFor(DeployedTestItem item)
    {
        SelectedTestTitle = $"{item.Name}  ·  {item.ResultCount} results";
        MeanDScore = 0;
        SampleSize = item.ResultCount;
        AverageRtMs = 0;
        HasResults = item.ResultCount > 0;

        SurveyTabs.Clear();
        SurveyTabs.Add(new SurveyResultTab
        {
            Header = "Summary",
            IsSummary = true
        });
        SelectedSurveyTab = SurveyTabs[0];
    }

    // ── Commands ───────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsRefreshing)
            return;

        IsRefreshing = true;
        try
        {
            var productKey = SafeReadField(Field.ProductKey);
            var email = SafeReadField(Field.UserEmail);

            if (string.IsNullOrWhiteSpace(productKey) || string.IsNullOrWhiteSpace(email))
            {
                AccountName = "Not activated";
                StorageRemaining = "—";
                AdministrationsRemaining = "0";
                DeployedTests.Clear();
                SelectedDeployedTest = null;
                LastSyncText = "not activated";
                await _dialogService.ShowNotificationAsync(
                    "Activate the product (product key + verified email) before loading the server report.",
                    "Activation Required");
                return;
            }

            // Ensure socket is up for the report exchange; leave it open afterwards.
            _webSocket.Start();

            var result = await _serverReportService.RetrieveServerReport(productKey, email);

            if (!_isActive)
                return; // Tab left while the request was in flight.

            if (!result.IsSuccess)
            {
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Could not retrieve the server report."
                    : result.Message;
                await _dialogService.ShowNotificationAsync(message, result.Title ?? "Server Report");
                return;
            }

            ApplyServerReport(_transactionState.ServerReport);
            LastSyncText = "just now";
        }
        catch (Exception ex)
        {
            if (_isActive)
            {
                await _dialogService.ShowNotificationAsync(
                    $"Failed to load server report: {ex.Message}",
                    "Server Report");
            }
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    /// <summary>
    /// Maps <see cref="ServerReport"/> into the account bar and the deployed-tests list.
    /// Empty / null reports are ignored when the list already has data so a mid-transaction
    /// reset can never blank the tab.
    /// </summary>
    private void ApplyServerReport(ServerReport report)
    {
        if (report is null)
            return;

        var hasIats = report.IATReport is { Count: > 0 };
        var hasIdentity = !string.IsNullOrWhiteSpace(report.ContactFName)
                          || !string.IsNullOrWhiteSpace(report.Organization);

        // Guard: never replace a populated UI with an empty report (e.g. leftover event
        // from a Clear that used to wipe ServerReport).
        if (!hasIats && !hasIdentity && DeployedTests.Count > 0)
            return;

        var name = $"{report.ContactFName} {report.ContactLName}".Trim();
        if (string.IsNullOrWhiteSpace(name))
            name = string.IsNullOrWhiteSpace(report.Organization) ? "Account" : report.Organization;

        AccountName = name;
        if (report.NumAdministrations < 0)
            AdministrationsRemaining = "Unlimited";
        else
            AdministrationsRemaining = report.NumAdministrations.ToString();

        // DiskAlottmentRemainingKB is in KB; present as MB when ≥ 1024.
        var remainingKb = report.DiskAlottmentRemainingKB;
        if (remainingKb < 0)
            StorageRemaining = "Unlimited";
        else
            StorageRemaining = remainingKb >= 1024
                ? $"{remainingKb / 1024.0:0.0} MB"
                : $"{remainingKb} KB";

        var previousSelection = SelectedDeployedTest?.Name;

        DeployedTests.Clear();
        foreach (var iat in report.IATReport ?? Enumerable.Empty<IATReport>())
        {
            if (string.IsNullOrWhiteSpace(iat.Name))
                continue;

            // Prefer upload timestamp (when the test was deployed). Fall back to last
            // data retrieval for older server payloads that only send that field.
            var uploadedRaw = !string.IsNullOrWhiteSpace(iat.UploadTimestamp)
                ? iat.UploadTimestamp
                : iat.LastDataRetrieval;

            var item = new DeployedTestItem
            {
                Name = iat.Name,
                SizeBytes = (long)iat.TestSizeKB * 1024L,
                ResultCount = iat.NumResultSets,
                Status = iat.NumResultSets > 0 ? "Ready" : "No results",
                Uploaded = ParseLastRetrieval(uploadedRaw),
                Url = iat.URL?.Trim() ?? string.Empty
            };
            DeployedTests.Add(item);
        }

        SelectedDeployedTest = DeployedTests.FirstOrDefault(t => t.Name == previousSelection)
            ?? DeployedTests.FirstOrDefault();
    }

    private static DateTime ParseLastRetrieval(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DateTime.MinValue;

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dt))
            return dt.ToLocalTime();
        if (DateTime.TryParse(value, out dt))
            return dt;
        return DateTime.MinValue;
    }

    private string SafeReadField(Field field)
    {
        try
        {
            return _localStorage[field] ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private bool CanActOnSelected() => SelectedDeployedTest is not null;

    /// <summary>
    /// Resolves the IAT password for server operations.
    /// Prefers the header text box (user-entered or auto-filled), then AppData storage.
    /// </summary>
    private string? ResolvePassword(string iatName)
    {
        if (!string.IsNullOrWhiteSpace(SelectedIatPassword))
            return SelectedIatPassword.Trim();

        var stored = _localStorage.TryGetIATPassword(iatName);
        return string.IsNullOrWhiteSpace(stored) ? null : stored;
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task RetrieveResultsAsync()
    {
        if (SelectedDeployedTest is null) return;

        // Capture the item at command start so post-await updates stay on the same instance
        // even if the user changes selection (or Refresh rebuilds the list).
        var target = SelectedDeployedTest;

        var productKey = SafeReadField(Field.ProductKey);
        if (string.IsNullOrWhiteSpace(productKey))
        {
            await _dialogService.ShowNotificationAsync(
                "Activate the product (product key + verified email) before retrieving results.",
                "Activation Required");
            return;
        }

        var password = ResolvePassword(target.Name);
        if (password is null)
        {
            await _dialogService.ShowNotificationAsync(
                $"Enter the password for “{target.Name}” in the header field before retrieving results.",
                "Password Required");
            return;
        }

        var previousStatus = target.Status;
        target.Status = "Retrieving…";
        try
        {
            _webSocket.Start();
            var doc = await _resultService.GetResults(productKey, target.Name, password);

            if (!_isActive) return;

            // Terminal result is always set by a handler (RSAKeyHandler on bad password,
            // ResultsReady on success, etc.). Never treat a null/Unset as success.
            var result = _transactionState.Result ?? TransactionResult.Failure;
            if (result.IsError || result == TransactionResult.Unset)
            {
                target.Status = previousStatus;
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Could not retrieve results."
                    : result.Message;
                var title = string.IsNullOrWhiteSpace(result.Title) ? "Results" : result.Title;
                await _dialogService.ShowNotificationAsync(message, title);
                return;
            }

            // Keep a copy on transaction state for any downstream consumers.
            if (doc is not null && doc.Root is not null)
                _transactionState.TestResultsDocument = doc;

            target.Status = target.ResultCount > 0 ? "Ready" : "No results";
            if (ReferenceEquals(SelectedDeployedTest, target))
                LoadPreviewFor(target);

            await _dialogService.ShowNotificationAsync(
                $"Retrieved results for “{target.Name}”.",
                "Results");
        }
        catch (Exception ex)
        {
            target.Status = previousStatus;
            if (_isActive)
            {
                // Unwrap so the user sees the real message, not "One or more errors occurred."
                var root = ex is AggregateException agg
                    ? agg.Flatten().InnerExceptions.FirstOrDefault() ?? ex
                    : ex.InnerException ?? ex;
                await _dialogService.ShowNotificationAsync(
                    $"Failed to retrieve results: {root.Message}",
                    "Results");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task ClearResultsAsync()
    {
        if (SelectedDeployedTest is null) return;

        // Capture the item at command start. Selection can change (or be cleared) while
        // we await the network call; never touch SelectedDeployedTest after an await without
        // checking it is still the same instance.
        var target = SelectedDeployedTest;

        var ok = await _dialogService.ShowConfirmationAsync(
            $"Clear all results for “{target.Name}”? This cannot be undone.",
            "Clear Results");
        if (!ok) return;

        var password = ResolvePassword(target.Name);
        if (password is null)
        {
            await _dialogService.ShowNotificationAsync(
                $"Enter the password for “{target.Name}” in the header field before clearing results.",
                "Password Required");
            return;
        }

        var previousStatus = target.Status;
        var previousCount = target.ResultCount;
        target.Status = "Clearing…";
        try
        {
            _webSocket.Start();
            var result = await _deletionService.DeleteTestData(target.Name, password);

            if (!_isActive) return;

            if (!result.IsSuccess)
            {
                target.Status = previousStatus;
                await _dialogService.ShowNotificationAsync(
                    string.IsNullOrWhiteSpace(result.Message) ? "Could not clear results on the server." : result.Message,
                    result.Title ?? "Clear Results");
                return;
            }

            target.ResultCount = 0;
            target.Status = "No results";
            if (ReferenceEquals(SelectedDeployedTest, target))
                LoadPreviewFor(target);

            // Refresh account quotas / list from the server when possible.
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            target.Status = previousStatus;
            target.ResultCount = previousCount;
            if (_isActive)
            {
                await _dialogService.ShowNotificationAsync(
                    $"Failed to clear results: {ex.Message}",
                    "Clear Results");
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task DeleteTestAsync()
    {
        if (SelectedDeployedTest is null) return;

        // Capture before any await — confirmation dialog and network call both yield.
        var target = SelectedDeployedTest;
        var targetName = target.Name;

        var ok = await _dialogService.ShowConfirmationAsync(
            $"Permanently delete “{targetName}” and all its results from the server?",
            "Delete Deployed Test");
        if (!ok) return;

        var password = ResolvePassword(targetName);
        if (password is null)
        {
            await _dialogService.ShowNotificationAsync(
                $"Enter the password for “{targetName}” in the header field before deleting.",
                "Password Required");
            return;
        }

        var previousStatus = target.Status;
        target.Status = "Deleting…";
        try
        {
            _webSocket.Start();
            var result = await _deletionService.DeleteTest(targetName, password);

            if (!_isActive) return;

            if (!result.IsSuccess)
            {
                target.Status = previousStatus;
                await _dialogService.ShowNotificationAsync(
                    string.IsNullOrWhiteSpace(result.Message) ? "Could not delete the test on the server." : result.Message,
                    result.Title ?? "Delete Deployed Test");
                return;
            }

            var doomed = DeployedTests.FirstOrDefault(t => t.Name == targetName);
            if (doomed is not null)
                DeployedTests.Remove(doomed);
            if (ReferenceEquals(SelectedDeployedTest, target) || SelectedDeployedTest?.Name == targetName)
                SelectedDeployedTest = DeployedTests.FirstOrDefault();

            await RefreshAsync();
        }
        catch (Exception ex)
        {
            target.Status = previousStatus;
            if (_isActive)
            {
                await _dialogService.ShowNotificationAsync(
                    $"Failed to delete test: {ex.Message}",
                    "Delete Deployed Test");
            }
        }
    }

    [RelayCommand]
    private async Task DeployCurrentTestAsync()
    {
        var ok = await _dialogService.ShowConfirmationAsync(
            $"Deploy the current test “{_currentTest.Name}” to the server? Existing deployments with the same name will be versioned.",
            "Deploy Test");
        if (!ok) return;
        await _dialogService.ShowNotificationAsync(
            "Deployment started. You will be notified when the server acknowledges the package.", "Deploy");
    }

    [RelayCommand]
    private async Task DownloadAllResultsAsync()
    {
        await Task.CompletedTask;
        await _dialogService.ShowNotificationAsync(
            "All results download started (placeholder).", "Download");
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task DeleteSelectedAsync()
    {
        await DeleteTestAsync();
    }
}

// ── Lightweight view models for the list / preview ─────────────────────────

public partial class DeployedTestItem : ObservableObject
{
    [ObservableProperty] private string name = string.Empty;
    [ObservableProperty] private DateTime uploaded;
    [ObservableProperty] private long sizeBytes;
    [ObservableProperty] private int resultCount;
    [ObservableProperty] private string status = "Ready";
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OpenUrlCommand))]
    private string url = string.Empty;

    public string SizeDisplay => SizeBytes >= 1_000_000
        ? $"{SizeBytes / 1_000_000.0:0.0} MB"
        : SizeBytes >= 1_000
            ? $"{SizeBytes / 1_000.0:0} KB"
            : $"{SizeBytes} B";

    public string UploadedDisplay =>
        Uploaded == DateTime.MinValue ? "—" : Uploaded.ToString("yyyy-MM-dd");

    /// <summary>URL for display; falls back to em-dash when empty.</summary>
    public string UrlDisplay =>
        string.IsNullOrWhiteSpace(Url) ? "—" : Url;

    partial void OnSizeBytesChanged(long value) => OnPropertyChanged(nameof(SizeDisplay));
    partial void OnUploadedChanged(DateTime value) => OnPropertyChanged(nameof(UploadedDisplay));
    partial void OnUrlChanged(string value) => OnPropertyChanged(nameof(UrlDisplay));

    private bool CanOpenUrl() => !string.IsNullOrWhiteSpace(Url);

    /// <summary>Opens the test URL in the default browser.</summary>
    [RelayCommand(CanExecute = nameof(CanOpenUrl))]
    private void OpenUrl()
    {
        if (string.IsNullOrWhiteSpace(Url))
            return;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(Url)
            {
                UseShellExecute = true
            });
        }
        catch
        {
            // Malformed URL or no registered browser handler — swallow; UI stays quiet.
        }
    }
}

public partial class SurveyResultTab : ObservableObject
{
    [ObservableProperty] private string header = string.Empty;
    [ObservableProperty] private bool isSummary;
    public ObservableCollection<QuestionHeader> QuestionHeaders { get; } = new();
    public ObservableCollection<ResponseRow> Rows { get; } = new();
}

public class QuestionHeader
{
    public string ShortText { get; set; } = string.Empty;
    public string FullText { get; set; } = string.Empty;
    public string ResponseType { get; set; } = string.Empty;
}

public class ResponseRow
{
    public string ParticipantId { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
}
