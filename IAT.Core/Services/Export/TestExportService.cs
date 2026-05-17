using IAT.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using FluentValidation;

namespace IAT.Core.Services.Export
{
    public interface ITestExportService
    {
        Task<ExportResult> PrepareForServerUploadAsync(IatTest test);
    }

    public class TestExportService : ITestExportService
    {
        private readonly IStimulusExportProcessor _stimulusProcessor;
        private readonly IBlockExportProcessor _blockProcessor;
        private readonly ITestMapperService _mapper;
        private readonly IValidator<IatTest> _validator;
        private readonly ILayoutCalculatorService _layoutCalculator;
        private readonly IItemSlideExportProcessor _itemSlideProcessor;
            
        /// <summary>
        /// Constructs a new instance of the TestExportService with the necessary dependencies for processing and validating IAT tests for export.
        /// </summary>
        /// <param name="stimulusProcessor"></param>
        /// <param name="blockProcessor"></param>
        /// <param name="mapper"></param>
        /// <param name="validator"></param>
        /// <param name="layoutCalculator"></param>
        /// <param name="itemSlideProcessor"></param>   
        public TestExportService(IStimulusExportProcessor stimulusProcessor, IBlockExportProcessor blockProcessor, 
            ITestMapperService mapper, IValidator<IatTest> validator, ILayoutCalculatorService layoutCalculator,
            IItemSlideExportProcessor itemSlideProcessor)
        {
            _stimulusProcessor = stimulusProcessor;
            _blockProcessor = blockProcessor;
            _mapper = mapper;
            _validator = validator;
            _layoutCalculator = layoutCalculator;
            _itemSlideProcessor = itemSlideProcessor;
        }

        /// <summary>
        /// Prepares an IAT test for server upload by validating, processing blocks, and building the configuration
        /// file.
        /// </summary>
        /// <param name="test">The IAT test to prepare for upload.</param>
        /// <returns>An export result containing the configuration file and manifest.</returns>
        public async Task<ExportResult> PrepareForServerUploadAsync(IatTest test)
        {
            await _validator.ValidateAsync(test);   // FluentValidation recommended

            var exportContext = new ExportContext
            {
                Test = test,
                LayoutRects = _layoutCalculator.GetFinalRects(test.Layout),
            };

            foreach (var block in test.AllBlocks.OrderBy(b => b.BlockNumber))
            {
                _blockProcessor.ProcessBlock(block, exportContext);
            }
            _itemSlideProcessor.ProcessItemSlides(exportContext);

            var configFile = _mapper.BuildConfigFile(test, exportContext);

            return new ExportResult(configFile, exportContext.FileManifest, exportContext.SlideManifest);
        }
    }
}
