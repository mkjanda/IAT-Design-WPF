using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.ConfigFile;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.ResultData;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Excel;
using IAT.Core.Services.Export;
using IAT.Core.Services.Network;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.IO;

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
    private readonly ITestExportService _exportService;
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

    /// <summary>
    /// Retrieved envelopes keyed by IAT name. <see cref="ApplyServerReport"/> rebuilds
    /// the list items; without this cache a Refresh would drop the preview payload.
    /// </summary>
    private readonly Dictionary<string, TestResults> _retrievedByName =
        new(StringComparer.OrdinalIgnoreCase);

    // ── Account / connection status (bound to top bar) ─────────────────────
    [ObservableProperty] private string accountName = "—";
    [ObservableProperty] private string storageRemaining = "—";
    [ObservableProperty] private string administrationsRemaining = "—";
    [ObservableProperty] private bool isConnected;
    [ObservableProperty] private string lastSyncText = "never";
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    private bool isRefreshing;

    /// <summary>
    /// True while a Retrieve / Clear / Delete (or other long-running Deploy action) is in flight.
    /// All action buttons are disabled via CanExecute until the operation completes, fails, or the
    /// underlying service times out / cancels.
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetrieveResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(RefreshCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeployCurrentTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportResultsCommand))]
    private bool isBusy;

    // ── Deployed tests list ────────────────────────────────────────────────
    public ObservableCollection<DeployedTestItem> DeployedTests { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetrieveResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteSelectedCommand))]
    [NotifyCanExecuteChangedFor(nameof(ExportResultsCommand))]
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

    /// <summary>
    /// Deployment name for the current local test. Bound on the Deploy tab so authors can
    /// set a server-facing name without renaming the package file. Syncs to <see cref="IatTest.Name"/>.
    /// </summary>
    [ObservableProperty] private string testName = string.Empty;

    // ── Results preview (right pane) ───────────────────────────────────────
    [ObservableProperty] private string selectedTestTitle = "No test selected";
    [ObservableProperty] private string resultsStatusText = string.Empty;
    [ObservableProperty] private double meanDScore;
    [ObservableProperty] private int sampleSize;
    [ObservableProperty] private double averageRtMs;
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExportResultsCommand))]
    private bool hasResults;

    /// <summary>One tab per survey in the selected test's results + a Summary tab.</summary>
    public ObservableCollection<SurveyResultTab> SurveyTabs { get; } = new();

    [ObservableProperty] private SurveyResultTab? selectedSurveyTab;

    public DeployManagerViewModel(
        ITestDeploymentService deploymentService,
        ITestExportService exportService,
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
        _exportService = exportService;
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

            // Pull the domain name so the Deploy-tab editor shows the current value
            // (may have been set at Save As, Open, or a prior edit session).
            TestName = _currentTest.Name ?? string.Empty;

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
            ResultsStatusText = string.Empty;
            HasResults = false;
            MeanDScore = 0;
            SampleSize = 0;
            AverageRtMs = 0;
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
        ApplyRowFilter(value);
    }

    private TestResults? ResolvePreviewSource(DeployedTestItem item)
    {
        if (item.RetrievedResults?.ResultSets is { Count: > 0 }
            || item.RetrievedResults?.EncryptedResultSets is { Count: > 0 })
            return item.RetrievedResults;

        if (_retrievedByName.TryGetValue(item.Name, out var cached)
            && cached is not null
            && ((cached.ResultSets?.Count ?? 0) > 0 || (cached.EncryptedResultSets?.Count ?? 0) > 0))
            return cached;

        var parked = _transactionState.TestResults;
        if (parked is not null
            && string.Equals(_transactionState.IATName, item.Name, StringComparison.OrdinalIgnoreCase)
            && ((parked.ResultSets?.Count ?? 0) > 0 || (parked.EncryptedResultSets?.Count ?? 0) > 0))
            return parked;

        return item.RetrievedResults ?? cached;
    }

    private void LoadPreviewFor(DeployedTestItem item)
    {
        SelectedTestTitle = $"{item.Name}  ·  {item.ResultCount} results";

        var envelope = ResolvePreviewSource(item);
        if (envelope is not null
            && ((envelope.ResultSets?.Count ?? 0) > 0 || (envelope.EncryptedResultSets?.Count ?? 0) > 0))
        {
            BindRetrievedResults(envelope, item);
            return;
        }

        MeanDScore = 0;
        SampleSize = 0;
        AverageRtMs = 0;
        HasResults = false;
        ResultsStatusText = item.ResultCount > 0
            ? "Results exist on the server. Retrieve to fill this preview."
            : "No administrations retrieved for this test.";

        SurveyTabs.Clear();
        SurveyTabs.Add(new SurveyResultTab { Header = "Summary", IsSummary = true });
        SelectedSurveyTab = SurveyTabs[0];
        ExportResultsCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Maps <see cref="TransactionState.TestResults"/> into the right-hand preview.
    /// Call after a successful retrieve. Holds a reference on the list item so
    /// changing selection does not drop the last download.
    /// </summary>
    private void BindRetrievedResults(TestResults results, DeployedTestItem item)
    {
        item.RetrievedResults = results;
        _retrievedByName[item.Name] = results;

        var plaintext = results.ResultSets ?? [];
        var cipherRows = results.EncryptedResultSets ?? [];
        var reported = results.Descriptor?.NumResults > 0
            ? results.Descriptor.NumResults
            : Math.Max(plaintext.Count, cipherRows.Count);

        if (item.ResultCount <= 0 && reported > 0)
            item.ResultCount = reported;

        SelectedTestTitle = $"{item.Name}  ·  {item.ResultCount} results";
        SampleSize = plaintext.Count > 0 ? plaintext.Count : reported;

        var scores = plaintext
            .Select(rs => IatDScore.Compute(rs.IATResult))
            .ToList();
        var included = scores.Where(s => s.Included && s.D is not null).ToList();
        MeanDScore = included.Count > 0 ? included.Average(s => s.D!.Value) : 0;

        var latencies = plaintext
            .SelectMany(rs => rs.IATResult?.Fragments ?? [])
            .Where(f => f.ResponseTime > 0 && f.ResponseTime <= IatDScore.MaxLatencyMs)
            .Select(f => (double)f.ResponseTime)
            .ToList();
        AverageRtMs = latencies.Count > 0 ? latencies.Average() : 0;

        HasResults = true;

        if (plaintext.Count == 0 && cipherRows.Count > 0)
        {
            ResultsStatusText =
                $"Downloaded {cipherRows.Count} encrypted administration(s). Plaintext did not unwrap — check the IAT password.";
        }
        else if (plaintext.Count == 0)
        {
            ResultsStatusText = "Retrieve finished with an empty result envelope.";
        }
        else
        {
            ResultsStatusText =
                $"{plaintext.Count} administration(s) unwrapped. {included.Count} included in mean D (Greenwald 2003).";
        }

        RebuildSurveyTabs(results);
        StampSummaryTab(included, plaintext.Count);
        ApplyRowFilter(SearchText);
        ExportResultsCommand.NotifyCanExecuteChanged();
    }

    private void StampSummaryTab(List<IatDScoreResult> included, int administrations)
    {
        var summary = SurveyTabs.FirstOrDefault(t => t.IsSummary) ?? SurveyTabs.FirstOrDefault();
        if (summary is null)
            return;

        summary.MeanDScore = MeanDScore;
        summary.SampleSize = SampleSize;
        summary.AverageRtMs = AverageRtMs;
        summary.IncludedCount = included.Count;
        summary.AdministrationCount = administrations;
        summary.Histogram.Clear();
        foreach (var bar in BuildHistogram(included))
            summary.Histogram.Add(bar);
    }

    private static List<HistogramBar> BuildHistogram(IReadOnlyList<IatDScoreResult> included)
    {
        var values = included
            .Where(s => s.D is not null)
            .Select(s => s.D!.Value)
            .ToList();

        var labels = new[] { "≤−1.2", "−1.2", "−0.6", "0", "+0.6", "+1.2", "≥+1.2" };
        var edges = new[] { double.NegativeInfinity, -1.2, -0.6, -0.15, 0.15, 0.6, 1.2, double.PositiveInfinity };
        var counts = new int[labels.Length];
        foreach (var d in values)
        {
            for (var i = 0; i < labels.Length; i++)
            {
                if (d >= edges[i] && d < edges[i + 1])
                {
                    counts[i]++;
                    break;
                }
            }
        }

        var max = Math.Max(1, counts.Max());
        var bars = new List<HistogramBar>(labels.Length);
        for (var i = 0; i < labels.Length; i++)
        {
            bars.Add(new HistogramBar
            {
                Label = labels[i],
                Count = counts[i],
                BarHeight = 8 + (88.0 * counts[i] / max)
            });
        }
        return bars;
    }

    private void ApplyRowFilter(string? query)
    {
        var needle = query?.Trim() ?? string.Empty;
        foreach (var tab in SurveyTabs.Where(t => !t.IsSummary))
        {
            tab.Rows.Clear();
            IEnumerable<ResponseRow> source = tab.AllRows;
            if (!string.IsNullOrEmpty(needle))
            {
                source = tab.AllRows.Where(row =>
                    row.ParticipantId.Contains(needle, StringComparison.OrdinalIgnoreCase)
                    || row.Values.Any(v => v.Contains(needle, StringComparison.OrdinalIgnoreCase)));
            }
            foreach (var row in source)
                tab.Rows.Add(row);
        }
    }

    private void RebuildSurveyTabs(TestResults results)
    {
        SurveyTabs.Clear();
        SurveyTabs.Add(new SurveyResultTab
        {
            Header = "Summary",
            IsSummary = true
        });

        var surveys = results.Descriptor?.ConfigFile?.Surveys ?? [];
        var administrations = results.ResultSets ?? [];

        var names = new List<string>();
        foreach (var survey in surveys)
        {
            var name = string.IsNullOrWhiteSpace(survey.SurveyName) ? string.Empty : survey.SurveyName.Trim();
            if (!string.IsNullOrEmpty(name) && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
                names.Add(name);
        }
        foreach (var set in administrations)
        {
            foreach (var sr in set.SurveyResults ?? [])
            {
                var name = string.IsNullOrWhiteSpace(sr.SurveyName) ? string.Empty : sr.SurveyName.Trim();
                if (!string.IsNullOrEmpty(name) && !names.Contains(name, StringComparer.OrdinalIgnoreCase))
                    names.Add(name);
            }
        }

        if (names.Count == 0 && administrations.Any(a => (a.SurveyResults?.Count ?? 0) > 0))
        {
            var max = administrations.Max(a => a.SurveyResults?.Count ?? 0);
            for (var i = 0; i < max; i++)
                names.Add($"Survey {i + 1}");
        }

        for (var s = 0; s < names.Count; s++)
        {
            var name = names[s];
            var tab = new SurveyResultTab { Header = name, IsSummary = false };
            var config = surveys.FirstOrDefault(sv =>
                string.Equals(sv.SurveyName?.Trim(), name, StringComparison.OrdinalIgnoreCase));
            var questions = config?.Contents.OfType<Core.ConfigFile.SurveyItem>()
                .Where(item => item.Response is not Instruction)
                .ToList() ?? [];

            if (questions.Count > 0)
            {
                foreach (var q in questions)
                {
                    var text = string.IsNullOrWhiteSpace(q.Text) ? $"Item {q.ItemNum}" : q.Text.Trim();
                    tab.QuestionHeaders.Add(new QuestionHeader
                    {
                        ShortText = Truncate(text, 24),
                        FullText = text,
                        ResponseType = ResponseTypeLabel(q.Response)
                    });
                }
            }
            else
            {
                var width = administrations
                    .Select(a => FindSurvey(a, name, s)?.Answers.Count ?? 0)
                    .DefaultIfEmpty(0)
                    .Max();
                for (var i = 0; i < width; i++)
                {
                    tab.QuestionHeaders.Add(new QuestionHeader
                    {
                        ShortText = $"Q{i + 1}",
                        FullText = $"{name} item {i + 1}",
                        ResponseType = "Answer"
                    });
                }
            }

            var columnCount = tab.QuestionHeaders.Count;
            for (var i = 0; i < administrations.Count; i++)
            {
                var surveyResult = FindSurvey(administrations[i], name, s);
                var answers = surveyResult?.Answers ?? [];
                var values = new List<string>(columnCount);
                for (var c = 0; c < columnCount; c++)
                    values.Add(c < answers.Count ? answers[c] ?? string.Empty : string.Empty);

                var row = new ResponseRow
                {
                    ParticipantId = (i + 1).ToString(CultureInfo.InvariantCulture),
                    Values = values
                };
                tab.AllRows.Add(row);
                tab.Rows.Add(row);
            }

            SurveyTabs.Add(tab);
        }

        SelectedSurveyTab = SurveyTabs[0];
    }

    private static SurveyResult? FindSurvey(ResultSet set, string name, int index)
    {
        var list = set.SurveyResults ?? [];
        var named = list.FirstOrDefault(s =>
            string.Equals(s.SurveyName?.Trim(), name, StringComparison.OrdinalIgnoreCase));
        if (named is not null)
            return named;
        return index >= 0 && index < list.Count ? list[index] : null;
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= max)
            return text;
        return text[..(max - 1)] + "…";
    }

    private static string ResponseTypeLabel(Response? response) => response switch
    {
        TrueFalse => "True/False",
        Likert => "Likert",
        Date => "Date",
        MultiChoice => "Choice",
        MultiSelect => "Multi-select",
        BoundedText => "Text",
        BoundedNumber => "Number",
        FixedDigit => "Digits",
        RegEx => "RegEx",
        _ => "Answer"
    };

    // ── Commands ───────────────────────────────────────────────────────────

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        if (IsRefreshing || IsBusy)
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

            var result = await _serverReportService.RetrieveServerReport(productKey, email, CancellationToken.None);

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
            if (_retrievedByName.TryGetValue(iat.Name, out var cached))
            {
                item.RetrievedResults = cached;
                var unwrapped = cached.ResultSets?.Count ?? 0;
                if (unwrapped > 0)
                    item.Status = "Ready";
            }
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

    private bool CanActOnSelected() => SelectedDeployedTest is not null && !IsBusy;

    private bool CanRefresh() => !IsBusy && !IsRefreshing;

    private bool CanDeploy() => !IsBusy;

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
        IsBusy = true;
        try
        {
            _webSocket.Start();
            var retrieved = await _resultService.GetResults(productKey, target.Name, password);

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

            // Prefer the object hanging on TransactionState — that is where the
            // handler parks the envelope + any unwrapped ResultSets.
            var payload = _transactionState.TestResults ?? retrieved;
            target.RetrievedResults = payload;
            var unwrapped = payload.ResultSets?.Count ?? 0;
            var cipher = payload.EncryptedResultSets?.Count ?? 0;
            if (unwrapped > 0)
                target.ResultCount = unwrapped;
            else if (cipher > 0 && target.ResultCount <= 0)
                target.ResultCount = cipher;

            target.Status = unwrapped > 0
                ? "Ready"
                : cipher > 0 ? "Encrypted" : "No results";

            if (ReferenceEquals(SelectedDeployedTest, target))
                BindRetrievedResults(payload, target);

            var previewNote = unwrapped > 0
                ? $"Retrieved {unwrapped} administration(s) for “{target.Name}”. Preview is on the right."
                : cipher > 0
                    ? $"Downloaded {cipher} encrypted administration(s) for “{target.Name}”, but none unwrapped. Check the password."
                    : $"Retrieve finished for “{target.Name}” with no administrations.";

            await _dialogService.ShowNotificationAsync(previewNote, "Results");
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
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExportResults()
        => !IsBusy && SelectedDeployedTest is not null;

    /// <summary>
    /// Last envelope that can feed the workbook: the selected test's retrieve,
    /// then whatever <see cref="TransactionState.TestResults"/> is still holding.
    /// </summary>
    private TestResults? ResolveExportSource(DeployedTestItem? target)
    {
        if (target?.RetrievedResults?.ResultSets is { Count: > 0 })
            return target.RetrievedResults;

        var parked = _transactionState.TestResults;
        if (parked?.ResultSets is { Count: > 0 })
            return parked;

        return target?.RetrievedResults ?? parked;
    }

    /// <summary>
    /// Writes the last unwrapped administrations through
    /// <see cref="IatResultsExcelExporter"/>. Ciphertext-only envelopes are refused.
    /// Always executable when a deployed test is selected so a dead CanExecute
    /// cannot swallow the click — the dialog explains what is missing.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExportResults))]
    private async Task ExportResultsAsync()
    {
        var target = SelectedDeployedTest;
        if (target is null)
        {
            await _dialogService.ShowNotificationAsync(
                "Select a deployed test first.",
                "Export Results");
            return;
        }

        var results = ResolveExportSource(target);
        if (results is not null && target.RetrievedResults is null)
            target.RetrievedResults = results;

        var sets = results?.ResultSets;
        var cipher = results?.EncryptedResultSets?.Count ?? 0;

        if (results is null || sets is not { Count: > 0 })
        {
            var reason = cipher > 0
                ? $"Downloaded {cipher} encrypted administration(s) for “{target.Name}”, but none unwrapped. Check the IAT password, then Retrieve again."
                : $"No unwrapped administrations for “{target.Name}”. Retrieve results first — export writes plaintext, not the ciphertext envelope.";
            ResultsStatusText = reason;
            await _dialogService.ShowNotificationAsync(reason, "Export Results");
            return;
        }

        var stamp = DateTime.Now.ToString("yyyyMMdd-HHmm", CultureInfo.InvariantCulture);
        var defaultName = $"{SanitizeFileName(target.Name)}-results-{stamp}.xlsx";
        var path = await _dialogService.ShowSaveFileDialogAsync(
            "Excel Workbook (*.xlsx)|*.xlsx",
            defaultName,
            "Export Results");
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            var rows = sets
                .Select((set, index) => new AdministrationExport
                {
                    ResultId = index + 1,
                    AdminTime = null,
                    Token = null,
                    Results = set
                })
                .ToList();

            await Task.Run(() => new IatResultsExcelExporter().Write(rows, path));
            ResultsStatusText = $"Wrote {rows.Count} administration(s) to {Path.GetFileName(path)}.";
            await _dialogService.ShowNotificationAsync(
                $"Saved {rows.Count} administration(s) to “{path}”.",
                "Export Results");
        }
        catch (Exception ex)
        {
            var root = ex is AggregateException agg
                ? agg.Flatten().InnerExceptions.FirstOrDefault() ?? ex
                : ex.InnerException ?? ex;
            await _dialogService.ShowNotificationAsync(
                $"Could not write the workbook: {root.Message}",
                "Export Results");
        }
    }

    private static string SanitizeFileName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "IAT-Results";
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(name.Trim().Select(ch => invalid.Contains(ch) ? '-' : ch).ToArray());
        return string.IsNullOrWhiteSpace(cleaned) ? "IAT-Results" : cleaned;
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
        IsBusy = true;
        var succeeded = false;
        try
        {
            _webSocket.Start();
            var result = await _deletionService.DeleteTestData(target.Name, password, CancellationToken.None);

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
            target.RetrievedResults = null;
            _retrievedByName.Remove(target.Name);
            if (ReferenceEquals(SelectedDeployedTest, target))
                LoadPreviewFor(target);
            ExportResultsCommand.NotifyCanExecuteChanged();

            succeeded = true;
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
        finally
        {
            IsBusy = false;
        }

        // Refresh after releasing the busy lock so CanExecute on Refresh is allowed.
        if (succeeded && _isActive)
            await RefreshAsync();
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
        IsBusy = true;
        var succeeded = false;
        try
        {
            _webSocket.Start();
            var result = await _deletionService.DeleteTest(targetName, password, CancellationToken.None);

            if (!_isActive) return;

            if (!result.IsSuccess)
            {
                target.Status = previousStatus;
                await _dialogService.ShowNotificationAsync(
                    string.IsNullOrWhiteSpace(result.Message) ? "Could not delete the test on the server." : result.Message,
                    result.Title ?? "Delete Deployed Test");
                return;
            }

            _retrievedByName.Remove(targetName);
            var doomed = DeployedTests.FirstOrDefault(t => t.Name == targetName);
            if (doomed is not null)
                DeployedTests.Remove(doomed);
            if (ReferenceEquals(SelectedDeployedTest, target) || SelectedDeployedTest?.Name == targetName)
                SelectedDeployedTest = DeployedTests.FirstOrDefault();

            succeeded = true;
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
        finally
        {
            IsBusy = false;
        }

        // Refresh after releasing the busy lock so CanExecute on Refresh is allowed.
        if (succeeded && _isActive)
            await RefreshAsync();
    }

    /// <summary>
    /// Writes the Deploy-tab name editor into <see cref="IatTest.Name"/>.
    /// </summary>
    private void CommitTestNameToDomain()
    {
        var trimmed = (TestName ?? string.Empty).Trim();
        if (!string.Equals(_currentTest.Name, trimmed, StringComparison.Ordinal))
            _currentTest.Name = trimmed;
        if (!string.Equals(TestName, trimmed, StringComparison.Ordinal))
            TestName = trimmed;
    }

    partial void OnTestNameChanged(string value)
    {
        if (_currentTest is null) return;
        // Mirror editor → domain as the user types (trim only at commit / deploy time).
        if (!string.Equals(_currentTest.Name, value ?? string.Empty, StringComparison.Ordinal))
            _currentTest.Name = value ?? string.Empty;
    }

    [RelayCommand(CanExecute = nameof(CanDeploy))]
    private async Task DeployCurrentTestAsync()
    {
        // Commit the editor value into the domain first so export + server use the same name.
        CommitTestNameToDomain();
        var name = _currentTest.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name) || name.Equals("New IAT Test", StringComparison.OrdinalIgnoreCase))
        {
            await _dialogService.ShowNotificationAsync(
                "Enter a unique test name in the Deploy tab before deploying. The package file name is not used as the server name.",
                "Name Required");
            return;
        }

        // Same password resolution path as Retrieve / Clear / Delete: header text box first, then AppData.
        var password = ResolvePassword(name);
        if (password is null)
        {
            await _dialogService.ShowNotificationAsync(
                "Enter a password in the header field before deploying. This password protects the test on the server and is stored locally after a successful upload.",
                "Password Required");
            return;
        }

        var productKey = SafeReadField(Field.ProductKey);
        if (string.IsNullOrWhiteSpace(productKey))
        {
            await _dialogService.ShowNotificationAsync(
                "Activate the product (product key + verified email) before deploying a test.",
                "Activation Required");
            return;
        }

        var ok = await _dialogService.ShowConfirmationAsync(
            $"Deploy the current test “{name}” to the server? Existing deployments with the same name will be versioned.",
            "Deploy Test");
        if (!ok) return;

        IsBusy = true;
        try
        {
            // Keep the socket alive for the Deploy tab session (same policy as Retrieve/Clear/Delete).
            _webSocket.Start();

            // 1. Validate + package via the export pipeline (config file, file manifest, slide manifest).
            var exportResult = await _exportService.PrepareForServerUploadAsync(_currentTest);

            // 2. Hand the package to the network deployment service (WebSocket transaction).
            var result = await _deploymentService.Deploy(name, password, exportResult, CancellationToken.None);

            if (!_isActive) return;

            // Prefer the terminal result on TransactionState when the service returns Unset mid-flow.
            result ??= _transactionState.Result ?? TransactionResult.Failure;
            if (result == TransactionResult.Unset)
                result = _transactionState.Result ?? TransactionResult.Failure;

            if (result.IsError || result == TransactionResult.Unset || !result.IsSuccess)
            {
                var message = string.IsNullOrWhiteSpace(result.Message)
                    ? "Deployment failed."
                    : result.Message;
                var title = string.IsNullOrWhiteSpace(result.Title) ? "Deploy" : result.Title;
                await _dialogService.ShowNotificationAsync(message, title);
                return;
            }

            // Persist password so later Retrieve / Clear / Delete can auto-fill for this IAT.
            _localStorage.SetIATPassword(name, password);
            if (string.Equals(SelectedDeployedTest?.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                SelectedIatPassword = password;
                HasStoredPassword = true;
            }

            await _dialogService.ShowNotificationAsync(
                $"“{name}” was deployed successfully.",
                "Deploy");

            if (_isActive)
                await RefreshAsync();
        }
        catch (Exception ex)
        {
            if (_isActive)
            {
                var root = ex is AggregateException agg
                    ? agg.Flatten().InnerExceptions.FirstOrDefault() ?? ex
                    : ex.InnerException ?? ex;
                await _dialogService.ShowNotificationAsync(
                    $"Deployment failed: {root.Message}",
                    "Deploy");
            }
        }
        finally
        {
            IsBusy = false;
        }
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

    /// <summary>
    /// Last retrieved envelope for this deployed IAT. Not bound directly —
    /// <see cref="DeployManagerViewModel"/> projects it into the preview pane.
    /// </summary>
    public TestResults? RetrievedResults { get; set; }

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
    [ObservableProperty] private double meanDScore;
    [ObservableProperty] private int sampleSize;
    [ObservableProperty] private double averageRtMs;
    [ObservableProperty] private int includedCount;
    [ObservableProperty] private int administrationCount;
    public ObservableCollection<QuestionHeader> QuestionHeaders { get; } = new();
    public ObservableCollection<ResponseRow> AllRows { get; } = new();
    public ObservableCollection<ResponseRow> Rows { get; } = new();
    public ObservableCollection<HistogramBar> Histogram { get; } = new();
}

public sealed class HistogramBar
{
    public string Label { get; init; } = string.Empty;
    public int Count { get; init; }
    public double BarHeight { get; init; }
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
