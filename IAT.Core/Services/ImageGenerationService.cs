using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.Core.Services;

/// <summary>
/// Defines methods for generating and manipulating images from various data sources, such as keys, formatted text, byte
/// arrays, and test data.
/// </summary>
/// <remarks>Implementations of this interface provide functionality to render images for display or further
/// processing, supporting scenarios such as dynamic image generation, text rendering, and image decoding. The returned
/// image types are suitable for use in WPF applications and related imaging workflows.</remarks>
public interface IImageGenerationService
{
    /// <summary>
    /// Renders the visual representation of the specified key as a bitmap image.
    /// </summary>
    /// <param name="keyId">The unique identifier of the key to render. This value must correspond to a valid key in the system.</param>
    /// <returns>A BitmapSource containing the rendered image of the key. Returns null if the key does not exist or cannot be
    /// rendered.</returns>
    BitmapSource RenderKeyToBitmap(Guid keyId);

    /// <summary>
    /// Renders the specified formatted text as a bitmap image.
    /// </summary>
    /// <remarks>The returned bitmap reflects the formatting and layout specified in the input text. The
    /// caller is responsible for managing the lifetime of the resulting BitmapSource as appropriate for their
    /// application.</remarks>
    /// <param name="text"></param>
    /// <returns>A BitmapSource containing the rendered text as an image.</returns>
    BitmapSource RenderTextToBitmap(IFormattedText text);
    /// <summary>
    /// Creates a BitmapSource from the specified byte array containing image data.
    /// </summary>
    /// <remarks>The method supports common bitmap formats such as PNG, JPEG, and BMP. The caller is
    /// responsible for ensuring that the input data is in a supported format.</remarks>
    /// <param name="bytes">A read-only memory region containing the image data in a supported bitmap format. Cannot be empty.</param>
    /// <returns>A BitmapSource representing the decoded image. Returns null if the byte array does not contain valid image data.</returns>
    BitmapSource BitmapFromBytes(ReadOnlyMemory<byte> bytes);
    /// <summary>
    /// Loads the specified encoded image bytes as a manipulable WriteableBitmap.
    /// </summary>
    /// <param name="encodedBytes">A byte array containing the encoded image data. Cannot be null or empty.</param>
    /// <returns>A WriteableBitmap that can be manipulated. Returns null if the byte array does not contain valid image data.</returns>
    WriteableBitmap LoadEncodedBytesAsManipulableImage(byte[] encodedBytes);
    /// <summary>
    /// Renders a slide from the specified test, block, and trial identifiers.
    /// </summary>
    /// <param name="test">The IatTest instance containing the slide data. Cannot be null.</param>
    /// <param name="blockId">The unique identifier of the block containing the slide. Must correspond to a valid block in the test.</param>
    /// <param name="trialId">The unique identifier of the trial containing the slide. Must correspond to a valid trial in the block.</param>
    /// <returns>A BitmapSource representing the rendered slide. Returns null if the slide cannot be rendered.</returns>
    BitmapSource RenderSlide(IatTest test, Guid blockId, Guid trialId);

    /// <summary>
    /// Renders an outline image of the key and returns it as a bitmap source.
    /// </summary>
    /// <returns>A <see cref="BitmapSource"/> representing the rendered outline of the key.</returns>
    BitmapSource RenderKeyOutline();

    /// <summary>
    /// Returns a new bitmap that is a resized version of the specified source bitmap, using the given target width and
    /// height.
    /// </summary>
    /// <remarks>The returned bitmap is independent of the original source. If the target dimensions do not
    /// match the source aspect ratio, the image may appear stretched or compressed.</remarks>
    /// <param name="bmpSource">The source bitmap to resize. Cannot be null.</param>
    /// <param name="targetWidth">The desired width, in pixels, of the resulting bitmap. Must be greater than zero.</param>
    /// <param name="targetHeight">The desired height, in pixels, of the resulting bitmap. Must be greater than zero.</param>
    /// <returns>A new BitmapSource instance representing the resized bitmap. The aspect ratio may not be preserved if the target
    /// dimensions differ from the source.</returns>
    BitmapSource GetResizedBitmap(BitmapSource bmpSource, int targetWidth, int targetHeight);

}

