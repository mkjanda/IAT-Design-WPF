using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Extensions;
using IAT.Core.Models;
using System.Globalization;
using System.IO;
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
    /// Renders the specified formatted text as a bitmap image.
    /// </summary>
    /// <remarks>The returned bitmap reflects the formatting and layout specified in the input text. The
    /// caller is responsible for managing the lifetime of the resulting BitmapSource as appropriate for their
    /// application.</remarks>
    /// <param name="text">The formatted text to render. Cannot be null.</param>
    /// <param name="boundingRect">The bounding rectangle that defines the size and position for rendering the text. Must not be null.</param>
    /// <returns>A BitmapSource containing the rendered text as an image.</returns>
    BitmapSource RenderTextToBitmap(IFormattedText text, Rect boundingRect);
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
    /// <param name="rects">The layout rectangles defining the positions and sizes of the slide elements. Must not be null.</param>
    /// <returns>A BitmapSource representing the rendered slide. Returns null if the slide cannot be rendered.</returns>
    BitmapSource RenderSlide(IatTest test, Guid blockId, Guid trialId, LayoutRects rects);

    /// <summary>
    /// Returns a new bitmap of <paramref name="targetWidth"/> × <paramref name="targetHeight"/> with
    /// <paramref name="bmpSource"/> scaled uniformly to fit inside that canvas (contain / letterbox).
    /// Aspect ratio is preserved. Empty margins stay transparent — never cropped, never squashed.
    /// </summary>
    BitmapSource GetResizedBitmap(BitmapSource bmpSource, int targetWidth, int targetHeight);

    /// <summary>
    /// Renders a response key (including stacked combined keys) into
    /// <paramref name="boundingRect"/>. Each combined-key component keeps its own style.
    /// </summary>
    BitmapSource RenderKeyToBitmap(IatTest test, Guid keyId, Rect boundingRect);

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
    private readonly IKeyService _keyService;
    private readonly IProjectPackageService _packageService;

    /// <summary>
    /// Initializes a new instance of the ImageGenerationService class with the specified dependencies.
    /// </summary>
    /// <param name="keyService">The service responsible for key management and related operations. Cannot be null.</param>
    /// <param name="packageService">The service used to manage project packages required for image generation. Cannot be null.</param>
    public ImageGenerationService(IKeyService keyService, IProjectPackageService packageService)
    {
        _keyService = keyService ?? throw new ArgumentNullException(nameof(keyService));
        _packageService = packageService ?? throw new ArgumentNullException(nameof(packageService));
    }

    /// <summary>
    /// True ink rectangle of <paramref name="formattedText"/> relative to a
    /// <c>DrawText(..., (0,0))</c> origin. WPF <see cref="System.Windows.Media.FormattedText.Width"/>
    /// is a layout box — <c>TextAlignment.Center</c> without <c>MaxTextWidth</c> paints
    /// glyphs at negative X, and unbreakable words can ink past the layout width.
    /// Using that box as the bitmap size is what cropped "Flower" to "wer".
    /// </summary>
    private static Rect MeasureInk(System.Windows.Media.FormattedText formattedText)
    {
        try
        {
            var geometry = formattedText.BuildGeometry(new Point(0, 0));
            if (geometry is not null)
            {
                var bounds = geometry.Bounds;
                if (!bounds.IsEmpty && bounds.Width > 0 && bounds.Height > 0)
                    return bounds;
            }
        }
        catch (Exception)
        {
            // Empty / control-only strings can throw. Fall through to metrics.
        }

        var layoutW = Math.Max(formattedText.WidthIncludingTrailingWhitespace, formattedText.Width);
        if (layoutW <= 0)
            layoutW = 1.0;
        var layoutH = Math.Max(formattedText.Height, formattedText.Extent);
        if (layoutH <= 0)
            layoutH = Math.Max(1.0, formattedText.Baseline);

        var left = Math.Min(0.0, formattedText.OverhangLeading);
        var right = layoutW - Math.Min(0.0, formattedText.OverhangTrailing);
        var bottom = layoutH + Math.Max(0.0, formattedText.OverhangAfter);
        return new Rect(left, 0.0, Math.Max(1.0, right - left), Math.Max(1.0, bottom));
    }

    /// <summary>
    /// Rasterize the string at authored size into an ink-tight bitmap (no clip),
    /// then <see cref="FitContain"/> that picture into <paramref name="boundingRect"/>.
    /// </summary>
    private RenderTargetBitmap RenderFormattedTextToBitmap(System.Windows.Media.FormattedText formattedText, Rect boundingRect)
    {
        if (formattedText == null) throw new ArgumentNullException(nameof(formattedText));

        const double dpi = 96.0;
        const double pad = 2.0;
        var destW = Math.Max(1, (int)Math.Ceiling(Math.Max(1.0, boundingRect.Width)));
        var destH = Math.Max(1, (int)Math.Ceiling(Math.Max(1.0, boundingRect.Height)));

        var ink = MeasureInk(formattedText);
        var origin = new Point(pad - ink.X, pad - ink.Y);
        var glyphW = Math.Max(1, (int)Math.Ceiling(ink.Width + pad * 2.0));
        var glyphH = Math.Max(1, (int)Math.Ceiling(ink.Height + pad * 2.0));

        var glyphBmp = new RenderTargetBitmap(glyphW, glyphH, dpi, dpi, PixelFormats.Pbgra32);
        var glyphVisual = new DrawingVisual();
        using (var dc = glyphVisual.RenderOpen())
            dc.DrawText(formattedText, origin);
        glyphBmp.Render(glyphVisual);
        glyphBmp.Freeze();

        var dest = new RenderTargetBitmap(destW, destH, dpi, dpi, PixelFormats.Pbgra32);
        var destVisual = new DrawingVisual();
        using (var dc = destVisual.RenderOpen())
        {
            dc.DrawImage(
                glyphBmp,
                FitContainDownOnly(
                    new Size(glyphBmp.PixelWidth, glyphBmp.PixelHeight),
                    new Rect(0, 0, destW, destH)));
        }
        dest.Render(destVisual);
        dest.Freeze();
        return dest;
    }

    /// <summary>
    /// Combined keys paint each component's live style; the "or" row is default black.
    /// Each line is left-aligned on its own FormattedText so Center-without-MaxTextWidth
    /// cannot shift glyphs to negative X. Manual X centers the line in the stack.
    /// </summary>
    public BitmapSource RenderKeyToBitmap(IatTest test, Guid keyId, Rect boundingRect)
    {
        var key = test.GetKeyById(keyId);
        var lines = Domain.Key.ResolveDisplayLines(key, test);
        if (lines.Count == 0)
            return RenderTextToBitmap(_keyService.GetResolvedKey(test, keyId), boundingRect);
        if (lines.Count == 1)
        {
            var only = lines[0];
            return RenderTextToBitmap(new Domain.Key
            {
                Id = keyId,
                Text = only.Text,
                Style = new TextStyle
                {
                    FontFamily = only.FontFamily,
                    FontSize = only.FontSize,
                    FontColor = only.FontColor
                }
            }, boundingRect);
        }

        var formatted = new List<System.Windows.Media.FormattedText>(lines.Count);
        foreach (var line in lines)
        {
            var typeface = new Typeface(
                new FontFamily(string.IsNullOrWhiteSpace(line.FontFamily) ? "Segoe UI" : line.FontFamily),
                FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            formatted.Add(new System.Windows.Media.FormattedText(
                string.IsNullOrEmpty(line.Text) ? " " : line.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                line.FontSize > 0 ? line.FontSize : 24.0,
                new SolidColorBrush(line.FontColor),
                1.0)
            {
                TextAlignment = TextAlignment.Left,
                Trimming = TextTrimming.None
            });
        }

        const double gap = 2.0;
        const double pad = 2.0;
        var inks = formatted.Select(MeasureInk).ToList();
        var stackW = inks.Max(b => b.Width);
        var stackH = inks.Sum(b => b.Height) + gap * (formatted.Count - 1);
        var glyphW = Math.Max(1, (int)Math.Ceiling(stackW + pad * 2.0));
        var glyphH = Math.Max(1, (int)Math.Ceiling(stackH + pad * 2.0));
        var glyphBmp = new RenderTargetBitmap(glyphW, glyphH, 96.0, 96.0, PixelFormats.Pbgra32);
        var glyphVisual = new DrawingVisual();
        using (var dc = glyphVisual.RenderOpen())
        {
            var y = pad;
            for (var i = 0; i < formatted.Count; i++)
            {
                var ft = formatted[i];
                var ink = inks[i];
                var x = pad + (stackW - ink.Width) / 2.0 - ink.X;
                dc.DrawText(ft, new Point(x, y - ink.Y));
                y += ink.Height + gap;
            }
        }
        glyphBmp.Render(glyphVisual);
        glyphBmp.Freeze();

        var destW = Math.Max(1, (int)Math.Ceiling(Math.Max(1.0, boundingRect.Width)));
        var destH = Math.Max(1, (int)Math.Ceiling(Math.Max(1.0, boundingRect.Height)));
        var dest = new RenderTargetBitmap(destW, destH, 96.0, 96.0, PixelFormats.Pbgra32);
        var destVisual = new DrawingVisual();
        using (var dc = destVisual.RenderOpen())
        {
            dc.DrawImage(
                glyphBmp,
                FitContainDownOnly(
                    new Size(glyphBmp.PixelWidth, glyphBmp.PixelHeight),
                    new Rect(0, 0, destW, destH)));
        }
        dest.Render(destVisual);
        dest.Freeze();
        return dest;
    }

    /*
    /// <summary>
    /// Renders the visual representation of the specified key as a bitmap image.
    /// </summary>
    /// <remarks>The rendered bitmap uses the key's font, color, and layout information. If the key
    /// text contains multiple words and is marked as combined, the words are rendered on separate lines. The bitmap
    /// is suitable for display in WPF user interfaces.</remarks>
    /// <param name="keyId">The unique identifier of the key to render. Must correspond to a valid key in the key service.</param>
    /// <param name="boundingRect">The bounding rectangle that defines the size and position for rendering the key. Must not be null.</param>
    /// <returns>A RenderTargetBitmap containing the rendered image of the key. The bitmap reflects the key's text, font, color,
    /// and layout settings.</returns>
    public RenderTargetBitmap RenderKeyToBitmap(Key key, Rect boundingRect)
    {
        var foreground = new SolidColorBrush(key.FontColor);
        var typeface = new Typeface(new FontFamily(key.FontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        var formattedText = new System.Windows.Media.FormattedText(
            key.IsCombined ? string.Join("\r\n", key.Text.Split(" ")) : _keyService.GetResolvedDisplayText(_iat, key.Id),
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            key.FontSize * 96.0 / 72.0,   // convert from points to DIPs   
            foreground,
            VisualTreeHelper.GetDpi(new Window()).PixelsPerDip);  // critical for crisp rendering
        return RenderFormattedTextToBitmap(formattedText, boundingRect);
    }
*/
    /// <summary>
    /// Renders the specified formatted text as a bitmap image using the provided text style and layout information.
    /// </summary>
    /// <remarks>The rendered bitmap uses the font family, size, and color specified in the text
    /// style. The method applies the current system DPI settings to ensure crisp text rendering. The caller is
    /// responsible for managing the lifetime of the returned BitmapSource if used outside the method's
    /// scope.</remarks>
    /// <param name="text">An object that defines the text content, style, and layout to be rendered as a bitmap. Cannot be null.</param>
    /// <param name="boundingRect">The bounding rectangle that defines the size and position for rendering the text. Must not be null.</param>
    /// <returns>A BitmapSource containing the rendered text image. The bitmap reflects the specified font, color, and
    /// layout.</returns>
    public BitmapSource RenderTextToBitmap(IFormattedText text, Rect boundingRect)
    {
        ArgumentNullException.ThrowIfNull(text);
        var style = text.Style ?? new TextStyle();
        var foreground = new SolidColorBrush(style.FontColor);
        var typeface = new Typeface(
            new FontFamily(string.IsNullOrWhiteSpace(style.FontFamily) ? "Segoe UI" : style.FontFamily),
            FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

        // Keys store "Good or Flower" on one line; slides and preview stack on "or".
        var display = text is Domain.Key key
            ? Domain.Key.FormatStackedDisplay(key.Text)
            : text.Text ?? string.Empty;

        // Raster left-aligned first so Center-without-MaxTextWidth cannot paint at
        // negative X (that is what cropped "Flower" → "wer" on the deployed PNG).
        // Instructions wrap at the layout width. After measuring, pin MaxTextWidth
        // to the layout box and switch to Center so wrapped lines stay centered;
        // MeasureInk then expands the bitmap for any unbreakable word that inks past
        // that box, and FitContain scales the whole picture into the dest rect.
        var formattedText = new System.Windows.Media.FormattedText(
            string.IsNullOrEmpty(display) ? " " : display,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            style.FontSize > 0 ? style.FontSize : 24.0,
            foreground,
            1.0)
        {
            TextAlignment = TextAlignment.Left,
            Trimming = TextTrimming.None
        };

        if (text is Domain.Key)
        {
            var stackWidth = Math.Max(1.0, formattedText.WidthIncludingTrailingWhitespace);
            formattedText.MaxTextWidth = stackWidth;
            formattedText.TextAlignment = TextAlignment.Center;
        }
        else
        {
            formattedText.MaxTextWidth = Math.Max(1.0, boundingRect.Width);
            formattedText.TextAlignment = TextAlignment.Center;
        }

        return RenderFormattedTextToBitmap(formattedText, boundingRect);
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
    /// <param name="rects">The layout rectangles defining the positions and sizes of the slide elements. Must not be null.</param>
    /// <returns>A frozen BitmapSource representing the rendered slide, including instructions, response keys, and stimulus
    /// content.</returns>
    /// <exception cref="ArgumentException">Thrown if the specified block does not contain a valid instruction.</exception>
    public BitmapSource RenderSlide(IatTest test, Guid blockId, Guid trialId, LayoutRects rects)
    {
        var block = test.GetBlockById(blockId) ?? new Block();
        var trial = test.GetTrialById(trialId) ?? new Trial();
        const double dpiX = 96.0;
        const double dpiY = 96.0;
        var bmp = new RenderTargetBitmap(
            Math.Max(1, (int)Math.Ceiling(rects.Interior.Width)),
            Math.Max(1, (int)Math.Ceiling(rects.Interior.Height)),
            dpiX, dpiY, PixelFormats.Pbgra32);

        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            dc.DrawRectangle(Brushes.White, null, rects.Interior);
            if (block.LeftResponseId != Guid.Empty && block.RightResponseId != Guid.Empty)
            {
                dc.DrawImage(RenderKeyToBitmap(test, block.LeftResponseId, rects.LeftKey), rects.LeftKey);
                dc.DrawImage(RenderKeyToBitmap(test, block.RightResponseId, rects.RightKey), rects.RightKey);
            }
            dc.DrawImage(RenderTextToBitmap(test.GetFormattedTextById(block.BlockInstructionsId) ?? 
                throw new ArgumentException("Block does not contain a valid instruction"), rects.BlockInstructions), 
                rects.BlockInstructions);
            if (trial.StimulusId != Guid.Empty)
            {
                var stimulus = test.GetStimulusById(trial.StimulusId);
                if (stimulus is ImageStimulus imageStimulus)
                {
                    var imageBytes = _packageService.GetImageBytes(imageStimulus.Id);
                    var bitmapImage = BitmapFromBytes(imageBytes);
                    // DrawImage(src, destRect) stretches. Fit first or the slide crops/squishes.
                    var fitted = ImageGenerationService.FitContainDownOnly(
                        new Size(bitmapImage.PixelWidth, bitmapImage.PixelHeight),
                        rects.Stimulus);
                    dc.DrawImage(bitmapImage, fitted);
                }
                else if (stimulus is TextStimulus textStimulus)
                {
                    dc.DrawImage(RenderTextToBitmap(textStimulus, rects.Stimulus), rects.Stimulus);
                }
            }
        }
        bmp.Render(visual);
        bmp.Freeze();
        return bmp;
    }

    /// <summary>
    /// Destination rectangle that scales <paramref name="sourcePixels"/> uniformly to fit inside
    /// <paramref name="dest"/> and centers it. Same rule as WPF <c>Stretch="Uniform"</c>.
    /// Upscales when the source is smaller than <paramref name="dest"/>.
    /// </summary>
    public static Rect FitContain(Size sourcePixels, Rect dest)
        => FitContain(sourcePixels, dest, allowUpscale: true);

    /// <summary>
    /// Same as <see cref="FitContain"/> but never scales up. Authored-size content that
    /// already fits the layout rect stays that size and is centered. Overflow shrinks.
    /// Same rule as WPF <c>Stretch="Uniform" StretchDirection="DownOnly"</c>.
    /// </summary>
    public static Rect FitContainDownOnly(Size sourcePixels, Rect dest)
        => FitContain(sourcePixels, dest, allowUpscale: false);

    private static Rect FitContain(Size sourcePixels, Rect dest, bool allowUpscale)
    {
        if (sourcePixels.Width <= 0 || sourcePixels.Height <= 0 || dest.Width <= 0 || dest.Height <= 0)
            return dest;

        var scale = Math.Min(dest.Width / sourcePixels.Width, dest.Height / sourcePixels.Height);
        if (!allowUpscale && scale > 1.0)
            scale = 1.0;

        var width = sourcePixels.Width * scale;
        var height = sourcePixels.Height * scale;

        return new Rect(
            dest.X + (dest.Width - width) / 2.0,
            dest.Y + (dest.Height - height) / 2.0,
            width,
            height);
    }

    /// <summary>
    /// Paints <paramref name="bmpSource"/> into a <paramref name="targetWidth"/> × <paramref name="targetHeight"/>
    /// canvas using contain-fit. Transparent margins fill the unused sides. Does not crop and does not squash.
    /// </summary>
    public BitmapSource GetResizedBitmap(BitmapSource bmpSource, int targetWidth, int targetHeight)
    {
        ArgumentNullException.ThrowIfNull(bmpSource);
        if (targetWidth <= 0) throw new ArgumentOutOfRangeException(nameof(targetWidth));
        if (targetHeight <= 0) throw new ArgumentOutOfRangeException(nameof(targetHeight));

        var bmpDest = new RenderTargetBitmap(targetWidth, targetHeight, 96.0, 96.0, PixelFormats.Pbgra32);
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            // White, not transparent: JPEG callers composite alpha to black and that
            // letterbox reads as a crop.
            dc.DrawRectangle(Brushes.White, null, new Rect(0, 0, targetWidth, targetHeight));
            var fitted = FitContain(
                new Size(bmpSource.PixelWidth, bmpSource.PixelHeight),
                new Rect(0, 0, targetWidth, targetHeight));
            dc.DrawImage(bmpSource, fitted);
        }
        bmpDest.Render(visual);
        bmpDest.Freeze();
        return bmpDest;
    }
    /*
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
    }*/
}
