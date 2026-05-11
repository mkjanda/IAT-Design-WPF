using IAT.Core.ConfigFile;
using IAT.Core.Models;
using IAT.Core.Domain;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using IAT.Core.Enumerations;
using sun.awt.image;
using IAT.Core.Extensions;
using com.sun.xml.@internal.messaging.saaj.soap;
using java.util.function;
using javax.imageio.plugins.bmp;
using System.Windows;

namespace IAT.Core.Services
{
    /// <summary>
    /// Provides functionality to map IatTest data to a TestPackage configuration, including generating required
    /// resources and organizing them for execution on the server.
    /// </summary>
    /// <remarks>Implementations of this interface are responsible for transforming the logical structure and
    /// assets of an IatTest into a format suitable for server-side execution. This includes creating files for stimuli
    /// and instructions, and ensuring all necessary resources are included in the resulting TestPackage.</remarks>
    public interface ITestPackageService
    {
        /// <summary>
        /// Maps the IatTest data to the TestPackage configuration, including creating necessary files for stimuli and instructions, and populating the events and 
        /// display items in the TestPackage. This method processes the blocks, trials, stimuli, and instructions defined in the IatTest, generating the appropriate 
        /// image files for text and image stimuli, and organizing them into the TestPackage structure for use in running the IAT. It ensures that all necessary 
        /// resources are included in the TestPackage and that the configuration is properly set up for execution on the server.
        /// </summary>
        void MapToServerConfig();
    }

    /// <summary>
    /// Provides services for generating and packaging test assets, including images and configuration files, for an IAT
    /// (Implicit Association Test) test package.
    /// </summary>
    /// <remarks>This class coordinates the creation of image resources, slide manifests, and configuration
    /// mappings required to prepare a test package for deployment or server upload. It relies on several supporting
    /// services for image generation, layout calculation, and project packaging. The class is intended for internal use
    /// and is not thread-safe.</remarks>
    internal class TestPackageService : ITestPackageService
    {
        private readonly IatTest _iat;
        private readonly TestPackage _testPackage;
        private readonly IImageGenerationService _imageService;
        private readonly IProjectPackageService _package;
        private readonly ILayoutCalculatorService _layout;

        /// <summary>
        /// Initializes a new instance of the TestPackageService class with the specified dependencies.
        /// </summary>
        /// <param name="iat">The IatTest instance used to provide test-related functionality.</param>
        /// <param name="testPackage">The TestPackage instance representing the test package to be managed.</param>
        /// <param name="imageService">The image generation service used for creating or processing images within the test package.</param>
        /// <param name="package">The project package service responsible for handling project package operations.</param>
        /// <param name="layout">The layout calculator service used to compute layout information for the test package.</param>
        public TestPackageService(IatTest iat, TestPackage testPackage, IImageGenerationService imageService,
            IProjectPackageService package, ILayoutCalculatorService layout)
        {
            _iat = iat;
            _testPackage = testPackage;
            _imageService = imageService;
            _package = package;
            _layout = layout;
        }