/// <summary>
/// Provides services for generating and rendering images and text as bitmap sources for use in WPF applications.
/// </summary>
/// <remarks>The ImageGenerationService offers methods to render keys, formatted text, and slides as bitmap
/// images, as well as to decode and manipulate image data from byte arrays. It integrates with layout, key, and project
/// package services to ensure accurate rendering based on application-specific data and layout information. All
/// returned BitmapSource instances are suitable for display in WPF user interfaces and are typically frozen for thread
/// safety.</remarks>
public class ImageGenerationService : IImageGenerationService
{
    private readonly ILayoutCalculatorService _layout;
    private readonly IatTest _iat;
    private readonly IKeyService _keyService;
    private readonly ProjectPackageService _packageService;

    /// <summary>
    /// Initializes a new instance of the ImageGenerationService class with the specified dependencies.
    /// </summary>
    /// <param name="layoutCalculatorService">The service used to calculate layout information for image generation. Cannot be null.</param>
    /// <param name="iat">The IatTest instance that provides test configuration and data. Cannot be null.</param>
    /// <param name="keyService">The service responsible for key management and related operations. Cannot be null.</param>
    /// <param name="packageService">The service used to manage project packages required for image generation. Cannot be null.</param>
    public ImageGenerationService(ILayoutCalculatorService layoutCalculatorService, IatTest iat, IKeyService keyService, ProjectPackageService packageService)
    {
        _layout = layoutCalculatorService;
        _iat = iat;
        _keyService = keyService;
        _packageService = packageService;
    }

