using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IAT.Core.Domain;
using IAT.Core.Services;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace IAT.ViewModels.Controls;

/// <summary>
/// Manages the stimuli list and the currently active edit panel for an IAT test.
/// The ListBox is bound directly to IatTest.Stimuli (the singleton domain collection)
/// via the DisplayedStimuli property. Selection drives creation of the appropriate
/// edit ViewModel (text or image). Save on the edit panel adds or updates the item
/// in that same ObservableCollection.
/// </summary>
public partial class StimuliManagerViewModel : ObservableObject
{
    private readonly IatTest _currentTest;
    private readonly IProjectPackageService _packageService;
    private readonly IImageGenerationService _imageGenService;
    private readonly ILayoutCalculatorService _layoutCalculatorService;

    /// <summary>
    /// Direct reference to the singleton domain model. Exposed so XAML (or other VMs)
    /// can reach IatTest.Stimuli if needed.
    /// </summary>
    public IatTest CurrentTest => _currentTest;

    [ObservableProperty]
    private Stimulus? selectedItem;

    [ObservableProperty]
    private string searchText = string.Empty;

    /// <summary>
    /// The ViewModel currently shown in the right-hand edit pane.
    /// Type is object so the DataTemplateSelector can distinguish Text vs Image.
    /// </summary>
    [ObservableProperty]
    private object? currentEditViewModel;

    private ObservableCollection<Stimulus>? _filteredStimuli;

    private ObservableCollection<StimulusEditViewModel> StimuliLibrary = new ObservableCollection<StimulusEditViewModel>();
    public ObservableCollection<Stimulus>? FilteredStimuli
    {
        get => _filteredStimuli;
        private set
        {
            if (_filteredStimuli != value)
            {
                _filteredStimuli = value;
                OnPropertyChanged(nameof(DisplayedStimuli));
            }
        }
    }

    /// <summary>
    /// Always returns a live bindable collection:
    /// - the full IatTest.Stimuli when no search filter is active
    /// - a temporary filtered collection when SearchText is set
    /// This is what the ListBox binds to.
    /// </summary>
    public ObservableCollection<Stimulus> DisplayedStimuli =>
        _filteredStimuli ?? _currentTest.Stimuli;

    public StimuliManagerViewModel(IatTest currentTest,
                                  IProjectPackageService packageService,
                                  IImageGenerationService imageGenService,
                                  ILayoutCalculatorService rectCalculator)
    {
        _currentTest = currentTest ?? throw new ArgumentNullException(nameof(currentTest));
        _packageService = packageService;
        _imageGenService = imageGenService;
        _layoutCalculatorService = rectCalculator;
    }

