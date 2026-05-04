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
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Represents the layout of the test, including positions and sizes of various UI elements. This property is initialized with a default layout configuration.
    /// </summary>
    public LayoutConfiguration Layout { get; set; } = new LayoutConfiguration();
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
    /// observers.</remarks>
    public ObservableCollection<Stimulus> Stimuli { get; } = new();

    /// <summary>
    /// Gets the collection of instruction screens displayed to the user.
    /// </summary>
    public ObservableCollection<InstructionsScreen> InstructionScreens { get; } = new();

    /// <summary>
    /// Gets the collection of keys managed by this instance.
    /// </summary>
    /// <remarks>The returned collection is observable. Changes to the collection, such as adding or removing
    /// keys, will raise collection change notifications. This property never returns null.</remarks>
    public ObservableCollection<Key> Keys { get; } = new();


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
    /// Returns the trial with the specified ID, or null if not found. This is useful for validation and other operations that need to look up trials by their unique identifier.
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
    public InstructionsScreen? GetInstructionScreenById(Guid id) => _instructionCache.TryGetValue(id, out var instruction) ? instruction : null;

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



    private readonly Dictionary<Guid, Block> _blockCache = new();
    private readonly Dictionary<Guid, Stimulus> _stimulusCache = new();
    private readonly Dictionary<Guid, Trial> _trialCache = new();
    private readonly Dictionary<Guid, InstructionsScreen> _instructionCache = new();
    private readonly Dictionary<Guid, Key> _keyCache = new();
}