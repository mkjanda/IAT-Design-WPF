using IAT.Core.Domain;
using IAT.Core.Enumerations;

namespace xUnitIAT;

/// <summary>
/// Domain validation foundation tests.
/// These lock the core rules that every IAT configuration must satisfy.
/// Prefer domain Validate() / ValidateEntireTest() over FluentValidation wrappers here —
/// the domain methods are what the designer and export path actually call.
/// </summary>
public class IatTestValidationTests
{
    // ── ValidationResult ───────────────────────────────────────────────────

    [Fact]
    public void ValidationResult_Success_IsValidAndEmpty()
    {
        var result = ValidationResult.Success;
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidationResult_Fail_ContainsMessage()
    {
        var result = ValidationResult.Fail("something broke");
        Assert.False(result.IsValid);
        Assert.Contains("something broke", result.Errors);
    }

    [Fact]
    public void ValidationResult_Combine_AccumulatesErrors()
    {
        var a = ValidationResult.Fail("error A");
        var b = ValidationResult.Fail("error B");
        a.Combine(b);
        Assert.False(a.IsValid);
        Assert.Equal(2, a.Errors.Count);
        Assert.Contains("error A", a.Errors);
        Assert.Contains("error B", a.Errors);
    }

    // ── TextStimulus ───────────────────────────────────────────────────────

    [Fact]
    public void TextStimulus_Validate_Fails_WhenTextEmpty()
    {
        var stim = new TextStimulus { Text = "   " };
        var result = stim.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TextStimulus_Validate_Succeeds_WhenTextPresent()
    {
        var stim = new TextStimulus { Text = "Happy" };
        Assert.True(stim.Validate().IsValid);
    }

    // ── Trial ──────────────────────────────────────────────────────────────

    [Fact]
    public void Trial_Validate_Fails_WhenStimulusIdEmpty()
    {
        var trial = new Trial
        {
            StimulusId = Guid.Empty,
            KeyedDirection = KeyedDirection.Left
        };
        var result = trial.Validate(stimulus: null);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("stimulus", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Trial_Validate_Fails_WhenKeyedDirectionNone()
    {
        var stim = new TextStimulus { Id = Guid.NewGuid(), Text = "Flower" };
        var trial = new Trial
        {
            StimulusId = stim.Id,
            KeyedDirection = KeyedDirection.None
        };
        var result = trial.Validate(stim);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("keyed direction", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Trial_Validate_Succeeds_WhenConfigured()
    {
        var stim = new TextStimulus { Id = Guid.NewGuid(), Text = "Flower" };
        var trial = new Trial
        {
            StimulusId = stim.Id,
            KeyedDirection = KeyedDirection.Left
        };
        Assert.True(trial.Validate(stim).IsValid);
    }

    // ── Instruction screens ────────────────────────────────────────────────

    [Fact]
    public void TextInstructionScreen_Validate_Fails_WhenContinueTextEmpty()
    {
        var screen = new TextInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "Welcome",
            ContinueInstructions = new FormattedText { Text = string.Empty }
        };
        var result = screen.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void TextInstructionScreen_Validate_Fails_WhenInstructionTextEmpty()
    {
        var screen = new TextInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = string.Empty
            // ContinueInstructions defaults to non-empty
        };
        var result = screen.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TextInstructionScreen_Validate_Succeeds_WhenConfigured()
    {
        var screen = new TextInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "Sort the items as quickly as you can."
        };
        Assert.True(screen.Validate().IsValid);
    }

    [Fact]
    public void KeyedInstructionScreen_Validate_Fails_WhenResponseKeysMissing()
    {
        var screen = new KeyedInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "Press E or I",
            LeftResponseId = Guid.Empty,
            RightResponseId = Guid.Empty
        };
        var result = screen.Validate();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("LeftResponseId", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.Errors, e => e.Contains("RightResponseId", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void KeyedInstructionScreen_Validate_Succeeds_WhenKeysSet()
    {
        var screen = new KeyedInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "Press E or I",
            LeftResponseId = Guid.NewGuid(),
            RightResponseId = Guid.NewGuid()
        };
        Assert.True(screen.Validate().IsValid);
    }

    // ── Block ──────────────────────────────────────────────────────────────

    [Fact]
    public void Block_Validate_Fails_WhenPresentationsZero()
    {
        var block = new Block
        {
            Id = Guid.NewGuid(),
            NumPresentations = 0,
            LeftResponseId = Guid.NewGuid(),
            RightResponseId = Guid.NewGuid()
        };
        block.InstructionsIds.Add(Guid.NewGuid());
        var result = block.Validate();
        Assert.False(result.IsValid);
    }

    [Fact]
    public void Block_Validate_Succeeds_WhenFullyConfigured()
    {
        var block = new Block
        {
            Id = Guid.NewGuid(),
            Name = "Practice",
            NumPresentations = 20,
            LeftResponseId = Guid.NewGuid(),
            RightResponseId = Guid.NewGuid()
        };
        block.InstructionsIds.Add(Guid.NewGuid());
        Assert.True(block.Validate().IsValid);
    }

    [Fact]
    public void Block_Name_RaisesPropertyChanged()
    {
        var block = new Block { Name = "Old" };
        string? changed = null;
        block.PropertyChanged += (_, e) => changed = e.PropertyName;
        block.Name = "New";
        Assert.Equal("Name", changed);
    }

    // ── ValidateEntireTest ─────────────────────────────────────────────────

    [Fact]
    public void ValidateEntireTest_Fails_WhenNoInstructionScreens()
    {
        var test = new IatTest();
        // 7 empty blocks so the block-count rule does not fire first
        for (var i = 0; i < 7; i++)
            test.AddBlock(new Block { Id = Guid.NewGuid(), Name = $"B{i + 1}" });

        var result = test.ValidateEntireTest();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("instruction screen", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateEntireTest_Fails_WhenBlockCountNotSeven()
    {
        var test = new IatTest();
        test.AddInstructionScreen(new TextInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "Start"
        });
        // Only 1 block
        test.AddBlock(new Block { Id = Guid.NewGuid(), Name = "Only" });

        var result = test.ValidateEntireTest();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("7 blocks", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateEntireTest_Fails_WhenStimulusUnused()
    {
        var test = BuildMinimalSevenBlockTest();
        var unused = new TextStimulus { Id = Guid.NewGuid(), Text = "Unused" };
        test.AddStimulus(unused);
        // No trial references unused

        var result = test.ValidateEntireTest();
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Every stimulus must be used", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void ValidateEntireTest_Succeeds_WhenMinimalValidConfiguration()
    {
        var test = BuildMinimalSevenBlockTest();
        var result = test.ValidateEntireTest();
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>
    /// Builds a minimal configuration that satisfies ValidateEntireTest:
    /// 7 blocks, 1 text stimulus used by 1 trial, 1 text instruction screen.
    /// Blocks themselves are not fully validated by ValidateEntireTest —
    /// only trial / stimulus / instruction / block-count rules apply there.
    /// </summary>
    private static IatTest BuildMinimalSevenBlockTest()
    {
        var test = new IatTest { Name = "Unit Test IAT" };

        var stim = new TextStimulus { Id = Guid.NewGuid(), Text = "Flower" };
        test.AddStimulus(stim);

        var trial = new Trial
        {
            Id = Guid.NewGuid(),
            StimulusId = stim.Id,
            KeyedDirection = KeyedDirection.Left,
            TrialNumber = 1
        };
        test.AddTrial(trial);

        for (var i = 0; i < 7; i++)
        {
            var block = new Block
            {
                Id = Guid.NewGuid(),
                Name = $"Block {i + 1}",
                NumPresentations = 1,
                LeftResponseId = Guid.NewGuid(),
                RightResponseId = Guid.NewGuid()
            };
            block.InstructionsIds.Add(Guid.NewGuid());
            if (i == 0)
                block.TrialIds.Add(trial.Id);
            test.AddBlock(block);
        }

        test.AddInstructionScreen(new TextInstructionScreen
        {
            Id = Guid.NewGuid(),
            Text = "Sort the items."
        });

        return test;
    }
}
