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
            PngBitmapEncoder encoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(memStream);
            _fileManifestBuilder.AddFile(exportContext.FileManifest, "ErrorMark.png", 1000, ResourceType.ErrorMark, "image/png", memStream.ToArray());
            var errorMarkDI = new DisplayItem()
            {
                Id = 1000,
                Guid = Guid.NewGuid(),
                X = (int)exportContext.LayoutRects.ErrorMark.X,
                Y = (int)exportContext.LayoutRects.ErrorMark.Y,
                Width = (int)exportContext.LayoutRects.ErrorMark.Width,
                Height = (int)exportContext.LayoutRects.ErrorMark.Height
            };

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
            encoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            encoder.Frames.Clear();
            encoder.Frames.Add(BitmapFrame.Create(renderBmp));
            encoder.Save(memStream);
            _fileManifestBuilder.AddFile(exportContext.FileManifest, "KeyOutlineLeft.png", 1001, ResourceType.KeyOutline, "image/png", memStream.ToArray());
            _fileManifestBuilder.AddFile(exportContext.FileManifest, "KeyOutlineRight.png", 1002, ResourceType.KeyOutline, "image/png", memStream.ToArray());
            memStream.Dispose();
            var leftOutlineDI = new DisplayItem()
            {
                Id = 1001,
                Guid = Guid.NewGuid(),
                X = (int)exportContext.LayoutRects.LeftKey.X,
                Y = (int)exportContext.LayoutRects.LeftKey.Y,
                Width = (int)exportContext.LayoutRects.LeftKey.Width,
                Height = (int)exportContext.LayoutRects.LeftKey.Height
            };
            var rightKeyOutlineDI = new DisplayItem()
            {
                Id = 1002,
                Guid = Guid.NewGuid(),
                X = (int)exportContext.LayoutRects.RightKey.X,
                Y = (int)exportContext.LayoutRects.RightKey.Y,
                Width = (int)exportContext.LayoutRects.RightKey.Width,
                Height = (int)exportContext.LayoutRects.RightKey.Height
            };
            exportContext.AddDisplayItem(errorMarkDI);
            exportContext.AddDisplayItem(leftOutlineDI);
            exportContext.AddDisplayItem(rightKeyOutlineDI);

            var config = new IATConfigFile
            {
                IATName = test.Name,
                ErrorMarkId = errorMarkDI.Guid,
                LeftKeyOutlineId = leftOutlineDI.Guid,
                RightKeyOutlineId = rightKeyOutlineDI.Guid,
                EventList = exportContext.Events,
                DisplayItems = exportContext.DisplayItems,
                NumIATItems = test.AllTrials.Count,
                RedirectOnComplete = "https://iatsoftware.net",
                Surveys = exportContext.Surveys
            };
            config.Layout.InteriorHeight = (int)exportContext.LayoutRects.Interior.Height;
            config.Layout.InteriorWidth = (int)exportContext.LayoutRects.Interior.Width;
            return config;
        }
    }
}
