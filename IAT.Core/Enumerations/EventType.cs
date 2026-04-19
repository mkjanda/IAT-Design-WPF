using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations;

/// <summary>
/// The type of event that is being represented. This is used to determine how to process the event and what properties it may have. 
/// The EventType is a string that can be used to identify the type of event, and it is used in the IATEvent class to determine how 
/// to process the event and what properties it may have.
/// </summary>
/// <param name="Name">The name of the type of event</param>
public abstract record EventType(String Name)
{
    /// <summary>
    /// Represents the event type for the beginning of an IAT (Implicit Association Test) block.
    /// </summary>
    public static readonly EventType BeginIATBlock = new _BeginIATBlock();

    /// <summary>
    /// Represents the event type that indicates the end of an IAT (Implicit Association Test) block.
    /// </summary>
    /// <remarks>Use this event type to identify when an IAT block has completed within an event stream or
    /// processing workflow. This value is typically used for event filtering or handling logic related to the
    /// conclusion of IAT blocks.</remarks>
    public static readonly EventType EndIATBlock = new _EndIATBlock();

    /// <summary>
    /// Represents the event type for an IAT (Implicit Association Test) item.
    /// </summary>
    public static readonly EventType IATItem = new _IATItem();

    /// <summary>
    /// Represents the event type that marks the beginning of an instruction block.
    /// </summary>
    public static readonly EventType BeginInstructionBlock = new _BeginInstructionBlock();

    /// <summary>
    /// Represents an event type for a screen that displays text instructions.
    /// </summary>
    public static readonly EventType TextInstructionScreen = new _TextInstructionScreen();

    /// <summary>
    /// Represents the event type for the mock item instruction screen.
    /// </summary>
    public static readonly EventType MockItemInstructionScreen = new _MockItemInstructionScreen();

    /// <summary>
    /// Represents the event type for a keyed instruction screen.
    /// </summary>
    /// <remarks>Use this event type to identify or handle events related to screens that require user input
    /// via key entry. This value is typically used in event handling or UI logic to distinguish keyed instruction
    /// screens from other event types.</remarks>
    public static readonly EventType KeyedInstructionScreen = new _KeyedInstructionScreen();

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the value of the Name property for this instance.</returns>
    public override string ToString() => Name;

    /// <summary>
    /// Represents an event type that marks the beginning of an IAT (Implicit Association Test) block.
    /// </summary>
    /// <remarks>This record is used to identify the start of a new block within an IAT sequence. It is
    /// typically used in event processing or logging scenarios where tracking the boundaries of IAT blocks is
    /// required.</remarks>
    private sealed record _BeginIATBlock() : EventType("BeginIATBlock");

    /// <summary>
    /// Represents an event type that marks the end of an IAT (Implicit Association Test) block.
    /// </summary>
    private sealed record _EndIATBlock() : EventType("EndIATBlock");

    /// <summary>
    /// Represents an event type for an IAT (Implicit Association Test) item.
    /// </summary>
    /// <remarks>This type is used to identify events related to IAT items within the event system. It is
    /// intended for internal use and is not intended to be instantiated directly by consumers.</remarks>
    private sealed record _IATItem() : EventType("IATItem");

    /// <summary>
    /// Represents an event type that marks the beginning of an instruction block.
    /// </summary>
    private sealed record _BeginInstructionBlock() : EventType("BeginInstructionBlock");

    /// <summary>
    /// Represents an event type for displaying a text instruction screen.
    /// </summary>
    /// <remarks>This record is used to identify events that correspond to showing instructional text to the
    /// user. It is typically used in scenarios where guidance or information needs to be presented as part of an
    /// event-driven workflow.</remarks>
    private sealed record _TextInstructionScreen() : EventType("TextInstructionScreen");

    /// <summary>
    /// Represents the event type for the mock item instruction screen, which is used to simulate a trial 
    /// item and demonstrate the task to participants.
    /// </summary>
    private sealed record _MockItemInstructionScreen() : EventType("MockItemInstructionScreen");

    /// <summary>
    /// Represents the event type for a keyed instruction screen, which includes response key information for participant guidance.
    /// </summary>
    private sealed record _KeyedInstructionScreen() : EventType("KeyedInstructionScreen");
}


