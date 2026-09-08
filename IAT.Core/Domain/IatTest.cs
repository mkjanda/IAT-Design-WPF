using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace IAT.Core.Domain;

/// <summary>
/// Represents an Implicit Association Test (IAT) configuration, including its trials, blocks, stimuli, and
/// instruction screens. Provides methods for validation and entity lookup by unique identifier.
/// </summary>
/// <remarks>Use this class to manage and validate the structure of an IAT, ensuring that all required
/// components are present and correctly configured. The class exposes collections for trials, blocks, stimuli, and
/// instruction screens, and provides helper methods to retrieve specific entities by their unique identifiers.
/// Validation methods are available to check the integrity of the entire test before execution.</remarks>
public partial class IatTest : ObservableObject
{
    /// <summary>
    /// The unique identifier for this IAT test instance. This property is initialized with a new GUID by default, ensuring that each test has a 
    /// distinct identifier. The ID can be used for tracking, referencing, and managing test instances within the application or when persisting data. 
    /// It is important to ensure that the ID remains unique across all test instances to avoid conflicts and maintain data integrity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Display / deployment name of the IAT test. Independent of the package file path.
    /// Editable on the Deploy tab before upload; persisted in the project package.
    /// </summary>
    [ObservableProperty]
    private string name = "New IAT Test";

    /// <summary>
    /// Represents the layout of the test, including positions and sizes of various UI elements. This property is initialized with a default layout configuration.
    /// </summary>
    public Layout Layout { get; set; } = new Layout();

    /// <summary>
    /// A read-only collection of all trials in the test. This property provides a snapshot of the current trials, allowing clients to access the trial data without
    /// directly modifying the underlying collection.
    /// </summary>
    public List<Trial> AllTrials 
    {
        get => Trials.ToList();
        set { Trials.Clear(); if (value != null) foreach (var t in value) Trials.Add(t); }
    }

    /// <summary>
    /// A read-only list of block objects representing the structure of the test. Each block contains a 
    /// collection of trials, and this property provides a way to access all blocks without allowing direct 
    /// modification of the underlying collection.
    /// </summary>
    public List<Block> AllBlocks
    {

        get => Blocks.ToList();
        set { Blocks.Clear(); if (value != null) foreach (var b in value) Blocks.Add(b); }
    }

    /// <summary>
    /// Gets a read-only list of all stimuli.
    /// </summary>
    public List<Stimulus> AllStimuli
    {
        get => Stimuli.ToList();
        set
        {
            // Stimuli is already public and usually populated from the "Stimuli" array.
            // Only use this if Stimuli is still empty.
            if (Stimuli.Count > 0 || value is null) return;
            Stimuli.Clear();
            foreach (var s in value) Stimuli.Add(s);
        }
    }

    /// <summary>
    /// Gets or sets a list of all instruction screens.
    /// The setter populates <see cref="InstructionScreens"/> when that collection is still empty
    /// (same pattern as <see cref="AllStimuli"/> / <see cref="AllKeys"/>), so package load is
    /// resilient whether the JSON uses the observable collection name or this alias.
    /// </summary>
    public List<InstructionScreen> AllInstructionScreens
    {
        get => InstructionScreens.ToList();
        set
        {
            if (InstructionScreens.Count > 0 || value is null) return;
            InstructionScreens.Clear();
            foreach (var s in value)
                InstructionScreens.Add(s);
        }
    }

    /// <summary>
    /// Gets a read-only list of all keys used in the test. This collection provides access to the key 
    /// configurations without allowing direct modifications, ensuring that any changes to the keys are 
    /// managed through controlled methods or properties.
    /// </summary>
    public List<Key> AllKeys
    {
        get => Keys.ToList();
        set { Keys.Clear(); if (value != null) foreach (var k in value) Keys.Add(k); }
    }

    /// <summary>
    /// Gets the collection of trials associated with this instance.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as
    /// additions or removals of trials. Modifying the collection will not automatically persist changes unless
    /// explicitly handled elsewhere.</remarks>
    public ObservableCollection<Trial> Trials { get; } = new();

    /// <summary>
    /// Gets the collection of blocks contained in the document.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as
    /// additions or removals of blocks. Modifying the collection will update the document's structure
    /// accordingly.</remarks>
    public ObservableCollection<Block> Blocks { get; } = new();

