using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using IAT.Core.ConfigFile;
using IAT.Core.Domain;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using System.Windows.Media;
using sun.nio.cs.ext;

namespace IAT.Core.Services.Export
{
    /// <summary>
    /// A interface defining the contract for mapping IAT test domain objects to configuration files suitable for the IAT software. This includes 
    /// the generation of required image resources and the construction of a configuration file that represents the test structure and content in 
    /// a format that can be serialized to JSON and consumed by the IAT software.
    /// </summary>
    public interface ITestMapperService
    {
        /// <summary>
        /// Builds a configuration file from the specified test and export context.
        /// </summary>
        /// <param name="test">The IAT test to build the configuration file from.</param>
        /// <param name="exportContext">The export context containing settings for the configuration file.</param>
        /// <returns>A configuration file built from the test and export context.</returns>
        IATConfigFile BuildConfigFile(IatTest test, ExportContext exportContext);    
    }

    /// <summary>
    /// Maps IAT test domain objects to configuration files suitable for the IAT software, including generation of
    /// required image resources.
    /// </summary>
    public class TestMapperService : ITestMapperService
    {
        private readonly IImageGenerationService _imageGenerationService;
        private readonly IFileManifestBuilder _fileManifestBuilder;

        /// <summary>
        /// Consttructs a new instance of the TestMapperService with the specified image generation service and file manifest builder.
        /// </summary>
        /// <param name="imageGenerationService">The service used to generate images for the test. Cannot be null.</param>
        /// <param name="fileManifestBuilder">The builder used to create file manifests for the export process. Cannot be null.</param>
        public TestMapperService(IImageGenerationService imageGenerationService, IFileManifestBuilder fileManifestBuilder)
        {
            _imageGenerationService = imageGenerationService ?? throw new ArgumentNullException(nameof(imageGenerationService));
            _fileManifestBuilder = fileManifestBuilder ?? throw new ArgumentNullException(nameof(fileManifestBuilder));
        }

        /// <summary>
        /// Maps an IatTest domain object and an export context to an IATConfigFile, which is a serializable object that can be converted to JSON 
        /// and read by the IAT software. This method also generates necessary image files (like the error mark and key outlines) and adds them to 
        /// the file manifest, as well as adding corresponding display items to the export context.
        /// </summary>
        /// <param name="test">The test to be exported</param>
        /// <param name="exportContext">The export context containing layout information and file manifest</param>
        /// <returns>The generated IATConfigFile</returns>
        public IATConfigFile BuildConfigFile(IatTest test, ExportContext exportContext)
        {
            PngBitmapEncoder encoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            IFormattedText errorMark = new Domain.FormattedText()
            {
                Id = Guid.NewGuid(),
                Text = "X",
                Style = new TextStyle()
                {
                    FontFamily = "Arial",
                    FontSize = exportContext.LayoutRects.ErrorMark.Height,
                    FontColor = Colors.Red
                },
                LayoutItem = LayoutItem.ErrorMark
            };
            var bmp = _imageGenerationService.RenderTextToBitmap(errorMark, exportContext.LayoutRects.ErrorMark);
            var memStream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(memStream);
            _fileManifestBuilder.AddFile(exportContext.FileManifest, "ErrorMark.png", FileResourceType.errorMark, "image/png", memStream.ToArray());
            exportContext.DisplayItems.Add(new DisplayItem()
            {
                Filename = "ErrorMark.png",
                Id = 1000,
                Guid = errorMark.Id,
                X = (int)exportContext.LayoutRects.ErrorMark.X,
                Y = (int)exportContext.LayoutRects.ErrorMark.Y,
                Width = (int)exportContext.LayoutRects.ErrorMark.Width,
                Height = (int)exportContext.LayoutRects.ErrorMark.Height
            });

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(null, new Pen(Brushes.LimeGreen, 2), new Rect(exportContext.LayoutRects.LeftKey.X, exportContext.LayoutRects.LeftKey.Y, 
                    exportContext.LayoutRects.LeftKey.Width, exportContext.LayoutRects.LeftKey.Height));
            }
            var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window());
            var renderBmp = new RenderTargetBitmap((int)exportContext.LayoutRects.LeftKey.Width + 4, (int)exportContext.LayoutRects.LeftKey.Height + 4, 
                dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
            renderBmp.Render(visual);
            memStream.Dispose(); memStream = new MemoryStream();
            encoder.Frames.Clear();
            encoder.Frames.Add(BitmapFrame.Create(renderBmp));
            encoder.Save(memStream);
            _fileManifestBuilder.AddFile(exportContext.FileManifest, "KeyOutline.png", FileResourceType.keyOutline, "image/png", memStream.ToArray());
            memStream.Dispose();
            exportContext.DisplayItems.Add(new DisplayItem()
            {
                Filename = "KeyOutline.png",
                Id = 1001,
                Guid = Guid.Empty,
                X = (int)exportContext.LayoutRects.LeftKey.X,
                Y = (int)exportContext.LayoutRects.LeftKey.Y,
                Width = (int)exportContext.LayoutRects.LeftKey.Width,
                Height = (int)exportContext.LayoutRects.LeftKey.Height
            });
            exportContext.DisplayItems.Add(new DisplayItem()
            {
                Filename = "KeyOutline.png",
                Id = 1002,
                Guid = Guid.Empty,
                X = (int)exportContext.LayoutRects.RightKey.X,
                Y = (int)exportContext.LayoutRects.RightKey.Y,
                Width = (int)exportContext.LayoutRects.RightKey.Width,
                Height = (int)exportContext.LayoutRects.RightKey.Height
            });

            var config = new IATConfigFile
            {
                Name = test.Name,
                ErrorMarkID = 1000,
                LeftKeyOutlineID = 1001,
                RightKeyOutlineID = 1002,
                EventList = exportContext.Events,
                DisplayItemList = exportContext.DisplayItems,
                NumIATItems = test.AllTrials.Count,
                RedirectOnComplete = "https://iatsoftware.net"
            };
            return config;
        }
    }
}