    /// <summary>
    /// Renders the specified formatted text onto a bitmap using the layout information provided.
    /// </summary>
    /// <remarks>The resulting bitmap uses a pixel format of Pbgra32 and a DPI of 96. The text is centered
    /// within the bounds defined by the layout item.</remarks>
    /// <param name="formattedText">The formatted text to render onto the bitmap. Must not be null.</param>
    /// <param name="layoutItem">The layout item that defines the positioning and size for rendering the text.</param>
    /// <returns>A BitmapSource containing the rendered text, sized and positioned according to the layout item.</returns>
    private BitmapSource RenderFormattedTextToBitmap(System.Windows.Media.FormattedText formattedText, LayoutItem layoutItem)
    {
        // Create the target bitmap (size based on measured text)
        var boundingRect = _layout.GetFinalRect(_iat.Layout, layoutItem);
        var width = boundingRect.Width;
        var height = boundingRect.Height;
        var bmp = new RenderTargetBitmap((int)boundingRect.Width, (int)boundingRect.Height, 96, 96, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        { 
            dc.DrawText(formattedText, new Point((int)((width - formattedText.Width) / 2), (int)((height - formattedText.Height) / 2)));  // offset for crisp edges
        }
        bmp.Render(visual);
        return bmp;
    }

    /// <summary>
    /// Renders the visual representation of the specified key as a bitmap image.
    /// </summary>
    /// <remarks>The rendered bitmap uses the key's font, color, and layout information. If the key
    /// text contains multiple words and is marked as combined, the words are rendered on separate lines. The bitmap
    /// is suitable for display in WPF user interfaces.</remarks>
    /// <param name="keyId">The unique identifier of the key to render. Must correspond to a valid key in the key service.</param>
    /// <returns>A BitmapSource containing the rendered image of the key. The bitmap reflects the key's text, font, color,
    /// and layout settings.</returns>
    public BitmapSource RenderKeyToBitmap(Guid keyId)
    {
        var key = _keyService.GetResolvedKey(_iat, keyId);
        var foreground = new SolidColorBrush(key.FontColor);
        var typeface = new Typeface(new FontFamily(key.FontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var formattedText = new System.Windows.Media.FormattedText(
            key.IsCombined ? string.Join("\r\n", key.Text.Split(" ")) : key.Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            key.FontSize * 96.0 / 72.0,   // convert from points to DIPs   
            foreground,
            VisualTreeHelper.GetDpi(new Window()).PixelsPerDip);  // critical for crisp rendering
        return RenderFormattedTextToBitmap(formattedText, key.LayoutItem);
    }

    /// <summary>
    /// Renders the specified formatted text as a bitmap image using the provided text style and layout information.
    /// </summary>
    /// <remarks>The rendered bitmap uses the font family, size, and color specified in the text
    /// style. The method applies the current system DPI settings to ensure crisp text rendering. The caller is
    /// responsible for managing the lifetime of the returned BitmapSource if used outside the method's
    /// scope.</remarks>
    /// <param name="text">An object that defines the text content, style, and layout to be rendered as a bitmap. Cannot be null.</param>
    /// <returns>A BitmapSource containing the rendered text image. The bitmap reflects the specified font, color, and
    /// layout.</returns>
    public BitmapSource RenderTextToBitmap(IFormattedText text)
    {
        var foreground = new SolidColorBrush(text.Style.FontColor);
        var typeface = new Typeface(new FontFamily(text.Style.FontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var formattedText = new System.Windows.Media.FormattedText(
            text.Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            text.Style.FontSize * 96.0 / 72.0,   // convert from points to DIPs   
            foreground,
            VisualTreeHelper.GetDpi(new Window()).PixelsPerDip);  // critical for crisp rendering
        return RenderFormattedTextToBitmap(formattedText, text.LayoutItem);
    }

    /// <summary>
    /// Creates a BitmapSource from the specified read-only byte memory containing image data.
    /// </summary>
    /// <remarks>The method expects the byte data to be in a format supported by BitmapImage, such as
    /// PNG, JPEG, or BMP. The returned BitmapSource is immutable and thread-safe due to being frozen.</remarks>
    /// <param name="bytes">A read-only memory region containing the image data in a supported bitmap format. Cannot be empty.</param>
    /// <returns>A BitmapSource representing the decoded image. The returned BitmapSource is frozen and can be safely shared
    /// across threads.</returns>
    public BitmapSource BitmapFromBytes(ReadOnlyMemory<byte> bytes)
    {
        using var stream = new MemoryStream(bytes.ToArray(), false);  
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.StreamSource = stream;
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;   
        bitmapImage.EndInit();   
        bitmapImage.Freeze();
        return bitmapImage;
    }

    /// <summary>
    /// Loads an encoded image from a byte array and returns a manipulable WriteableBitmap instance.
    /// </summary>
    /// <remarks>The returned WriteableBitmap is in BGRA32 pixel format and is fully independent of
    /// the input byte array. The method ensures thread safety by freezing the intermediate BitmapImage before
    /// conversion. This method is useful when you need to decode an image from memory and perform direct pixel
    /// operations.</remarks>
    /// <param name="encodedBytes">The byte array containing the encoded image data. Must not be null or empty.</param>
    /// <returns>A WriteableBitmap representing the decoded image, suitable for pixel manipulation.</returns>
    /// <exception cref="ArgumentException">Thrown if encodedBytes is null or empty.</exception>
    public WriteableBitmap LoadEncodedBytesAsManipulableImage(byte[] encodedBytes)
    {
        if (encodedBytes == null || encodedBytes.Length == 0)
            throw new ArgumentException("Encoded byte array cannot be empty", nameof(encodedBytes));

        BitmapImage bitmapImage;
        using (var stream = new MemoryStream(encodedBytes, false))  // false = do not dispose the byte[] itself
        {
            bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;   // ← THIS is what makes disposal safe
            bitmapImage.EndInit();   // ← Decoding happens here; stream is now fully consumed

            // Stream can be disposed the moment we exit this block
        }

        // Freeze for thread-safety across your multithreaded service
        if (bitmapImage.CanFreeze)
            bitmapImage.Freeze();

        // Convert to manipulable WriteableBitmap (still BGRA32)
        var writable = new WriteableBitmap(bitmapImage);
        if (writable.Format != PixelFormats.Bgra32 && writable.Format != PixelFormats.Pbgra32)
        {
            writable = new WriteableBitmap(new FormatConvertedBitmap(writable, PixelFormats.Bgra32, null, 0));
        }

        return writable;   // ← Stream was already disposed; everything is now self-contained
    }

    /// <summary>
    /// Renders a slide for the specified test, block, and trial as a bitmap image.
    /// </summary>
    /// <remarks>The rendered bitmap uses the layout and DPI settings from the current application window. The
    /// slide includes block instructions, response keys (if present), and the trial's stimulus, which may be an image
    /// or text.</remarks>
    /// <param name="test">The test instance containing the blocks, trials, and layout information to be rendered.</param>
    /// <param name="blockId">The unique identifier of the block to render. Specifies which block's instructions and response keys to display.</param>
    /// <param name="trialId">The unique identifier of the trial to render. Specifies which stimulus to display on the slide.</param>
    /// <returns>A frozen BitmapSource representing the rendered slide, including instructions, response keys, and stimulus
    /// content.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified block does not contain a valid instruction.</exception>
    public BitmapSource RenderSlide(IatTest test, Guid blockId, Guid trialId)
    {
        var block = test.GetBlockById(blockId) ?? new Block();
        var trial = test.GetTrialById(trialId) ?? new Trial();
        var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window());
        double dpiX = dpi.PixelsPerInchX;
        double dpiY = dpi.PixelsPerInchY;
        var rects = _layout.GetFinalRects(test.Layout);
        var bmp = new RenderTargetBitmap((int)rects.Interior.Width, (int)rects.Interior.Height, dpiX, dpiY, PixelFormats.Pbgra32);

        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.Black, null, rects.Interior);
            if (block.LeftResponseId != Guid.Empty && block.RightResponseId != Guid.Empty)
            {
                dc.DrawImage(RenderKeyToBitmap(block.LeftResponseId), rects.LeftKey);
                dc.DrawImage(RenderKeyToBitmap(block.RightResponseId), rects.RightKey);
            }
            dc.DrawImage(RenderTextToBitmap(test.GetFormattedTextById(block.BlockInstructionsId) ?? throw new ArgumentException("Block does not contain a valid instruction")), 
                rects.BlockInstructions);
            if (trial.StimulusId != Guid.Empty)
            {
                var stimulus = test.GetStimulusById(trial.StimulusId);
                if (stimulus is ImageStimulus imageStimulus)
                {
                    var imageBytes = _packageService.GetImageBytes(imageStimulus.Id);
                    var bitmapImage = BitmapFromBytes(imageBytes);
                    dc.DrawImage(bitmapImage, rects.Stimulus);
                }
                else if (stimulus is TextStimulus textStimulus)
                {
                    dc.DrawImage(RenderTextToBitmap(textStimulus), rects.Stimulus);
                }
            }
        }
        bmp.Render(visual);
        bmp.Freeze();
        return bmp;
    }

    /// <summary>
    /// Asynchronously loads an image from the specified package and returns a bitmap resized to the given dimensions.
    /// </summary>
    /// <remarks>The returned BitmapSource is frozen for thread safety. If the original image dimensions match
    /// the requested size, the image is returned without resizing.</remarks>
    /// <param name="bmpSource">The source bitmap to resize. Cannot be null.</param>
    /// <param name="targetWidth">The desired width, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <param name="targetHeight">The desired height, in pixels, of the resulting bitmap. Must be a positive integer.</param>
    /// <returns>A BitmapSource containing the image resized to the specified width and height.</returns>
    /// <exception cref="ArgumentException">Thrown if stimulus is not of type ImageStimulus.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the PackageUri property is not set on the ImageStimulus.</exception>
    public BitmapSource GetResizedBitmap(BitmapSource bmpSource, int targetWidth, int targetHeight)
    {
        var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window());
        RenderTargetBitmap bmpDest = new RenderTargetBitmap(targetWidth, targetHeight, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawImage(bmpSource, new Rect(0, 0, targetWidth, targetHeight));
        }
        bmpDest.Render(visual);
        bmpDest.Freeze();
        return bmpDest;
    }

    /// <summary>
    /// Renders a visual outline of the key area defined in the layout as a bitmap image. The outline is drawn with a lime green border 
    /// and is sized according to the layout's left key rectangle. This method is useful for debugging or visualizing the key area within 
    /// the application's layout. The resulting BitmapSource is frozen for thread safety and can be displayed in WPF user interfaces.
    /// </summary>
    /// <returns>A BitmapSource containing the visual outline of the key area.</returns>
    public BitmapSource RenderKeyOutline()
    {
        var outlineRect = _layout.GetFinalRects(_iat.Layout).LeftKey;
        var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window());
        var bmp = new RenderTargetBitmap((int)outlineRect.Width, (int)outlineRect.Height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(null, new Pen(Brushes.LimeGreen, 4), new Rect(2, 2, outlineRect.Width - 4, outlineRect.Height - 4));
        }
        bmp.Render(visual);
        bmp.Freeze();
        return bmp;
    }
}