    /// <summary>
    /// Gets the collection of stimuli associated with this instance.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as
    /// additions or removals of stimuli. Modifications to the collection will be reflected in any data bindings or
    /// observers. Prefer using AddStimulus / UpdateStimulus / RemoveStimulus for proper cache maintenance.
    /// Direct binding to this collection is supported for UI list views (e.g. StimuliManager ListBox).</remarks>
    public ObservableCollection<Stimulus> Stimuli { get; } = new();          // already public

    /// <summary>
    /// Gets the collection of instruction screens displayed to the user.
    /// </summary>
    public ObservableCollection<InstructionScreen> InstructionScreens { get; } = new();

    /// <summary>
    /// Gets the collection of keys managed by this instance.
    /// </summary>
    /// <remarks>The returned collection is observable. Changes to the collection, such as adding or removing
    /// keys, will raise collection change notifications. This property never returns null.</remarks>
    public ObservableCollection<Key> Keys { get; } = new();

    /// <summary>
    /// Gets the collection of surveys (questionnaires) associated with this test.
    /// Surveys can be administered before or after the IAT blocks.
    /// </summary>
    public ObservableCollection<Survey> Surveys { get; } = new();

    /// <summary>
    /// JSON-friendly alias for <see cref="Surveys"/> (same pattern as <see cref="AllKeys"/>).
    /// System.Text.Json does not reliably populate a get-only <see cref="ObservableCollection{T}"/>.
    /// </summary>
    public List<Survey> AllSurveys
    {
        get => Surveys.ToList();
        set
        {
            if (Surveys.Count > 0 || value is null) return;
            Surveys.Clear();
            foreach (var s in value)
                Surveys.Add(s);
        }
    }

    /// <summary>
    /// Live collection of surveys. Prefer <see cref="AddSurvey"/> / <see cref="RemoveSurvey"/> for mutation.
    /// </summary>
    [JsonIgnore]
    public ObservableCollection<Survey> SurveysCollection => Surveys;

    /// <summary>
    /// Formatted-text fragments owned by this test (block instructions, etc.).
    /// Referenced by Guid from domain objects such as <see cref="Block.BlockInstructionsId"/>.
    /// Prefer <see cref="AddFormattedText"/> / <see cref="RemoveFormattedText"/> /
    /// <see cref="EnsureBlockInstructions"/> for mutation so the lookup cache stays consistent.
    /// </summary>
    public ObservableCollection<FormattedText> FormattedTexts { get; } = new();

    /// <summary>
    /// JSON-friendly alias for <see cref="FormattedTexts"/> (same pattern as <see cref="AllKeys"/>).
    /// </summary>
    public List<FormattedText> AllFormattedTexts
    {
        get => FormattedTexts.ToList();
        set
        {
            if (FormattedTexts.Count > 0 || value is null) return;
            FormattedTexts.Clear();
            foreach (var ft in value)
                FormattedTexts.Add(ft);
        }
    }

    /// <summary>
    /// Add stimulus to the test and update the stimulus cache. This method ensures that 
    /// the stimulus is properly associated with the test and that the cache is kept up to 
    /// date for efficient retrieval.
    /// </summary>
    /// <param name="stimulus">The stimulus to add.</param>
    public void AddStimulus(Stimulus stimulus)
    {
        Stimuli.Add(stimulus);
        _stimulusCache[stimulus.Id] = stimulus;
        stimulus.IatTest = this;
    }

    /// <summary>
    /// Removes a stimulus from the collection and cache.
    /// </summary>
    /// <param name="stimulus">The stimulus to remove.</param>
    /// <returns>The removed stimulus.</returns>
    public Stimulus RemoveStimulus(Stimulus stimulus)
    {
        Stimuli.Remove(stimulus);
        _stimulusCache.Remove(stimulus.Id);
        return stimulus;
    }

