using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Models;
using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.ConfigFile;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Export
{
    public interface IBlockExportProcessor
    {
        public void ProcessBlock(Block block, ExportContext exportContext);
    }

    public class BlockExportProcessor : IBlockExportProcessor
    {
        private readonly ImageGenerationService _imageGenerationService;
        private readonly IFileManifestBuilder _fileManifestBuilder;
        private readonly ITextExportProcessor _textExportProcessor;
        private readonly IStimulusExportProcessor _stimulusExportProcessor;
        private readonly IProjectPackageService _projectPackageService;

        public BlockExportProcessor(ImageGenerationService imageGenerationService, IFileManifestBuilder fileManifestBuilder, 
            ITextExportProcessor textExportProcessor, IStimulusExportProcessor stimulusExportProcessor,
            IProjectPackageService projectPackageService)
        {
            _imageGenerationService = imageGenerationService;
            _fileManifestBuilder = fileManifestBuilder;
            _textExportProcessor = textExportProcessor;
            _stimulusExportProcessor = stimulusExportProcessor;
            _projectPackageService = projectPackageService;
        }

        private void ProcessTextInstructionScreen(Domain.TextInstructionScreen screen, ExportContext exportContext)
        {
            _textExportProcessor.ProcessText(screen, exportContext.LayoutRects.TextInstructions, exportContext);
            _textExportProcessor.ProcessText(screen.ContinueInstructions, exportContext.LayoutRects.ContinueInstructions, exportContext);

            exportContext.Events.Add(new ConfigFile.TextInstructionScreen()
            {
                InstructionsDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.Id).Select(di => di.Id).FirstOrDefault(),
                ContinueInstructionsDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.ContinueInstructions.Id).Select(di => di.Id).FirstOrDefault()
            });
        }

        private void ProcessKeyedInstructionsScreen(Domain.KeyedInstructionScreen screen, ExportContext exportContext)
        {
            _textExportProcessor.ProcessText(screen, exportContext.LayoutRects.KeyedInstructions, exportContext);
            _textExportProcessor.ProcessText(screen.ContinueInstructions, exportContext.LayoutRects.ContinueInstructions, exportContext);
            _textExportProcessor.ProcessText(exportContext.Test.GetFormattedTextById(screen.LeftResponseId) ?? throw new ArgumentException(), 
                exportContext.LayoutRects.LeftKey, exportContext);
            _textExportProcessor.ProcessText(exportContext.Test.GetFormattedTextById(screen.RightResponseId) ?? throw new ArgumentException(), 
                exportContext.LayoutRects.RightKey, exportContext);
                
            exportContext.Events.Add(new ConfigFile.KeyedInstructionScreen()
            {
                InstructionsDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.Id).Select(di => di.Id).FirstOrDefault(),
                ContinueInstructionsDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.ContinueInstructions.Id).Select(di => di.Id).FirstOrDefault(),
                LeftResponseDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.LeftResponseId).Select(di => di.Id).FirstOrDefault(),
                RightResponseDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.RightResponseId).Select(di => di.Id).FirstOrDefault()
            });
        }

        private void ProcessMockItemInstructionScreen(Domain.MockItemInstructionScreen screen, ExportContext exportContext)
        {
            _textExportProcessor.ProcessText(screen, exportContext.LayoutRects.MockItemInstructions, exportContext);
            _textExportProcessor.ProcessText(screen.ContinueInstructions, exportContext.LayoutRects.ContinueInstructions, exportContext);
            _textExportProcessor.ProcessText(exportContext.Test.GetFormattedTextById(screen.LeftResponseId) ?? throw new ArgumentException(), exportContext.LayoutRects.LeftKey, exportContext);
            _textExportProcessor.ProcessText(exportContext.Test.GetFormattedTextById(screen.RightResponseId) ?? throw new ArgumentException(), exportContext.LayoutRects.RightKey, exportContext);
            _stimulusExportProcessor.ProcessStimulus(exportContext.Test.GetStimulusById(screen.StimulusId) ?? throw new ArgumentNullException(), exportContext);
            exportContext.Events.Add(new ConfigFile.MockItemInstructionScreen()
            {
                InstructionsDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.Id).Select(di => di.Id).FirstOrDefault(),
                ContinueInstructionsDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.ContinueInstructions.Id).Select(di => di.Id).FirstOrDefault(),
                LeftResponseDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.LeftResponseId).Select(di => di.Id).FirstOrDefault(),
                RightResponseDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.RightResponseId).Select(di => di.Id).FirstOrDefault(),
                StimulusDisplayID = exportContext.DisplayItems.Where(di => di.Guid == screen.StimulusId).Select(di => di.Id).FirstOrDefault(),
                ErrorMarkIsDisplayed = screen.ShowErrorMark,
                OutlineLeftResponse = (screen.KeyedDirection == KeyedDirection.Left) && screen.OutlineCorrectResponse,
                OutlineRightResponse = (screen.KeyedDirection == KeyedDirection.Right) && screen.OutlineCorrectResponse
            });
        }

        public void ProcessBlock(Block block, ExportContext exportContext)
        {
            if (block.InstructionsIds.Count > 0)
            {
                exportContext.AddEvent(new BeginInstructionBlock()
                {
                    NumInstructionScreens = block.InstructionsIds.Count
                });
                foreach (var instructionsIs in block.InstructionsIds)
                {
                    switch (exportContext.Test.GetInstructionScreenById(instructionsIs))
                    {
                        case Domain.TextInstructionScreen textInstructionScreen:
                            ProcessTextInstructionScreen(textInstructionScreen, exportContext);
                            break;
                        case Domain.KeyedInstructionScreen keyedInstructionScreen:
                            ProcessKeyedInstructionsScreen(keyedInstructionScreen, exportContext);
                            break;
                        case Domain.MockItemInstructionScreen mockItemInstructionScreen:
                            ProcessMockItemInstructionScreen(mockItemInstructionScreen, exportContext);
                            break;
                    }
                }
            }


            _textExportProcessor.ProcessText(exportContext.Test.GetFormattedTextById(block.BlockInstructionsId) ?? throw new ArgumentNullException(), 
                exportContext.LayoutRects.BlockInstructions, exportContext);
            _textExportProcessor.ProcessText(exportContext.Test.GetFormattedTextById(block.LeftResponseId) ?? throw new ArgumentNullException(), 
                exportContext.LayoutRects.LeftKey, exportContext);
            _textExportProcessor.ProcessText(exportContext.Test.GetFormattedTextById(block.RightResponseId) ?? throw new ArgumentNullException(), 
                exportContext.LayoutRects.RightKey, exportContext);
            exportContext.AddEvent(new BeginIATBlock()
            {
                LeftResponseDisplayID = exportContext.DisplayItems.Where(di => di.Guid == block.LeftResponseId).Select(di => di.Id).FirstOrDefault(),
                RightResponseDisplayID = exportContext.DisplayItems.Where(di => di.Guid == block.RightResponseId).Select(di => di.Id).FirstOrDefault(),
                InstructionsDisplayID = exportContext.DisplayItems.Where(di => di.Guid == block.BlockInstructionsId).Select(di => di.Id).FirstOrDefault(),
                NumItems = block.TrialIds.Count,
                BlockNumber = exportContext.Events.Where(evt => evt.EventType == EventType.BeginIATBlock).Count() + 1,
            });

            foreach (var trialId in block.TrialIds)
            {
                var trial = exportContext.Test.GetTrialById(trialId) ?? throw new ArgumentNullException();
                _stimulusExportProcessor.ProcessStimulus(exportContext.Test.GetStimulusById(trial.StimulusId) ?? throw new ArgumentNullException(), exportContext);
                exportContext.AddEvent(new ConfigFile.Trial()
                {
                    StimulusDisplayID = exportContext.DisplayItems.Where(di => di.Guid == trial.StimulusId).Select(di => di.Id).FirstOrDefault(),
                    KeyedDir = trial.KeyedDirection,
                    ItemNum = block.TrialIds.IndexOf(trial.Id),
                    BlockNum = block.BlockNumber,
                    OriginatingBlock = trial.OriginatingBlock,
                });
            }
            exportContext.Events.Add(new ConfigFile.EndIATBlock());
        }
    }
}
