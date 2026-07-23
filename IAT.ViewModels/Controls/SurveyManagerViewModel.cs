using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
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

    // ── Item-level editable properties ─────────────────────────────────────

    [ObservableProperty]
    private string itemText = string.Empty;          // Header / Instruction / Question text

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
            TimeoutSeconds = 0;
            AllQuestionsOptional = false;
        }
        else
        {
            SurveyName = value.Name;
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
                    ItemText = h.Text;
                    ItemIsOptional = null;
                    CurrentResponse = null;
                    break;

                case SurveyInstruction i:
                    ItemText = i.Text;
                    ItemIsOptional = null;
                    CurrentResponse = null;
                    break;

                case SurveyImage img:
                    ItemText = string.Empty;
                    ItemIsOptional = null;
                    CurrentResponse = null;
                    LoadImagePreview(img.ImageId);
                    break;

                case SurveyQuestion q:
                    ItemText = q.Text;
                    ItemIsOptional = q.IsOptional;
                    CurrentResponse = q.Response;
                    SelectedResponseType = q.Response?.GetType().Name.Replace("Response", "") ?? "Likert";
                    break;
            }
        }

        OnPropertyChanged(nameof(IsImageItemSelected));
        OnPropertyChanged(nameof(ShowItemTextEditor));
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
    // Commands – Surveys
    // ─────────────────────────────────────────────────────────────────────

    [RelayCommand]
    private void AddSurvey()
    {
        var survey = new Survey
        {
            Name = $"Survey {Surveys.Count + 1}",
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

        var item = new SurveyHeader { Text = "New Header" };
        SelectedSurvey.Items.Insert(0, item);
        SelectedItem = item;
        NotifyItemCommands();
    }

    [RelayCommand(CanExecute = nameof(CanModifyItems))]
    private void AddInstruction()
    {
        if (SelectedSurvey is null) return;
        var item = new SurveyInstruction { Text = "New instruction text…" };
        SelectedSurvey.Items.Add(item);
        SelectedItem = item;
    }

    [RelayCommand(CanExecute = nameof(CanModifyItems))]
    private void AddQuestion()
    {
        if (SelectedSurvey is null) return;
        var item = new SurveyQuestion
        {
            Text = "New question",
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
                    FileName = dialog.FileName,
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

        SelectedSurvey.Items.RemoveAt(idx);
        SelectedSurvey.Items.Insert(idx - 1, SelectedItem);
        SelectedItem = SelectedSurvey.Items[idx - 1];
    }

    [RelayCommand(CanExecute = nameof(CanMoveItem))]
    private void MoveItemDown()
    {
        if (SelectedSurvey is null || SelectedItem is null) return;
        if (SelectedItem is SurveyHeader) return; // header stays at top

        var idx = SelectedSurvey.Items.IndexOf(SelectedItem);
        if (idx < 0 || idx >= SelectedSurvey.Items.Count - 1) return;
        SelectedSurvey.Items.RemoveAt(idx);
        SelectedSurvey.Items.Insert(idx + 1, SelectedItem);
        SelectedItem = SelectedSurvey.Items[idx + 1];
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

    private static ResponseDefinition CreateResponse(string typeName) => typeName switch
    {
        "Likert" => CreateLikert(),
        "MultipleChoice" => CreateMultipleChoice(),
        "MultiSelect" => CreateMultiSelect(),
        "Date" => new DateResponse(),
        "FixedDigits" => new FixedDigitsResponse { DigitCount = 4 },
        "BoundedText" => new BoundedTextResponse { MinLength = 0, MaxLength = 200 },
        "BoundedNumber" => new BoundedNumberResponse(),
        "Regex" => new RegexResponse { Pattern = @".+" },
        _ => CreateLikert()
    };

    private static LikertResponse CreateLikert()
    {
        var r = new LikertResponse { Min = 1, Max = 5 };
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
        if (text.Length == 0) return;

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
