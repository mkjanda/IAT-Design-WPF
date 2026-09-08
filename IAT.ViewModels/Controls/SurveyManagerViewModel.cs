using com.sun.tools.javac.jvm;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.ViewModels.Controls;

/// <summary>
/// ViewModel for creating, editing and deleting surveys (questionnaires) that belong to the current IatTest.
/// Left list = Surveys. Selecting a survey exposes its properties and ordered Items collection.
/// Selecting an item exposes type-specific editors (especially ResponseDefinition for questions).
/// </summary>
public partial class SurveyManagerViewModel : ObservableObject
{
    private readonly IatTest _currentTest;
    private readonly IProjectPackageService _packageService;

    public IatTest CurrentTest => _currentTest;

    /// <summary>Live collection bound to the left-hand survey list.</summary>
    public ObservableCollection<Survey> Surveys => _currentTest.Surveys;

    [ObservableProperty]
    private Survey? selectedSurvey;

    [ObservableProperty]
    private SurveyItem? selectedItem;

    // ── Survey-level editable properties (pushed back to the domain object) ──

    [ObservableProperty]
    private string surveyName = string.Empty;

    [ObservableProperty]
    private int timeoutSeconds;

    [ObservableProperty]
    private bool allQuestionsOptional;

    // Creation / placeholder copy. Keep in lock-step with SurveyManagerControl
    // PlaceholderAttached values so focus-clear / blur-restore match what Add* writes.
    public const string DefaultHeaderText = "New Header";
    public const string DefaultInstructionText = "New instruction text…";
    public const string DefaultQuestionText = "New question";
    public const string DefaultTrueStatement = "True";
    public const string DefaultFalseStatement = "False";
    public const string DefaultChoiceText = "New choice";
    public const string DefaultRegexPattern = @".+";

    // ── Item-level editable properties ─────────────────────────────────────

    [ObservableProperty]
    private string itemText = string.Empty;          // Header / Instruction / Question text

    /// <summary>Placeholder restored into the item-text box when the author leaves it blank.</summary>
    [ObservableProperty]
    private string itemTextPlaceholder = DefaultQuestionText;

    /// <summary>Placeholder restored into the survey-name box when the author leaves it blank.</summary>
    [ObservableProperty]
    private string surveyNamePlaceholder = "Survey 1";

    [ObservableProperty]
    private bool? itemIsOptional;                   // only meaningful for SurveyQuestion

    [ObservableProperty]
    private ResponseDefinition? currentResponse;    // only for SurveyQuestion

    // Response-type specific helpers (bound from XAML)
    [ObservableProperty]
    private string selectedResponseType = "Likert";

    // ── Image item helpers ─────────────────────────────────────────────────

    [ObservableProperty]
    private BitmapImage? currentImagePreview;

    [ObservableProperty]
    private string currentImageFileName = string.Empty;

    /// <summary>True when the selected item is a <see cref="SurveyImage"/>.</summary>
    public bool IsImageItemSelected => SelectedItem is SurveyImage;

    /// <summary>True when the selected item has editable text (header / instruction / question).</summary>
    public bool ShowItemTextEditor =>
        SelectedItem is SurveyHeader or SurveyInstruction or SurveyQuestion;

    /// <summary>True when the caption style editor should be shown (header only).</summary>
    public bool ShowHeaderStyleEditor => SelectedItem is SurveyHeader;

    // ── Header / caption style (SVG effect: font, fill, banner, separator) ──

    [ObservableProperty] private string headerFontFamily = SurveyHeaderStyle.DefaultFontFamily;
    [ObservableProperty] private double headerFontSize = SurveyHeaderStyle.DefaultFontSize;
    [ObservableProperty] private Color headerFontColor = Colors.Black;
    [ObservableProperty] private Color headerBackColor = Colors.White;
    [ObservableProperty] private Color headerSeparatorColor = Colors.Black;
    [ObservableProperty] private int headerSeparatorWidth = SurveyHeaderStyle.DefaultSeparatorWidth;

    public SolidColorBrush HeaderFontPreviewBrush => new(HeaderFontColor);
    public SolidColorBrush HeaderBackPreviewBrush => new(HeaderBackColor);
    public SolidColorBrush HeaderSeparatorPreviewBrush => new(HeaderSeparatorColor);

