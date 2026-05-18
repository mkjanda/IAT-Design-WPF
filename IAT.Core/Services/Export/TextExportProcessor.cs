using System;
using System.Collections.Generic;
using System.Windows;
using System.IO;
using System.Windows.Media.Imaging;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Extensions;
using IAT.Core.ConfigFile;


namespace IAT.Core.Services.Export
{
    /// <summary>
    /// The interface ITextExportService defines a contract for exporting formatted text content as part of a test package.
    /// </summary>
    public interface ITextExportProcessor   
    {
        /// <summary>
        /// Exports the specified formatted text to the target output using the provided layout rectangle and identifier
        /// mapping.
        /// </summary>
        /// <param name="text">The formatted text to export. Cannot be null.</param>
        /// <param name="textRect">The rectangle that defines the layout area for the exported text.</param>
        /// <param name="exportContext">The context that provides configuration and data for export operations. Cannot be null.</param>
        public void ProcessText(IFormattedText text, Rect textRect, ExportContext exportContext);
    }

    /// <summary>
    /// Provides functionality to export formatted text as image resources and display items within a test package.
    /// </summary>
    /// <remarks>This service generates image files from formatted text and updates the test package's file
    /// manifest and display items accordingly. It ensures that each unique text stimulus is exported only once and
    /// referenced consistently throughout the package. The service depends on an image generation service and a test
    /// package instance, which are supplied via the constructor.</remarks>
    public class TextExportProcessor : ITextExportProcessor
    {
        private readonly IImageGenerationService _imageGenerationService;
        private readonly IFileManifestBuilder _fileManifestBuilder;

        /// <summary>
        /// Initializes a new instance of the TextExportProcessor class with the specified image generation service and
        /// file manifest builder.
        /// </summary>
        /// <param name="imageGenerationService">The service used to generate images for export operations. Cannot be null.</param>
        /// <param name="fileManifestBuilder">The builder used to construct the file manifest for the test package. Cannot be null.</param>
        public TextExportProcessor(IImageGenerationService imageGenerationService, IFileManifestBuilder fileManifestBuilder) 
        {
            _imageGenerationService = imageGenerationService;
            _fileManifestBuilder = fileManifestBuilder;
        }

        /// <summary>
        /// Exports the specified formatted text as an image and registers its metadata for display within the test
        /// package.
        /// </summary>
        /// <remarks>If the text has not been previously exported, a new image file is generated and added
        /// to the file manifest. Subsequent calls with the same text will reuse the existing image and ID. The method
        /// updates the display items collection to include the exported text at the specified location.</remarks>
        /// <param name="text">The formatted text to export as an image. Cannot be null.</param>
        /// <param name="textRect">The rectangle specifying the location and size of the text within the display. Cannot be null.</param>
        /// <param name="exportContext">The context that provides configuration and data for export operations. Cannot be null.</param>
        public void ProcessText(IFormattedText text, Rect textRect, ExportContext exportContext)
        {
            var encoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            string filename = string.Empty;
            if (!exportContext.IdDictionary.ContainsKey(text.Id))
            {
                exportContext.IdDictionary[text.Id] = exportContext.IdDictionary.Count + 1;
                var textBmp = _imageGenerationService.RenderTextToBitmap(text, textRect);
                var memStream = new MemoryStream();
                encoder.Save(memStream);
                filename = $"stimulus{exportContext.IdDictionary[text.Id]}.png";
                _fileManifestBuilder.AddFile(exportContext.FileManifest, filename, FileResourceType.image, "image/png", memStream.ToArray());
            }
            filename = $"stimulus{exportContext.IdDictionary[text.Id]}.png";
            exportContext.DisplayItems.Add(new DisplayItem()
            {
                Id = exportContext.IdDictionary[text.Id],
                Guid = text.Id,
                Filename = filename,
                X = (int)textRect.X,
                Y = (int)textRect.Y,
                Width = (int)textRect.Width,
                Height = (int)textRect.Height
            });
        }
    }
}
