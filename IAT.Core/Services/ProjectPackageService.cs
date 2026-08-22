using IAT.Core.Domain;
using IAT.Core.Exceptions;
using System.IO;
using System.IO.Packaging;
using System.Text.Json;

namespace IAT.Core.Services;

/// <summary>
/// Saves and loads IAT test projects as OPC packages, keeping image bytes in an in-memory cache
/// keyed by stimulus Id so save never depends on the original disk path still existing.
/// </summary>
public interface IProjectPackageService
{
    Task SaveProjectAsync(IatTest test, string filePath, CancellationToken ct = default);
    Task<IatTest> LoadProjectAsync(string filePath, CancellationToken ct = default);

    /// <summary>
    /// Caches image bytes under a new Id. Prefer <see cref="SetImageBytes"/> when updating an existing stimulus.
    /// </summary>
    Task<Guid> AddImageAsync(byte[] imageData, string originalFileName);

    /// <summary>
    /// Stores or replaces image bytes under an existing stimulus Id (the normal path after the user picks a file).
    /// </summary>
    void SetImageBytes(Guid stimulusId, byte[] imageData, string originalFileName);

    byte[] GetImageBytes(Guid stimulusId);
    string GetImageType(Guid stimulusId);
    void RemoveImage(Guid stimulusId);
}

