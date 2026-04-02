using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Models;
using com.sun.crypto.provider;
namespace IAT.Core.Services.Validation
{

    /// <summary>
    /// Provides validation logic for the properties of a block, ensuring that each block contains at least one stimulus
    /// and has a valid response key assigned.
    /// </summary>
    /// <remarks>Use this validator to verify the integrity of block data before processing. The validation
    /// rules help prevent incomplete or improperly configured blocks from being used in further operations.</remarks>
    public class BlockValidator : AbstractValidator<Block> 
    {
        /// <summary>
        /// Initializes a new instance of the BlockValidator class with validation rules for block properties.
        /// </summary>
        /// <remarks>This constructor defines validation rules to ensure that each block contains at least
        /// one stimulus and has a valid response key assigned. Use this validator to check the integrity of block data
        /// before processing.</remarks>
        public BlockValidator()
        {
            RuleFor(x => x.Trials.Count)
                .NotEmpty()
                .WithMessage(x=> $"Block #{x.BlockNumber} has no stimuli.").WithState(x => x.Id);
            RuleFor(x => x.KeyId)
                .NotEqual(Guid.Empty)
                .WithMessage(block => $"Block #{block.BlockNumber} has not been assigned a response key.").WithState(x => x.Id);
        }
    }
}
