using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Messages;
using IAT.Core.Services;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for the Instructions tab.
/// Left list = all instruction screens on the test.
/// Center = type-specific properties + block assignment.
/// Right = live preview locked to Layout.Interior aspect ratio.
/// </summary>
public partial class InstructionManagerViewModel : ObservableObject
{
    private readonly IatTest _currentTest;
    private readonly LayoutViewModel _layoutEditor;
    private readonly IProjectPackageService _packageService;
    private bool _suppressPropertyPush;
    /// <summary>Prevents ApplyInstructionPreview → layout PropertyChanged → re-apply loops.</summary>
    private bool _suppressLayoutReapply;

    public IatTest CurrentTest => _currentTest;

    /// <summary>
    /// Shared layout geometry + stage preview model (same singleton Blocks and Layout tabs use).
    /// The Instructions-tab live stage binds its Canvas to this object so positions, sizes,
    /// keys, stimulus, error mark, and continue band all follow the real layout — not a hard-coded stack.
    /// </summary>
    public LayoutViewModel LayoutEditor => _layoutEditor;

    /// <summary>Live collection bound to the left-hand screen list.</summary>
    public ObservableCollection<InstructionScreen> InstructionScreens => _currentTest.InstructionScreens;

    /// <summary>All blocks — used to build the assignment checklist.</summary>
    public ObservableCollection<Block> Blocks => _currentTest.Blocks;

    /// <summary>Available stimuli for Mock Item screens.</summary>
    public ObservableCollection<Stimulus> Stimuli => _currentTest.Stimuli;

    /// <summary>Available response keys for Keyed / Mock Item screens.</summary>
    public ObservableCollection<Key> Keys => _currentTest.Keys;

    /// <summary>Checklist of blocks the current screen is (or can be) assigned to.</summary>
    public ObservableCollection<BlockAssignmentItem> BlockAssignments { get; } = new();

    public static IReadOnlyList<string> TypeOptions { get; } = new[] { "Text", "Keyed Response", "Mock Item" };

    public static IReadOnlyList<string> DirectionOptions { get; } = new[] { "None", "Left", "Right" };

    // ── Selection ──────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteScreenCommand))]
    [NotifyCanExecuteChangedFor(nameof(AssignToSelectedBlocksCommand))]
    [NotifyCanExecuteChangedFor(nameof(RemoveFromSelectedBlocksCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddNewLeftKeyCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddNewRightKeyCommand))]
    private InstructionScreen? selectedScreen;

    // ── Shared editable properties ─────────────────────────────────────────

    [ObservableProperty]
    private string selectedType = "Text";

    [ObservableProperty]
    private string instructionText = string.Empty;

    /// <summary>
    /// Continue key is deliberately fixed to Space. Free-text or multi-key selection
    /// creates more problems than it solves for standard IATs (invisible characters,
    /// multi-byte input, validation surface). The domain still stores the string so
    /// serialization and ConfigFile mapping remain unchanged.
    /// </summary>
    [ObservableProperty]
    private string continueKey = " ";

    [ObservableProperty]
    private string continueInstructionsText = "Press the spacebar to continue";

    // ── Keyed / Mock Item ──────────────────────────────────────────────────

    [ObservableProperty]
    private Key? selectedLeftKey;

    [ObservableProperty]
    private Key? selectedRightKey;

    /// <summary>
    /// Editable text for the currently selected left key.
    /// Only writable when the key is free-floating (not owned by any block in the Trials tab).
    /// </summary>
    [ObservableProperty]
    private string leftKeyValue = string.Empty;

    /// <summary>
    /// Editable text for the currently selected right key.
    /// Only writable when the key is free-floating (not owned by any block in the Trials tab).
    /// </summary>
    [ObservableProperty]
    private string rightKeyValue = string.Empty;

    /// <summary>
    /// True when the selected left key may be renamed from the Instructions tab
    /// (i.e. it is not referenced by any block's LeftResponseId / RightResponseId).
    /// </summary>
    public bool IsLeftKeyEditable =>
        SelectedLeftKey is not null && !IsKeyOwnedByBlock(SelectedLeftKey);