    public ObservableCollection<string> AvailableFontFamilies { get; } =
    [
        "Segoe UI", "Arial", "Calibri", "Verdana", "Trebuchet MS", "Tahoma",
        "Georgia", "Times New Roman", "Cambria", "Garamond", "Palatino Linotype",
        "Consolas", "Courier New", "Segoe Script", "Impact"
    ];

    public ObservableCollection<double> AvailableFontSizes { get; } =
        [12, 16, 18, 20, 24, 28, 32, 36, 48, 54, 66, 72];

    public ObservableCollection<int> AvailableSeparatorWidths { get; } =
        [2, 4, 6, 8, 10, 12, 16, 20];

    private bool _suppressHeaderStylePush;
    private SurveyHeaderStyle _lastHeaderStyle = new();

    public SurveyManagerViewModel(IatTest currentTest, IProjectPackageService packageService)
    {
        _currentTest = currentTest ?? throw new ArgumentNullException(nameof(currentTest));
        _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
    }

    /// <summary>
    /// Called when the underlying document is reset or replaced. Clears selection so the UI does not
    /// hold references to objects that no longer belong to the current test.
    /// </summary>
    public void OnDocumentReset()
    {
        SelectedSurvey = null;
        SelectedItem = null;
        CurrentImagePreview = null;
        CurrentImageFileName = string.Empty;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Survey selection
    // ─────────────────────────────────────────────────────────────────────

    partial void OnSelectedSurveyChanged(Survey? value)
    {
        SelectedItem = null;

        if (value is null)
        {
            SurveyName = string.Empty;
            SurveyNamePlaceholder = "Survey 1";
            TimeoutSeconds = 0;
            AllQuestionsOptional = false;
        }
        else
        {
            SurveyName = value.Name;
            SurveyNamePlaceholder = DefaultNameFor(value);
            TimeoutSeconds = value.TimeoutSeconds;
            AllQuestionsOptional = value.AllQuestionsOptional;
        }

        NotifyItemCommands();
        DeleteSurveyCommand.NotifyCanExecuteChanged();
    }

    private void NotifyItemCommands()
    {
        // CommunityToolkit.Mvvm only evaluates CanExecute once unless notified.
        AddHeaderCommand.NotifyCanExecuteChanged();
        AddInstructionCommand.NotifyCanExecuteChanged();
        AddQuestionCommand.NotifyCanExecuteChanged();
        AddImageCommand.NotifyCanExecuteChanged();
        DeleteItemCommand.NotifyCanExecuteChanged();
        MoveItemUpCommand.NotifyCanExecuteChanged();
        MoveItemDownCommand.NotifyCanExecuteChanged();
    }

    partial void OnSurveyNameChanged(string value)
    {
        if (SelectedSurvey is not null)
            SelectedSurvey.Name = value ?? string.Empty;
    }

    partial void OnTimeoutSecondsChanged(int value)
    {
        if (SelectedSurvey is not null)
            SelectedSurvey.TimeoutSeconds = Math.Max(0, value);
    }

    partial void OnAllQuestionsOptionalChanged(bool value)
    {
        if (SelectedSurvey is not null)
            SelectedSurvey.AllQuestionsOptional = value;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Item selection
    // ─────────────────────────────────────────────────────────────────────

    partial void OnSelectedItemChanged(SurveyItem? value)
    {
        CurrentImagePreview = null;
        CurrentImageFileName = string.Empty;

        if (value is null)
        {
            ItemTextPlaceholder = DefaultQuestionText;
            ItemText = string.Empty;
            ItemIsOptional = null;
            CurrentResponse = null;
            SelectedResponseType = "Likert";
        }
        else
        {
            switch (value)
            {
                case SurveyHeader h:
                    ItemTextPlaceholder = DefaultHeaderText;
                    ItemText = h.Text;
                    ItemIsOptional = null;
                    CurrentResponse = null;
                    break;

                case SurveyInstruction i:
                    ItemTextPlaceholder = DefaultInstructionText;
                    ItemText = i.Text;
                    ItemIsOptional = null;
                    CurrentResponse = null;
                    break;

                case SurveyImage img:
                    ItemTextPlaceholder = DefaultQuestionText;
                    ItemText = string.Empty;
                    ItemIsOptional = null;
                    CurrentResponse = null;
                    LoadImagePreview(img.ImageId);
                    break;

                case SurveyQuestion q:
                    ItemTextPlaceholder = DefaultQuestionText;
                    ItemText = q.Text;
                    ItemIsOptional = q.IsOptional;
                    CurrentResponse = q.Response;
                    SelectedResponseType = ResponseTypeDisplayName(q.Response);
                    break;
            }
        }

        LoadHeaderStyleFromSelection();

        OnPropertyChanged(nameof(IsImageItemSelected));
        OnPropertyChanged(nameof(ShowItemTextEditor));
        OnPropertyChanged(nameof(ShowHeaderStyleEditor));
        NotifyItemCommands();
    }

    private void LoadImagePreview(Guid imageId)
    {
        if (imageId == Guid.Empty)
        {
            CurrentImagePreview = null;
            CurrentImageFileName = "(no image selected)";
            return;
        }

        try
        {
            var bytes = _packageService.GetImageBytes(imageId);
            if (bytes is null || bytes.Length == 0)
            {
                CurrentImagePreview = null;
                CurrentImageFileName = imageId.ToString("N")[..8] + "… (missing data)";
                return;
            }

            var bmp = new BitmapImage();
            using var ms = new MemoryStream(bytes);
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = ms;
            bmp.EndInit();
            bmp.Freeze();
            CurrentImagePreview = bmp;

            // Prefer the stimulus file name if this image is also registered as a stimulus.
            var stim = _currentTest.Stimuli.OfType<ImageStimulus>().FirstOrDefault(s => s.Id == imageId);
            CurrentImageFileName = stim is not null
                ? Path.GetFileName(stim.FileName)
                : imageId.ToString("N")[..8] + "…";
        }
        catch
        {
            CurrentImagePreview = null;
            CurrentImageFileName = "(failed to load preview)";
        }
    }

    partial void OnItemTextChanged(string value)
    {
        if (SelectedItem is null) return;

        switch (SelectedItem)
        {
            case SurveyHeader h: h.Text = value ?? string.Empty; break;
            case SurveyInstruction i: i.Text = value ?? string.Empty; break;
            case SurveyQuestion q: q.Text = value ?? string.Empty; break;
        }
    }

    partial void OnItemIsOptionalChanged(bool? value)
    {
        if (SelectedItem is SurveyQuestion q)
            q.IsOptional = value;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Header / caption style
    // ─────────────────────────────────────────────────────────────────────

    private void LoadHeaderStyleFromSelection()
    {
        _suppressHeaderStylePush = true;
        try
        {
            if (SelectedItem is not SurveyHeader header)
                return;

            header.Style ??= new SurveyHeaderStyle();
            if (string.IsNullOrWhiteSpace(header.Style.FontFamily))
                header.Style.FontFamily = SurveyHeaderStyle.DefaultFontFamily;
            if (header.Style.FontSize <= 0)
                header.Style.FontSize = SurveyHeaderStyle.DefaultFontSize;
            if (header.Style.SeparatorWidth <= 0)
                header.Style.SeparatorWidth = SurveyHeaderStyle.DefaultSeparatorWidth;

            HeaderFontFamily = header.Style.FontFamily;
            HeaderFontSize = header.Style.FontSize;
            HeaderFontColor = header.Style.FontColor;
            HeaderBackColor = header.Style.BackColor;
            HeaderSeparatorColor = header.Style.SeparatorColor;
            HeaderSeparatorWidth = header.Style.SeparatorWidth;
            NotifyHeaderPreviewBrushes();
        }
        finally
        {
            _suppressHeaderStylePush = false;
        }
    }

    private void PersistHeaderStyle()
    {
        if (_suppressHeaderStylePush || SelectedItem is not SurveyHeader header)
            return;

        header.Style ??= new SurveyHeaderStyle();
        header.Style.FontFamily = string.IsNullOrWhiteSpace(HeaderFontFamily)
            ? SurveyHeaderStyle.DefaultFontFamily
            : HeaderFontFamily;
        header.Style.FontSize = HeaderFontSize > 0 ? HeaderFontSize : SurveyHeaderStyle.DefaultFontSize;
        header.Style.FontColor = HeaderFontColor;
        header.Style.BackColor = HeaderBackColor;
        header.Style.SeparatorColor = HeaderSeparatorColor;
        header.Style.SeparatorWidth = HeaderSeparatorWidth > 0
            ? HeaderSeparatorWidth
            : SurveyHeaderStyle.DefaultSeparatorWidth;

        _lastHeaderStyle = header.Style.Clone();
    }

    private void NotifyHeaderPreviewBrushes()
    {
        OnPropertyChanged(nameof(HeaderFontPreviewBrush));
        OnPropertyChanged(nameof(HeaderBackPreviewBrush));
        OnPropertyChanged(nameof(HeaderSeparatorPreviewBrush));
    }

    partial void OnHeaderFontFamilyChanged(string value) => PersistHeaderStyle();
    partial void OnHeaderFontSizeChanged(double value) => PersistHeaderStyle();
    partial void OnHeaderSeparatorWidthChanged(int value) => PersistHeaderStyle();

    partial void OnHeaderFontColorChanged(Color value)
    {
        OnPropertyChanged(nameof(HeaderFontPreviewBrush));
        PersistHeaderStyle();
    }

    partial void OnHeaderBackColorChanged(Color value)
    {
        OnPropertyChanged(nameof(HeaderBackPreviewBrush));
        PersistHeaderStyle();
    }

    partial void OnHeaderSeparatorColorChanged(Color value)
    {
        OnPropertyChanged(nameof(HeaderSeparatorPreviewBrush));
        PersistHeaderStyle();
    }

    private static Color ResolvePalette(string? paletteType) =>
        (paletteType ?? string.Empty).ToLowerInvariant() switch
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
            _ => Colors.Black
        };

    [RelayCommand]
    private void ApplyHeaderTextPalette(string? paletteType)
    {
        if (SelectedItem is not SurveyHeader) return;
        HeaderFontColor = ResolvePalette(paletteType);
    }

    [RelayCommand]
    private void ApplyHeaderBackPalette(string? paletteType)
    {
        if (SelectedItem is not SurveyHeader) return;
        HeaderBackColor = ResolvePalette(paletteType);
    }

    [RelayCommand]
    private void ApplyHeaderSeparatorPalette(string? paletteType)
    {
        if (SelectedItem is not SurveyHeader) return;
        HeaderSeparatorColor = ResolvePalette(paletteType);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Commands – Surveys
    // ─────────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void AddSurvey()
    {
        var survey = new Survey
        {
            Name = DefaultNameForIndex(Surveys.Count),
            TimeoutSeconds = 0,
            AllQuestionsOptional = false
        };
        _currentTest.AddSurvey(survey);
        SelectedSurvey = survey;
    }

    [RelayCommand(CanExecute = nameof(CanDeleteSurvey))]
    private void DeleteSurvey()
    {
        if (SelectedSurvey is null) return;
        _currentTest.RemoveSurvey(SelectedSurvey);
        SelectedSurvey = null;
    }

    private bool CanDeleteSurvey() => SelectedSurvey is not null;

    private string DefaultNameFor(Survey survey)
    {
        var idx = Surveys.IndexOf(survey);
        return DefaultNameForIndex(idx >= 0 ? idx : Surveys.Count);
    }

    private static string DefaultNameForIndex(int zeroBasedIndex) =>
        $"Survey {Math.Max(0, zeroBasedIndex) + 1}";

    // ─────────────────────────────────────────────────────────────────────
    // Commands – Items
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a header at the top of the items list. Only one header is allowed per survey.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAddHeader))]
    private void AddHeader()
    {
        if (SelectedSurvey is null) return;
        if (SelectedSurvey.Items.OfType<SurveyHeader>().Any()) return;

        var item = new SurveyHeader
        {
            Text = DefaultHeaderText,
            Style = _lastHeaderStyle.Clone()
        };
        SelectedSurvey.Items.Insert(0, item);
        SelectedItem = item;
        NotifyItemCommands();
    }

