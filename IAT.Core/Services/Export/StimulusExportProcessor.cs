using IAT.Core.Domain;
using System;
using IAT.Core.ConfigFile;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using IAT.Core.Enumerations;
using System.Windows;

namespace IAT.Core.Services.Export
{
    /// <summary>
    /// Defines a contract for processing a stimulus and exporting its data using the provided context information.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for handling the export logic for a given
    /// stimulus. The method receives the stimulus to process, its bounding rectangle, and a dictionary mapping unique
    /// identifiers to integer values, which may be used to track or reference exported items.</remarks>
    public interface IStimulusExportProcessor
    {
        /// <summary>
        /// Processes the specified stimulus and exports its data based on the provided context information. This may involve generating image files, 
        /// updating the export manifest, and recording display information for the stimulus within the test package. The method ensures that each 
        /// stimulus is processed only once per unique identifier, and it supports both text and image stimuli. The idDictionary is used to track processed 
        /// stimuli and assign display item IDs, ensuring consistent referencing throughout the export process.
        /// </summary>
        /// <param name="stimulus">The stimulus to be processed and exported.</param>
        /// <param name="exportContext">The context information for the export process, including bounding rectangle and ID dictionary.</param>
        void ProcessStimulus(Stimulus stimulus, ExportContext exportContext);
    }

    /// <summary>
    /// Provides functionality to process stimuli for export by generating image representations, updating file
    /// manifests, and recording display information within a test package.
    /// </summary>
    /// <remarks>This processor supports both text and image stimuli, ensuring each stimulus is processed only
    /// once per unique identifier. It coordinates with image generation, file manifest, and project package services to
    /// manage the export workflow. This class is intended for internal use within the export pipeline and is not
    /// thread-safe.</remarks>
    public sealed class StimulusExportProcessor : IStimulusExportProcessor
    {
        private readonly IImageGenerationService _imageGenerationService;
        private readonly IFileManifestBuilder _fileManifestBuilder;
        private readonly IProjectPackageService _projectPackageService;

        /// <summary>
        /// Initializes a new instance of the StimulusExportProcessor class with the specified supporting services.
        /// </summary>
        /// <param name="imageGenerationService">The service used to generate images for stimuli. Cannot be null.</param>
        /// <param name="fileManifestBuilder">The builder used to create file manifests for the export process. Cannot be null.</param>
        /// <param name="projectPackageService">The service responsible for managing project packages during export. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if any of the parameters are null.</exception>
        public StimulusExportProcessor(IImageGenerationService imageGenerationService, IFileManifestBuilder fileManifestBuilder, 
            IProjectPackageService projectPackageService)
        {
            _imageGenerationService = imageGenerationService ?? throw new ArgumentNullException(nameof(imageGenerationService));
            _fileManifestBuilder = fileManifestBuilder ?? throw new ArgumentNullException(nameof(fileManifestBuilder));
            _projectPackageService = projectPackageService ?? throw new ArgumentNullException(nameof(projectPackageService));
        }

        /// <summary>
        /// Processes the specified stimulus and adds its image representation and display information to the test
        /// package if it has not already been processed.
        /// </summary>
        /// <remarks>If the stimulus has not been previously processed, this method generates an image
        /// file for the stimulus, adds it to the file manifest, and records its display information. Supported stimulus
        /// types include text and image stimuli. The method does not process a stimulus more than once per unique
        /// identifier.</remarks>
        /// <param name="stimulus">The stimulus to process. Must be a valid instance of a supported stimulus type.</param>
        /// <param name="exportContext">The context information for the export process, including bounding rectangle and ID dictionary.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stimulus"/> or <paramref name="exportContext"/> is null.</exception>
        public void ProcessStimulus(Stimulus stimulus, ExportContext exportContext)
        {
            if (stimulus == null) throw new ArgumentNullException(nameof(stimulus));
            if (exportContext == null) throw new ArgumentNullException(nameof(exportContext));

            PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder()
            {
                QualityLevel = 90
            };
            string filename = string.Empty;
            using var memStream = new MemoryStream();
            if (!exportContext.IdDictionary.ContainsKey(stimulus.Id))
            {
                exportContext.IdDictionary[stimulus.Id] = exportContext.IdDictionary.Count + 1;

                if (stimulus is TextStimulus text)
                {
                    var bmp = _imageGenerationService.RenderTextToBitmap(text);
                    pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
                    pngEncoder.Save(memStream);
                    filename = $"stimulus{exportContext.IdDictionary[stimulus.Id]}.png";
                    _fileManifestBuilder.AddFile(exportContext.FileManifest, filename, FileResourceType.image, "image/png", memStream.ToArray());
                }
                else if (stimulus is ImageStimulus imageStimulus)
                {
                    using var stimulusByteStream = new MemoryStream(_projectPackageService.GetImageBytes(imageStimulus.Id));
                    var stimulusBitmap = new BitmapImage();
                    stimulusBitmap.BeginInit();
                    stimulusBitmap.CacheOption = BitmapCacheOption.OnLoad;
                    stimulusBitmap.StreamSource = stimulusByteStream;
                    stimulusBitmap.EndInit();
                    stimulusBitmap.Freeze();
                    double bmpAR = stimulusBitmap.PixelWidth / (double)stimulusBitmap.PixelHeight;
                    double rectAR = exportContext.LayoutRects.Stimulus.Width / exportContext.LayoutRects.Stimulus.Height;
                    int width = 0; int height = 0;
                    if (rectAR > bmpAR)
                    {
                        height = (int)exportContext.LayoutRects.Stimulus.Height;
                        width = (int)(height * bmpAR);
                    }
                    else
                    {
                        width = (int)exportContext.LayoutRects.Stimulus.Width;
                        height = (int)(width / bmpAR);
                    }
                    var resizedStimulus = _imageGenerationService.GetResizedBitmap(stimulusBitmap, width, height);
                    string imageType = _projectPackageService.GetImageType(imageStimulus.Id);
                    string mimeType = $"image/{imageType}";
                    filename = $"stimulus{exportContext.IdDictionary[stimulus.Id]}.{imageType}";
                    if (imageType == "png")
                    {
                        pngEncoder.Frames.Add(BitmapFrame.Create(resizedStimulus));
                        pngEncoder.Save(memStream);
                    }
                    else if (imageType == "jpg" || imageType == "jpeg")
                    {
                        jpegEncoder.Frames.Add(BitmapFrame.Create(resizedStimulus));
                        jpegEncoder.Save(memStream);
                    }
                    _fileManifestBuilder.AddFile(exportContext.FileManifest, filename, FileResourceType.image, mimeType, memStream.ToArray());
                }
                exportContext.DisplayItems.Add(new DisplayItem()
                {
                    Id = exportContext.IdDictionary[stimulus.Id],
                    Guid = stimulus.Id,
                    Filename = filename,
                    X = (int)exportContext.LayoutRects.Stimulus.X,
                    Y = (int)exportContext.LayoutRects.Stimulus.Y,
                    Width = (int)exportContext.LayoutRects.Stimulus.Width,
                    Height = (int)exportContext.LayoutRects.Stimulus.Height
                }); 
            }
        }
    }
}
