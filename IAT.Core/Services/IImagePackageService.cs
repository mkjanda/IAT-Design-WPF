using IAT.Core.Domain;
using IAT.Core.Exceptions;
using System.IO;
using System.IO.Packaging;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.Core.Services;

/// <summary>
/// Defines a service for handling image stimuli within a package, including importing images and retrieving resized bitmaps for display.
/// </summary>
public interface IImagePackageService
{
    /// <summary>
    /// Imports the specified text stimulus into the given package asynchronously.
    /// </summary>
    /// <param name="stimulus">The text stimulus to import. Cannot be null.</param>
    /// <param name="package">The package into which the stimulus will be imported. Cannot be null.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the import operation.</param>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    Task ImportTextStimulusAsync(TextStimulus stimulus, Package package, CancellationToken ct);

    /// <summary>
    /// Imports an image stimulus into the specified package asynchronously.
    /// </summary>
    /// <param name="stimulus">The image stimulus to be imported. Cannot be null.</param>
    /// <param name="sourceFilePath">The full file path to the source image to import. Must refer to an existing file.</param>
    /// <param name="package">The package into which the image stimulus will be imported. Cannot be null.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the import operation.</param>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    Task ImportImageStimulusAsync(ImageStimulus stimulus, string sourceFilePath, Package package, CancellationToken ct);

    /// <summary>
    /// Asynchronously retrieves a resized bitmap image for the specified stimulus using the given dimensions and
    /// package context.
    /// </summary>
    /// <param name="stimulus">The stimulus for which to generate the resized bitmap. Cannot be null.</param>
    /// <param name="targetWidth">The desired width, in pixels, of the resulting bitmap. Must be greater than zero.</param>
    /// <param name="targetHeight">The desired height, in pixels, of the resulting bitmap. Must be greater than zero.</param>
    /// <param name="package">The package context used to resolve resources associated with the stimulus. Cannot be null.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a bitmap image resized to the
    /// specified dimensions.</returns>
    Task<BitmapSource> GetResizedBitmapAsync(Stimulus stimulus, int targetWidth, int targetHeight, Package package, CancellationToken ct);
}

/// <summary>
/// Provides services for importing and managing image and text stimuli within a package, including support for image
/// resizing and package integration.
/// </summary>
/// <remarks>This service is intended for use with packages that store stimuli as part of a larger test or
/// assessment. It supports importing both text and image stimuli, handling validation, and managing the storage of
/// image data within the package. Methods in this service may throw exceptions if validation fails or if required
/// stimulus properties are not set. Thread safety is not guaranteed; callers should ensure appropriate synchronization
/// if accessing the service from multiple threads.</remarks>
public class ImagePackageService : IImagePackageService
{
    /// <summary>
    /// Initializes a new instance of the ImagePackageService class.
    /// </summary>
    public ImagePackageService() { }

    /// <summary>
    /// Asynchronously imports a text stimulus into the specified package after validating its contents.
    /// </summary>
    /// <remarks>The text stimulus is embedded directly into the package's main JSON representation during
    /// serialization. No additional stream manipulation is required.</remarks>
    /// <param name="stimulus">The text stimulus to import. Must be valid according to the stimulus validation rules.</param>
    /// <param name="package">The package into which the text stimulus will be imported.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the import operation.</param>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    /// <exception cref="ValidationException">Thrown if the stimulus is invalid according to its validation rules.</exception>
    public async Task ImportTextStimulusAsync(TextStimulus stimulus, Package package, CancellationToken ct)
    {
        // Validate the stimulus (optional, but good practice)
        var validationResult = stimulus.Validate();
        if (!validationResult.IsValid)
        {
            throw new ValidationException("Invalid TextStimulus: " + String.Join("|", validationResult.Errors));
        }

        // No stream manipulation needed—text is embedded in the main IatTest JSON during serialization
        await Task.CompletedTask; // Placeholder for any async work (e.g., logging)
    }