    [RelayCommand(CanExecute = nameof(CanModifyItems))]
    private void AddInstruction()
    {
        if (SelectedSurvey is null) return;
        var item = new SurveyInstruction { Text = DefaultInstructionText };
        SelectedSurvey.Items.Add(item);
        SelectedItem = item;
    }

    [RelayCommand(CanExecute = nameof(CanModifyItems))]
    private void AddQuestion()
    {
        if (SelectedSurvey is null) return;
        var item = new SurveyQuestion
        {
            Text = DefaultQuestionText,
            Response = CreateResponse("Likert")
        };
        SelectedSurvey.Items.Add(item);
        SelectedItem = item;
    }

    /// <summary>
    /// Opens a file dialog, stores the image in the project package, and adds a SurveyImage item.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanModifyItems))]
    private async Task AddImageAsync()
    {
        if (SelectedSurvey is null) return;

        var imageId = await PickAndStoreImageAsync();
        if (imageId is null) return;

        var item = new SurveyImage { ImageId = imageId.Value };
        SelectedSurvey.Items.Add(item);
        SelectedItem = item;
    }

    /// <summary>
    /// Replaces the image on the currently selected SurveyImage item.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanChangeImage))]
    private async Task ChangeImageAsync()
    {
        if (SelectedItem is not SurveyImage img) return;

        var imageId = await PickAndStoreImageAsync();
        if (imageId is null) return;

        img.ImageId = imageId.Value;
        LoadImagePreview(img.ImageId);
    }