    /// <summary>
    /// Updates an existing stimulus in the collection and cache.
    /// Prefers in-place property copy when the runtime type matches so the
    /// ObservableCollection item identity is preserved. That keeps ListBox
    /// selection stable and prevents the editor's Saved handler from being
    /// detached mid-Save (Remove would clear SelectedItem → OnSelectedItemChanged(null)
    /// → DetachEditorEvents before Saved?.Invoke runs).
    /// Falls back to Remove+Add only when the concrete type changes.
    /// </summary>
    /// <param name="stim">The stimulus to update.</param>
    public void UpdateStimulus(Stimulus stim)
    {
        if (stim is null) return;
        var existing = Stimuli.FirstOrDefault(s => s.Id == stim.Id);
        if (existing is null) return;

        // Same concrete type → mutate in place (preserves object identity / selection).
        if (existing is TextStimulus existingText && stim is TextStimulus newText)
        {
            existingText.Text = newText.Text;
            existingText.Style = new TextStyle
            {
                FontFamily = newText.Style?.FontFamily ?? existingText.Style?.FontFamily ?? "Segoe UI",
                FontSize   = newText.Style?.FontSize   ?? existingText.Style?.FontSize   ?? 24.0,
                FontColor  = newText.Style?.FontColor  ?? existingText.Style?.FontColor  ?? System.Windows.Media.Colors.Black
            };
            existingText.OriginatingBlock = newText.OriginatingBlock;
            existingText.KeyedDirection   = newText.KeyedDirection;
            _stimulusCache[stim.Id] = existingText;
            return;
        }

        if (existing is ImageStimulus existingImage && stim is ImageStimulus newImage)
        {
            existingImage.FileName   = newImage.FileName;
            existingImage.AltText    = newImage.AltText;
            existingImage.PackageUri = newImage.PackageUri;
            existingImage.OriginatingBlock = newImage.OriginatingBlock;
            existingImage.KeyedDirection   = newImage.KeyedDirection;
            _stimulusCache[stim.Id] = existingImage;
            return;
        }

        // Type changed (rare) — fall back to replace.
        _stimulusCache[stim.Id] = stim;
        Stimuli.Remove(existing);
        Stimuli.Add(stim);
        stim.IatTest = this;
    }

    /// <summary>
    /// Adds a block to the collection and cache, and associates it with this IAT test.
    /// </summary>
    /// <param name="block">The block to add.</param>
    public void AddBlock(Block block)
    {
        Blocks.Add(block);
        _blockCache[block.Id] = block;
        block.IatTest = this;
    }

    /// <summary>
    /// Removes a block from the collection and cache, and drops its block-instructions
    /// <see cref="FormattedText"/> when no other block still references that Id.
    /// </summary>
    /// <param name="block">The block to remove.</param>
    /// <returns>The removed block.</returns>
    public Block RemoveBlock(Block block)
    {
        Blocks.Remove(block);
        _blockCache.Remove(block.Id);

        if (block.BlockInstructionsId != Guid.Empty
            && !Blocks.Any(b => b.BlockInstructionsId == block.BlockInstructionsId)
            && _formattedTextCache.TryGetValue(block.BlockInstructionsId, out var ft))
        {
            RemoveFormattedText(ft);
        }

        return block;
    }

    /// <summary>
    /// Adds a trial to the collection and cache.
    /// </summary>
    /// <param name="trial">The trial to add.</param>
    public void AddTrial(Trial trial)
    {
        if (trial is null) return;
        if (!_trialCache.ContainsKey(trial.Id))
        {
            Trials.Add(trial);
            _trialCache[trial.Id] = trial;
        }
    }

    /// <summary>
    /// Removes a trial from the collection and cache, and removes its ID from every block that references it.
    /// </summary>
    /// <param name="trial">The trial to remove.</param>
    /// <returns>The removed trial, or null if not found.</returns>
    public Trial? RemoveTrial(Trial trial)
    {
        if (trial is null) return null;
        if (Trials.Remove(trial))
        {
            _trialCache.Remove(trial.Id);
            foreach (var block in Blocks)
            {
                block.TrialIds.Remove(trial.Id);
            }
            return trial;
        }
        return null;
    }

    /// <summary>
    /// Adds a key to the collection and cache.
    /// </summary>
    /// <param name="key">The key to add.</param>
    public void AddKey(Key key)
    {
        if (key is null) return;
        if (!_keyCache.ContainsKey(key.Id))
        {
            Keys.Add(key);
            _keyCache[key.Id] = key;
        }
    }

    /// <summary>
    /// Removes a key from the collection and cache.
    /// </summary>
    /// <param name="key">The key to remove.</param>
    /// <returns>The removed key, or null if not found.</returns>
    public Key? RemoveKey(Key key)
    {
        if (key is null) return null;
        if (Keys.Remove(key))
        {
            _keyCache.Remove(key.Id);
            return key;
        }
        return null;
    }

