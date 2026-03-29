using java.awt;
using jdk.@internal.dynalink.beans;
using net.sf.saxon.expr;
using System;
using static java.util.concurrent.locks.ReentrantReadWriteLock;

namespace IAT.Core.Models.Enumerations
{
    /// <summary>
    /// Smart Enum for IAT block types. Replaces old magic strings and switch statements.
    /// Fully serializable to XML for your XSLT reports and server payloads.
    /// </summary>
    public abstract record PartType(string name, string description, Type type)
    {
        /// <summary>
        /// Represents the block type used for IAT (Implicit Association Test) blocks.
        /// </summary>

        /// <summary>
        /// Represents a block type for instructions within the document structure.
        /// </summary>
        public static readonly PartType Block = new _Block();

        /// <summary>
        /// Represents the part type for a history entry in the system.
        /// </summary>
        /// <remarks>Use this field to identify or work with history entry parts when interacting with
        /// APIs that require a part type. This value is typically used for operations involving audit trails or change
        /// tracking.</remarks>
        public static readonly PartType HistoryEntry = new _HistoryEntry();

        /// <summary>
        /// Represents the part type for a stimulus element.
        /// </summary>
        public static readonly PartType Stimulus = new _Stimulus();

        /// <summary>
        /// Represents the part type for instructions within the system.
        /// </summary>
        /// <remarks>Use this field to identify or work with instruction-related parts. This value is
        /// typically used when processing or filtering parts by their type.</remarks>
        public static readonly PartType Instructions = new _Instructions();

        /// <summary>
        /// Represents the trial part type used to identify trial components within the system.
        /// </summary>
        public static readonly PartType Trial = new _Trial();

        /// <summary>
        /// Represents a part type that groups multiple alternatives for pattern matching or parsing scenarios.
        /// </summary>
        /// <remarks>Use this part type to define a set of alternative options within a pattern. It is
        /// typically used in scenarios where one of several possible elements may match at a given position.</remarks>
        public static readonly PartType AlternationGroup = new _AlternationGroup();

        public static readonly PartType TransactionRequest = new _TransactionRequest();

        /// <summary>
        /// Returns the corresponding BlockType value for the specified block type name.
        /// </summary>
        /// <remarks>The method recognizes several common block type names, including "block", "iatblock",
        /// "instructionblock", and "Instructions". The comparison is performed in a case-insensitive manner.</remarks>
        /// <param name="name">The name of the block type to look up. The comparison is case-insensitive.</param>
        /// <returns>A BlockType value that matches the specified name.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified name does not correspond to a known block type.</exception>
        public static PartType FromName(string name) => 
            name.Equals("block", StringComparison.OrdinalIgnoreCase) ? Block :
            name.Equals("instructionblock", StringComparison.OrdinalIgnoreCase) ? Instructions :
            name.Equals("trial", StringComparison.OrdinalIgnoreCase) ? Trial :
            name.Equals("alternationgroup", StringComparison.OrdinalIgnoreCase) ? AlternationGroup :
            name.Equals("historyentry", StringComparison.OrdinalIgnoreCase) ? HistoryEntry :
            name.Equals("stimulus", StringComparison.OrdinalIgnoreCase) ? Stimulus :
            throw new ArgumentException($"Unknown block type: {name}");

        public static PartType FromType(Type type) => 
            type == typeof(Block) ? Block :
            type == typeof(InstructionBlock) ? Instructions :
            type == typeof(Trial) ? Trial :
            type == typeof(AlternationGroup) ? AlternationGroup :
            type == typeof(HistoryEntry) ? HistoryEntry :
            type == typeof(Stimulus) ? Stimulus :
            throw new ArgumentException($"Unknown block type: {type}");


        /// <summary>
        /// Represents a block of trials within an Implicit Association Test (IAT) configuration.
        /// </summary>
        /// <remarks>A block groups a sequence of trials that share common settings or instructions.
        /// Blocks are typically used to organize the structure of an IAT, allowing for the definition of different
        /// phases or conditions within the test.</remarks>
        public sealed record _Block() : PartType("Block", "Block of trials", typeof(Block))
        {
        }

        /// <summary>
        /// Represents a request to perform a transaction within the system.
        /// </summary>
        /// <remarks>This type is used to encapsulate the details required to initiate a transaction
        /// operation. It is typically used in scenarios where transaction processing or tracking is needed. Instances
        /// of this record are immutable and thread-safe.</remarks>
        public sealed record _TransactionRequest() : PartType("TransactionRequest", "A request for a transaction to be performed", typeof(TransactionRequest))
        {
        }

        /// <summary>
        /// Represents a part type for a block of instruction screens.
        /// </summary>
        /// <remarks>Use this type to define or identify sections of instructional content within a larger
        /// workflow or application. This part type is associated with the InstructionBlock class and is intended for
        /// scenarios where a sequence of instructions needs to be presented as a distinct block.</remarks>
        public sealed record _Instructions() : PartType("Instructions", "Block of instruction screens", typeof(InstructionBlock))
        {
        }

        /// <summary>
        /// Represents a stimulus part type, such as a word or image, used in the system.
        /// </summary>
        /// <remarks>This type is used to identify and describe stimulus items within the application. It
        /// is a sealed record and cannot be inherited.</remarks>
        public sealed record _Trial() : PartType("Stimulus", "A stimulus item (word or image)", typeof(Trial))
        {
        }

        /// <summary>
        /// Represents a group of items that alternate during a trial.
        /// </summary>
        /// <remarks>Use this type to define a set of items where only one is active at a time,
        /// alternating according to trial logic. This is typically used in scenarios where mutually exclusive options
        /// or states are required within a trial context.</remarks>
        public sealed record _AlternationGroup() : PartType("AlternationGroup", "A group of items that alternate during a trial", typeof(AlternationGroup))
        {
        }

        public sealed record _HistoryEntry() : PartType("HistoryEntry", "A entry in the save file regarding previous use", typeof(HistoryEntry))
        {
        }

        public sealed record _Stimulus() : PartType("Stimulus", "A stimulus item (word or image)", typeof(Stimulus))
        {
        }

}