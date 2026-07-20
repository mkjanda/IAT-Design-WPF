using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

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
    /// Gets or sets the name of the IAT test.
    /// </summary>
    public string Name { get; set; } = "New IAT Test";

    /// <summary>
    /// Represents the layout of the test, including positions and sizes of various UI elements. This property is initialized with a default layout configuration.
    /// </summary>
    public Layout Layout { get; set; } = new Layout();

    /// <summary>
    /// A read-only collection of all trials in the test. This property provides a snapshot of the current trials, allowing clients to access the trial data without
    /// directly modifying the underlying collection.
    /// </summary>
    public IReadOnlyList<Trial> AllTrials => Trials.ToList().AsReadOnly();

    /// <summary>
    /// A read-only list of block objects representing the structure of the test. Each block contains a 
    /// collection of trials, and this property provides a way to access all blocks without allowing direct 
    /// modification of the underlying collection.
    /// </summary>
    public IReadOnlyList<Block> AllBlocks => Blocks.ToList().AsReadOnly();

    /// <summary>
    /// Gets a read-only list of all stimuli.
    /// </summary>
    public IReadOnlyList<Stimulus> AllStimuli => Stimuli.ToList().AsReadOnly();

    /// <summary>
    /// Gets a read-only list of all instruction screens.
    /// </summary>
    public IReadOnlyList<InstructionScreen> AllInstructionScreens => InstructionScreens.ToList().AsReadOnly();

    /// <summary>
    /// Gets a read-only list of all keys used in the test. This collection provides access to the key 
    /// configurations without allowing direct modifications, ensuring that any changes to the keys are 
    /// managed through controlled methods or properties.
    /// </summary>
    public IReadOnlyList<Key> AllKeys => Keys.ToList().AsReadOnly();

    /// <summary>
    /// Gets the collection of trials associated with this instance.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as
    /// additions or removals of trials. Modifying the collection will not automatically persist changes unless
    /// explicitly handled elsewhere.</remarks>
    private ObservableCollection<Trial> Trials { get; } = new();

    /// <summary>
    /// Gets the collection of blocks contained in the document.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as
    /// additions or removals of blocks. Modifying the collection will update the document's structure
    /// accordingly.</remarks>
    private ObservableCollection<Block> Blocks { get; } = new();

    /// <summary>
    /// Gets the collection of stimuli associated with this instance.
    /// </summary>
    /// <remarks>The returned collection is observable, allowing clients to monitor changes such as
    /// additions or removals of stimuli. Modifications to the collection will be reflected in any data bindings or
    /// observers. Prefer using AddStimulus / UpdateStimulus / RemoveStimulus for proper cache maintenance.
    /// Direct binding to this collection is supported for UI list views (e.g. StimuliManager ListBox).</remarks>
    public ObservableCollection<Stimulus> Stimuli { get; } = new();

    /// <summary>
    /// Gets the collection of instruction screens displayed to the user.
    /// </summary>
    private ObservableCollection<InstructionScreen> InstructionScreens { get; } = new();

    /// <summary>
    /// Gets the collection of keys managed by this instance.
    /// </summary>
    /// <remarks>The returned collection is observable. Changes to the collection, such as adding or removing
    /// keys, will raise collection change notifications. This property never returns null.</remarks>
    private ObservableCollection<Key> Keys { get; } = new();


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

    public void UpdateStimulus(Stimulus stim)
    {
        if (stim is null) return;
        var existing = Stimuli.FirstOrDefault(s => s.Id == stim.Id);
        if (existing is not null)
        {
            _stimulusCache[stim.Id] = stim;
            Stimuli.Remove(existing);
            Stimuli.Add(stim);
            stim.IatTest = this;
        }
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
    /// Removes a block from the collection and cache.
    /// </summary>
    /// <param name="block">The block to remove.</param>
    /// <returns>The removed block.</returns>
    public Block RemoveBlock(Block block)
    {
        Blocks.Remove(block);
        _blockCache.Remove(block.Id);
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
    /// valid, each stimulus is used in at least one trial and is itself valid, and that at least one instruction
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

        // 2. Stimulus reuse across blocks with different keying is allowed — but every stimulus must appear in at least one trial
        if (!Stimuli.Any(s => Trials.Any(t => t.StimulusId == s.Id)))
            result.AddError("Every stimulus must be used in at least one trial");
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

        // Order matters for referential integrity: stimuli/keys before trials/blocks.
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
    }

    private readonly Dictionary<Guid, FormattedText> _formattedTextCache = new();
    private readonly Dictionary<Guid, Block> _blockCache = new();
    private readonly Dictionary<Guid, Stimulus> _stimulusCache = new();
    private readonly Dictionary<Guid, Trial> _trialCache = new();
    private readonly Dictionary<Guid, InstructionScreen> _instructionCache = new();
    private readonly Dictionary<Guid, Key> _keyCache = new();
}