        /// <summary>
        /// Generates and renders item slide images for each trial in all blocks, adding the resulting JPEG files to the
        /// test package manifest.
        /// </summary>
        /// <remarks>This method processes all blocks and their associated trials, rendering visual
        /// representations that include stimuli and response options. Each slide is saved as a JPEG image and
        /// registered in the test package manifest. The method assumes that all referenced resources are present and
        /// accessible; missing resources will result in exceptions.</remarks>
        /// <exception cref="KeyNotFoundException">Thrown if a required trial, stimulus, or formatted text resource cannot be found by its identifier.</exception>
        private void ProcessItemSlides()
        {
            var rects = _layout.GetFinalRects(_iat.Layout);
            var ar = rects.Interior.Width / rects.Interior.Height;
            var width = 500;
            var height = (int)(500 / ar);
            foreach (var block in _iat.Blocks.OrderBy(e => e.BlockNumber))
            {
                foreach (var trialId in block.TrialIds)
                {
                    var trial = _iat.GetTrialById(trialId) ?? throw new KeyNotFoundException("No trial found with ID " + trialId);
                    var visual = new DrawingVisual();
                    using (var dc = visual.RenderOpen())
                    {
                        dc.DrawRectangle(Brushes.Black, null, rects.Interior);
                        if (trial.StimulusId != Guid.Empty)
                        {
                            var stimulus = _iat.GetStimulusById(trial.StimulusId) ?? throw new KeyNotFoundException("No stimulus found with ID " + trial.StimulusId);
                            if (stimulus is ImageStimulus imageStimulus)
                            {
                                BitmapImage stimulusBmp = new BitmapImage();
                                var imageData = _package.GetImageBytes(imageStimulus.Id);
                                var imageStream = new MemoryStream(imageData);
                                stimulusBmp.BeginInit();
                                stimulusBmp.StreamSource = imageStream;
                                stimulusBmp.CacheOption = BitmapCacheOption.OnLoad;
                                stimulusBmp.EndInit();
                                stimulusBmp.Freeze();
                                double arImage = stimulusBmp.PixelWidth / (double)stimulusBmp.PixelHeight;
                                int renderWidth, renderHeight;
                                if (arImage > ar)
                                {
                                    renderWidth = (int)(rects.Interior.Width);
                                    renderHeight = (int)(rects.Interior.Width / arImage);
                                }
                                else
                                {
                                    renderHeight = (int)(rects.Interior.Height);
                                    renderWidth = (int)(rects.Interior.Height * arImage);
                                }
                                dc.DrawImage(stimulusBmp, rects.Stimulus);
                            }
                            else if (stimulus is TextStimulus textStimulus)
                            {
                                BitmapSource stimulusBmp = _imageService.RenderTextToBitmap(textStimulus);
                                dc.DrawImage(stimulusBmp, rects.Stimulus);
                            }
                        }

                        var leftResponseId = block.LeftResponseId;
                        var leftResponse = _iat.GetFormattedTextById(leftResponseId) ?? throw new KeyNotFoundException("No formatted text found with ID " + leftResponseId);
                        var leftResponseBmp = _imageService.RenderTextToBitmap(leftResponse);
                        dc.DrawImage(leftResponseBmp, rects.LeftKey);

                        var rightResponseId = block.RightResponseId;
                        var rightResponse = _iat.GetFormattedTextById(rightResponseId) ?? throw new KeyNotFoundException("No formatted text found with ID " + rightResponseId);
                        var rightResponseBmp = _imageService.RenderTextToBitmap(rightResponse);
                        dc.DrawImage(rightResponseBmp, rects.RightKey);

                        var blockInstructionsId = block.BlockInstructionsId;
                        var blockInstructions = _iat.GetFormattedTextById(blockInstructionsId) ?? throw new KeyNotFoundException("No formatted text found with ID " + blockInstructionsId);
                        var blockInstructionsBmp = _imageService.RenderTextToBitmap(blockInstructions);
                        dc.DrawImage(blockInstructionsBmp, rects.BlockInstructions);

                        if (trial.KeyedDirection == KeyedDirection.Left)
                        {
                            var outlineBmp = _imageService.RenderKeyOutline();
                            dc.DrawImage(outlineBmp, rects.LeftKey);
                        }
                        else if (trial.KeyedDirection == KeyedDirection.Right)
                        {
                            var outlineBmp = _imageService.RenderKeyOutline();
                            dc.DrawImage(outlineBmp, rects.RightKey);
                        }
                    }
                    var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window());
                    RenderTargetBitmap renderedBmp = new RenderTargetBitmap((int)rects.Interior.Width, (int)rects.Interior.Height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
                    renderedBmp.Render(visual);
                    renderedBmp.Freeze();
                    var slideBmp = _imageService.GetResizedBitmap(renderedBmp, width, height);
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder()
                    {
                        QualityLevel = 90
                    };
                    using var memStream = new MemoryStream();
                    encoder.Frames.Add(BitmapFrame.Create(slideBmp));
                    _testPackage.SlideManifest.AddFile(new ManifestFile()
                    {
                        Path = $"slide{_testPackage.SlideManifest.Contents.Count + 1}.jpg",
                        Size = memStream.Length,
                        ResourceType = ManifestFile.EResourceType.itemSlide,
                        ResourceId = _testPackage.SlideManifest.Contents.Count + 1,
                        MimeType = "image/jpeg",
                        Content = memStream.ToArray()
                    });
                }
            }
        }

