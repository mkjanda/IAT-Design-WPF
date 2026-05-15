using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.ConfigFile;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using IAT.Core.Extensions;

namespace IAT.Core.Services.Export
{
    /// <summary>
    /// Defines a processor that handles raw byte data export operations using a manifest, a specified region, a unique
    /// identifier, and an identifier mapping.
    /// </summary>
    public interface IRawByteExportProcessor
    {
        /// <summary>
        /// Processes the specified data using the provided manifest, region, unique identifier, and identifier mapping.
        /// </summary>
        /// <param name="manifest">The manifest that defines the structure or metadata required for processing the data. Cannot be null.</param>
        /// <param name="data">The byte array containing the data to process. Cannot be null.</param>
        /// <param name="rect">The rectangular region within which the data should be processed.</param>
        /// <param name="guid">The unique identifier associated with the current processing operation.</param>
        /// <param name="idDictionary">A dictionary mapping unique identifiers to integer values, used to track or associate processed items.
        /// Cannot be null.</param>
        void Process(Manifest manifest, byte[] data, Rect rect, Guid guid, Dictionary<Guid, int> idDictionary);
    }

    /// <summary>
    /// Provides functionality to process raw image byte data, generate image files, and update the export manifest and
    /// display items as part of the export process.
    /// </summary>
    /// <remarks>This class coordinates image generation and manifest management for export operations using
    /// the provided test package, image generation service, and file manifest builder. It is typically used in
    /// scenarios where images must be processed and tracked as part of a larger export workflow. Instances of this
    /// class are not thread-safe.</remarks>
    public class RawByteExportProcessor : IRawByteExportProcessor
    {
        private readonly TestPackage _testPackage;
        private readonly IImageGenerationService _imageGenerationService;
        private readonly IFileManifestBuilder _fileManifestBuilder;

        /// <summary>
        /// Initializes a new instance of the RawByteExportProcessor class with the specified test package, image
        /// generation service, and file manifest builder.
        /// </summary>
        /// <param name="testPackage">The test package that provides the data and configuration for the export process. Cannot be null.</param>
        /// <param name="imageGenerationService">The service used to generate images required during the export process. Cannot be null.</param>
        /// <param name="fileManifestBuilder">The builder used to create and manage the file manifest for the export. Cannot be null.</param>
        public RawByteExportProcessor(TestPackage testPackage, IImageGenerationService imageGenerationService, 
            IFileManifestBuilder fileManifestBuilder)
        {
            _testPackage = testPackage;
            _imageGenerationService = imageGenerationService;
            _fileManifestBuilder = fileManifestBuilder;
        }

        /// <summary>
        /// Processes image data and updates the manifest and display items with the associated image and metadata.
        /// </summary>
        /// <remarks>If the specified GUID is not already present in the dictionary, a new image file is
        /// created and added to the manifest. The method also updates the display items collection with metadata for
        /// the processed image. The method does not validate the contents of the image data; callers should ensure the
        /// data is valid and compatible with the expected image format.</remarks>
        /// <param name="manifest">The manifest to which the generated image file will be added.</param>
        /// <param name="data">The byte array containing the image data to process. Must represent a valid image format.</param>
        /// <param name="rect">The rectangle specifying the position and size for rendering the image.</param>
        /// <param name="guid">The unique identifier associated with the image. Used to track and reference the image within the manifest
        /// and display items.</param>
        /// <param name="idDictionary">A dictionary mapping unique image identifiers to resource IDs. Used to ensure consistent resource
        /// identification across multiple calls.</param>
        public void Process(Manifest manifest, byte[] data, Rect rect, Guid guid, Dictionary<Guid, int> idDictionary)
        {
            if (!idDictionary.ContainsKey(guid))
            {
                idDictionary[guid] = idDictionary.Count + 1;

                using var ms = new MemoryStream(data);
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                var visual = new DrawingVisual();
                using (var context = visual.RenderOpen())
                {
                    context.DrawImage(bitmapImage, rect);
                }
                var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window());
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)rect.Width, (int)rect.Height,
                    dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(visual);

                PngBitmapEncoder encoder = new PngBitmapEncoder()
                {
                    Interlace = PngInterlaceOption.On
                };
                encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using var memStream = new MemoryStream();
                encoder.Save(memStream);
                 
                manifest.AddFile(new ManifestFile()
                {
                    Path = $"{idDictionary[guid]}.png",
                    MimeType = "image/png",
                    Size = memStream.Length,
                    Content = memStream.ToArray(),
                    ResourceId = idDictionary[guid],
                });
            }
            _testPackage.DisplayItems.Add(new DisplayItem()
            {
                Id = idDictionary[guid],
                Guid = guid,
                Filename = $"image{idDictionary[guid]}.png",
                X = (int)rect.X,
                Y = (int)rect.Y,
                Width = (int)rect.Width,
                Height = (int)rect.Height
            });
        }
    }
}
