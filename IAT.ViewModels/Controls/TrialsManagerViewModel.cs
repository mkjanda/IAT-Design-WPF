using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using System.Collections.ObjectModel;
using System.Windows;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for the Trials tab. Enables assignment of stimuli to blocks, keying stimuli left or right,
/// setting the number of trials per block, and basic response-key design for the selected block.
/// Follows the same MVVM + singleton IatTest pattern used by StimuliManagerViewModel and BlockEditViewModel.
/// </summary>
public partial class TrialsManagerViewModel : ObservableObject
{
    private readonly IatTest _currentTest;

    /// <summary>
    /// Direct reference to the domain model.
    /// </summary>
    public IatTest CurrentTest => _currentTest;

    /// <summary>
    /// Live shared collection of blocks from the domain model.
    /// Blocks created on the Blocks tab appear here automatically (and vice-versa).
    /// </summary>
    public ObservableCollection<Block> Blocks => _currentTest.BlocksCollection;

    [ObservableProperty]
    private Block? _selectedBlock;

    [ObservableProperty]
    private ObservableCollection<TrialRowViewModel> _trials = new();

    [ObservableProperty]
    private TrialRowViewModel? _selectedTrial;

    [ObservableProperty]
    private string _leftKeyText = string.Empty;

    [ObservableProperty]
    private string _rightKeyText = string.Empty;

    [ObservableProperty]
    private int _numPresentations;

    /// <summary>
    /// All stimuli available for assignment (bound to ComboBoxes in the trial list).
    /// </summary>
    public ObservableCollection<Stimulus> AvailableStimuli => _currentTest.Stimuli;