        /// <summary>
        /// Processes the specified formatted text and adds its rendered image to the file manifest if it has not
        /// already been processed.
        /// </summary>
        /// <remarks>This method ensures that each unique formatted text is rendered to an image only
        /// once. If the text has not been processed, it is rendered, encoded as a PNG, and added to the file manifest.
        /// Subsequent calls with the same text identifier will not re-render or add duplicate images.</remarks>
        /// <param name="text">The formatted text to be rendered and added to the manifest. Cannot be null.</param>
        /// <param name="idDictionary">A dictionary mapping unique text identifiers to resource IDs. Used to track which texts have already been
        /// processed. Cannot be null.</param>
        private void ProcessFormattedText(IFormattedText text, Dictionary<Guid, int> idDictionary)
        {
            if (!idDictionary.ContainsKey(text.Id))
            {
                idDictionary[text.Id] = idDictionary.Count + 1;
                var bmp = _imageService.RenderTextToBitmap(text);
                var visual = new DrawingVisual();
                using(var dc = visual.RenderOpen())
                {
                    dc.DrawImage(bmp, new System.Windows.Rect(0, 0, bmp.PixelWidth, bmp.PixelHeight));
                }
                var dpi = VisualTreeHelper.GetDpi(Application.Current.MainWindow ?? new Window());
                var renderedBmp = new RenderTargetBitmap(bmp.PixelWidth, bmp.PixelHeight, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);    
                renderedBmp.Render(visual);
                renderedBmp.Freeze();
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
                {
                    Interlace = PngInterlaceOption.On
                };
                pngEncoder.Frames.Add(BitmapFrame.Create(renderedBmp));
                using var memStream = new MemoryStream();
                pngEncoder.Save(memStream);
                _testPackage.FileManifest.AddFile(new ManifestFile()
                {
                    Path = $"image{idDictionary[text.Id]}.png",
                    Size = memStream.Length,
                    ResourceType = ManifestFile.EResourceType.image,
                    ResourceId = idDictionary[text.Id],
                    MimeType = $"image/png",
                    Content = memStream.ToArray()
                });
            }
        }

