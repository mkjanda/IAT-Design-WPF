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
    public interface ITestMapperService
    {
        IATConfigFile BuildConfigFile(IatTest test, ExportContext exportContext);    
    }

    public class TestMapperService : ITestMapperService
    {
        private readonly IImageGenerationService _imageGenerationService;
        private readonly IFileManifestBuilder _fileManifestBuilder;
              
        public TestMapperService(IImageGenerationService imageGenerationService, IFileManifestBuilder fileManifestBuilder)
        {
            _imageGenerationService = imageGenerationService;
            _fileManifestBuilder = fileManifestBuilder;
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
            var bmp = _imageGenerationService.RenderTextToBitmap(errorMark);
            var memStream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(memStream);
            _fileManifestBuilder.AddFile(exportContext.FileManifest, "ErrorMark.png", FileResourceType.errorMark, "image/png", memStream.ToArray());
            exportContext.DisplayItems.Add(new DisplayItem()
            {
                Filename = "ErrorMark.png",
                Id = 1000,
                Guid = Guid.Empty,
                X = (int)exportContext.LayoutRects.ErrorMark.X,
                Y = (int)exportContext.LayoutRects.ErrorMark.Y,
                Width = (int)exportContext.LayoutRects.ErrorMark.Width,
                Height = (int)exportContext.LayoutRects.ErrorMark.Height
            });

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawRectangle(null, new Pen(Brushes.LimeGreen, 2), new Rect(exportContext.LayoutRects.LeftKey.X, exportContext.LayoutRects.LeftKey.Y, exportContext.LayoutRects.LeftKey.Width, exportContext.LayoutRects.LeftKey.Height));
            }
            var renderBmp = new RenderTargetBitmap((int)exportContext.LayoutRects.LeftKey.Width + 4, (int)exportContext.LayoutRects.LeftKey.Height + 4, 96, 96, PixelFormats.Pbgra32);
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
