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

    }

    public class TestExportService : ITestExportService
    {
        private readonly IStimulusExportProcessor _stimulusProcessor;
        private readonly IBlockExportProcessor _blockProcessor;
        private readonly ITestMapperService _mapper;
        private readonly IValidator<IatTest> _validator;
        private readonly ILayoutCalculatorService _layoutCalculator;

        public TestExportService(IStimulusExportProcessor stimulusProcessor, IBlockExportProcessor blockProcessor, 
            ITestMapperService mapper, IValidator<IatTest> validator, ILayoutCalculatorService layoutCalculator)
        {
            _stimulusProcessor = stimulusProcessor;
            _blockProcessor = blockProcessor;
            _mapper = mapper;
            _validator = validator;
            _layoutCalculator = layoutCalculator;
        }

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

            var configFile = _mapper.BuildConfigFile(test, exportContext);

            return new ExportResult(configFile, exportContext.Manifest);
        }
    }
}
