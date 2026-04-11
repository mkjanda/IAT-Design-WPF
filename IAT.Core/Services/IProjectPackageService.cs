using IAT.Core.Domain;
using System.IO;
using System.IO.Packaging;
using System.Text.Json;

namespace IAT.Core.Services;

public interface IProjectPackageService
{
    Task SaveProjectAsync(IatTest test, string filePath, CancellationToken ct = default);
    Task<IatTest> LoadProjectAsync(string filePath, CancellationToken ct = default);
}

public class ProjectPackageService : IProjectPackageService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonPolymorphicStimulusConverter() } // we'll create this next
    };

    public async Task SaveProjectAsync(IatTest test, string filePath, CancellationToken ct)
    {
        test.ValidateEntireTest(); // domain enforces rules before save

        using var package = Package.Open(filePath, FileMode.Create);
        // TODO: write domain objects as JSON part + images
        await Task.CompletedTask; // placeholder — we'll fill this in the next step
    }

    public async Task<IatTest> LoadProjectAsync(string filePath, CancellationToken ct)
    {
        using var package = Package.Open(filePath, FileMode.Open);
        // TODO: read JSON part and reconstruct IatTest
        await Task.CompletedTask;
        return new IatTest();
    }
}