        /// <summary>
        /// Processes a block of instruction screens, generating display items and events for each instruction and its
        /// associated responses.
        /// </summary>
        /// <remarks>This method updates the test package by adding display items and events corresponding
        /// to each instruction screen and its related components. The method modifies the provided <paramref
        /// name="idDictionary"/> to include any new display item IDs generated during processing.</remarks>
        /// <param name="instructionIds">A list of unique identifiers representing the instruction screens to process. The order of IDs determines
        /// the sequence in which instructions are handled.</param>
        /// <param name="idDictionary">A dictionary mapping unique identifiers to display item IDs. This dictionary is updated with new mappings as
        /// additional display items are created during processing.</param>
        /// <exception cref="ArgumentException">Thrown if an instruction screen or stimulus cannot be found for a specified identifier in <paramref
        /// name="instructionIds"/>.</exception>
        /// <exception cref="ArgumentNullException">Thrown if a required formatted text or response key cannot be found when processing a keyed or mock item
        /// instruction screen.</exception>
        private void ProcessInstructionBlock(List<Guid> instructionIds, Dictionary<Guid, int> idDictionary)
        {
            var rects = _layout.GetFinalRects(_iat.Layout);
            int x = 0, y = 0, width = 0, height = 0;
            string filename = string.Empty;
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder()
            {
                QualityLevel = 90
            };
            DisplayItem instructions, continueInstructions, leftResponse, rightResponse, stimulus;
            var memStream = new MemoryStream();
            _testPackage.Events.Add(new BeginInstructionBlock()
            {
                NumInstructionScreens = instructionIds.Count
            });
            foreach (var instructionId in instructionIds)
            {
                var screen = _iat.GetInstructionScreenById(instructionId) ?? throw new ArgumentException($"No instruction screen found with ID {instructionId}");
                if (screen is Domain.TextInstructionScreen || screen is Domain.KeyedInstructionScreen || screen is Domain.MockItemInstructionScreen)
                {
                    ProcessFormattedText(screen, idDictionary);
                    instructions = new DisplayItem()
                    {
                        Filename = $"image{idDictionary[screen.Id]}.png",
                        Id = idDictionary[screen.Id],
                        X = (int)rects.TextInstructions.X,
                        Y = (int)rects.TextInstructions.Y,
                        Width = (int)rects.TextInstructions.Width,
                        Height = (int)rects.TextInstructions.Height
                    };

                    ProcessFormattedText(screen.ContinueInstructions, idDictionary);
                    continueInstructions = new DisplayItem()
                    {
                        Filename = $"image{idDictionary[screen.ContinueInstructions.Id]}.png",
                        Id = idDictionary[screen.ContinueInstructions.Id],
                        X = (int)rects.ContinueInstructions.X,
                        Y = (int)rects.ContinueInstructions.Y,
                        Width = (int)rects.ContinueInstructions.Width,
                        Height = (int)rects.ContinueInstructions.Height
                    };
                    if (screen is Domain.TextInstructionScreen)
                    {
                        ConfigFile.TextInstructionScreen textInstructionsEvent = new ConfigFile.TextInstructionScreen()
                        {
                            ContinueInstructionsDisplayID = continueInstructions.Id,
                            InstructionsDisplayID = instructions.Id
                        };

                        _testPackage.Events.Add(textInstructionsEvent);
                    }
                    _testPackage.DisplayItems.Add(instructions);
                    _testPackage.DisplayItems.Add(continueInstructions);
                    if (screen is Domain.KeyedInstructionScreen || screen is Domain.MockItemInstructionScreen)
                    {
                        var leftResponseId = (screen is Domain.KeyedInstructionScreen keyedScreen) ? keyedScreen.LeftResponseId : ((Domain.MockItemInstructionScreen)screen).LeftResponseKeyId;
                        var rightResponseId = (screen is Domain.KeyedInstructionScreen) ? ((Domain.KeyedInstructionScreen)screen).RightResponseId : ((Domain.MockItemInstructionScreen)screen).RightResponseKeyId;
                        if (!idDictionary.ContainsKey(leftResponseId))
                        {
                            idDictionary[leftResponseId] = idDictionary.Count + 1;
                            ProcessFormattedText(_iat.GetFormattedTextById(leftResponseId) ?? throw new ArgumentNullException(), idDictionary);
                        }
                        leftResponse = new DisplayItem()
                        {
                            Filename = $"image{idDictionary[leftResponseId]}.png",
                            Id = idDictionary[leftResponseId],
                            X = (int)rects.LeftKey.X,
                            Y = (int)rects.LeftKey.Y,
                            Width = (int)rects.LeftKey.Width,
                            Height = (int)rects.LeftKey.Height
                        };

                        if (!idDictionary.ContainsKey(rightResponseId))
                        {
                            idDictionary[rightResponseId] = idDictionary.Count + 1;
                            ProcessFormattedText(_iat.GetFormattedTextById(rightResponseId) ?? throw new ArgumentNullException(), idDictionary);
                        }
                        rightResponse = new DisplayItem()
                        {
                            Filename = $"image{idDictionary[rightResponseId]}.png",
                            Id = idDictionary[rightResponseId],
                            X = (int)rects.RightKey.X,
                            Y = (int)rects.RightKey.Y,
                            Width = (int)rects.RightKey.Width,
                            Height = (int)rects.RightKey.Height
                        };

                        if (screen is Domain.KeyedInstructionScreen)
                        {
                            _testPackage.Events.Add(new ConfigFile.KeyedInstructionScreen()
                            {
                                LeftResponseDisplayID = leftResponse.Id,
                                RightResponseDisplayID = rightResponse.Id,
                                ContinueInstructionsDisplayID = continueInstructions.Id,
                                InstructionsDisplayID = instructions.Id
                            });
                        }
                        if (screen is Domain.MockItemInstructionScreen mockItemScreen)
                        {
                            var mockItemStimulus = _iat.GetStimulusById(mockItemScreen.StimulusId) ??
                                throw new ArgumentException($"No stimulus found with ID {mockItemScreen.StimulusId}");
                            if (mockItemStimulus is ImageStimulus imageStimulus)
                                ProcessImageStimulus(imageStimulus, idDictionary);
                            else
                                ProcessTextStimulus((TextStimulus)mockItemStimulus, idDictionary);
                            _testPackage.Events.Add(new ConfigFile.MockItemInstructionScreen()
                            {
                                StimulusDisplayID = idDictionary[mockItemScreen.StimulusId],
                                LeftResponseDisplayID = leftResponse.Id,
                                RightResponseDisplayID = rightResponse.Id,
                                ContinueInstructionsDisplayID = continueInstructions.Id,
                                ErrorMarkIsDisplayed = mockItemScreen.ShowErrorMark,
                                OutlineLeftResponse = mockItemScreen.OutlineCorrectResponse && (mockItemScreen.KeyedDirection == KeyedDirection.Left),
                                OutlineRightResponse = mockItemScreen.OutlineCorrectResponse && (mockItemScreen.KeyedDirection == KeyedDirection.Right)
                            });
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes an image stimulus by generating a resized bitmap, assigning a unique resource identifier, and
        /// adding the image to the test package manifest if it has not already been processed.
        /// </summary>
        /// <remarks>This method ensures that each image stimulus is only processed and added to the
        /// manifest once. The image is resized to fit the designated stimulus area while maintaining its aspect ratio.
        /// The resulting image is encoded as either JPEG or PNG based on its type and added to the file manifest with
        /// the appropriate metadata.</remarks>
        /// <param name="stimulus">The image stimulus to process and include in the test package manifest.</param>
        /// <param name="idDictionary">A dictionary mapping stimulus identifiers to unique resource IDs. This dictionary is updated with new
        /// entries for unprocessed stimuli.</param>
        private void ProcessImageStimulus(ImageStimulus stimulus, Dictionary<Guid, int> idDictionary)
        {
            var stimulusId = stimulus.Id;
            var resourceId = 0;
            var rects = _layout.GetFinalRects(_iat.Layout);
            if (!idDictionary.ContainsKey(stimulusId))
            {
                idDictionary[stimulusId] = idDictionary.Count + 1;
                resourceId = idDictionary.Count;
                if (stimulus is ImageStimulus)
                {
                    var imageData = _package.GetImageBytes(stimulusId);
                    var memStream = new MemoryStream(imageData);
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memStream;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    double arStimulusRect = rects.Stimulus.Width / rects.Stimulus.Height;
                    double arImage = bitmapImage.PixelWidth / (double)bitmapImage.PixelHeight;
                    int width, height;
                    if (arImage > arStimulusRect)
                    {
                        width = (int)rects.Stimulus.Width;
                        height = (int)(rects.Stimulus.Width / arImage);
                    }
                    else
                    {
                        height = (int)rects.Stimulus.Height;
                        width = (int)(rects.Stimulus.Height * arImage);
                    }
                    var bmp = _imageService.GetResizedBitmap(bitmapImage, width, height);
                    width = bmp.PixelWidth;
                    height = bmp.PixelHeight;
                    int x = (int)(rects.Stimulus.X + (rects.Stimulus.Width - width) / 2);
                    int y = (int)(rects.Stimulus.Y + (rects.Stimulus.Height - height) / 2);
                    string filename;
                    if (_package.GetImageType(stimulusId) == "jpeg" || _package.GetImageType(stimulusId) == "jpg")
                    {
                        JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder()
                        {
                            QualityLevel = 90
                        };
                        jpegEncoder.Frames.Add(BitmapFrame.Create(bmp));
                        memStream.Dispose(); memStream = new MemoryStream();
                        jpegEncoder.Save(memStream);
                        filename = $"image{idDictionary[stimulusId]}.jpg";
                    }
                    else
                    {
                        PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
                        {
                            Interlace = PngInterlaceOption.On
                        };
                        pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
                        memStream.Dispose(); memStream = new MemoryStream();
                        pngEncoder.Save(memStream);
                        filename = $"image{idDictionary[stimulusId]}.png";
                    }
                    _testPackage.FileManifest.AddFile(new ManifestFile()
                    {
                        Path = filename,
                        Size = memStream.Length,
                        ResourceType = ManifestFile.EResourceType.image,
                        ResourceId = idDictionary[stimulusId],
                        MimeType = $"image/{_package.GetImageType(stimulusId)}",
                        Content = memStream.ToArray()
                    });
                }
            }
        }

        /// <summary>
        /// Processes a text stimulus by rendering it to an image, assigning a unique resource identifier, and adding
        /// the resulting image and display information to the test package if it has not already been processed.
        /// </summary>
        /// <remarks>If the specified stimulus has already been processed and exists in the dictionary,
        /// this method does not perform any additional actions. The method ensures that each text stimulus is rendered
        /// and added only once to the test package.</remarks>
        /// <param name="stimulus">The text stimulus to be rendered and added to the test package. Cannot be null.</param>
        /// <param name="idDictionary">A dictionary mapping stimulus identifiers to unique resource IDs. Used to track and assign resource IDs for
        /// each stimulus. Cannot be null.</param>
        private void ProcessTextStimulus(TextStimulus stimulus, Dictionary<Guid, int> idDictionary)
        {
            var stimulusId = stimulus.Id;
            var resourceId = 0;
            var rects = _layout.GetFinalRects(_iat.Layout);
            if (!idDictionary.ContainsKey(stimulusId))
            {
                idDictionary[stimulusId] = idDictionary.Count + 1;
                resourceId = idDictionary.Count;
                var bmp = _imageService.RenderTextToBitmap(stimulus);
                int width = bmp.PixelWidth;
                int height = bmp.PixelHeight;
                int x = (int)(rects.Stimulus.X + (rects.Stimulus.Width - width) / 2);
                int y = (int)(rects.Stimulus.Y + (rects.Stimulus.Height - height) / 2);
                string filename = $"image{idDictionary[stimulusId]}.png";
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
                {
                    Interlace = PngInterlaceOption.On
                };
                pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
                using var memStream = new MemoryStream();
                pngEncoder.Save(memStream);
                _testPackage.FileManifest.AddFile(new ManifestFile()
                {
                    Path = filename,
                    Size = memStream.Length,
                    ResourceType = ManifestFile.EResourceType.image,
                    ResourceId = idDictionary[stimulusId],
                    MimeType = $"image/png",
                    Content = memStream.ToArray()
                });
                _testPackage.DisplayItems.Add(new DisplayItem()
                {
                    Id = idDictionary[stimulusId],
                    Filename = filename,
                    X = (int)rects.Stimulus.X,
                    Y = (int)rects.Stimulus.Y,
                    Width = bmp.PixelWidth,
                    Height = bmp.PixelHeight
                });

            }
        }

        /// <summary>
        /// Processes the specified block by rendering associated images and instructions, updating the file manifest,
        /// and adding display items and events to the test package.
        /// </summary>
        /// <remarks>This method generates image resources for block responses and instructions, updates
        /// the file manifest and display items, and processes all trials within the block. The idDictionary parameter
        /// is modified to include any new resource IDs encountered during processing.</remarks>
        /// <param name="block">The block to process, containing instructions, response identifiers, and trial information to be rendered
        /// and added to the test package.</param>
        /// <param name="idDictionary">A dictionary mapping unique resource identifiers to integer IDs. This dictionary is updated with new
        /// resources as they are processed.</param>
        /// <exception cref="ArgumentException">Thrown if a trial referenced by the block does not exist in the test data.</exception>
        private void ProcessBlock(Block block, Dictionary<Guid, int> idDictionary)
        {
            int x = 0, y = 0, width = 0, height = 0;
            string filename = string.Empty;
            var rects = _layout.GetFinalRects(_iat.Layout);
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder()
            {
                QualityLevel = 90
            };
            if (block.InstructionsIds.Count > 0)
                ProcessInstructionBlock(block.InstructionsIds, idDictionary);

            var memStream = new MemoryStream();
            var bmp = _imageService.RenderKeyToBitmap(block.LeftResponseId);
            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
            pngEncoder.Save(memStream);
            if (!idDictionary.ContainsKey(block.LeftResponseId)) idDictionary[block.LeftResponseId] = idDictionary.Count + 1;
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"image{idDictionary[block.LeftResponseId]}.png",
                Size = memStream.Length, // will be updated later
                ResourceType = ManifestFile.EResourceType.image,
                ResourceId = idDictionary[block.LeftResponseId],
                MimeType = $"image/png",
                Content = memStream.ToArray()
            });
            memStream.Dispose();

            x = 0; y = 0;
            _testPackage.DisplayItems.Add(new DisplayItem()
            {
                Filename = $"image{idDictionary[block.LeftResponseId]}.png",
                Id = idDictionary[block.LeftResponseId],
                X = x,
                Y = y,
                Width = bmp.PixelWidth,
                Height = bmp.PixelHeight
            });

            memStream = new MemoryStream();
            bmp = _imageService.RenderKeyToBitmap(block.RightResponseId);
            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
            pngEncoder.Save(memStream);
            if (!idDictionary.ContainsKey(block.RightResponseId)) idDictionary[block.RightResponseId] = idDictionary.Count + 1;
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"image{idDictionary[block.RightResponseId]}.png",
                Size = memStream.Length, // will be updated later
                ResourceType = ManifestFile.EResourceType.image,
                ResourceId = idDictionary[block.RightResponseId],
                MimeType = $"image/png",
                ReferenceIds = new List<int> { block.BlockNumber },
                Content = memStream.ToArray()
            });
            memStream.Dispose(); memStream = new MemoryStream();

            _testPackage.DisplayItems.Add(new DisplayItem()
            {
                Filename = $"image{idDictionary[block.RightResponseId]}.png",
                Id = idDictionary[block.RightResponseId],
                X = (int)rects.RightKey.X,
                Y = (int)rects.RightKey.Y,
                Width = bmp.PixelWidth,
                Height = bmp.PixelHeight
            });

            var instructions = _iat.GetFormattedTextById(block.BlockInstructionsId);
            bmp = _imageService.RenderTextToBitmap(instructions);
            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
            pngEncoder.Save(memStream);
            if (!idDictionary.ContainsKey(block.BlockInstructionsId)) idDictionary[block.BlockInstructionsId] = idDictionary.Count + 1;
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"image{idDictionary[block.BlockInstructionsId]}.png",
                Size = memStream.Length, // will be updated later
                ResourceType = ManifestFile.EResourceType.image,
                ResourceId = idDictionary[block.BlockInstructionsId],
                MimeType = $"image/png",
                ReferenceIds = new List<int> { block.BlockNumber },
                Content = memStream.ToArray()
            });