    private async Task<Guid?> PickAndStoreImageAsync()
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select survey image",
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true)
            return null;

        try
        {
            var bytes = await File.ReadAllBytesAsync(dialog.FileName);
            var imageId = await _packageService.AddImageAsync(bytes, Path.GetFileName(dialog.FileName));

            // Also register as an ImageStimulus so the image is part of the test's asset set
            // and can be reused / found by file name in the Stimuli tab.
            if (_currentTest.Stimuli.All(s => s.Id != imageId))
            {
                _currentTest.AddStimulus(new ImageStimulus
                {
                    Id = imageId,
                    FileName = Path.GetFileName(dialog.FileName),
                    AltText = string.Empty
                });
            }

            return imageId;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Failed to add image:\n{ex.Message}",
                "Image Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            return null;
        }
    }

    [RelayCommand(CanExecute = nameof(CanDeleteItem))]
    private void DeleteItem()
    {
        if (SelectedSurvey is null || SelectedItem is null) return;
        SelectedSurvey.Items.Remove(SelectedItem);
        SelectedItem = null;
        NotifyItemCommands(); // re-enable Add Header if the only header was removed
    }

    [RelayCommand(CanExecute = nameof(CanMoveItem))]
    private void MoveItemUp()
    {
        if (SelectedSurvey is null || SelectedItem is null) return;
        if (SelectedItem is SurveyHeader) return; // header is locked at the top

        var idx = SelectedSurvey.Items.IndexOf(SelectedItem);
        if (idx <= 0) return;

        // Do not move anything above the header
        if (idx == 1 && SelectedSurvey.Items[0] is SurveyHeader) return;

        var item = SelectedItem;
        SelectedSurvey.Items.Move(idx, idx - 1);
        SelectedItem = item;
    }

    [RelayCommand(CanExecute = nameof(CanMoveItem))]
    private void MoveItemDown()
    {
        if (SelectedSurvey is null || SelectedItem is null) return;
        if (SelectedItem is SurveyHeader) return; // header stays at top

        var idx = SelectedSurvey.Items.IndexOf(SelectedItem);
        if (idx < 0 || idx >= SelectedSurvey.Items.Count - 1) return;
        var item = SelectedItem;
        SelectedSurvey.Items.Move(idx, idx + 1);
        SelectedItem = item;
    }

    private bool CanModifyItems() => SelectedSurvey is not null;

    /// <summary>Only one header is allowed, and it must sit at the top.</summary>
    private bool CanAddHeader() =>
        SelectedSurvey is not null &&
        !SelectedSurvey.Items.OfType<SurveyHeader>().Any();

    private bool CanDeleteItem() => SelectedSurvey is not null && SelectedItem is not null;

    /// <summary>Headers are locked at position 0 and cannot be reordered.</summary>
    private bool CanMoveItem() =>
        SelectedSurvey is not null &&
        SelectedItem is not null &&
        SelectedItem is not SurveyHeader;

    private bool CanChangeImage() => SelectedItem is SurveyImage;

    // ─────────────────────────────────────────────────────────────────────
    // Response type switching + list helpers
    // ─────────────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string newChoiceText = string.Empty;

    [ObservableProperty]
    private string? selectedChoice;

    [RelayCommand]
    private void ChangeResponseType(string? typeName)
    {
        if (SelectedItem is not SurveyQuestion q) return;
        if (string.IsNullOrWhiteSpace(typeName)) return;

        ResponseDefinition newResponse = CreateResponse(typeName);
        q.Response = newResponse;
        CurrentResponse = newResponse;
        SelectedResponseType = typeName;
        NewChoiceText = string.Empty;
        SelectedChoice = null;
    }

    private static string ResponseTypeDisplayName(ResponseDefinition? response) => response switch
    {
        TrueFalseResponse => "True False",
        LikertResponse => "Likert",
        MultipleChoiceResponse => "Multi-Choice",
        MultiSelectResponse => "Multi-Select",
        DateResponse => "Date",
        FixedDigitsResponse => "Fixed Digits",
        BoundedTextResponse => "Bounded Text",
        BoundedNumberResponse => "Bounded Number",
        RegexResponse => "Regex",
        _ => "Likert"
    };

    private static ResponseDefinition CreateResponse(string typeName) => typeName switch
    {
        "True False" or "TrueFalse" => new TrueFalseResponse(),
        "Likert" => CreateLikert(),
        "Multi-Choice" or "MultiChoice" or "MultipleChoice" => CreateMultipleChoice(),
        "Multi-Select" or "MultiSelect" => CreateMultiSelect(),
        "Date" => new DateResponse(),
        "Fixed Digits" or "FixedDigits" => new FixedDigitsResponse { DigitCount = 4 },
        "Bounded Text" or "BoundedText" => new BoundedTextResponse { MinLength = 0, MaxLength = 200 },
        "Bounded Number" or "BoundedNumber" => new BoundedNumberResponse(),
        "Regex" => new RegexResponse { Pattern = DefaultRegexPattern },
        _ => CreateLikert()
    };

    private static LikertResponse CreateLikert()
    {
        var r = new LikertResponse { Min = 1, Max = LikertResponse.DefaultLabels.Length };
        foreach (var label in LikertResponse.DefaultLabels)
            r.Labels.Add(label);
        return r;
    }

    private static MultipleChoiceResponse CreateMultipleChoice()
    {
        var r = new MultipleChoiceResponse();
        r.Choices.Add("Option A");
        r.Choices.Add("Option B");
        return r;
    }

    private static MultiSelectResponse CreateMultiSelect()
    {
        var r = new MultiSelectResponse();
        r.Choices.Add("Option A");
        r.Choices.Add("Option B");
        return r;
    }

    /// <summary>Adds a choice/label to the current response (MultipleChoice, MultiSelect, or Likert labels).</summary>
    [RelayCommand]
    private void AddChoice()
    {
        var text = (NewChoiceText ?? string.Empty).Trim();
        if (text.Length == 0 || string.Equals(text, DefaultChoiceText, StringComparison.Ordinal))
            return;

        switch (CurrentResponse)
        {
            case MultipleChoiceResponse mc:
                mc.Choices.Add(text);
                break;
            case MultiSelectResponse ms:
                ms.Choices.Add(text);
                break;
            case LikertResponse lr:
                lr.Labels.Add(text);
                break;
            default:
                return;
        }

        NewChoiceText = string.Empty;
    }

    /// <summary>Removes the selected choice/label from the current response.</summary>
    [RelayCommand]
    private void RemoveChoice()
    {
        if (SelectedChoice is null) return;

        switch (CurrentResponse)
        {
            case MultipleChoiceResponse mc:
                mc.Choices.Remove(SelectedChoice);
                break;
            case MultiSelectResponse ms:
                ms.Choices.Remove(SelectedChoice);
                break;
            case LikertResponse lr:
                lr.Labels.Remove(SelectedChoice);
                break;
        }

        SelectedChoice = null;
    }
}
