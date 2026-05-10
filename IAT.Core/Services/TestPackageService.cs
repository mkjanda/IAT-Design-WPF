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

        public void ProcessBlock(Block block, Dictionary<Guid, int> idDictionary)
        {
            int x = 0, y = 0, width = 0, height = 0, id = 0;
            string filename = string.Empty;
            PngBitmapEncoder pngEncoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder()
            {
                QualityLevel = 90
            };
            var memStream = new MemoryStream();
            var bmp = _imageService.RenderKeyToBitmap(block.LeftResponseId);
            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
            pngEncoder.Save(memStream);
            if (!idDictionary.ContainsKey(block.LeftResponseId)) idDictionary[block.LeftResponseId] = idDictionary.Count + 1;
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"image{idDictionary.Count}.png",
                Size = memStream.Length, // will be updated later
                ResourceType = ManifestFile.EResourceType.image,
                ResourceId = idDictionary.Count,
                MimeType = $"image/png",
                Content = memStream.ToArray()
            });
            memStream.Dispose();

            memStream = new MemoryStream();
            bmp = _imageService.RenderKeyToBitmap(block.RightResponseId);
            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
            pngEncoder.Save(memStream);
            if (!idDictionary.ContainsKey(block.RightResponseId)) idDictionary[block.RightResponseId] = idDictionary.Count + 1;
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"image{idDictionary.Count}.png",
                Size = memStream.Length, // will be updated later
                ResourceType = ManifestFile.EResourceType.image,
                ResourceId = idDictionary.Count,
                MimeType = $"image/png",
                ReferenceIds = new List<int> { block.BlockNumber },
                Content = memStream.ToArray()
            });
            memStream.Dispose(); memStream = new MemoryStream();

            var instructions = _iat.GetFormattedTextById(block.BlockInstructionsId);
            bmp = _imageService.RenderTextToBitmap(instructions);
            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
            pngEncoder.Save(memStream);
            if (!idDictionary.ContainsKey(block.BlockInstructionsId)) idDictionary[block.BlockInstructionsId] = idDictionary.Count + 1;
            _testPackage.FileManifest.AddFile(new ManifestFile()
            {
                Path = $"image{idDictionary.Count}.png",
                Size = memStream.Length, // will be updated later
                ResourceType = ManifestFile.EResourceType.image,
                ResourceId = idDictionary.Count,
                MimeType = $"image/png",
                ReferenceIds = new List<int> { block.BlockNumber },
                Content = memStream.ToArray()
            });


            var layoutRects = _layout.GetFinalRects(_iat.Layout);
            foreach (var trialId in block.TrialIds)
            {
                byte[] imageData;
                var trial = _iat.GetTrialById(trialId) ?? throw new ArgumentException($"No trial found with ID {trialId}");
                var stimulusId = trial?.StimulusId ?? Guid.Empty;
                var stimulus = _iat.GetStimulusById(stimulusId);
                var rects = _layout.GetFinalRects(_iat.Layout);
                var resourceId = 0;
                if (!idDictionary.ContainsKey(stimulusId)) {
                    idDictionary[stimulusId] = idDictionary.Count + 1;
                    resourceId = idDictionary.Count;
                    if (stimulus is ImageStimulus) {
                        imageData = _package.GetImageBytes(stimulusId);
                        memStream = new MemoryStream(imageData);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memStream;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        double arStimulusRect = rects.Stimulus.Width / rects.Stimulus.Height;
                        double arImage = bitmapImage.PixelWidth / (double)bitmapImage.PixelHeight;
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

                        bmp = _imageService.GetResizedBitmap(bitmapImage, width, height);
                        width = bmp.PixelWidth;
                        height = bmp.PixelHeight;
                        x = (int)(rects.Stimulus.Width - width) / 2;
                        y = (int)(rects.Stimulus.Height - height) / 2;
                        if (_package.GetImageType(stimulusId) == "jpeg" || _package.GetImageType(stimulusId) == "jpg")
                        {
                            jpegEncoder.Frames.Clear();
                            jpegEncoder.Frames.Add(BitmapFrame.Create(bmp));
                            memStream.Dispose(); memStream = new MemoryStream();
                            jpegEncoder.Save(memStream);
                            filename = $"image{idDictionary.Count}.jpg";
                        } else
                        {
                            pngEncoder.Frames.Clear();
                            pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
                            memStream.Dispose(); memStream = new MemoryStream();
                            pngEncoder.Save(memStream);
                            filename = $"image{idDictionary.Count}.png";
                        }
                        _testPackage.FileManifest.Contents.Add(new ManifestFile()
                        {
                            Path = filename,
                            Size = memStream.Length,
                            ResourceType = ManifestFile.EResourceType.image,
                            ResourceId = resourceId,
                            MimeType = $"image/{_package.GetImageType(stimulusId)}",
                            Content = memStream.ToArray()
                        });
                    }
                    else if (stimulus is TextStimulus textStimulus)
                    {
                        bmp = _imageService.RenderTextToBitmap(textStimulus);
                        width = bmp.PixelWidth;
                        height = bmp.PixelHeight;
                        x = (int)(rects.Stimulus.Width - width) / 2;
                        y = (int)(rects.Stimulus.Height - height) / 2;
                        filename = $"image{idDictionary.Count}.png";
                        pngEncoder.Frames.Clear();
                        pngEncoder.Frames.Add(BitmapFrame.Create(bmp));
                        memStream.Dispose(); memStream = new MemoryStream();
                        pngEncoder.Save(memStream);
                        imageData = memStream.ToArray();
                        _testPackage.FileManifest.Contents.Add(new ManifestFile()
                        {
                            Path = filename,
                            Size = imageData.Length,
                            ResourceType = ManifestFile.EResourceType.image,
                            ResourceId = resourceId,
                            MimeType = $"image/png",
                            Content = imageData
                        });
                    }
                }
                var iatTrial = new ConfigFile.Trial()
                {
                    StimulusDisplayID = resourceId,
                    KeyedDir = trial?.KeyedDirection ?? KeyedDirection.None,
                    BlockNum = block.BlockNumber,
                    OriginatingBlock = (resourceId == idDictionary.Count) ? block.BlockNumber : 
                        _testPackage.Events.Where(e => e.EventType == EventType.Trial).Cast<ConfigFile.Trial>().Where(e => e.StimulusDisplayID == resourceId).Select(e => e.BlockNum).FirstOrDefault(),
                    ItemNum = _testPackage.Events.Where(e => e.EventType == EventType.Trial).Count()
                };
                var displayItem = new DisplayItem()
                {
                    Id = resourceId,
                    X = x,
                    Y = y,
                    Width = width,
                    Height = height
                };
                _testPackage.Events.Add(iatTrial);
                _testPackage.DisplayItems.Add(displayItem);
            }
        }

        public void CreateFilesForTest(IatTest iat)
        {
            var manifest = new Manifest();
            var pngEncoder = new PngBitmapEncoder()
            {
                Interlace = PngInterlaceOption.On
            };
            var jpegEncoder = new JpegBitmapEncoder();
            var errorMark = new IAT.Core.Domain.FormattedText()
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
            manifest.AddFile(new ManifestFile()
            {
                Path = $"ErrorMark.png",
                Size = imageBytes.Length,
                ResourceType = ManifestFile.EResourceType.errorMark,
                ResourceId = 1000,
                MimeType = "image/png",
                ReferenceIds = new List<int> { 1000 },
                Content = imageBytes
            });
            memoryStream.Dispose();

            memoryStream = new MemoryStream();
            var keyOutlineBmp = _imageService.RenderKeyOutline();
            pngEncoder.Frames.Clear();
            pngEncoder.Frames.Add(BitmapFrame.Create(keyOutlineBmp));
            pngEncoder.Save(memoryStream);
            imageBytes = memoryStream.ToArray();
            manifest.AddFile(new ManifestFile()
            {
                Path = $"KeyOutline.png",
                Size = imageBytes.Length,
                ResourceType = ManifestFile.EResourceType.keyOutline,
                ResourceId = 1001,
                MimeType = "image/png",
                ReferenceIds = new List<int> { 1001, 1002 },
                Content = imageBytes
            });
            memoryStream.Dispose();
            var idDictionary = new Dictionary<Guid, int>();
            foreach (var block in iat.Blocks)
                ProcessBlock(block, manifest, idDictionary);
        }

        private ManifestFile GenerateItemSlide()

        public ConfigFile.IATConfigFile MapToServerConfig(IatTest iat)
        {
            var configFile = new ConfigFile.IATConfigFile()
            {
                Name = iat.Name,
                NumIATItems = iat.Trials.Count
            };
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
                    }
                };
                if (block.InstructionsIds.Count > 0)
                {
                    var beginInstructionBlock = new BeginInstructionBlock()
                    {
                        NumInstructionScreens = block.InstructionsIds.Count
                    };
                    beginInstrutiatblock.BlockNumber switch
                    {

                    }


                        ;
                }
                foreach (var trial in block.Trials)
                {
                    var displayItem = new DisplayItem()
                    {
                        Id = trial.Id,
                        Type = DisplayItemType.Trial,
                        Content = trial.StimulusId.ToString()
                    };
                    configFile.DisplayItemList.Add(displayItem);
                }

        }
    }
}
