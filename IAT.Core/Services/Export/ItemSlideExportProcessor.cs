
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Extensions;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.Core.Services.Export;

/// <summary>
/// Defines methods for generating and manipulating images from various data sources, such as keys, formatted text, byte
/// arrays, and test data.
/// </summary>
/// <remarks>Implementations of this interface provide functionality to render images for display or further
/// processing, supporting scenarios such as dynamic image generation, text rendering, and image decoding. The returned
/// image types are suitable for use in WPF applications and related imaging workflows.</remarks>
public interface IItemSlideExportProcessor
{
    /// <summary>
    /// Processes the slides for the given export context, generating images for each slide based on the test data and layout
    /// </summary>
    /// <param name="context">The export context containing the test data and layout information.</param>
    void ProcessItemSlides(ExportContext context);
}

/// <summary>
/// Provides services for generating and rendering images and text as bitmap sources for use in WPF applications.
/// </summary>
/// <remarks>The ItemSlideExportProcessor offers methods to render keys, formatted text, and slides as bitmap
/// images, as well as to decode and manipulate image data from byte arrays. It integrates with layout, key, and project
/// package services to ensure accurate rendering based on application-specific data and layout information. All
/// returned BitmapSource instances are suitable for display in WPF user interfaces and are typically frozen for thread
/// safety.</remarks>
public class ItemSlideExportProcessor : IItemSlideExportProcessor
{
    private readonly IImageGenerationService _imageGenerationService;

    /// <summary>
    /// Initializes a new instance of the ItemSlideExportProcessor class with the specified dependencies.
    /// </summary>
    /// <param name="imageGenerationService">The service used to generate and render images. Cannot be null.</param>
    public ItemSlideExportProcessor(IImageGenerationService imageGenerationService)
    {
        _imageGenerationService = imageGenerationService ?? throw new ArgumentNullException(nameof(imageGenerationService));
    }

    /// <summary>
    /// Processes and exports all slides from the test blocks and trials as JPEG images, adding them to the slide
    /// manifest.   
    /// </summary>
    /// <param name="context">The export context containing the test data, layout rectangles, and slide manifest to populate.</param>
    public void ProcessItemSlides(ExportContext context)
    {
        JpegBitmapEncoder encoder = new JpegBitmapEncoder()
        {
            QualityLevel = 90
        };
        foreach (var block in context.Test.AllBlocks)
        {
            foreach (var trial in block.TrialIds.Select(tId => context.Test.GetTrialById(tId)))
            {
                var bmp = _imageGenerationService.RenderSlide(context.Test, block.Id, trial.Id, context.LayoutRects);
                var filename = $"slide_{block.Id}_{trial.Id}.png";
                var memStream = new MemoryStream();
                encoder.Frames.Clear();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(memStream);
                context.SlideManifest.AddFile(new Serializable.ManifestFile()
                {
                    ResourceType = FileResourceType.itemSlide,
                    MimeType = "image/jpeg",
                    Size = memStream.Length,
                    Content = memStream.ToArray()
                });
            }
        }
    }
}