    /// <summary>
    /// True when the selected right key may be renamed from the Instructions tab.
    /// </summary>
    public bool IsRightKeyEditable =>
        SelectedRightKey is not null && !IsKeyOwnedByBlock(SelectedRightKey);

    // ── Mock Item only ─────────────────────────────────────────────────────

    [ObservableProperty]
    private Stimulus? selectedStimulus;

    [ObservableProperty]
    private bool showErrorMark;

    [ObservableProperty]
    private bool outlineCorrectResponse;

    [ObservableProperty]
    private string selectedDirection = "None";

    // ── Visibility helpers for the form ────────────────────────────────────

    public bool IsKeyedOrMock =>
        SelectedScreen is KeyedInstructionScreen or MockItemInstructionScreen;

    public bool IsMockItem => SelectedScreen is MockItemInstructionScreen;

    public bool HasSelection => SelectedScreen is not null;

    // ── Preview (Interior aspect) ──────────────────────────────────────────

    /// <summary>Logical stage width from the shared layout.</summary>
    public double InteriorWidth => _layoutEditor.InteriorWidth > 0 ? _layoutEditor.InteriorWidth : 600;

    /// <summary>Logical stage height from the shared layout.</summary>
    public double InteriorHeight => _layoutEditor.InteriorHeight > 0 ? _layoutEditor.InteriorHeight : 600;

    /// <summary>Human-readable stage size for the preview header.</summary>
    public string StageSizeLabel => $"Stage {InteriorWidth:0}×{InteriorHeight:0}";

    /// <summary>Preview text shown in the live stage (instruction body).</summary>
    public string PreviewBodyText =>
        string.IsNullOrWhiteSpace(InstructionText) ? "(no instruction text)" : InstructionText;

    /// <summary>Preview continue line.</summary>
    public string PreviewContinueText =>
        string.IsNullOrWhiteSpace(ContinueInstructionsText) ? "Press the spacebar to continue" : ContinueInstructionsText;

    public string PreviewLeftKeyText => SelectedLeftKey?.Text ?? "E";
    public string PreviewRightKeyText => SelectedRightKey?.Text ?? "I";
    public string PreviewStimulusText => SelectedStimulus?.Text ?? "Stimulus";

    /// <summary>
    /// True when the live preview should draw an outline on the left response key
    /// (Mock Item + Outline Correct Response + direction Left).
    /// </summary>
    public bool IsLeftKeyOutlined =>
        IsMockItem
        && OutlineCorrectResponse
        && string.Equals(SelectedDirection, "Left", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// True when the live preview should draw an outline on the right response key
    /// (Mock Item + Outline Correct Response + direction Right).
    /// </summary>
    public bool IsRightKeyOutlined =>
        IsMockItem
        && OutlineCorrectResponse
        && string.Equals(SelectedDirection, "Right", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Radio-button helper: correct response is Left.
    /// </summary>
    public bool IsDirectionLeft
    {
        get => string.Equals(SelectedDirection, "Left", StringComparison.OrdinalIgnoreCase);
        set { if (value) SelectedDirection = "Left"; }
    }

    /// <summary>
    /// Radio-button helper: correct response is Right.
    /// </summary>
    public bool IsDirectionRight
    {
        get => string.Equals(SelectedDirection, "Right", StringComparison.OrdinalIgnoreCase);
        set { if (value) SelectedDirection = "Right"; }
    }

    /// <summary>
    /// Radio-button helper: no correct side designated.
    /// </summary>
    public bool IsDirectionNone
    {
        get => string.Equals(SelectedDirection, "None", StringComparison.OrdinalIgnoreCase)
               || string.IsNullOrWhiteSpace(SelectedDirection);
        set { if (value) SelectedDirection = "None"; }
    }

    /// <summary>Bitmap for the selected image stimulus, or null when the stimulus is text / unloadable.</summary>
    [ObservableProperty]
    private ImageSource? previewStimulusImage;

    /// <summary>True when the live preview should show an image instead of stimulus text.</summary>
    public bool IsPreviewStimulusImage => PreviewStimulusImage is not null;

    /// <summary>True when the live preview should show the stimulus text (non-image or fallback).</summary>
    public bool IsPreviewStimulusText => PreviewStimulusImage is null && IsMockItem;

    // ─────────────────────────────────────────────────────────────────────

    public InstructionManagerViewModel(IatTest currentTest, LayoutViewModel layoutEditor, IProjectPackageService packageService)
    {
        _currentTest = currentTest ?? throw new ArgumentNullException(nameof(currentTest));
        _layoutEditor = layoutEditor ?? throw new ArgumentNullException(nameof(layoutEditor));
        _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));

        // Keep assignment list in sync when blocks are added/removed.
        _currentTest.Blocks.CollectionChanged += (_, _) => RebuildBlockAssignments();

        // Stage size label + re-push the selected instruction into the shared layout
        // preview whenever geometry that affects instruction screens changes.
        // Without this, editing the Layout tab leaves the Instructions-tab Canvas
        // on stale ActiveInstructions*/key/error/continue positions.
        _layoutEditor.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(LayoutViewModel.InteriorWidth)
                or nameof(LayoutViewModel.InteriorHeight)
                or nameof(LayoutViewModel.ScaleFactor))
            {
                OnPropertyChanged(nameof(InteriorWidth));
                OnPropertyChanged(nameof(InteriorHeight));
                OnPropertyChanged(nameof(StageSizeLabel));
            }