    partial void OnSearchTextChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _filteredStimuli = null;
        }
        else
        {
            var filtered = _currentTest.Stimuli
                .Where(s => s is TextStimulus ts && ts.Text.Contains(value, StringComparison.OrdinalIgnoreCase)
                         || s is ImageStimulus img &&
                            ((img.FileName?.Contains(value, StringComparison.OrdinalIgnoreCase) == true)
                             || (img.AltText?.Contains(value, StringComparison.OrdinalIgnoreCase) == true)))
                .ToList();
            _filteredStimuli = new ObservableCollection<Stimulus>(filtered);
        }
        OnPropertyChanged(nameof(DisplayedStimuli));
    }

    partial void OnSelectedItemChanged(Stimulus? value)
    {
        if (value is null)
        {
            DetachEditorEvents();
            CurrentEditViewModel = null;
            return;
        }

        DetachEditorEvents();

        if (value is TextStimulus textStim)
        {
            var vm = new StimulusEditViewModel(textStim, _currentTest);
            AttachEditorEvents(vm);
            CurrentEditViewModel = vm;
        }
        else if (value is ImageStimulus imageStim)
        {
            var vm = new ImageStimulusEditViewModel(imageStim, _currentTest, _packageService);
            AttachEditorEvents(vm);
            CurrentEditViewModel = vm;
        }
        else
        {
            CurrentEditViewModel = null;
        }
    }

    private void AttachEditorEvents(StimulusEditViewModel vm)
    {
        vm.Saved += OnEditorSaved;
        vm.Deleted += OnEditorDeleted;
        vm.Closed += OnEditorClosed;
        vm.AddNewRequested += OnAddNewRequested;
    }

    private void DetachEditorEvents()
    {
        if (CurrentEditViewModel is StimulusEditViewModel old)
        {
            old.Saved -= OnEditorSaved;
            old.Deleted -= OnEditorDeleted;
            old.Closed -= OnEditorClosed;
            old.AddNewRequested -= OnAddNewRequested;
        }
    }

    private void OnEditorSaved()
    {
        // Domain collection already notified via AddStimulus / UpdateStimulus.
        // Re-select so the ListBox and editor stay consistent after a replace.
        var id = SelectedItem?.Id;
        if (id.HasValue)
        {
            SelectedItem = _currentTest.GetStimulusById(id.Value);
        }
        // Also refresh filter if active
        if (_filteredStimuli is not null)
            OnSearchTextChanged(SearchText);
    }

    private void OnEditorDeleted()
    {
        CurrentEditViewModel = null;
        SelectedItem = null;
        if (_filteredStimuli is not null)
            OnSearchTextChanged(SearchText);
    }

    private void OnEditorClosed()
    {
        CurrentEditViewModel = null;
    }

    private void OnAddNewRequested()
    {
        AddTextStimulus();
    }

    [RelayCommand]
    private void AddTextStimulus()
    {
        var newStimulus = new TextStimulus
        {
            Id = Guid.NewGuid(),
            Text = "New Text Stimulus"
        };
        _currentTest.AddStimulus(newStimulus);
        SelectedItem = newStimulus; // triggers creation of edit VM
    }

    [RelayCommand]
    private async Task AddImageStimulusAsync()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp",
            Multiselect = false
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            var bytes = await File.ReadAllBytesAsync(dialog.FileName);
            var imageId = await _packageService.AddImageAsync(bytes, Path.GetFileName(dialog.FileName));

            var newStimulus = new ImageStimulus
            {
                Id = imageId,
                FileName = dialog.FileName,
                AltText = string.Empty
            };

            _currentTest.AddStimulus(newStimulus);
            SelectedItem = newStimulus;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to add image: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void DeleteStimulus(Stimulus? item)
    {
        if (item is null) return;

        // TODO: Production check – is this stimulus used in any trial?
        _currentTest.RemoveStimulus(item);
        if (ReferenceEquals(SelectedItem, item) || SelectedItem?.Id == item.Id)
        {
            SelectedItem = null;
            CurrentEditViewModel = null;
        }
        if (_filteredStimuli is not null)
            OnSearchTextChanged(SearchText);
    }

    /// <summary>
    /// Called by the shell after New/Open so the Stimuli tab clears selection and filters.
    /// </summary>
    public void OnDocumentReset()
    {
        SelectedItem = null;
        CurrentEditViewModel = null;
        SearchText = string.Empty;
        _filteredStimuli = null;

        // Rebuild the library from the current document
        StimuliLibrary.Clear();   // or whatever the actual collection property is named

        if (_currentTest is not null)   // or however you hold the shared IatTest
        {
            foreach (var stim in _currentTest.AllStimuli)
            {
                if (stim is TextStimulus textStim)
                {
                    StimuliLibrary.Add(new StimulusEditViewModel(textStim, _currentTest));
                }
                else if (stim is ImageStimulus imgStim)
                {
                    StimuliLibrary.Add(new ImageStimulusEditViewModel(
                        imgStim,
                        _currentTest,
                        _packageService));   // you need the package service here
                }
            }
        }

        OnPropertyChanged(nameof(FilteredStimuli));
    }
}
