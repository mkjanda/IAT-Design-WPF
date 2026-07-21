using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using IAT.Core.Domain;
using IAT.Core.Messages;
using IAT.Core.Services;
using System.IO;
using System.Collections.Specialized;

namespace IAT.ViewModels.Controls
{
    /// <summary>
    /// Main ViewModel for the entire designer (Blocks + Layout + Stimuli + Trials + Deploy tabs).
    /// Owns document-level state (current path, dirty flag, window title) and the New/Open/Save/Save As commands.
    /// Holds references to the per-tab ViewModels that share the same singleton <see cref="IatTest"/>.
    /// </summary>
    public partial class TestDesignerViewModel : ObservableObject
    {
        private const string IatFilter = "IAT Project (*.iat)|*.iat|All files (*.*)|*.*";

        private readonly IProjectPackageService _packageService;
        private readonly IUserNotificationService _userNotificationService;
        private readonly IDialogService _dialogService;
        private readonly IatTest _currentTest;

        /// <summary>ViewModel for the Blocks tab.</summary>
        public BlockEditViewModel BlockEditor { get; }

        /// <summary>Shared layout editor used by the Layout tab and the Blocks preview.</summary>
        public LayoutViewModel LayoutEditor { get; }

        /// <summary>ViewModel for the Stimuli tab.</summary>
        public StimuliManagerViewModel StimuliManager { get; }

        /// <summary>ViewModel for the Trials tab.</summary>
        public TrialsManagerViewModel TrialsManager { get; }

        /// <summary>Full path of the open project file, or null if never saved.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WindowTitle))]
        private string? currentFilePath;