            if (_suppressLayoutReapply || SelectedScreen is null)
                return;

            // Re-apply when derived or primary geometry that positions instruction content moves.
            if (e.PropertyName is nameof(LayoutViewModel.InteriorWidth)
                or nameof(LayoutViewModel.InteriorHeight)
                or nameof(LayoutViewModel.LeftKeyX)
                or nameof(LayoutViewModel.LeftKeyY)
                or nameof(LayoutViewModel.RightKeyX)
                or nameof(LayoutViewModel.RightKeyY)
                or nameof(LayoutViewModel.KeyWidth)
                or nameof(LayoutViewModel.KeyHeight)
                or nameof(LayoutViewModel.StimulusX)
                or nameof(LayoutViewModel.StimulusY)
                or nameof(LayoutViewModel.StimulusWidth)
                or nameof(LayoutViewModel.StimulusHeight)
                or nameof(LayoutViewModel.ErrorMarkX)
                or nameof(LayoutViewModel.ErrorMarkY)
                or nameof(LayoutViewModel.ErrorMarkWidth)
                or nameof(LayoutViewModel.ErrorMarkHeight)
                or nameof(LayoutViewModel.TextInstructionsX)
                or nameof(LayoutViewModel.TextInstructionsY)
                or nameof(LayoutViewModel.TextInstructionsWidth)
                or nameof(LayoutViewModel.TextInstructionsHeight)
                or nameof(LayoutViewModel.KeyedInstructionsX)
                or nameof(LayoutViewModel.KeyedInstructionsY)
                or nameof(LayoutViewModel.KeyedInstructionsWidth)
                or nameof(LayoutViewModel.KeyedInstructionsHeight)
                or nameof(LayoutViewModel.MockItemInstructionsX)
                or nameof(LayoutViewModel.MockItemInstructionsY)
                or nameof(LayoutViewModel.MockItemInstructionsWidth)
                or nameof(LayoutViewModel.MockItemInstructionsHeight)
                or nameof(LayoutViewModel.ContinueInstructionsX)
                or nameof(LayoutViewModel.ContinueInstructionsY)
                or nameof(LayoutViewModel.ContinueInstructionsWidth)
                or nameof(LayoutViewModel.ContinueInstructionsHeight)
                or nameof(LayoutViewModel.BlockInstructionsX)
                or nameof(LayoutViewModel.BlockInstructionsY)
                or nameof(LayoutViewModel.BlockInstructionsWidth)
                or nameof(LayoutViewModel.BlockInstructionsHeight))
            {
                RefreshInstructionPreview();
            }
        };
    }

    /// <summary>
    /// Pushes the currently selected instruction screen into the shared
    /// <see cref="LayoutViewModel"/> stage (same path the Blocks tab uses).
    /// Call when selection changes, instruction properties change, the tab becomes
    /// visible again, or layout geometry is edited.
    /// </summary>
    public void RefreshInstructionPreview()
    {
        if (_suppressLayoutReapply)
            return;

        _suppressLayoutReapply = true;
        try
        {
            if (SelectedScreen is null)
            {
                // Blocks leaves trial stimulus/keys on the shared LayoutViewModel stage.
                // Null ApplyInstructionPreview only restores the body band — it does not
                // hide that chrome. Own the stage so the Instructions preview is empty.
                _layoutEditor.ClearStageForInstructionsIdle();
            }
            else
            {
                _layoutEditor.ApplyInstructionPreview(SelectedScreen);
            }
        }
        finally
        {
            _suppressLayoutReapply = false;
        }
    }

    /// <summary>
    /// Called when the underlying document is reset or replaced.
    /// </summary>
    public void OnDocumentReset()
    {
        SelectedScreen = null;
        BlockAssignments.Clear();
        RefreshInstructionPreview();
    }

    // ── Selection changed ──────────────────────────────────────────────────

    partial void OnSelectedScreenChanged(InstructionScreen? value)
    {
        _suppressPropertyPush = true;
        try
        {
            if (value is null)
            {
                SelectedType = "Text";
                InstructionText = string.Empty;
                ContinueKey = " ";
                ContinueInstructionsText = "Press the spacebar to continue";
                SelectedLeftKey = null;
                SelectedRightKey = null;
                SelectedStimulus = null;
                ShowErrorMark = false;
                OutlineCorrectResponse = false;
                SelectedDirection = "None";
            }
            else
            {
                SelectedType = value switch
                {
                    TextInstructionScreen => "Text",
                    KeyedInstructionScreen => "Keyed Response",
                    MockItemInstructionScreen => "Mock Item",
                    _ => "Text"
                };

                InstructionText = value.Text ?? string.Empty;
                // Continue key is fixed to Space for all instruction screens.
                ContinueKey = " ";
                value.ContinueKey = " ";
                ContinueInstructionsText = value.ContinueInstructions?.Text
                    ?? "Press the spacebar to continue";

                SelectedLeftKey = null;
                SelectedRightKey = null;
                SelectedStimulus = null;
                ShowErrorMark = false;
                OutlineCorrectResponse = false;
                SelectedDirection = "None";

                if (value is KeyedInstructionScreen keyed)
                {
                    SelectedLeftKey = _currentTest.GetKeyById(keyed.LeftResponseId);
                    SelectedRightKey = _currentTest.GetKeyById(keyed.RightResponseId);
                }
                else if (value is MockItemInstructionScreen mock)
                {
                    SelectedLeftKey = _currentTest.GetKeyById(mock.LeftResponseId);
                    SelectedRightKey = _currentTest.GetKeyById(mock.RightResponseId);
                    SelectedStimulus = _currentTest.GetStimulusById(mock.StimulusId);
                    ShowErrorMark = mock.ShowErrorMark;
                    OutlineCorrectResponse = mock.OutlineCorrectResponse;
                    SelectedDirection = mock.KeyedDirection?.Name ?? "None";
                }
            }

            RebuildBlockAssignments();
            NotifyVisibility();
            NotifyPreview();
            // Drive the shared layout stage so Mock Item / Keyed / Text positions match Blocks.
            RefreshInstructionPreview();
        }
        finally
        {
            _suppressPropertyPush = false;
        }

        DeleteScreenCommand.NotifyCanExecuteChanged();
        AssignToSelectedBlocksCommand.NotifyCanExecuteChanged();
        RemoveFromSelectedBlocksCommand.NotifyCanExecuteChanged();
    }

    private void NotifyVisibility()
    {
        OnPropertyChanged(nameof(IsKeyedOrMock));
        OnPropertyChanged(nameof(IsMockItem));
        OnPropertyChanged(nameof(HasSelection));
        OnPropertyChanged(nameof(IsPreviewStimulusImage));
        OnPropertyChanged(nameof(IsPreviewStimulusText));
    }

    private void NotifyPreview()
    {
        OnPropertyChanged(nameof(PreviewBodyText));
        OnPropertyChanged(nameof(PreviewContinueText));
        OnPropertyChanged(nameof(PreviewLeftKeyText));
        OnPropertyChanged(nameof(PreviewRightKeyText));
        OnPropertyChanged(nameof(PreviewStimulusText));
        OnPropertyChanged(nameof(IsPreviewStimulusImage));
        OnPropertyChanged(nameof(IsPreviewStimulusText));
        OnPropertyChanged(nameof(IsLeftKeyOutlined));
        OnPropertyChanged(nameof(IsRightKeyOutlined));
        OnPropertyChanged(nameof(InteriorWidth));
        OnPropertyChanged(nameof(InteriorHeight));
        OnPropertyChanged(nameof(StageSizeLabel));
    }

    /// <summary>
    /// Keeps the Left/Right/None radio helpers in sync when <see cref="SelectedDirection"/> changes.
    /// </summary>
    private void NotifyDirectionRadios()
    {
        OnPropertyChanged(nameof(IsDirectionLeft));
        OnPropertyChanged(nameof(IsDirectionRight));
        OnPropertyChanged(nameof(IsDirectionNone));
    }

    // ── Push property edits back to domain ─────────────────────────────────

    partial void OnInstructionTextChanged(string value)
    {
        if (_suppressPropertyPush || SelectedScreen is null) return;
        SelectedScreen.Text = value ?? string.Empty;
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    partial void OnContinueKeyChanged(string value)
    {
        // Always force Space. The property exists only to keep the domain model happy.
        if (_suppressPropertyPush || SelectedScreen is null) return;
        if (value != " ")
        {
            ContinueKey = " ";          // snap back
            return;
        }
        SelectedScreen.ContinueKey = " ";
        MarkDirty();
    }

    partial void OnContinueInstructionsTextChanged(string value)
    {
        if (_suppressPropertyPush || SelectedScreen is null) return;
        if (SelectedScreen.ContinueInstructions is not null)
            SelectedScreen.ContinueInstructions.Text = value ?? string.Empty;
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    partial void OnSelectedLeftKeyChanged(Key? value)
    {
        // Always keep the editable value field in sync with the selection.
        LeftKeyValue = value?.Text ?? string.Empty;
        OnPropertyChanged(nameof(IsLeftKeyEditable));

        if (_suppressPropertyPush || SelectedScreen is null) return;
        var id = value?.Id ?? Guid.Empty;
        switch (SelectedScreen)
        {
            case KeyedInstructionScreen k: k.LeftResponseId = id; break;
            case MockItemInstructionScreen m: m.LeftResponseId = id; break;
        }
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    partial void OnSelectedRightKeyChanged(Key? value)
    {
        RightKeyValue = value?.Text ?? string.Empty;
        OnPropertyChanged(nameof(IsRightKeyEditable));

        if (_suppressPropertyPush || SelectedScreen is null) return;
        var id = value?.Id ?? Guid.Empty;
        switch (SelectedScreen)
        {
            case KeyedInstructionScreen k: k.RightResponseId = id; break;
            case MockItemInstructionScreen m: m.RightResponseId = id; break;
        }
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    /// <summary>
    /// Writes the edited left-key text back onto the domain Key when the key is free-floating.
    /// Keys owned by a block (Trials tab) are intentionally ignored so Trials remains the source of truth.
    /// </summary>
    partial void OnLeftKeyValueChanged(string value)
    {
        if (_suppressPropertyPush) return;
        if (SelectedLeftKey is null || IsKeyOwnedByBlock(SelectedLeftKey)) return;

        var trimmed = value?.Trim() ?? string.Empty;
        if (SelectedLeftKey.Text == trimmed) return;

        SelectedLeftKey.Text = trimmed;
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    /// <summary>
    /// Writes the edited right-key text back onto the domain Key when the key is free-floating.
    /// </summary>
    partial void OnRightKeyValueChanged(string value)
    {
        if (_suppressPropertyPush) return;
        if (SelectedRightKey is null || IsKeyOwnedByBlock(SelectedRightKey)) return;

        var trimmed = value?.Trim() ?? string.Empty;
        if (SelectedRightKey.Text == trimmed) return;

        SelectedRightKey.Text = trimmed;
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    /// <summary>
    /// A key is "owned" by the Trials tab when any block references it as a response key.
    /// Instruction screens may still select those keys, but must not mutate their Text.
    /// </summary>
    private bool IsKeyOwnedByBlock(Key key)
    {
        if (key is null) return false;
        return _currentTest.Blocks.Any(b =>
            b.LeftResponseId == key.Id || b.RightResponseId == key.Id);
    }

    partial void OnSelectedStimulusChanged(Stimulus? value)
    {
        if (!_suppressPropertyPush && SelectedScreen is MockItemInstructionScreen mock)
        {
            mock.StimulusId = value?.Id ?? Guid.Empty;
            MarkDirty();
        }

        LoadPreviewStimulusImage(value);
        NotifyPreview();
        // LayoutViewModel loads its own image via TryLoadImage — keep the stage in sync.
        RefreshInstructionPreview();
    }

    /// <summary>
    /// Loads a bitmap for the given stimulus when it is an <see cref="ImageStimulus"/>.
    /// Prefers in-memory package cache bytes, then falls back to <see cref="ImageStimulus.PackageUri"/>.
    /// </summary>
    private void LoadPreviewStimulusImage(Stimulus? stimulus)
    {
        PreviewStimulusImage = null;

        if (stimulus is not ImageStimulus imageStim)
        {
            OnPropertyChanged(nameof(IsPreviewStimulusImage));
            OnPropertyChanged(nameof(IsPreviewStimulusText));
            return;
        }

        // 1. Preferred: bytes from the package service cache
        try
        {
            var bytes = _packageService.GetImageBytes(imageStim.Id);
            if (bytes is { Length: > 0 })
            {
                PreviewStimulusImage = BitmapFromBytes(bytes);
                OnPropertyChanged(nameof(IsPreviewStimulusImage));
                OnPropertyChanged(nameof(IsPreviewStimulusText));
                return;
            }
        }
        catch
        {
            // fall through
        }

        // 2. Fallback: PackageUri (saved package)
        if (imageStim.PackageUri is not null)
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = imageStim.PackageUri;
                bmp.DecodePixelWidth = 256; // sufficient for the live preview stage
                bmp.EndInit();
                bmp.Freeze();
                PreviewStimulusImage = bmp;
            }
            catch
            {
                // leave null → text fallback
            }
        }

        OnPropertyChanged(nameof(IsPreviewStimulusImage));
        OnPropertyChanged(nameof(IsPreviewStimulusText));
    }

    private static BitmapSource BitmapFromBytes(byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.DecodePixelWidth = 256;
        image.EndInit();
        image.Freeze();
        return image;
    }

    partial void OnShowErrorMarkChanged(bool value)
    {
        if (_suppressPropertyPush || SelectedScreen is not MockItemInstructionScreen mock) return;
        mock.ShowErrorMark = value;
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    partial void OnOutlineCorrectResponseChanged(bool value)
    {
        if (_suppressPropertyPush || SelectedScreen is not MockItemInstructionScreen mock) return;
        mock.OutlineCorrectResponse = value;
        NotifyPreview();
        RefreshInstructionPreview();
        MarkDirty();
    }

    partial void OnSelectedDirectionChanged(string value)
    {
        // Always refresh radio helpers so the UI stays consistent even under suppress.
        NotifyDirectionRadios();
        NotifyPreview();

        if (_suppressPropertyPush || SelectedScreen is not MockItemInstructionScreen mock) return;
        try
        {
            mock.KeyedDirection = KeyedDirection.FromName(value ?? "None");
        }
        catch (ArgumentException)
        {
            mock.KeyedDirection = KeyedDirection.None;
            SelectedDirection = "None";
            RefreshInstructionPreview();
            return;
        }
        RefreshInstructionPreview();
        MarkDirty();
    }

    // ── Type change (replace instance) ─────────────────────────────────────

    partial void OnSelectedTypeChanged(string value)
    {
        if (_suppressPropertyPush || SelectedScreen is null) return;

        var currentKind = SelectedScreen switch
        {
            TextInstructionScreen => "Text",
            KeyedInstructionScreen => "Keyed Response",
            MockItemInstructionScreen => "Mock Item",
            _ => "Text"
        };

        if (string.Equals(currentKind, value, StringComparison.Ordinal))
            return;

        // Capture common state + block membership before replacing.
        var id = SelectedScreen.Id;
        var text = InstructionText;
        const string contKey = " ";          // Continue key is fixed to Space
        var contText = ContinueInstructionsText;
        var leftId = SelectedLeftKey?.Id ?? Guid.Empty;
        var rightId = SelectedRightKey?.Id ?? Guid.Empty;
        var stimId = SelectedStimulus?.Id ?? Guid.Empty;
        var showErr = ShowErrorMark;
        var outline = OutlineCorrectResponse;
        // New Mock Item screens always start with no correct side designated.
        // Direction is only meaningful once the designer deliberately picks Left/Right.

        var assignedBlockIds = Blocks
            .Where(b => b.InstructionsIds.Contains(id))
            .Select(b => b.Id)
            .ToList();

        // Remove old, create new of the requested type, preserve Id.
        _currentTest.RemoveInstructionScreen(SelectedScreen);

        InstructionScreen next = value switch
        {
            "Keyed Response" => new KeyedInstructionScreen
            {
                Id = id,
                Text = text,
                ContinueKey = contKey,
                LeftResponseId = leftId,
                RightResponseId = rightId
            },
            "Mock Item" => new MockItemInstructionScreen
            {
                Id = id,
                Text = text,
                ContinueKey = contKey,
                LeftResponseId = leftId,
                RightResponseId = rightId,
                StimulusId = stimId,
                ShowErrorMark = showErr,
                OutlineCorrectResponse = outline,
                KeyedDirection = KeyedDirection.None
            },
            _ => new TextInstructionScreen
            {
                Id = id,
                Text = text,
                ContinueKey = contKey
            }
        };

        if (next.ContinueInstructions is not null)
            next.ContinueInstructions.Text = contText;

        _currentTest.AddInstructionScreen(next);

        // Restore block assignments.
        foreach (var block in Blocks)
        {
            if (assignedBlockIds.Contains(block.Id) && !block.InstructionsIds.Contains(id))
                block.InstructionsIds.Add(id);
        }

        SelectedScreen = next;
        MarkDirty();
    }

    // ── Block assignment checklist ─────────────────────────────────────────

    private void RebuildBlockAssignments()
    {
        BlockAssignments.Clear();
        var screenId = SelectedScreen?.Id ?? Guid.Empty;

        foreach (var block in Blocks)
        {
            var item = new BlockAssignmentItem(block, screenId != Guid.Empty && block.InstructionsIds.Contains(screenId));
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(BlockAssignmentItem.IsAssigned) && SelectedScreen is not null)
                {
                    if (item.IsAssigned)
                    {
                        if (!item.Block.InstructionsIds.Contains(SelectedScreen.Id))
                            item.Block.InstructionsIds.Add(SelectedScreen.Id);
                    }
                    else
                    {
                        item.Block.InstructionsIds.Remove(SelectedScreen.Id);
                    }
                    MarkDirty();
                }
            };
            BlockAssignments.Add(item);
        }
    }

    // ── Commands ───────────────────────────────────────────────────────────

    [RelayCommand]
    private void AddTextScreen()
    {
        var screen = new TextInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "New text instructions",
            ContinueKey = " "
        };
        screen.ContinueInstructions.Text = "Press the spacebar to continue";
        _currentTest.AddInstructionScreen(screen);
        SelectedScreen = screen;
        MarkDirty();
    }

    [RelayCommand]
    private void AddKeyedScreen()
    {
        var screen = new KeyedInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "New keyed instructions",
            ContinueKey = " "
        };
        screen.ContinueInstructions.Text = "Press the spacebar to continue";
        _currentTest.AddInstructionScreen(screen);
        SelectedScreen = screen;
        MarkDirty();
    }

    [RelayCommand]
    private void AddMockItemScreen()
    {
        var screen = new MockItemInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "New mock-item instructions",
            ContinueKey = " ",
            KeyedDirection = KeyedDirection.None
        };
        screen.ContinueInstructions.Text = "Press the spacebar to continue";
        _currentTest.AddInstructionScreen(screen);
        SelectedScreen = screen;
        MarkDirty();
    }

    /// <summary>
    /// Creates a new Key on the test (not tied to any block) and assigns it as the Left response key
    /// for the current Keyed / Mock Item screen.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddKey))]
    private void AddNewLeftKey()
    {
        var key = CreateNewKey(preferredSide: LayoutItem.LeftKey);
        _currentTest.AddKey(key);
        SelectedLeftKey = key;
        MarkDirty();
    }

    /// <summary>
    /// Creates a new Key on the test (not tied to any block) and assigns it as the Right response key
    /// for the current Keyed / Mock Item screen.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddKey))]
    private void AddNewRightKey()
    {
        var key = CreateNewKey(preferredSide: LayoutItem.RightKey);
        _currentTest.AddKey(key);
        SelectedRightKey = key;
        MarkDirty();
    }

    private bool CanAddKey() => SelectedScreen is KeyedInstructionScreen or MockItemInstructionScreen;

    private Key CreateNewKey(LayoutItem preferredSide)
    {
        // Prefer classic IAT defaults when the letter is still free; otherwise invent a unique name.
        var existing = new HashSet<string>(Keys.Select(k => k.Text), StringComparer.OrdinalIgnoreCase);
        string text;
        if (preferredSide == LayoutItem.LeftKey && !existing.Contains("E"))
            text = "E";
        else if (preferredSide == LayoutItem.RightKey && !existing.Contains("I"))
            text = "I";
        else
        {
            var n = 1;
            do { text = $"Key {n++}"; } while (existing.Contains(text));
        }

        return new Key
        {
            Id = Guid.NewGuid(),
            Text = text,
            LayoutItem = preferredSide,
            Style = new TextStyle()
        };
    }

    [RelayCommand(CanExecute = nameof(CanDeleteScreen))]
    private void DeleteScreen()
    {
        if (SelectedScreen is null) return;
        var toRemove = SelectedScreen;
        SelectedScreen = null;
        _currentTest.RemoveInstructionScreen(toRemove);
        MarkDirty();
    }

    private bool CanDeleteScreen() => SelectedScreen is not null;

    [RelayCommand(CanExecute = nameof(CanAssignOrRemove))]
    private void AssignToSelectedBlocks()
    {
        if (SelectedScreen is null) return;
        var id = SelectedScreen.Id;
        foreach (var item in BlockAssignments.Where(b => b.IsAssigned))
        {
            if (!item.Block.InstructionsIds.Contains(id))
                item.Block.InstructionsIds.Add(id);
        }
        // Also treat currently checked items as the target set if the user
        // checked them specifically for this action — already handled above.
        // Re-sync checkboxes in case any were toggled only for the button click.
        RebuildBlockAssignments();
        MarkDirty();
    }

    [RelayCommand(CanExecute = nameof(CanAssignOrRemove))]
    private void RemoveFromSelectedBlocks()
    {
        if (SelectedScreen is null) return;
        var id = SelectedScreen.Id;
        foreach (var item in BlockAssignments.Where(b => b.IsAssigned))
            item.Block.InstructionsIds.Remove(id);

        RebuildBlockAssignments();
        MarkDirty();
    }

    private bool CanAssignOrRemove() => SelectedScreen is not null && BlockAssignments.Count > 0;

    private static void MarkDirty() =>
        WeakReferenceMessenger.Default.Send(TestModifiedMessage.Instance);
}

/// <summary>
/// One row in the "Assigned to Blocks" checklist.
/// Toggling <see cref="IsAssigned"/> immediately updates the block's InstructionsIds.
/// </summary>
public partial class BlockAssignmentItem : ObservableObject
{
    public Block Block { get; }

    [ObservableProperty]
    private bool isAssigned;

    public string DisplayName =>
        !string.IsNullOrWhiteSpace(Block.Name)
            ? Block.Name
            : (Block.BlockNumber > 0 ? $"Block {Block.BlockNumber}" : $"Block {Block.Id.ToString()[..8]}");

    public BlockAssignmentItem(Block block, bool isAssigned)
    {
        Block = block ?? throw new ArgumentNullException(nameof(block));
        IsAssigned = isAssigned;
    }
}
