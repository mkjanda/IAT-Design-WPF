using IAT.Core.Domain;
using IAT.Core.Exceptions;
using System.IO;
using System.IO.Packaging;
using System.Text.Json;

namespace IAT.Core.Services;

/// <summary>
/// Defines methods for saving and loading project packages asynchronously.
/// </summary>
/// <remarks>Implementations of this interface provide functionality to persist and retrieve project data,
/// typically to and from a file. Methods are asynchronous and support cancellation via a cancellation token.</remarks>
public interface IProjectPackageService
{
    /// <summary>
    /// Saves the specified IAT test project to a file at the given path asynchronously.        
    /// </summary>
    /// <param name="test">The IAT test project to save.</param>
    /// <param name="filePath">The file path where the test will be saved.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveProjectAsync(IatTest test, string filePath, CancellationToken ct = default);
    
    /// <summary>
    /// Loads an IAT test project from the specified file path asynchronously.
    /// </summary>
    /// <param name="filePath">The file path from which to load the test project.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the load operation.</param>
    /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded IAT test project.</returns>    
    Task<IatTest> LoadProjectAsync(string filePath, CancellationToken ct = default);
}

/// <summary>
/// Provides functionality to save and load IAT test projects as package files.
/// </summary>
/// <remarks>This service serializes and deserializes IAT test data, including associated resources, to and from a
/// file-based package format. It is intended for use in applications that require persistent storage and retrieval of
/// IAT test projects.</remarks>
public class ProjectPackageService : IProjectPackageService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonPolymorphicStimulusConverter() } // we'll create this next
    };
    private readonly IImagePackageService _imagePackageService;

    /// <summary>
    /// Initializes a new instance of the ProjectPackageService class with the specified image package service.
    /// </summary>
    /// <param name="imagePackageService">The image package service to be used by this instance. Cannot be null.</param>
    public ProjectPackageService(IImagePackageService imagePackageService) => _imagePackageService = imagePackageService;

    /// <summary>
    /// Asynchronously saves the specified IAT test to a file at the given path.
    /// </summary>
    /// <remarks>The method validates the test before saving. The file is created or overwritten at the
    /// specified path. The operation can be cancelled via the provided cancellation token.</remarks>
    /// <param name="test">The IAT test to be saved. Must be a fully validated test instance.</param>
    /// <param name="filePath">The file path where the test will be saved. If the file exists, it will be overwritten.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the save operation.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    public async Task SaveProjectAsync(IatTest test, string filePath, CancellationToken ct)
    {
        var validationResult = test.ValidateEntireTest();
        if (!validationResult.IsValid)
        {
            throw new ValidationException($"Validation failed: {string.Join("|", validationResult.Errors)}");
        }

        using var package = Package.Open(filePath, FileMode.Create);

        // Create and serialize the main test JSON part
        Uri testPartUri = PackUriHelper.CreatePartUri(new Uri("test.json", UriKind.Relative));
        PackagePart testPart = package.CreatePart(testPartUri, "application/json");
        using (Stream testStream = testPart.GetStream())
        {
            await JsonSerializer.SerializeAsync(testStream, test, _jsonOptions, ct);
        }

        // Import stimuli (images and text) into separate parts
        foreach (var block in test.Blocks)
        {
            foreach (var trialId in block.TrialIds)
            {
                var trial = test.GetTrialById(trialId) ?? throw new InvalidOperationException($"Trial with ID {trialId} not found in test.");
                if (test.GetStimulusById(trial.StimulusId) is ImageStimulus imageStimulus)
                {
                    // Assume sourceFilePath is available (e.g., from stimulus.FileName or a lookup)
                    string sourceFilePath = GetSourceFilePath(imageStimulus);
                    await _imagePackageService.ImportImageStimulusAsync(imageStimulus, sourceFilePath, package, ct);
                }
                else if (test.GetStimulusById(trial.StimulusId) is TextStimulus textStimulus)
                {
                    await _imagePackageService.ImportTextStimulusAsync(textStimulus, package, ct);
                }
            }
        }
    }

    /// <summary>
    /// Asynchronously loads an IAT test project from the specified file path.
    /// </summary>
    /// <param name="filePath">The path to the project file to load. The file must exist and be accessible.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous load operation. The task result contains the loaded IAT test project.</returns>
    public async Task<IatTest> LoadProjectAsync(string filePath, CancellationToken ct)
    {
        using var package = Package.Open(filePath, FileMode.Open);

        // Get the main test JSON part
        Uri testPartUri = PackUriHelper.CreatePartUri(new Uri("test.json", UriKind.Relative));
        if (!package.PartExists(testPartUri))
        {
            throw new FileNotFoundException("Test data not found in package.");
        }

        PackagePart testPart = package.GetPart(testPartUri);
        using Stream testStream = testPart.GetStream();

        // Deserialize the IatTest from JSON
        IatTest? test = await JsonSerializer.DeserializeAsync<IatTest>(testStream, _jsonOptions, ct);
        if (test == null)
        {
            throw new JsonException("Failed to deserialize IatTest from package.");
        }

        return test;
    }

    private string GetSourceFilePath(ImageStimulus stimulus)
    {
        return stimulus.FileName;
    }
}