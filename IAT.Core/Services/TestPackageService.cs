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

namespace IAT.Core.Services
{
    public interface ITestPackageService
    {
        IATConfigFile MapToServerConfig(IatTest iat);
    }

    internal class TestPackageService
    {
        private readonly IatTest _iat;
        private readonly TestPackage _testPackage;
        private readonly IImageGenerationService _imageService;
        private readonly IProjectPackageService _package;
        private readonly ILayoutCalculatorService _layout;

        public TestPackageService(IatTest iat, TestPackage testPackage, IImageGenerationService imageService,
            IProjectPackageService package, ILayoutCalculatorService layout)
        {
            _iat = iat;
            _testPackage = testPackage;
            _imageService = imageService;
            _package = package;
            _layout = layout;
        }

        private void ProcessFormattedText(IFormattedText text, Dictionary<Guid, int> idDictionary)
        {
            if (!idDictionary.ContainsKey(text.Id))
            {
                idDictionary[text.Id] = idDictionary.Count + 1;
                var bmp = _imageService.RenderTextToBitmap(text);

                PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
                {
                    Interlace = PngInterlaceOption.On
                };
                pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
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

        public void ProcessImageStimulus(ImageStimulus stimulus, Dictionary<Guid, int> idDictionary)
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

        public void ProcessTextStimulus(TextStimulus stimulus, Dictionary<Guid, int> idDictionary)
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

        public void MapToServerConfig(IatTest iat)
        {
            var rects = _layout.GetFinalRects(_iat.Layout);
            var configFile = new IATConfigFile()
            {
                Name = iat.Name,
                NumIATItems = iat.Trials.Count,
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
        }
    }
}
