using com.sun.tools.corba.se.idl;
using IAT.Core.Domain;
using IAT.Core.Exceptions;
using System.IO;
using System.IO.Packaging;
using System.Text.Json;
using System.Windows.Media.Imaging;

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

    /// <summary>
    /// Adds an image to the cache and returns a unique identifier for the stored image. This method is asynchronous and allows for cancellation via a cancellation token.
    /// </summary>
    /// <param name="imageData">The byte array containing the image data to be added to the cache.</param>
    /// <param name="originalFileName">The original file name of the image being added.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier for the stored image.</returns>
    Task<Guid> AddImageAsync(byte[] imageData, string originalFileName);

    /// <summary>
    /// Retrieves the byte array of an image from the cache using its unique identifier.
    /// </summary>
    /// <param name="stimulusId">The unique identifier of the image to retrieve.</param>
    /// <returns>A read-only memory region containing the image data.</returns>
    ReadOnlyMemory<byte> GetImageBytes(Guid stimulusId);

    /// <summary>
    /// Gets the image type associated with the specified stimulus identifier.
    /// </summary>
    /// <param name="stimulusId">The unique identifier of the stimulus for which to retrieve the image type.</param>
    /// <returns>A string representing the image type of the specified stimulus. Returns an empty string if the stimulus does not
    /// have an associated image type.</returns>
    string GetImageType(Guid stimulusId);

    /// <summary>
    /// Removes the image associated with the specified stimulus identifier.
    /// </summary>
    /// <param name="stimulusId">The unique identifier of the stimulus whose image is to be removed.</param>
    void RemoveImage(Guid stimulusId);
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
    private readonly Dictionary<Guid, ReadOnlyMemory<byte>> _imageCache = new();
    private readonly Dictionary<Guid, string> _imageTypes = new();
    private readonly IImagePackageService _imagePackageService;
    private readonly Dictionary<Guid, string> _originalNames = new();   

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

    /// <summary>
    /// Asynchronously adds an image to the cache and returns a unique identifier for the stored image.
    /// </summary>
    /// <param name="imageData">The binary data of the image to add. Cannot be null or empty.</param>
    /// <param name="originalFileName">The original file name of the image, including its extension. Used to determine the image type.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier assigned to
    /// the stored image.</returns>
    /// <exception cref="ArgumentException">Thrown if imageData is null or empty.</exception>
    public Task<Guid> AddImageAsync(byte[] imageData, string originalFileName)
    {
        if (imageData == null || imageData.Length == 0)
        {
            throw new ArgumentException("Image data cannot be null or empty.", nameof(imageData));
        }
        var imageId = Guid.NewGuid();
        _imageCache[imageId] = imageData; _imageTypes[imageId] = originalFileName.Reverse().TakeWhile(ch => ch != '.').Reverse().ToString()?.ToLowerInvariant() ?? string.Empty;
        _originalNames[imageId] = originalFileName;
        return Task.FromResult(imageId);
    }


    /// <summary>
    /// Retrieves the image data associated with the specified stimulus identifier.
    /// </summary>
    /// <param name="stimulusId">The unique identifier of the stimulus whose image data is to be retrieved.</param>
    /// <returns>A read-only memory region containing the image bytes for the specified stimulus. Returns an empty memory region
    /// if no image is found for the given identifier.</returns>
    public ReadOnlyMemory<byte> GetImageBytes(Guid stimulusId) => _imageCache.GetValueOrDefault(stimulusId);

    /// <summary>
    /// Retrieves the image type associated with the specified stimulus identifier.
    /// </summary>
    /// <param name="stimulusId">The unique identifier of the stimulus for which to retrieve the image type.</param>
    /// <returns>A string representing the image type associated with the specified stimulus identifier, or null if no image type
    /// is found.</returns>
    public string GetImageType(Guid stimulusId) => _imageTypes.GetValueOrDefault(stimulusId) ?? string.Empty;


    /// <summary>
    /// Removes the image data associated with the specified stimulus ID from the cache. This is useful for managing memory and ensuring that 
    /// outdated or unused image data does not consume resources unnecessarily.
    /// </summary>
    /// <param name="stimulusId">The ID of the stimulus whose image data should be removed from the cache.</param>
    public void RemoveImage(Guid stimulusId)
    {
        if (_imageCache.ContainsKey(stimulusId))
        {
            _imageCache.Remove(stimulusId);
            _imageTypes.Remove(stimulusId);
            _originalNames.Remove(stimulusId);
        }
    }

 
    private string GetSourceFilePath(ImageStimulus stimulus)
    {
        return stimulus.FileName;
    }
}