    /// <summary>
    /// Imports an image stimulus into the specified package by copying the image file and associating it with the
    /// stimulus.
    /// </summary>
    /// <remarks>The method determines the image content type based on the file extension and stores the image
    /// in the package under a URI derived from the stimulus's image ID. After import, the stimulus's PackageUri
    /// property is set to reference the new image part.</remarks>
    /// <param name="stimulus">The image stimulus to import. Must be valid according to its validation rules.</param>
    /// <param name="sourceFilePath">The full file path of the source image to import. The file must exist and be accessible.</param>
    /// <param name="package">The package into which the image stimulus will be imported. The image will be added as a new part in this
    /// package.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the import operation.</param>
    /// <returns>A task that represents the asynchronous import operation.</returns>
    /// <exception cref="ValidationException">Thrown if the specified stimulus is not valid.</exception>
    public async Task ImportImageStimulusAsync(ImageStimulus stimulus, string sourceFilePath, Package package, CancellationToken ct)
    {
        // Validate the stimulus
        var validationResult = stimulus.Validate();
        if (!validationResult.IsValid)
        {
            throw new ValidationException($"Invalid ImageStimulus: " + String.Join("|", validationResult.Errors));
        }

        // Create a URI for the image part (e.g., /images/{ImageId}.png)
        string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
        string contentType = extension switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            _ => "image/png" // Default fallback
        };
        Uri imagePartUri = PackUriHelper.CreatePartUri(new Uri($"images/{stimulus.ImageId}{extension}", UriKind.Relative));

        // Create the package part and copy the file
        PackagePart imagePart = package.CreatePart(imagePartUri, contentType);
        using (Stream sourceStream = File.OpenRead(sourceFilePath))
        using (Stream partStream = imagePart.GetStream())
        {
            await sourceStream.CopyToAsync(partStream, ct);
        }

        // Optionally, set a PackageUri on the stimulus for reference (add this property to ImageStimulus if needed)
        stimulus.PackageUri = imagePartUri;
    }

    /// <summary>
    /// Asynchronously loads an image from the specified package and returns a bitmap resized to the given dimensions.
    /// </summary>
    /// <remarks>The returned BitmapSource is frozen for thread safety. If the original image dimensions match
    /// the requested size, the image is returned without resizing.</remarks>
    /// <param name="stimulus">The stimulus representing the image to load. Must be of type ImageStimulus.</param>
    /// <param name="targetWidth">The desired width, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="targetHeight">The desired height, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="package">The package containing the image resource to be loaded.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A BitmapSource containing the image resized to the specified width and height.</returns>
    /// <exception cref="ArgumentException">Thrown if stimulus is not of type ImageStimulus.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the PackageUri property is not set on the ImageStimulus.</exception>
    public async Task<BitmapSource> GetResizedBitmapAsync(Stimulus stimulus, int targetWidth, int targetHeight, Package package, CancellationToken ct)
    {
        if (stimulus is not ImageStimulus imageStimulus)
        {
            throw new ArgumentException("Stimulus must be an ImageStimulus", nameof(stimulus));
        }

        // Assume PackageUri is set on ImageStimulus (e.g., during import)
        // If not, construct it: Uri imagePartUri = PackUriHelper.CreatePartUri(new Uri($"images/{imageStimulus.ImageId}.png", UriKind.Relative));
        if (imageStimulus.PackageUri == null)
        {
            throw new InvalidOperationException("PackageUri not set on ImageStimulus");
        }

        PackagePart imagePart = package.GetPart(imageStimulus.PackageUri);
        using Stream partStream = imagePart.GetStream();

        // Load the bitmap
        BitmapImage bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = partStream;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze(); // Make it thread-safe

        // Resize if needed
        BitmapSource resizedBitmap;
        if (bitmap.PixelWidth == targetWidth && bitmap.PixelHeight == targetHeight)
        {
            resizedBitmap = bitmap;
        }
        else
        {
            double scaleX = (double)targetWidth / bitmap.PixelWidth;
            double scaleY = (double)targetHeight / bitmap.PixelHeight;
            ScaleTransform scaleTransform = new ScaleTransform(scaleX, scaleY);
            resizedBitmap = new TransformedBitmap(bitmap, scaleTransform);
            resizedBitmap.Freeze();
        }

        return resizedBitmap;
    }
}