public class ProjectPackageService : IProjectPackageService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new KeyedDirectionJsonConverter() }
    };

    private readonly Dictionary<Guid, byte[]> _imageCache = new();
    private readonly Dictionary<Guid, string> _imageTypes = new();   // extension without dot, e.g. "png"
    private readonly Dictionary<Guid, string> _originalNames = new(); // leaf file name only
    private readonly IImagePackageService _imagePackageService;

    public ProjectPackageService(IImagePackageService imagePackageService) =>
        _imagePackageService = imagePackageService ?? throw new ArgumentNullException(nameof(imagePackageService));

    /// <inheritdoc />
    public async Task SaveProjectAsync(IatTest test, string filePath, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(test);
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.", nameof(filePath));

        using var package = Package.Open(filePath, FileMode.Create);

        var testPartUri = PackUriHelper.CreatePartUri(new Uri("test.json", UriKind.Relative));
        var testPart = package.CreatePart(testPartUri, "application/json");
        await using (var testStream = testPart.GetStream())
        {
            await JsonSerializer.SerializeAsync(testStream, test, _jsonOptions, ct).ConfigureAwait(false);
        }

        foreach (var stim in test.AllStimuli)
        {
            if (stim is ImageStimulus imageStim)
            {
                await EmbedImageAsync(imageStim, package, ct).ConfigureAwait(false);
            }
            else if (stim is TextStimulus textStim)
            {
                await _imagePackageService.ImportTextStimulusAsync(textStim, package, ct).ConfigureAwait(false);
            }
        }
    }

    /// <summary>
    /// Embeds one image into the package. Resolution order:
    /// 1. In-memory cache (bytes loaded at Add / Load / Change Image) — preferred, no disk access.
    /// 2. Absolute path still on disk (legacy FileName that stored a full path).
    /// Never resolves a leaf name against the process working directory.
    /// </summary>
    private async Task EmbedImageAsync(ImageStimulus imageStim, Package package, CancellationToken ct)
    {
        var cached = GetImageBytes(imageStim.Id);
        if (cached.Length > 0)
        {
            var ext = GetExtensionFor(imageStim);
            await _imagePackageService
                .ImportImageStimulusFromBytesAsync(imageStim, cached, ext, package, ct)
                .ConfigureAwait(false);
            // Normalize domain to leaf name so JSON never carries a machine-local path.
            imageStim.FileName = Path.GetFileName(
                string.IsNullOrWhiteSpace(imageStim.FileName)
                    ? (_originalNames.GetValueOrDefault(imageStim.Id) ?? $"image{ext}")
                    : imageStim.FileName);
            return;
        }

        var diskPath = ResolveExistingAbsolutePath(imageStim);
        if (diskPath is not null)
        {
            await _imagePackageService
                .ImportImageStimulusAsync(imageStim, diskPath, package, ct)
                .ConfigureAwait(false);

            // Cache for subsequent saves in this session and normalize FileName to leaf.
            var bytes = await File.ReadAllBytesAsync(diskPath, ct).ConfigureAwait(false);
            SetImageBytes(imageStim.Id, bytes, diskPath);
            imageStim.FileName = Path.GetFileName(diskPath);
            return;
        }

        throw new FileNotFoundException(
            $"Image data for stimulus '{imageStim.Id}' is not in the package cache and no absolute source file is available. " +
            $"Stored FileName was '{imageStim.FileName}'. Re-add the image or open the original .iat package.",
            imageStim.FileName);
    }

    /// <summary>
    /// Returns an absolute path that still exists on disk, or null.
    /// Leaf names and relative paths are intentionally rejected — they resolve against cwd and are wrong.
    /// </summary>
    private static string? ResolveExistingAbsolutePath(ImageStimulus imageStim)
    {
        var candidate = imageStim.FileName;
        if (string.IsNullOrWhiteSpace(candidate))
            return null;

        if (Path.IsPathRooted(candidate) && File.Exists(candidate))
            return candidate;

        return null;
    }

    private string GetExtensionFor(ImageStimulus imageStim)
    {
        var type = GetImageType(imageStim.Id);
        if (!string.IsNullOrWhiteSpace(type))
            return type.StartsWith('.') ? type : "." + type;

        var fromName = Path.GetExtension(imageStim.FileName);
        if (!string.IsNullOrWhiteSpace(fromName))
            return fromName;

        return ".png";
    }

    /// <inheritdoc />
    public async Task<IatTest> LoadProjectAsync(string filePath, CancellationToken ct = default)
    {
        using var package = Package.Open(filePath, FileMode.Open);

        var testPartUri = PackUriHelper.CreatePartUri(new Uri("test.json", UriKind.Relative));
        if (!package.PartExists(testPartUri))
            throw new FileNotFoundException("Test data not found in package.");

        await using var testStream = package.GetPart(testPartUri).GetStream();
        var test = await JsonSerializer.DeserializeAsync<IatTest>(testStream, _jsonOptions, ct).ConfigureAwait(false)
                   ?? throw new JsonException("Failed to deserialize IatTest.");

        // Populate image cache while the package is still open so subsequent Save works offline.
        foreach (var stim in test.Stimuli.OfType<ImageStimulus>())
        {
            foreach (var ext in new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" })
            {
                var uri = PackUriHelper.CreatePartUri(new Uri($"images/{stim.Id}{ext}", UriKind.Relative));
                if (!package.PartExists(uri)) continue;

                await using var partStream = package.GetPart(uri).GetStream();
                using var ms = new MemoryStream();
                await partStream.CopyToAsync(ms, ct).ConfigureAwait(false);
                var bytes = ms.ToArray();
                _imageCache[stim.Id] = bytes;
                _imageTypes[stim.Id] = ext.TrimStart('.');
                _originalNames[stim.Id] = string.IsNullOrWhiteSpace(stim.FileName)
                    ? $"image{ext}"
                    : Path.GetFileName(stim.FileName);
                stim.PackageUri = uri;
                // Never keep a machine-local full path in the domain after load.
                stim.FileName = Path.GetFileName(stim.FileName);
                break;
            }
        }

        test.RebuildCaches();

        foreach (var stim in test.Stimuli)
            stim.IatTest = test;
        foreach (var block in test.Blocks)
            block.IatTest = test;

        return test;
    }

    /// <inheritdoc />
    public Task<Guid> AddImageAsync(byte[] imageData, string originalFileName)
    {
        if (imageData is null || imageData.Length == 0)
            throw new ArgumentException("Image data cannot be null or empty.", nameof(imageData));

        var imageId = Guid.NewGuid();
        SetImageBytes(imageId, imageData, originalFileName);
        return Task.FromResult(imageId);
    }

    /// <inheritdoc />
    public void SetImageBytes(Guid stimulusId, byte[] imageData, string originalFileName)
    {
        if (imageData is null || imageData.Length == 0)
            throw new ArgumentException("Image data cannot be null or empty.", nameof(imageData));

        var leaf = string.IsNullOrWhiteSpace(originalFileName)
            ? "image.png"
            : Path.GetFileName(originalFileName);

        var ext = Path.GetExtension(leaf);
        if (string.IsNullOrWhiteSpace(ext))
            ext = ".png";

        _imageCache[stimulusId] = imageData;
        _imageTypes[stimulusId] = ext.TrimStart('.').ToLowerInvariant();
        _originalNames[stimulusId] = leaf;
    }

    /// <inheritdoc />
    public byte[] GetImageBytes(Guid stimulusId) =>
        _imageCache.GetValueOrDefault(stimulusId) ?? Array.Empty<byte>();

    /// <inheritdoc />
    public string GetImageType(Guid stimulusId) =>
        _imageTypes.GetValueOrDefault(stimulusId) ?? string.Empty;

    /// <inheritdoc />
    public void RemoveImage(Guid stimulusId)
    {
        _imageCache.Remove(stimulusId);
        _imageTypes.Remove(stimulusId);
        _originalNames.Remove(stimulusId);
    }
}
