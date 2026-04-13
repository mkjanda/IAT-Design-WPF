using IAT.Core.Domain;
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
    Task SaveProjectAsync(IatTest test, string filePath, CancellationToken ct = default);
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
            throw new ValidationException($"Validation failed: {string.Join(", ", validationResult.Errors)}");
        }

        using var package = Package.Open(filePath, FileMode.Create);
        // TODO: write domain objects as JSON part + images
        await Task.CompletedTask; // placeholder — we'll fill this in the next step
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
        // TODO: read JSON part and reconstruct IatTest
        await Task.CompletedTask;
        return new IatTest();
    }
}