        /// <summary>True when the in-memory test has unsaved changes.</summary>
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(WindowTitle))]
        private bool isDirty;

        /// <summary>Bound to MainWindow.Title.</summary>
        public string WindowTitle
        {
            get
            {
                var name = string.IsNullOrWhiteSpace(CurrentFilePath)
                    ? (_currentTest.Name is { Length: > 0 } n ? n : "Untitled")
                    : Path.GetFileName(CurrentFilePath);
                return IsDirty ? $"IAT Designer - {name} *" : $"IAT Designer - {name}";
            }
        }

        [ObservableProperty] private bool _isStimuliSelected;
        [ObservableProperty] private bool _isBlocksSelected;
        [ObservableProperty] private bool _isLayoutSelected;
        [ObservableProperty] private bool _isTrialsSelected;

        public TestDesignerViewModel(
            IProjectPackageService packageService,
            IDialogService dialogService,
            IatTest currentTest,
            IUserNotificationService userNotificationService,
            BlockEditViewModel blockEditor,
            LayoutViewModel layoutEditor,
            StimuliManagerViewModel stimuliManager,
            TrialsManagerViewModel trialsManager)
        {
            _packageService = packageService;
            _dialogService = dialogService;
            _currentTest = currentTest ?? throw new ArgumentNullException(nameof(currentTest));
            _userNotificationService = userNotificationService ?? throw new ArgumentNullException(nameof(userNotificationService));
            BlockEditor = blockEditor;
            LayoutEditor = layoutEditor;
            StimuliManager = stimuliManager;
            TrialsManager = trialsManager;

            // Child tabs broadcast edits; mark the document dirty.
            WeakReferenceMessenger.Default.Register<TestModifiedMessage>(this, (_, _) => MarkDirty());

            // Structural changes on the shared domain model mark the document dirty.
            _currentTest.BlocksCollection.CollectionChanged += (_, _) => MarkDirty();
            _currentTest.Stimuli.CollectionChanged += (_, _) => MarkDirty();
            _currentTest.TrialsCollection.CollectionChanged += (_, _) => MarkDirty();
            _currentTest.KeysCollection.CollectionChanged += (_, _) => MarkDirty();
        }

        private bool _suppressDirty;

        /// <summary>Marks the document dirty (also callable from child VMs if messenger is not used).</summary>
        public void MarkDirty()
        {
            if (_suppressDirty || IsDirty)
                return;
            IsDirty = true;
        }

        /// <summary>
        /// Returns false if the user cancels a dirty prompt (discard changes?).
        /// Returns true if it is safe to proceed (not dirty, or user confirmed).
        /// </summary>
        public async Task<bool> ConfirmDiscardIfDirtyAsync()
        {
            if (!IsDirty)
                return true;

            return await _dialogService.ShowConfirmationAsync(
                "You have unsaved changes. Discard them and continue?",
                "Unsaved Changes");
        }

        // ── File commands ────────────────────────────────────────────────────

        [RelayCommand]
        private async Task NewAsync()
        {
            if (!await ConfirmDiscardIfDirtyAsync())
                return;

            _suppressDirty = true;
            try
            {
                _currentTest.Reset();
                CurrentFilePath = null;
                IsDirty = false;
                NotifyDocumentReset();
            }
            finally
            {
                _suppressDirty = false;
            }
            OnPropertyChanged(nameof(WindowTitle));
        }

        [RelayCommand]
        private async Task OpenAsync()
        {
            if (!await ConfirmDiscardIfDirtyAsync())
                return;

            var path = await _dialogService.ShowOpenFileDialogAsync(IatFilter, "Open IAT Project");
            if (path is null)
                return;

            try
            {
                var loaded = await _packageService.LoadProjectAsync(path);
                // --- temporary diagnostics ---
                System.Diagnostics.Debug.WriteLine($"LOADED Stimuli : {loaded.Stimuli.Count}");
                System.Diagnostics.Debug.WriteLine($"LOADED Blocks  : {loaded.Blocks.Count}");
                System.Diagnostics.Debug.WriteLine($"LOADED Trials  : {loaded.Trials.Count}");
                System.Diagnostics.Debug.WriteLine($"LOADED Keys    : {loaded.Keys.Count}");
                // -----------------------------
                _suppressDirty = true;
                try
                {
                    _currentTest.ReplaceWith(loaded);
                    CurrentFilePath = path;
                    IsDirty = false;

                    NotifyDocumentReset();
                }
                finally
                {
                    _suppressDirty = false;
                }
                OnPropertyChanged(nameof(WindowTitle));
            }
            catch (Exception ex)
            {
                _userNotificationService.ShowError(new ErrorNotificationMessage($"Could not open the project:\n{ex.Message}", "Open Failed"));
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (string.IsNullOrWhiteSpace(CurrentFilePath))
            {
                await SaveAsAsync();
                return;
            }

            await SaveToPathAsync(CurrentFilePath);
        }

        [RelayCommand]
        private async Task SaveAsAsync()
        {
            var defaultName = string.IsNullOrWhiteSpace(CurrentFilePath)
                ? SanitizeFileName(_currentTest.Name) + ".iat"
                : Path.GetFileName(CurrentFilePath);

            var path = await _dialogService.ShowSaveFileDialogAsync(IatFilter, defaultName, "Save IAT Project As");
            if (path is null)
                return;

            if (await SaveToPathAsync(path))
            {
                CurrentFilePath = path;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private async Task<bool> SaveToPathAsync(string path)
        {
            try
            {
                // Keep the domain name aligned with the file when still at the default.
                if (string.IsNullOrWhiteSpace(_currentTest.Name) || _currentTest.Name == "New IAT Test")
                    _currentTest.Name = Path.GetFileNameWithoutExtension(path);

                await _packageService.SaveProjectAsync(_currentTest, path);
                IsDirty = false;
                OnPropertyChanged(nameof(WindowTitle));
                return true;
            }
            catch (Exception ex)
            {
                _userNotificationService.ShowError(new ErrorNotificationMessage($"Could not save the project:\n{ex.Message}", "Save Failed"));
                return false;
            }
        }

        /// <summary>
        /// Pushes the new document state into every tab ViewModel.
        /// </summary>
        private void NotifyDocumentReset()
        {
            // Caller is responsible for _suppressDirty while the document is being replaced.
            LayoutEditor.ReloadGeometry();
            BlockEditor.OnDocumentReset();
            TrialsManager.OnDocumentReset();
            StimuliManager.OnDocumentReset();
        }

        private static string SanitizeFileName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "Untitled";
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name.Trim();
        }

        // ── Tab selection (existing) ─────────────────────────────────────────

        [RelayCommand]
        private void OnStimuliTabSelected() => IsStimuliSelected = true;

        [RelayCommand]
        private void OnBlocksTabSelected() => IsBlocksSelected = true;

        [RelayCommand]
        private void OnLayoutTabSelected()
        {
            IsLayoutSelected = true;
            if (LayoutEditor is not null)
                LayoutEditor.IsLayoutEditMode = true;
        }

        [RelayCommand]
        private void OnTrialsTabSelected() => IsTrialsSelected = true;
    }
}
