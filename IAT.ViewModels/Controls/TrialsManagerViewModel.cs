using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

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
    private string _leftKeyFontFamily = "Segoe UI";

    [ObservableProperty]
    private double _leftKeyFontSize = 24.0;

    [ObservableProperty]
    private Color _leftKeyTextColor = Colors.Black;

    [ObservableProperty]
    private string _rightKeyFontFamily = "Segoe UI";

    [ObservableProperty]
    private double _rightKeyFontSize = 24.0;

    [ObservableProperty]
    private Color _rightKeyTextColor = Colors.Black;

    [ObservableProperty]
    private int _numPresentations;

    /// <summary>Swatch brush for the left key color control.</summary>
    public SolidColorBrush LeftKeyPreviewBrush => new(LeftKeyTextColor);

    /// <summary>Swatch brush for the right key color control.</summary>
    public SolidColorBrush RightKeyPreviewBrush => new(RightKeyTextColor);

    /// <summary>
    /// Shared font list (identical to Blocks / Stimuli authoring) for a consistent look.
    /// </summary>
    public ObservableCollection<string> AvailableFontFamilies { get; } = new()
    {
        "Segoe UI", "Arial", "Calibri", "Verdana", "Trebuchet MS", "Tahoma",
        "Georgia", "Times New Roman", "Cambria", "Garamond", "Palatino Linotype",
        "Consolas", "Courier New", "Segoe Script", "Impact"
    };

    public ObservableCollection<double> AvailableFontSizes { get; } =
        new() { 12, 16, 18, 20, 24, 28, 32, 36, 48, 54, 66, 72 };

    /// <summary>
    /// All stimuli available for assignment (bound to ComboBoxes in the trial list).
    /// </summary>
    public ObservableCollection<Stimulus> AvailableStimuli => _currentTest.Stimuli;

    /// <summary>
    /// True while key text / style fields are being filled from the selected block.
    /// Suppresses persist so loading one block cannot overwrite another block's keys.
    /// </summary>
    private bool _loadingKeys;

    /// <summary>
    /// Practice keys (blocks 1–2) are authoritative. After a standard 7-block structure
    /// exists, blocks 3–7 are derived: text boxes and style controls are read-only and
    /// follow practice-key edits via propagation.
    /// </summary>
    public bool AreResponseKeysEditable =>
        SelectedBlock is not null
        && !(Blocks.Count == 7 && SelectedBlock.BlockNumber is >= 3 and <= 7);

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
            _loadingKeys = true;
            try
            {
                Trials.Clear();
                NumPresentations = 0;
                LeftKeyText = string.Empty;
                RightKeyText = string.Empty;
                ResetKeyStyleEditors();
            }
            finally
            {
                _loadingKeys = false;
            }
            OnPropertyChanged(nameof(AreResponseKeysEditable));
            return;
        }

        NumPresentations = value.NumPresentations;

        // Load left / right key text + style WITHOUT writing back. Setting bound properties
        // fires On*Changed → Persist; without this guard the first assignment would push the
        // previous block's still-stale opposite side onto the newly selected block (and onto
        // any Key instance shared after Generate 7-Block).
        _loadingKeys = true;
        try
        {
            var leftKey = value.LeftResponseId != Guid.Empty
                ? _currentTest.GetKeyById(value.LeftResponseId)
                : null;
            var rightKey = value.RightResponseId != Guid.Empty
                ? _currentTest.GetKeyById(value.RightResponseId)
                : null;

            // Authoring form is always single-line ("Good or Flower"); stacked multiline
            // from older packages is collapsed so the Trials text boxes stay readable.
            LeftKeyText = Key.FormatAuthoringDisplay(leftKey?.Text);
            RightKeyText = Key.FormatAuthoringDisplay(rightKey?.Text);
            LoadKeyStyleEditor(isLeft: true, leftKey);
            LoadKeyStyleEditor(isLeft: false, rightKey);
        }
        finally
        {
            _loadingKeys = false;
        }

        OnPropertyChanged(nameof(AreResponseKeysEditable));
        ReloadTrialsForSelectedBlock();
    }

    private void ResetKeyStyleEditors()
    {
        LeftKeyFontFamily = "Segoe UI";
        LeftKeyFontSize = 24.0;
        LeftKeyTextColor = Colors.Black;
        RightKeyFontFamily = "Segoe UI";
        RightKeyFontSize = 24.0;
        RightKeyTextColor = Colors.Black;
        OnPropertyChanged(nameof(LeftKeyPreviewBrush));
        OnPropertyChanged(nameof(RightKeyPreviewBrush));
    }

    private void LoadKeyStyleEditor(bool isLeft, Key? key)
    {
        var family = key?.Style?.FontFamily ?? key?.FontFamily ?? "Segoe UI";
        var size = key?.Style?.FontSize > 0 ? key.Style.FontSize
            : key is { FontSize: > 0 } ? key.FontSize : 24.0;
        var color = key?.Style?.FontColor ?? key?.FontColor ?? Colors.Black;

        if (isLeft)
        {
            LeftKeyFontFamily = family;
            LeftKeyFontSize = size;
            LeftKeyTextColor = color;
            OnPropertyChanged(nameof(LeftKeyPreviewBrush));
        }
        else
        {
            RightKeyFontFamily = family;
            RightKeyFontSize = size;
            RightKeyTextColor = color;
            OnPropertyChanged(nameof(RightKeyPreviewBrush));
        }
    }

    partial void OnNumPresentationsChanged(int value)
    {
        if (SelectedBlock is null) return;
        if (value < 0) value = 0;
        // Only write when the value actually differs. Writing the same value back
        // through ObservableProperty still raises PropertyChanged and can re-enter
        // SyncTrialCount / NotifyTrialsChanged under certain binding timings.
        if (SelectedBlock.NumPresentations != value)
        {
            SelectedBlock.NumPresentations = value;
            CommunityToolkit.Mvvm.Messaging.WeakReferenceMessenger.Default.Send(
                IAT.Core.Messages.TestModifiedMessage.Instance);
        }
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

    /// <summary>
    /// Raises change notification on the block's Trials collection so the
    /// Blocks-tab grid refreshes. Also pulls the current domain NumPresentations
    /// into the VM property when they have diverged (e.g. after an external edit
    /// on the Blocks tab).
    /// </summary>
    private void SyncTrialCount()
    {
        if (SelectedBlock is null) return;
        SelectedBlock.NotifyTrialsChanged(); // raises Trials PropertyChanged only
        // Keep the VM copy aligned with the domain without forcing the domain value.
        if (NumPresentations != SelectedBlock.NumPresentations)
            NumPresentations = SelectedBlock.NumPresentations;
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
        SyncTrialCount();
    }

    [RelayCommand]
    private void RemoveSelectedTrial()
    {
        if (SelectedTrial is null || SelectedBlock is null) return;

        var trial = SelectedTrial.Trial;
        _currentTest.RemoveTrial(trial);
        SelectedBlock.TrialIds.Remove(trial.Id);
        Trials.Remove(SelectedTrial);
        SelectedTrial = null;

        // renumber remaining
        int n = 1;
        foreach (var row in Trials)
        {
            row.Trial.TrialNumber = n++;
        }
        SyncTrialCount();
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
        SyncTrialCount();
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
        SyncTrialCount();
    }

    /// <summary>
    /// Persist key text into the domain as the user types so the Blocks-tab preview
    /// can show response-key labels without requiring an explicit Save click.
    /// </summary>
    partial void OnLeftKeyTextChanged(string value)
    {
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: true, text: value, styleOnly: false);
    }

    partial void OnRightKeyTextChanged(string value)
    {
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: false, text: value, styleOnly: false);
    }

    partial void OnLeftKeyFontFamilyChanged(string value)
    {
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: true, styleOnly: true);
    }

    partial void OnLeftKeyFontSizeChanged(double value)
    {
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: true, styleOnly: true);
    }

    partial void OnLeftKeyTextColorChanged(Color value)
    {
        OnPropertyChanged(nameof(LeftKeyPreviewBrush));
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: true, styleOnly: true);
    }

    partial void OnRightKeyFontFamilyChanged(string value)
    {
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: false, styleOnly: true);
    }

    partial void OnRightKeyFontSizeChanged(double value)
    {
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: false, styleOnly: true);
    }

    partial void OnRightKeyTextColorChanged(Color value)
    {
        OnPropertyChanged(nameof(RightKeyPreviewBrush));
        if (_loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: false, styleOnly: true);
    }

    [RelayCommand]
    private void SaveKeys()
    {
        if (SelectedBlock is null || _loadingKeys || !AreResponseKeysEditable) return;
        PersistKeySide(isLeft: true, text: LeftKeyText, styleOnly: false);
        PersistKeySide(isLeft: false, text: RightKeyText, styleOnly: false);
    }

    [RelayCommand]
    private void ApplyLeftKeyPalette(string paletteType)
    {
        if (!AreResponseKeysEditable) return;
        LeftKeyTextColor = ResolvePaletteColor(paletteType, LeftKeyTextColor);
    }

    [RelayCommand]
    private void ApplyRightKeyPalette(string paletteType)
    {
        if (!AreResponseKeysEditable) return;
        RightKeyTextColor = ResolvePaletteColor(paletteType, RightKeyTextColor);
    }

    private static Color ResolvePaletteColor(string paletteType, Color fallback) =>
        paletteType.ToLowerInvariant() switch
        {
            "black" => Colors.Black,
            "white" => Colors.White,
            "flame scarlet" => Color.FromRgb(205, 33, 42),
            "firefly" => Color.FromRgb(209, 206, 32),
            "silver sconce" => Color.FromRgb(161, 159, 165),
            "ultra violet" => Color.FromRgb(95, 75, 139),
            "knockout pink" => Color.FromRgb(255, 62, 165),
            "emerald" => Color.FromRgb(0, 148, 115),
            "sunset gold" => Color.FromRgb(247, 196, 148),
            "radiant orchid" => Color.FromRgb(174, 93, 153),
            "raspberry" => Color.FromRgb(255, 46, 94),
            "acid lime" => Color.FromRgb(187, 223, 50),
            "bluebird" => Color.FromRgb(0, 161, 180),
            "star sapphire" => Color.FromRgb(69, 104, 154),
            "angel blue" => Color.FromRgb(131, 198, 207),
            "ember glow" => Color.FromRgb(234, 103, 89),
            "pale gold" => Color.FromRgb(189, 152, 101),
            "blackened pearl" => Color.FromRgb(77, 75, 80),
            _ => fallback
        };

    private TextStyle CurrentKeyStyle(bool isLeft) => new()
    {
        FontFamily = (isLeft ? LeftKeyFontFamily : RightKeyFontFamily) ?? "Segoe UI",
        FontSize = isLeft
            ? (LeftKeyFontSize > 0 ? LeftKeyFontSize : 24.0)
            : (RightKeyFontSize > 0 ? RightKeyFontSize : 24.0),
        FontColor = isLeft ? LeftKeyTextColor : RightKeyTextColor
    };

    /// <summary>
    /// Writes one side's key text and/or style onto the selected block only.
    /// <list type="bullet">
    ///   <item>Never touches the opposite side (avoids stale cross-writes when switching blocks).</item>
    ///   <item>Practice blocks (1–2): update in place even when shared (Block 5 reuses Block 2 keys)
    ///   so derived blocks follow, then rebuild combined keys that list this key as a component.</item>
    ///   <item>Non-practice freeform blocks: copy-on-write when the Key is shared.</item>
    /// </list>
    /// </summary>
    private void PersistKeySide(bool isLeft, string? text = null, bool styleOnly = false)
    {
        if (SelectedBlock is null || _loadingKeys || !AreResponseKeysEditable) return;

        var effectiveText = styleOnly
            ? Key.FormatAuthoringDisplay(isLeft ? LeftKeyText : RightKeyText)
            : Key.FormatAuthoringDisplay(text ?? (isLeft ? LeftKeyText : RightKeyText));
        var style = CurrentKeyStyle(isLeft);
        var existingId = isLeft ? SelectedBlock.LeftResponseId : SelectedBlock.RightResponseId;
        var layoutSlot = isLeft ? LayoutItem.LeftKey : LayoutItem.RightKey;
        var isPracticeSource = SelectedBlock.BlockNumber is 1 or 2;

        Key key;
        if (existingId != Guid.Empty)
        {
            var existing = _currentTest.GetKeyById(existingId);
            if (existing is null)
            {
                key = new Key
                {
                    Id = existingId,
                    LayoutItem = layoutSlot,
                    Style = CloneStyle(style)
                };
                ApplyKeyText(key, effectiveText);
                ApplyKeyStyle(key, style);
                _currentTest.AddKey(key);
            }
            else if (IsKeySharedWithOtherBlocks(existingId, SelectedBlock) && !isPracticeSource)
            {
                // Freeform / non-practice: copy-on-write so edits stay local.
                key = CloneKey(existing, layoutSlot);
                ApplyKeyText(key, effectiveText);
                ApplyKeyStyle(key, style);
                _currentTest.AddKey(key);
                if (isLeft)
                    SelectedBlock.LeftResponseId = key.Id;
                else
                    SelectedBlock.RightResponseId = key.Id;
            }
            else
            {
                // Sole owner, or practice source shared with Block 5 — update in place.
                key = existing;
                ApplyKeyText(key, effectiveText);
                ApplyKeyStyle(key, style);
            }
        }
        else
        {
            key = new Key
            {
                Id = Guid.NewGuid(),
                LayoutItem = layoutSlot,
                Style = CloneStyle(style)
            };
            ApplyKeyText(key, effectiveText);
            ApplyKeyStyle(key, style);
            _currentTest.AddKey(key);
            if (isLeft)
                SelectedBlock.LeftResponseId = key.Id;
            else
                SelectedBlock.RightResponseId = key.Id;
        }

        // Guard against a pathological same-Id left/right assignment.
        if (SelectedBlock.LeftResponseId != Guid.Empty
            && SelectedBlock.LeftResponseId == SelectedBlock.RightResponseId)
        {
            var clone = CloneKey(key, isLeft ? LayoutItem.RightKey : LayoutItem.LeftKey);
            ApplyKeyText(clone, isLeft ? RightKeyText : LeftKeyText);
            ApplyKeyStyle(clone, CurrentKeyStyle(!isLeft));
            _currentTest.AddKey(clone);
            if (isLeft)
                SelectedBlock.RightResponseId = clone.Id;
            else
                SelectedBlock.LeftResponseId = clone.Id;
        }

        // Practice keys drive combined keys on blocks 3–4 and 6–7.
        if (isPracticeSource)
            PropagateDerivedKeysFromPractice();

        WeakReferenceMessenger.Default.Send(IAT.Core.Messages.TestModifiedMessage.Instance);
    }

    /// <summary>
    /// Rebuilds every combined key whose <see cref="Key.ComponentIds"/> reference practice keys.
    /// Text is recomposed as <c>"A or C"</c>; style follows the first component (same rule as generate).
    /// Block 5 shares practice key instances and updates automatically without this path.
    /// </summary>
    private void PropagateDerivedKeysFromPractice()
    {
        foreach (var key in _currentTest.Keys)
        {
            if (!key.IsCombined || key.ComponentIds is not { Count: >= 2 })
                continue;

            var first = _currentTest.GetKeyById(key.ComponentIds[0]);
            var second = _currentTest.GetKeyById(key.ComponentIds[1]);
            if (first is null || second is null)
                continue;

            var t1 = Key.FormatAuthoringDisplay(first.Text);
            var t2 = Key.FormatAuthoringDisplay(second.Text);
            key.Text = string.IsNullOrEmpty(t1) && string.IsNullOrEmpty(t2)
                ? string.Empty
                : $"{t1} or {t2}".Trim();
            key.Separator = " or ";
            key.LayoutMode = KeyLayoutMode.VerticalWithOr;

            // Style from the first component — matches CreateCombinedKey at generate time.
            ApplyKeyStyle(key, StyleFromKey(first));
        }
    }

    private static TextStyle StyleFromKey(Key key) => new()
    {
        FontFamily = key.Style?.FontFamily ?? key.FontFamily ?? "Segoe UI",
        FontSize = key.Style?.FontSize > 0 ? key.Style.FontSize
            : key.FontSize > 0 ? key.FontSize : 24.0,
        FontColor = key.Style?.FontColor ?? key.FontColor
    };

    private static TextStyle CloneStyle(TextStyle style) => new()
    {
        FontFamily = style.FontFamily ?? "Segoe UI",
        FontSize = style.FontSize > 0 ? style.FontSize : 24.0,
        FontColor = style.FontColor
    };

    private static void ApplyKeyStyle(Key key, TextStyle style)
    {
        key.Style = CloneStyle(style);
        key.FontFamily = key.Style.FontFamily;
        key.FontSize = key.Style.FontSize;
        key.FontColor = key.Style.FontColor;
    }

    /// <summary>
    /// True when more than one block points at this key id (left or right slot).
    /// </summary>
    private bool IsKeySharedWithOtherBlocks(Guid keyId, Block owner)
    {
        foreach (var block in _currentTest.BlocksCollection)
        {
            if (ReferenceEquals(block, owner))
                continue;
            if (block.LeftResponseId == keyId || block.RightResponseId == keyId)
                return true;
        }
        return false;
    }

    private static Key CloneKey(Key source, LayoutItem layoutSlot) =>
        new()
        {
            Id = Guid.NewGuid(),
            LayoutItem = layoutSlot,
            IsCombined = source.IsCombined,
            ComponentIds = source.ComponentIds is { Count: > 0 }
                ? new List<Guid>(source.ComponentIds)
                : new List<Guid>(),
            Separator = source.Separator,
            LayoutMode = source.LayoutMode,
            Text = source.Text,
            Style = source.Style is null
                ? new TextStyle()
                : new TextStyle
                {
                    FontFamily = source.Style.FontFamily,
                    FontSize = source.Style.FontSize,
                    FontColor = source.Style.FontColor
                },
            FontFamily = source.FontFamily,
            FontSize = source.FontSize,
            FontColor = source.FontColor
        };

    /// <summary>
    /// Stores Trials-tab key text in authoring form and marks combined keys so slide
    /// rendering can stack them into a three-row column.
    /// </summary>
    private static void ApplyKeyText(Key key, string? raw)
    {
        var text = Key.FormatAuthoringDisplay(raw);
        key.Text = text;
        if (text.Contains(" or ", StringComparison.OrdinalIgnoreCase))
        {
            key.IsCombined = true;
            key.LayoutMode = KeyLayoutMode.VerticalWithOr;
            key.Separator = " or ";
        }
        else if (!key.IsCombined || key.ComponentIds is not { Count: > 0 })
        {
            // Clear combined flag when the author removes the " or " so a plain label
            // is not treated as a stacked key on slides. Leave ComponentIds-backed
            // combined keys alone — they are structural, not free text.
            key.IsCombined = false;
            key.LayoutMode = KeyLayoutMode.VerticalStack;
        }
    }

    /// <summary>
    /// Assign the given stimulus as a Left-keyed trial on the selected block.
    /// Bound directly from the L buttons in the Available Stimuli list.
    /// </summary>
    [RelayCommand]
    private void AssignLeft(Stimulus? stimulus)
    {
        if (stimulus is null) return;
        AssignStimulusCore(stimulus, KeyedDirection.Left);
    }

    /// <summary>
    /// Assign the given stimulus as a Right-keyed trial on the selected block.
    /// Bound directly from the R buttons in the Available Stimuli list.
    /// </summary>
    [RelayCommand]
    private void AssignRight(Stimulus? stimulus)
    {
        if (stimulus is null) return;
        AssignStimulusCore(stimulus, KeyedDirection.Right);
    }

    /// <summary>
    /// Shared implementation for creating a trial with the specified keying direction.
    /// </summary>
    private void AssignStimulusCore(Stimulus stim, KeyedDirection direction)
    {
        if (SelectedBlock is null) return;

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
        SyncTrialCount();
    }

    /// <summary>
    /// Called by the shell after New/Open so the Trials tab reflects the current document.
    /// Selects the first block and its first trial so the list is not left empty-selected.
    /// </summary>
    public void OnDocumentReset()
    {
        SelectedTrial = null;
        SelectedBlock = Blocks.OrderBy(b => b.BlockNumber).FirstOrDefault();
        if (SelectedBlock is null)
        {
            _loadingKeys = true;
            try
            {
                Trials.Clear();
                NumPresentations = 0;
                LeftKeyText = string.Empty;
                RightKeyText = string.Empty;
            }
            finally
            {
                _loadingKeys = false;
            }
            return;
        }

        // OnSelectedBlockChanged already reloaded Trials; pick the first row.
        SelectedTrial = Trials.FirstOrDefault();
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