    /// <summary>
    /// Registers a <see cref="FormattedText"/> in the collection and lookup cache.
    /// No-ops when <paramref name="text"/> is null or already cached under the same Id.
    /// </summary>
    public void AddFormattedText(FormattedText text)
    {
        if (text is null) return;
        if (_formattedTextCache.ContainsKey(text.Id)) return;
        FormattedTexts.Add(text);
        _formattedTextCache[text.Id] = text;
    }

    /// <summary>
    /// Removes a <see cref="FormattedText"/> from the collection and cache.
    /// </summary>
    public FormattedText? RemoveFormattedText(FormattedText text)
    {
        if (text is null) return null;
        if (!FormattedTexts.Remove(text)) return null;
        _formattedTextCache.Remove(text.Id);
        return text;
    }

    /// <summary>
    /// Ensures <paramref name="block"/> has a live <see cref="FormattedText"/> for its
    /// block-level instructions. Creates one when missing, migrates legacy string-only
    /// data, and keeps <see cref="Block.BlockInstructions"/> / <see cref="Block.BlockInstructionsId"/>
    /// in sync with the FormattedText entry (the form export and slide rendering consume).
    /// </summary>
    /// <param name="block">Block whose instructions should be resolved.</param>
    /// <param name="text">Optional text override; when null the existing block / FormattedText value is kept.</param>
    /// <param name="style">Optional style override; when null the existing style (or defaults) is kept.</param>
    /// <returns>The FormattedText bound to the block.</returns>
    public FormattedText EnsureBlockInstructions(Block block, string? text = null, TextStyle? style = null)
    {
        ArgumentNullException.ThrowIfNull(block);

        FormattedText ft;
        if (block.BlockInstructionsId != Guid.Empty
            && _formattedTextCache.TryGetValue(block.BlockInstructionsId, out var existing))
        {
            ft = existing;
        }
        else
        {
            ft = new FormattedText
            {
                Id = Guid.NewGuid(),
                LayoutItem = IAT.Core.Enumerations.LayoutItem.BlockInstructions,
                Text = text ?? block.BlockInstructions ?? string.Empty,
                Style = style ?? new TextStyle()
            };
            AddFormattedText(ft);
            block.BlockInstructionsId = ft.Id;
        }

        if (text is not null)
            ft.Text = text;

        if (style is not null)
            ft.Style = style;

        // Keep the convenience string on Block aligned with the FormattedText source of truth.
        block.BlockInstructions = ft.Text ?? string.Empty;
        return ft;
    }

    /// <summary>
    /// Adds a survey to the collection.
    /// </summary>
    /// <param name="survey">The survey to add.</param>
    public void AddSurvey(Survey survey)
    {
        if (survey is null) return;
        if (!Surveys.Contains(survey))
            Surveys.Add(survey);
    }

    /// <summary>
    /// Removes a survey from the collection.
    /// </summary>
    /// <param name="survey">The survey to remove.</param>
    /// <returns>The removed survey, or null if not found.</returns>
    public Survey? RemoveSurvey(Survey survey)
    {
        if (survey is null) return null;
        if (Surveys.Remove(survey))
            return survey;
        return null;
    }

    /// <summary>
    /// Live collection of trials. Prefer <see cref="AddTrial"/> / <see cref="RemoveTrial"/> for mutation.
    /// </summary>
    public ObservableCollection<Trial> TrialsCollection => Trials;

    /// <summary>
    /// Live collection of keys. Prefer <see cref="AddKey"/> / <see cref="RemoveKey"/> for mutation.
    /// </summary>
    public ObservableCollection<Key> KeysCollection => Keys;

    /// <summary>
    /// Live collection of blocks. Prefer <see cref="AddBlock"/> / <see cref="RemoveBlock"/> for mutation.
    /// Shared by Blocks tab and Trials tab so both stay in sync.
    /// </summary>
    public ObservableCollection<Block> BlocksCollection => Blocks;

