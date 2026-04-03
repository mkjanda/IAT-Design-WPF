using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Validation
{
    /// <summary>
    /// Specifies the possible causes of a validation exception encountered during configuration or runtime validation. 
    /// </summary>
    /// <remarks>Use this enumeration to identify the specific reason for a validation failure when handling
    /// or logging validation exceptions. Each value represents a distinct validation error scenario that may require
    /// different handling or user feedback.</remarks>
    public enum Error
    {
        /// <summary>
        /// Represents an error condition where a required block response key is undefined.
        /// </summary>
        BlockResponseKeyUndefined, 

        /// <summary>
        /// Represents a directory entry with an undefined or unspecified key.
        /// </summary>
        ItemKeyedDirUndefined, 
        
        /// <summary>
        /// Represents a state where the item stimulus is undefined.
        /// </summary>
        ItemStimulusUndefined, 
        
        /// <summary>
        /// Represents an error condition where an image stimulus is not fully initialized.
        /// </summary>
        ImageStimulusIncompletelyInitialized, 
        
        /// <summary>
        /// Represents an error condition indicating that a text stimulus was not fully initialized.
        /// </summary>
        TextStimlusIncompletelyInitialized,

        /// <summary>
        /// Represents an instruction screen that does not specify a particular instruction type.
        /// </summary>
        InstructionScreenWithoutType, 

        /// <summary>
        /// Represents a key instruction screen that does not require a response key from the user.
        /// </summary>
        KeyInstructionScreenWithoutResponseKey, 

        /// <summary>
        /// Represents a mock item screen that does not require a response key. 
        /// </summary>
        MockItemScreenWithoutResponseKey, 

        /// <summary>
        /// Represents a mock item screen that does not include a stimulus element.
        /// </summary>
        MockItemScreenWithoutStimulus, 

        /// <summary>
        /// Represents a mock item screen that contains a text stimulus which may not be fully initialized.
        /// </summary>
        /// <remarks>This type is intended for testing scenarios where incomplete initialization of text
        /// stimuli needs to be simulated. It can be used to verify error handling and robustness in UI components that
        /// depend on fully initialized text stimuli.</remarks>
        MockItemScreenWithIncompletelyInitializedTextStimulus,

        /// <summary>
        /// Represents a mock item screen that contains an image stimulus which may not be fully initialized.
        /// </summary>
        /// <remarks>This type is intended for testing scenarios where image stimuli are partially
        /// constructed or missing required initialization. Use to simulate error handling or incomplete data cases in
        /// UI workflows.</remarks>
        MockItemScreenWithIncompletelyInitializedImageStimulus, 
        
        /// <summary>
        /// Gets or sets the instructions text to display when no specific instructions are available.
        /// </summary>
        TextInstructionsBlank, 
        
        /// <summary>
        /// Gets the instructions to display when the user should continue and no specific instructions are provided.
        /// </summary>
        ContinueInstructionsBlank, 

        /// <summary>
        /// Represents an error that occurs when a block does not contain any items.
        /// </summary>
        BlockHasNoItems, 

        /// <summary>
        /// Represents an error condition indicating that an instruction block does not contain any items.
        /// </summary>
        InstructionBlockHasNoItems
    };
}