    public TrialsManagerViewModel(IatTest currentTest)
    {
        _currentTest = currentTest ?? throw new ArgumentNullException(nameof(currentTest));

        // Select first block if any exist
        if (Blocks.Count > 0)
            SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).First();
    }

    partial void OnSelectedBlockChanged(Block? value)
    {
        if (value is null)
        {
            Trials.Clear();
            NumPresentations = 0;
            LeftKeyText = string.Empty;
            RightKeyText = string.Empty;
            return;
        }

        NumPresentations = value.NumPresentations;

        // Load left / right key text
        var leftKey = value.LeftResponseId != Guid.Empty
            ? _currentTest.GetKeyById(value.LeftResponseId)
            : null;
        var rightKey = value.RightResponseId != Guid.Empty
            ? _currentTest.GetKeyById(value.RightResponseId)
            : null;

        LeftKeyText = leftKey?.Text ?? string.Empty;
        RightKeyText = rightKey?.Text ?? string.Empty;

        ReloadTrialsForSelectedBlock();
    }

    partial void OnNumPresentationsChanged(int value)
    {
        if (SelectedBlock is null) return;
        if (value < 0) value = 0;
        SelectedBlock.NumPresentations = value;
    }

    private void ReloadTrialsForSelectedBlock()
    {
        Trials.Clear();
        if (SelectedBlock is null) return;

        var blockTrials = SelectedBlock.TrialIds
            .Select(id => _currentTest.GetTrialById(id))
            .Where(t => t is not null)
            .OrderBy(t => t!.TrialNumber)
            .ToList();

        foreach (var trial in blockTrials)
        {
            Trials.Add(new TrialRowViewModel(trial!, _currentTest));
        }
    }

    [RelayCommand]
    private void AddTrial()
    {
        if (SelectedBlock is null) return;

        var trial = new Trial
        {
            Id = Guid.NewGuid(),
            BlockNumber = SelectedBlock.BlockNumber,
            TrialNumber = Trials.Count + 1,
            KeyedDirection = KeyedDirection.Left,
            StimulusId = AvailableStimuli.FirstOrDefault()?.Id ?? Guid.Empty
        };

        _currentTest.AddTrial(trial);
        SelectedBlock.TrialIds.Add(trial.Id);

        var row = new TrialRowViewModel(trial, _currentTest);
        Trials.Add(row);
        SelectedTrial = row;
    }

    [RelayCommand]
    private void RemoveSelectedTrial()
    {
        if (SelectedTrial is null || SelectedBlock is null) return;

        var trial = SelectedTrial.Trial;
        _currentTest.RemoveTrial(trial);
        Trials.Remove(SelectedTrial);
        SelectedTrial = null;

        // renumber remaining
        int n = 1;
        foreach (var row in Trials)
        {
            row.Trial.TrialNumber = n++;
        }
    }

    [RelayCommand]
    private void GenerateTrials()
    {
        if (SelectedBlock is null) return;
        if (NumPresentations <= 0)
        {
            MessageBox.Show("Set the number of trials (NumPresentations) to a positive value first.",
                "Generate Trials", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (AvailableStimuli.Count == 0)
        {
            MessageBox.Show("No stimuli exist. Create stimuli on the Stimuli tab first.",
                "Generate Trials", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Clear existing trials for this block
        var existingIds = SelectedBlock.TrialIds.ToList();
        foreach (var id in existingIds)
        {
            var t = _currentTest.GetTrialById(id);
            if (t is not null) _currentTest.RemoveTrial(t);
        }
        SelectedBlock.TrialIds.Clear();
        Trials.Clear();

        // Simple generation: cycle through stimuli, alternate Left/Right
        var stimList = AvailableStimuli.ToList();
        for (int i = 0; i < NumPresentations; i++)
        {
            var stim = stimList[i % stimList.Count];
            var direction = (i % 2 == 0) ? KeyedDirection.Left : KeyedDirection.Right;

            var trial = new Trial
            {
                Id = Guid.NewGuid(),
                BlockNumber = SelectedBlock.BlockNumber,
                TrialNumber = i + 1,
                StimulusId = stim.Id,
                KeyedDirection = direction
            };

            _currentTest.AddTrial(trial);
            SelectedBlock.TrialIds.Add(trial.Id);
            Trials.Add(new TrialRowViewModel(trial, _currentTest));
        }
    }

    [RelayCommand]
    private void ClearTrials()
    {
        if (SelectedBlock is null) return;

        var existingIds = SelectedBlock.TrialIds.ToList();
        foreach (var id in existingIds)
        {
            var t = _currentTest.GetTrialById(id);
            if (t is not null) _currentTest.RemoveTrial(t);
        }
        SelectedBlock.TrialIds.Clear();
        Trials.Clear();
    }

    [RelayCommand]
    private void SaveKeys()
    {
        if (SelectedBlock is null) return;

        // Left key
        Key leftKey;
        if (SelectedBlock.LeftResponseId != Guid.Empty)
        {
            leftKey = _currentTest.GetKeyById(SelectedBlock.LeftResponseId) ?? new Key { Id = SelectedBlock.LeftResponseId };
        }
        else
        {
            leftKey = new Key { Id = Guid.NewGuid(), LayoutItem = LayoutItem.LeftKey };
            SelectedBlock.LeftResponseId = leftKey.Id;
        }
        leftKey.Text = LeftKeyText?.Trim() ?? string.Empty;
        if (_currentTest.GetKeyById(leftKey.Id) is null)
            _currentTest.AddKey(leftKey);

        // Right key
        Key rightKey;
        if (SelectedBlock.RightResponseId != Guid.Empty)
        {
            rightKey = _currentTest.GetKeyById(SelectedBlock.RightResponseId) ?? new Key { Id = SelectedBlock.RightResponseId };
        }
        else
        {
            rightKey = new Key { Id = Guid.NewGuid(), LayoutItem = LayoutItem.RightKey };
            SelectedBlock.RightResponseId = rightKey.Id;
        }
        rightKey.Text = RightKeyText?.Trim() ?? string.Empty;
        if (_currentTest.GetKeyById(rightKey.Id) is null)
            _currentTest.AddKey(rightKey);
    }

    /// <summary>
    /// Assign a stimulus to the selected block with the given keying direction by creating a new trial.
    /// </summary>
    [RelayCommand]
    private void AssignStimulus(object? parameter)
    {
        if (SelectedBlock is null || parameter is not object[] args || args.Length < 2) return;
        if (args[0] is not Stimulus stim) return;
        if (args[1] is not string dirName) return;

        var direction = dirName.Equals("Left", StringComparison.OrdinalIgnoreCase)
            ? KeyedDirection.Left
            : KeyedDirection.Right;

        var trial = new Trial
        {
            Id = Guid.NewGuid(),
            BlockNumber = SelectedBlock.BlockNumber,
            TrialNumber = Trials.Count + 1,
            StimulusId = stim.Id,
            KeyedDirection = direction
        };

        _currentTest.AddTrial(trial);
        SelectedBlock.TrialIds.Add(trial.Id);
        Trials.Add(new TrialRowViewModel(trial, _currentTest));
    }
}

/// <summary>
/// Lightweight row ViewModel used by the Trials list.
/// Keeps the underlying Trial in sync when the user changes stimulus or direction.
/// Direction is exposed as a simple string ("Left" / "Right") for easy ComboBox binding.
/// </summary>
public partial class TrialRowViewModel : ObservableObject
{
    public Trial Trial { get; }

    private readonly IatTest _test;

    [ObservableProperty]
    private Stimulus? _selectedStimulus;

    [ObservableProperty]
    private string _directionName = "Left";

    public int TrialNumber => Trial.TrialNumber;

    /// <summary>
    /// Friendly display text for the currently selected stimulus.
    /// </summary>
    public string StimulusPreview => SelectedStimulus?.GetDisplayPreview() ?? "(none)";

    public static IReadOnlyList<string> DirectionOptions { get; } = new[] { "Left", "Right" };

    public TrialRowViewModel(Trial trial, IatTest test)
    {
        Trial = trial;
        _test = test;
        _selectedStimulus = test.GetStimulusById(trial.StimulusId);
        _directionName = trial.KeyedDirection == KeyedDirection.Right ? "Right" : "Left";
    }

    partial void OnSelectedStimulusChanged(Stimulus? value)
    {
        if (value is not null)
            Trial.StimulusId = value.Id;
        OnPropertyChanged(nameof(StimulusPreview));
    }

    partial void OnDirectionNameChanged(string value)
    {
        Trial.KeyedDirection = value.Equals("Right", StringComparison.OrdinalIgnoreCase)
            ? KeyedDirection.Right
            : KeyedDirection.Left;
    }
}