            _testPackage.DisplayItems.Add(new DisplayItem()
            {
                Filename = $"image{idDictionary[block.BlockInstructionsId]}.png",
                Id = idDictionary[block.BlockInstructionsId],
                X = (int)rects.BlockInstructions.X,
                Y = (int)rects.BlockInstructions.Y,
                Width = width,
                Height = height
            });

            var layoutRects = _layout.GetFinalRects(_iat.Layout);
            foreach (var trialId in block.TrialIds)
            {
                var trial = _iat.GetTrialById(trialId) ?? throw new ArgumentException($"No trial found with ID {trialId}");
                var stimulusId = trial?.StimulusId ?? Guid.Empty;
                var stimulus = _iat.GetStimulusById(stimulusId);
                var resourceId = 0;
                if (!idDictionary.ContainsKey(stimulusId))
                {
                    idDictionary[stimulusId] = idDictionary.Count + 1;
                    resourceId = idDictionary.Count;
                    if (stimulus is ImageStimulus imageStimulus)
                    {
                        ProcessImageStimulus(imageStimulus, idDictionary);
                    }
                    else if (stimulus is TextStimulus textStimulus)
                    {
                        ProcessTextStimulus(textStimulus, idDictionary);
                    }
                }
                var iatTrial = new ConfigFile.Trial()
                {
                    StimulusDisplayID = idDictionary[stimulusId],
                    KeyedDir = trial?.KeyedDirection ?? KeyedDirection.None,
                    BlockNum = block.BlockNumber,
                    OriginatingBlock = (resourceId == idDictionary.Count) ? block.BlockNumber :
                        _testPackage.Events.Where(e => e.EventType == EventType.Trial).Cast<ConfigFile.Trial>().Where(e => e.StimulusDisplayID == resourceId).Select(e => e.BlockNum).FirstOrDefault(),
                    ItemNum = _testPackage.Events.Where(e => e.EventType == EventType.Trial).Count()
                };
                _testPackage.Events.Add(iatTrial);
            }
            _testPackage.Events.Add(new EndIATBlock());
        }

