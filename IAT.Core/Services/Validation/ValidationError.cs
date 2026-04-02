using com.sun.xml.@internal.ws.developer;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services.Validation
{
    public class ValidationError 
    {
        public required Error Error { get; init; }
        public required int Container { get; init; }
        public required int Item { get; init; } 

        public ValidationError() { }

        public ValidationError(Error type, int container, int item = -1)
        {
            Error = type;
            Container = container;
            Item = item;
        }


        public string ErrorText
        {
            get
            {
                return Error switch
                {
                    Error.ImageStimulusIncompletelyInitialized => ,
                    Error.InstructionBlockHasNoItems => $"Instruction Block #{Container} has no screens.",
                    Error.InstructionScreenWithoutType => $"Instruction screen #{Item} in instruction block #{Container} was not assigned a type.",
                    Error.TextInstructionsBlank => $"Instruction screen #{Item} in instruction block #{Container} has blank text instructions.",
                    Error.TextStimlusIncompletelyInitialized => $"The text stimulus for item #{Item} in IAT block #{Container} has not been properly defined.",
                    Error.ContinueInstructionsBlank => $"Instruction screen #{Item} in instruction block #{Container} has blank continue instructions.",
                    Error.KeyInstructionScreenWithoutResponseKey => $"Instruction screen #{Item} in instruction block #{Container} is a key instruction screen but does not have a response key assigned.",
                    Error.MockItemScreenWithIncompletelyInitializedImageStimulus => $"The image stimulus for mock item screen #{Item} in instruction block #{Container} has not been properly defined.",
                    Error.MockItemScreenWithIncompletelyInitializedTextStimulus => $"The text stimulus for mock item screen #{Item} in instruction block #{Container} has not been properly defined.",
                    Error.MockItemScreenWithoutResponseKey => $"Mock item screen #{Item} in instruction block #{Container} does not have a response key assigned.",
                    Error.MockItemScreenWithoutStimulus => $"Mock item screen #{Item} in instruction block #{Container} does not have a stimulus assigned.",
                    _ => String.Empty
                };
            }
        }
    }
}

