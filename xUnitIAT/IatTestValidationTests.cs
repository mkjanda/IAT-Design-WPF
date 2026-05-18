using IAT.Core.Domain;
using Xunit;

namespace xUnitIAT;


public class IatTestValidationTests
{
    [Fact]
    public void ValidateEntireTest_AccumulatesErrors_WhenInvalidData()
    {
        // Arrange: Build sample IatTest with invalid data
        var test = new IatTest();
        var block = new Block(); // Assume Block has properties like Id, Trials
        block.Id = Guid.NewGuid();  // Valid
        test.AddBlock(block);

        var trial = new Trial();
        trial.Id = Guid.NewGuid();
        trial.StimulusId = Guid.Empty; // Invalid: empty stimulus ID
        block.TrialIds.Add(trial.Id);


        // Act: Validate entire test
        var result = test.ValidateEntireTest(); // Implement this in IatTest to combine sub-results

        // Assert: Check accumulation
        Assert.False(result.IsValid);
        Assert.Contains("Image ID cannot be empty", result.Errors); // Example error
        Assert.Contains("Invalid stimulus ID", result.Errors);  // Another example
    }

    [Fact]
    public void ValidateEntireTest_Succeeds_WhenValidData()
    {
        // Arrange: Build valid IatTest
        var test = new IatTest();
        var block = new Block { Id = Guid.NewGuid() };
        test.AddBlock(block);

        var trial = new Trial { Id = Guid.NewGuid(), StimulusId = Guid.NewGuid() };
        block.TrialIds.Add(trial.Id);

        var stimulus = new ImageStimulus(); 
        test.AddStimulus(stimulus);

        // Act
        var result = test.ValidateEntireTest();

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}