        /// <summary>
        /// Generates and adds test image files and display items to the test package for use in IAT test scenarios.
        /// </summary>
        /// <remarks>This method creates image resources such as error marks and key outlines, encodes
        /// them as PNG files, and registers them in the test package's manifest and display item collections. The
        /// generated files are intended for use during test execution and visualization.</remarks>
        /// <param name="iat">The IAT test instance for which the test files and display items are created. Cannot be null.</param>
        public void CreateFilesForTest(IatTest iat)
        {
            var rects = _layout.GetFinalRects(_iat.Layout);
            var manifest = new Manifest();
            var pngEncoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            var jpegEncoder = new JpegBitmapEncoder();
            var errorMark = new Domain.FormattedText()
            {
                Id = Guid.NewGuid(),
                Text = "X",
                Style = new TextStyle()
                {
                    FontSize = 48,
                    FontFamily = "Arial",
                    FontColor = Colors.Red
                },
                LayoutItem = LayoutItem.ErrorMark
            };
            var errorMarkBmp = _imageService.RenderTextToBitmap(errorMark);
            var memoryStream = new MemoryStream();
            pngEncoder.Frames.Add(BitmapFrame.Create(errorMarkBmp));
            pngEncoder.Save(memoryStream);
            byte[] imageBytes = memoryStream.ToArray();
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"ErrorMark.png",
                Size = imageBytes.Length,
                ResourceType = ManifestFile.EResourceType.errorMark,
                ResourceId = 1000,
                MimeType = "image/png",
                Content = imageBytes
            });
            memoryStream.Dispose();
            _testPackage.DisplayItems.Add(new DisplayItem()
            {
                Filename = "ErrorMark.png",
                Id = 1000,
                X = (int)rects.ErrorMark.X,
                Y = (int)rects.ErrorMark.Y,
                Width = (int)rects.ErrorMark.Width,
                Height = (int)rects.ErrorMark.Height
            });


            memoryStream = new MemoryStream();
            var keyOutlineBmp = _imageService.RenderKeyOutline();
            pngEncoder.Frames.Clear();
            pngEncoder.Frames.Add(BitmapFrame.Create(keyOutlineBmp));
            pngEncoder.Save(memoryStream);
            imageBytes = memoryStream.ToArray();
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"KeyOutline.png",
                Size = imageBytes.Length,
                ResourceType = ManifestFile.EResourceType.keyOutline,
                ResourceId = 1001,
                MimeType = "image/png",
                Content = imageBytes
            });
            _testPackage.DisplayItems.Add(new DisplayItem()
            {
                Filename = "KeyOutline.png",
                Id = 1001,
                X = (int)rects.LeftKey.X,
                Y = (int)rects.LeftKey.Y,
                Width = (int)rects.LeftKey.Width,
                Height = (int)rects.LeftKey.Height
            });
            _testPackage.DisplayItems.Add(new DisplayItem()
            {
                Filename = "KeyOutline.png",
                Id = 1002,
                X = (int)rects.RightKey.X,
                Y = (int)rects.RightKey.Y,
                Width = (int)rects.RightKey.Width,
                Height = (int)rects.RightKey.Height
            });

            memoryStream.Dispose();
        }

        /// <summary>
        /// Maps the current IAT configuration and layout to the server configuration format, preparing all necessary
        /// files and data structures for server deployment.
        /// </summary>
        /// <remarks>Call this method before deploying the IAT to the server to ensure that all
        /// configuration files and layout data are correctly generated. The method processes all blocks and items, and
        /// prepares the configuration required for server-side operation.</remarks>
        /// <exception cref="Exception">Thrown if an invalid block number is encountered during the mapping process.</exception>
        public void MapToServerConfig()
        {
            CreateFilesForTest(_iat);
            var rects = _layout.GetFinalRects(_iat.Layout);
            var configFile = new IATConfigFile()
            {
                Name = _iat.Name,
                NumIATItems = _iat.Trials.Count,
                ErrorMarkID = 1000,
                LeftKeyOutlineID = 1001,
                RightKeyOutlineID = 1002,
                Layout = new ConfigFile.Layout()
                {
                    InteriorWidth = (int)rects.Interior.Width,
                    InteriorHeight = (int)rects.Interior.Height,
                    ResponseWidth = rects.Interior.Width > rects.Interior.Height ? (int)(rects.Interior.Width + 50) : (int)(rects.Interior.Height + 50),
                    ResponseHeight = rects.Interior.Width > rects.Interior.Height ? (int)(rects.Interior.Width + 50) : (int)(rects.Interior.Height + 50)
                }
            };
            Dictionary<Guid, int> idDictionary = new Dictionary<Guid, int>();
            foreach (var block in iat.Blocks.OrderBy<Block, int>(b => b.BlockNumber))
            {
                var beginBlock = new BeginIATBlock()
                {
                    BlockNumber = block.BlockNumber,
                    AlternatedWith = block.BlockNumber switch
                    {
                        1 => -1,
                        2 => -1,
                        3 => 6,
                        4 => 7,
                        5 => -1,
                        6 => 3,
                        7 => 4,
                        _ => throw new Exception($"Invalid block number {block.BlockNumber}")
                    },
                    NumPresentations = block.BlockNumber switch
                    {
                        1 => 10,
                        2 => 10,
                        3 => 10,
                        4 => 20,
                        5 => 10,
                        6 => 10,
                        7 => 20,
                        _ => throw new Exception($"Invalid block number {block.BlockNumber}"),
                    },
                    NumItems = block.TrialIds.Count
                };
                ProcessBlock(block, idDictionary);
            }
            ProcessItemSlides();
        }
    }
}