    /// <summary>
    /// Validates the entire test configuration, including all trials, stimuli, and instruction screens.
    /// </summary>
    /// <remarks>This method performs a comprehensive validation by checking that every trial is
    /// valid, each stimulus is used in a trial, mock-item screen, or survey image and is itself valid, and that at least one instruction
    /// screen is present and valid. Validation stops at the first failure encountered and returns the corresponding
    /// error.</remarks>
    /// <returns>A ValidationResult indicating whether the test configuration is valid. Returns ValidationResult.Success if
    /// all checks pass; otherwise, returns a ValidationResult describing the first validation error encountered.</returns>
    public ValidationResult ValidateEntireTest()
    {
        var result = ValidationResult.Success;
        // 1. Every trial must be valid
        foreach (var trial in Trials)
        {
            var stimulus = GetStimulusById(trial.StimulusId);
            result.Combine(trial.Validate(stimulus));
        }

        // 2. Every stimulus must appear on a trial, mock-item screen, or survey image. Orphans only.
        if (Stimuli.Any(s => !IsStimulusReferenced(s.Id)))
            result.AddError("Every stimulus must be used in at least one trial, mock-item screen, or survey image");
        foreach (var stimulus in Stimuli)
            result.Combine(stimulus.Validate());

        if (InstructionScreens.Count == 0)
            result.AddError("At least one instruction screen is required");
        foreach (var instruction in InstructionScreens)
            result.Combine(instruction.Validate());

        if (Blocks.Count != 7)
            result.AddError("Exactly 7 blocks are required for a standard IAT");

        return result;
    }

    /// <summary>
    /// Helper methods to retrieve entities by ID, which can be used during validation and other operations.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found; otherwise, null.</returns>
    public Stimulus? GetStimulusById(Guid id) => _stimulusCache.TryGetValue(id, out var stimulus) ? stimulus : null;

    /// <summary>
    /// True when <paramref name="stimulusId"/> is referenced by a trial, a
    /// <see cref="MockItemInstructionScreen"/>, or a <see cref="SurveyImage"/>.
    /// A stimulus used only on a questionnaire is not an orphan.
    /// </summary>
    public bool IsStimulusReferenced(Guid stimulusId)
    {
        if (stimulusId == Guid.Empty)
            return false;
        if (Trials.Any(t => t.StimulusId == stimulusId))
            return true;
        if (InstructionScreens.OfType<MockItemInstructionScreen>()
            .Any(screen => screen.StimulusId == stimulusId))
            return true;
        return Surveys
            .SelectMany(survey => survey.Items)
            .OfType<SurveyImage>()
            .Any(image => image.ImageId == stimulusId);
    }

    /// <summary>
    /// Returns the trial with the specified ID, or null if not found. This is useful for validation and other 
    /// operations that need to look up trials by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the trial.</param>
    /// <returns>The trial if found; otherwise, null.</returns>
    public Trial? GetTrialById(Guid id) => _trialCache.TryGetValue(id, out var trial) ? trial : null;

    /// <summary>
    /// Retrieves the instruction screen associated with the specified unique identifier.   
    /// </summary>
    /// <param name="id">The unique identifier of the instruction screen to retrieve.</param>
    /// <returns>The instruction screen corresponding to the specified identifier, or null if no matching instruction screen
    /// is found.</returns>
    public InstructionScreen? GetInstructionScreenById(Guid id) => _instructionCache.TryGetValue(id, out var instruction) ? instruction : null;

    /// <summary>
    /// Retrieves a block with the specified unique identifier, if it exists.
    /// </summary>
    /// <param name="id">The unique identifier of the block to retrieve.</param>
    /// <returns>The block associated with the specified identifier, or null if no such block exists.</returns>
    public Block? GetBlockById(Guid id) => _blockCache.TryGetValue(id, out var block) ? block : null;

    /// <summary>
    /// Retrieves the key associated with the specified identifier, if it exists in the cache.
    /// </summary>
    /// <param name="id">The unique identifier of the key to retrieve.</param>
    /// <returns>The key associated with the specified identifier if found; otherwise, null.</returns>
    public Key? GetKeyById(Guid id) => _keyCache.TryGetValue(id, out var key) ? key : null;

    /// <summary>
    /// Retrieves the formatted text associated with the specified identifier, if it exists.
    /// </summary>
    /// <param name="id">The unique identifier of the formatted text to retrieve.</param>
    /// <returns>The formatted text associated with the specified identifier, or null if 
    /// no such entry exists.</returns>
    public FormattedText? GetFormattedTextById(Guid id) => _formattedTextCache.TryGetValue(id, out var formattedText) ? formattedText : null;

