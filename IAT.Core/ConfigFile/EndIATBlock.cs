using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;

namespace IAT.Core.ConfigFile;

/// <summary>
/// Represents the event that marks the end of an IAT (Implicit Association Test) block. This event is used to indicate the conclusion of an IAT block 
/// within the event stream or processing workflow. It contains a property for the event type, which is set to EventType.EndIATBlock, allowing for easy 
/// identification and handling of this event type in the application. This class serves as a marker for the end of an IAT block, enabling appropriate 
/// processing and logic related to the completion of blocks within the test structure.
/// </summary>
public sealed class EndIATBlock
{
    /// <summary>
    /// The type of event that is being represented. This is used to determine how to process the event and what properties it may have. 
    /// For this class, it is set to EventType.EndIATBlock.
    /// </summary>
    public EventType EventType => EventType.EndIATBlock;
}
