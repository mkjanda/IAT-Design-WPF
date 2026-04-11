using IAT.Core.Domain;
using System.IO.Packaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.Core.Services;

public interface IImagePackageService
{
    Task ImportTextStimulusAsync(TextStimulus stimulus, Package package, CancellationToken ct);
    Task ImportImageStimulusAsync(ImageStimulus stimulus, string sourceFilePath, Package package, CancellationToken ct);
    Task<BitmapSource> GetResizedBitmapAsync(Stimulus stimulus, int targetWidth, int targetHeight, Package package, CancellationToken ct);
}

public class ImagePackageService : IImagePackageService
{
    private readonly IBitmapCache _cache; // your old 55 MB optimization lives here

    public ImagePackageService(IBitmapCache cache) => _cache = cache;

    // Implement the three methods exactly as we sketched earlier — I’ll give you the full code once the domain push is final
}