    /// <summary>
    /// Clears and rebuilds all internal caches from their respective source collections.
    /// </summary>
    public void RebuildCaches()
    {
        _stimulusCache.Clear();
        foreach (var stimulus in Stimuli)
            _stimulusCache[stimulus.Id] = stimulus;
        _trialCache.Clear();
        foreach (var trial in Trials)
            _trialCache[trial.Id] = trial;
        _instructionCache.Clear();
        foreach (var instruction in InstructionScreens)
            _instructionCache[instruction.Id] = instruction;
        _blockCache.Clear();
        foreach (var block in Blocks)
            _blockCache[block.Id] = block;
        _keyCache.Clear();
        foreach (var key in Keys)
            _keyCache[key.Id] = key;
        _formattedTextCache.Clear();
        foreach (var ft in FormattedTexts)
            _formattedTextCache[ft.Id] = ft;
    }

    /// <summary>
    /// Adds an instruction screen to the collection and cache.
    /// </summary>
    public void AddInstructionScreen(InstructionScreen screen)
    {
        if (screen is null) return;
        if (!_instructionCache.ContainsKey(screen.Id))
        {
            InstructionScreens.Add(screen);
            _instructionCache[screen.Id] = screen;
        }
    }

    /// <summary>
    /// Removes an instruction screen from the collection and cache, and strips its Id
    /// from every block's <see cref="Block.InstructionsIds"/> list.
    /// </summary>
    /// <param name="screen">The screen to remove.</param>
    /// <returns>The removed screen, or null if it was not present.</returns>
    public InstructionScreen? RemoveInstructionScreen(InstructionScreen screen)
    {
        if (screen is null) return null;
        if (!InstructionScreens.Remove(screen))
            return null;

        _instructionCache.Remove(screen.Id);

        foreach (var block in Blocks)
            block.InstructionsIds.Remove(screen.Id);

        return screen;
    }

    /// <summary>
    /// Resets this test to an empty "New IAT Test" state without replacing the instance.
    /// Child ViewModels that hold a reference to this singleton remain valid; their bound
    /// ObservableCollections raise CollectionChanged as items are removed.
    /// </summary>
    public void Reset()
    {
        Id = Guid.NewGuid();
        Name = "New IAT Test";

        Stimuli.Clear();
        Blocks.Clear();
        Trials.Clear();
        Keys.Clear();
        InstructionScreens.Clear();
        Surveys.Clear();
        FormattedTexts.Clear();

        _stimulusCache.Clear();
        _blockCache.Clear();
        _trialCache.Clear();
        _keyCache.Clear();
        _instructionCache.Clear();
        _formattedTextCache.Clear();

        Layout = new Layout();
    }

    /// <summary>
    /// Replaces the contents of this singleton instance with data from <paramref name="source"/>.
    /// Object identity is preserved so every ViewModel that holds this reference stays valid.
    /// </summary>
    public void ReplaceWith(IatTest source)
    {
        if (source is null || ReferenceEquals(source, this)) return;

        // Clear existing content first (raises CollectionChanged for bound UIs).
        Stimuli.Clear();
        Blocks.Clear();
        Trials.Clear();
        Keys.Clear();
        InstructionScreens.Clear();
        Surveys.Clear();
        FormattedTexts.Clear();

        _stimulusCache.Clear();
        _blockCache.Clear();
        _trialCache.Clear();
        _keyCache.Clear();
        _instructionCache.Clear();
        _formattedTextCache.Clear();

        Id = source.Id;
        Name = source.Name ?? "Untitled";

        Layout = new Layout();
        if (source.Layout is not null)
            Layout.CopyFrom(source.Layout);

        // Order matters for referential integrity: formatted text / stimuli / keys before trials / blocks.
        foreach (var ft in source.AllFormattedTexts)
            AddFormattedText(ft);

        foreach (var stimulus in source.AllStimuli)
            AddStimulus(stimulus);

        foreach (var key in source.AllKeys)
            AddKey(key);

        foreach (var trial in source.AllTrials)
            AddTrial(trial);

        foreach (var block in source.AllBlocks)
            AddBlock(block);

        foreach (var screen in source.AllInstructionScreens)
            AddInstructionScreen(screen);

        foreach (var survey in source.AllSurveys.Count > 0 ? source.AllSurveys : source.Surveys.ToList())
            AddSurvey(survey);
    }


    private readonly Dictionary<Guid, FormattedText> _formattedTextCache = new();
    private readonly Dictionary<Guid, Block> _blockCache = new();
    private readonly Dictionary<Guid, Stimulus> _stimulusCache = new();
    private readonly Dictionary<Guid, Trial> _trialCache = new();
    private readonly Dictionary<Guid, InstructionScreen> _instructionCache = new();
    private readonly Dictionary<Guid, Key> _keyCache = new();
}