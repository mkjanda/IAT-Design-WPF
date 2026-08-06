using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for the Deploy tab: account status, list of deployed tests on the server,
/// result retrieval/preview, and deployment of the current local test.
/// Follows the same MVVM + CommunityToolkit.Mvvm patterns used by the other manager VMs.
/// </summary>
public partial class DeployManagerViewModel : ObservableObject
{
    private readonly ITestDeploymentService _deploymentService;
    private readonly IResultRetrievalService _resultService;
    private readonly IDialogService _dialogService;
    private readonly IUserNotificationService _notificationService;
    private readonly IWebSocketService _webSocket;
    private readonly IatTest _currentTest;

    // ── Account / connection status (bound to top bar) ─────────────────────
    [ObservableProperty] private string accountName = "Dr. Elena Vargas";
    [ObservableProperty] private string storageRemaining = "142.3 MB";
    [ObservableProperty] private int administrationsRemaining = 87;
    [ObservableProperty] private bool isConnected = true;
    [ObservableProperty] private string lastSyncText = "2 min ago";
    [ObservableProperty] private bool isRefreshing;

    // ── Deployed tests list ────────────────────────────────────────────────
    public ObservableCollection<DeployedTestItem> DeployedTests { get; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RetrieveResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(ClearResultsCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteTestCommand))]
    private DeployedTestItem? selectedDeployedTest;

    [ObservableProperty] private string searchText = string.Empty;

    // ── Results preview (right pane) ───────────────────────────────────────
    [ObservableProperty] private string selectedTestTitle = "No test selected";
    [ObservableProperty] private double meanDScore;
    [ObservableProperty] private int sampleSize;
    [ObservableProperty] private double averageRtMs;
    [ObservableProperty] private bool hasResults;

    /// <summary>One tab per survey in the selected test's results + a Summary tab.</summary>
    public ObservableCollection<SurveyResultTab> SurveyTabs { get; } = new();

    [ObservableProperty] private SurveyResultTab? selectedSurveyTab;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeployManagerViewModel"/> class with the specified services and current test.
    /// </summary>
    /// <param name="deploymentService">The service responsible for test deployment operations.</param>
    /// <param name="resultService">The service responsible for result retrieval operations.</param>
    /// <param name="dialogService">The service responsible for displaying dialogs.</param>
    /// <param name="notificationService">The service responsible for user notifications.</param>
    /// <param name="webSocketService">The service responsible for WebSocket communications.</param>
    /// <param name="currentTest">The current test being managed.</param>
    public DeployManagerViewModel(
        ITestDeploymentService deploymentService,
        IResultRetrievalService resultService,
        IDialogService dialogService,
        IUserNotificationService notificationService,
        IWebSocketService webSocketService,
        IatTest currentTest)
    {
        _deploymentService = deploymentService;
        _resultService = resultService;
        _dialogService = dialogService;
        _webSocket = webSocketService;
        _notificationService = notificationService;
        _currentTest = currentTest;

        // Seed realistic sample data so the UI is reviewable immediately.
        SeedSampleData();
    }

    private void SeedSampleData()
    {
        DeployedTests.Add(new DeployedTestItem
        {
            Name = "Race IAT v3",
            Uploaded = DateTime.Now.AddDays(-4),
            SizeBytes = 4_200_000,
            ResultCount = 25,
            Status = "Ready"
        });
        DeployedTests.Add(new DeployedTestItem
        {
            Name = "Gender Career IAT",
            Uploaded = DateTime.Now.AddDays(-12),
            SizeBytes = 3_800_000,
            ResultCount = 11,
            Status = "Ready"
        });
        DeployedTests.Add(new DeployedTestItem
        {
            Name = "Age Stereotype Pilot",
            Uploaded = DateTime.Now.AddDays(-30),
            SizeBytes = 2_100_000,
            ResultCount = 0,
            Status = "No results"
        });

        SelectedDeployedTest = DeployedTests[0];
        LoadPreviewFor(SelectedDeployedTest);
    }

    /// <summary>
    /// Called when the view is activated. Starts the WebSocket connection and refreshes the deployed tests list.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task OnActivatedAsync()
    {
        _webSocket.Start();          // or await _webSocket.ConnectAsync() if you want to await readiness

        // Optional: keep IsConnected in sync
        _webSocket.ConnectionStateChanged -= OnConnectionStateChanged;
        _webSocket.ConnectionStateChanged += OnConnectionStateChanged;
        IsConnected = _webSocket.ConnectionState == WebSocketConnectionState.Connected;

        // Then load real data
        await RefreshAsync();

    }

    private void OnConnectionStateChanged(object? sender, WebSocketConnectionState newState)
    {
        IsConnected = newState == WebSocketConnectionState.Connected;
    }

    partial void OnSelectedDeployedTestChanged(DeployedTestItem? value)
    {
        if (value is null)
        {
            SelectedTestTitle = "No test selected";
            HasResults = false;
            SurveyTabs.Clear();
            return;
        }
        LoadPreviewFor(value);
    }

    partial void OnSearchTextChanged(string value)
    {
        // Client-side filter of the deployed list (simple Contains on name).
        // Real implementation would re-query or use CollectionViewSource.
        // For now the list stays as-is; filter is applied in the view via CollectionView if needed.
    }

    private void LoadPreviewFor(DeployedTestItem item)
    {
        SelectedTestTitle = $"{item.Name}  ·  {item.ResultCount} results";
        MeanDScore = 0.42;
        SampleSize = item.ResultCount;
        AverageRtMs = 685;
        HasResults = item.ResultCount > 0;

        SurveyTabs.Clear();

        // Summary always first
        SurveyTabs.Add(new SurveyResultTab
        {
            Header = "Summary",
            IsSummary = true
        });

        // Mock two surveys that a real result packet would contain
        var demographics = new SurveyResultTab
        {
            Header = "Demographics",
            IsSummary = false,
            QuestionHeaders =
            {
                new QuestionHeader { ShortText = "Age", FullText = "What is your age in years?", ResponseType = "BoundedNumber" },
                new QuestionHeader { ShortText = "Gender", FullText = "Which gender do you identify with?", ResponseType = "MultipleChoice" },
                new QuestionHeader { ShortText = "DOB", FullText = "Date of birth (YYYY-MM-DD)", ResponseType = "Date" },
                new QuestionHeader { ShortText = "Comment", FullText = "Any additional comments about the study?", ResponseType = "BoundedText" }
            },
            Rows =
            {
                new ResponseRow { ParticipantId = "P001", Values = { "24", "Female", "2001-03-12", "Interesting study" } },
                new ResponseRow { ParticipantId = "P002", Values = { "31", "Male", "1994-11-05", "" } },
                new ResponseRow { ParticipantId = "P003", Values = { "19", "Non-binary", "2006-07-22", "A bit long" } },
                new ResponseRow { ParticipantId = "P004", Values = { "27", "Female", "1998-01-30", "" } }
            }
        };
        SurveyTabs.Add(demographics);

        var attitudes = new SurveyResultTab
        {
            Header = "Attitudes",
            IsSummary = false,
            QuestionHeaders =
            {
                new QuestionHeader { ShortText = "Q1", FullText = "I feel comfortable working with people from different racial backgrounds.", ResponseType = "Likert" },
                new QuestionHeader { ShortText = "Q2", FullText = "Stereotypes are mostly accurate reflections of group differences.", ResponseType = "Likert" },
                new QuestionHeader { ShortText = "Q3", FullText = "How often do you notice automatic judgments about others?", ResponseType = "Likert" }
            },
            Rows =
            {
                new ResponseRow { ParticipantId = "P001", Values = { "5", "2", "3" } },
                new ResponseRow { ParticipantId = "P002", Values = { "4", "3", "4" } },
                new ResponseRow { ParticipantId = "P003", Values = { "6", "1", "5" } },
                new ResponseRow { ParticipantId = "P004", Values = { "5", "2", "2" } }
            }
        };
        SurveyTabs.Add(attitudes);

        SelectedSurveyTab = SurveyTabs[0];
    }

    // ── Commands ───────────────────────────────────────────────────────────

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            // Real: await _deploymentService.RefreshManifestAsync() or similar
            await Task.Delay(600); // simulate network
            LastSyncText = "just now";
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    private bool CanActOnSelected() => SelectedDeployedTest is not null;

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task RetrieveResultsAsync()
    {
        if (SelectedDeployedTest is null) return;
        SelectedDeployedTest.Status = "Retrieving…";
        // Real: await _resultService.RetrieveAsync(SelectedDeployedTest.Name);
        await Task.Delay(800);
        SelectedDeployedTest.Status = "Ready";
        LoadPreviewFor(SelectedDeployedTest);
        await _dialogService.ShowNotificationAsync(
            $"Retrieved {SelectedDeployedTest.ResultCount} results for {SelectedDeployedTest.Name}.", "Results");
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task ClearResultsAsync()
    {
        if (SelectedDeployedTest is null) return;
        var ok = await _dialogService.ShowConfirmationAsync(
            $"Clear all results for “{SelectedDeployedTest.Name}”? This cannot be undone.",
            "Clear Results");
        if (!ok) return;
        SelectedDeployedTest.ResultCount = 0;
        SelectedDeployedTest.Status = "No results";
        LoadPreviewFor(SelectedDeployedTest);
    }

    [RelayCommand(CanExecute = nameof(CanActOnSelected))]
    private async Task DeleteTestAsync()
    {
        if (SelectedDeployedTest is null) return;
        var ok = await _dialogService.ShowConfirmationAsync(
            $"Permanently delete “{SelectedDeployedTest.Name}” and all its results from the server?",
            "Delete Deployed Test");
        if (!ok) return;
        DeployedTests.Remove(SelectedDeployedTest);
        SelectedDeployedTest = DeployedTests.FirstOrDefault();
    }

    [RelayCommand]
    private async Task DeployCurrentTestAsync()
    {
        // Real path: validate → package → _deploymentService.DeployAsync(...)
        var ok = await _dialogService.ShowConfirmationAsync(
            $"Deploy the current test “{_currentTest.Name}” to the server? Existing deployments with the same name will be versioned.",
            "Deploy Test");
        if (!ok) return;
        // Placeholder success — real path wires to ITestDeploymentService + progress via messenger
        await _dialogService.ShowNotificationAsync(
            "Deployment started. You will be notified when the server acknowledges the package.", "Deploy");
    }

    [RelayCommand]
    private async Task DownloadAllResultsAsync()
    {
        // Real: zip all result packets via IResultRetrievalService
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

    public string SizeDisplay => SizeBytes >= 1_000_000
        ? $"{SizeBytes / 1_000_000.0:0.0} MB"
        : $"{SizeBytes / 1_000.0:0} KB";

    public string UploadedDisplay => Uploaded.ToString("yyyy-MM-dd");
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
