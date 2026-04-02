using System;
using IAT.Core.Models;
namespace IAT.Core.Models.Enumerations
{
    /// <summary>
    /// Smart Enum for IAT block types. Replaces old magic strings and switch statements.
    /// Fully serializable to XML for your XSLT reports and server payloads.
    /// </summary>
    public abstract record PartType(string Name, string Description, Type type, Type BaseType, string MimeType)
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
        /// Represents the part type for a stimulus image.
        /// </summary>
        public static readonly PartType StimulusImage = new _StimulusImage();

        /// <summary>
        /// Represents the part type for stimulus text in the system.
        /// </summary>
        /// <remarks>Use this field to identify or work with parts that contain stimulus text content.
        /// This value is typically used when constructing or analyzing items that require a stimulus text
        /// component.</remarks>
        public static readonly PartType StimulusText = new _StimulusText();

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


        /// <summary>
        /// Represents the IAT part type.
        /// </summary>
        public static readonly PartType IAT = new _IAT();

        /// <summary>
        /// Represents the part type for save file metadata used in serialization or deserialization operations.
        /// </summary>
        /// <remarks>Use this field to identify or work with the metadata section of a save file when
        /// processing file parts. This value is typically used as a key or identifier in file handling APIs.</remarks>
        public static readonly PartType SaveFileMetaData = new _SaveFileMetaData();

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
            name.Equals("instructions", StringComparison.OrdinalIgnoreCase) ? Instructions :
            name.Equals("trial", StringComparison.OrdinalIgnoreCase) ? Trial :
            name.Equals("alternationgroup", StringComparison.OrdinalIgnoreCase) ? AlternationGroup :
            name.Equals("historyentry", StringComparison.OrdinalIgnoreCase) ? HistoryEntry :
            name.Equals("stimulusimage", StringComparison.OrdinalIgnoreCase) ? StimulusImage :
            name.Equals("stimulustext", StringComparison.OrdinalIgnoreCase) ? StimulusText :
            name.Equals("savefilemetadata", StringComparison.OrdinalIgnoreCase) ? SaveFileMetaData :
            name.Equals("iat", StringComparison.OrdinalIgnoreCase) ? IAT :
            throw new ArgumentException($"Unknown block type: {name}");

        /// <summary>
        /// Returns the corresponding PartType enumeration value for the specified type.
        /// </summary>
        /// <param name="type">The type to map to a PartType value. Must represent a supported part type.</param>
        /// <returns>The PartType value that corresponds to the specified type.</returns>
        /// <exception cref="ArgumentException">Thrown if the specified type does not correspond to a known part type.</exception>
        public static PartType FromType(Type type) =>
            type == typeof(Block) ? Block :
            type == typeof(Instructions) ? Instructions :
            type == typeof(Trial) ? Trial :
            type == typeof(AlternationGroup) ? AlternationGroup :
            type == typeof(HistoryEntry) ? HistoryEntry :
            type == typeof(StimulusImage) ? StimulusImage :
            type == typeof(StimulusText) ? StimulusText :
            type == typeof(SaveFileMetaData) ? SaveFileMetaData :
            type == typeof(Test) ? IAT :
            throw new ArgumentException($"Unknown block type: {type}");


        /// <summary>
        /// Represents the part type definition for an Implicit Association Test (IAT) configuration.
        /// </summary>
        /// <remarks>This type is used to identify and describe IAT configuration parts within the system.
        /// It provides metadata for recognizing and handling IAT-related data structures. This record is sealed and
        /// cannot be inherited.</remarks>
        public sealed record _IAT() : PartType("IAT", "An Implicit Association Test configuration", typeof(Test), typeof(Test), "text/xml+" + typeof(Test).ToString())
        {
        }

        /// <summary>
        /// Represents the type information and metadata definition for save file metadata parts.
        /// </summary>
        /// <remarks>This type is used to describe and register the metadata structure associated with
        /// save files within the part system. It is typically used for type discovery and metadata management
        /// scenarios.</remarks>
        public sealed record _SaveFileMetaData() : PartType("SaveFileMetaData", "Metadata for save files", typeof(SaveFileMetaData), typeof(SaveFileMetaData), "text/xml+" + typeof(SaveFileMetaData).ToString())
        {
        }

        /// <summary>
        /// Represents a block of trials within an Implicit Association Test (IAT) configuration.
        /// </summary>
        /// <remarks>A block groups a sequence of trials that share common settings or instructions.
        /// Blocks are typically used to organize the structure of an IAT, allowing for the definition of different
        /// phases or conditions within the test.</remarks>
        public sealed record _Block() : PartType("Block", "Block of trials", typeof(Block), typeof(Block))
        {
        }

        /// <summary>
        /// Represents a part type for a block of instruction screens.
        /// </summary>
        /// <remarks>Use this type to define or identify sections of instructional content within a larger
        /// workflow or application. This part type is associated with the InstructionBlock class and is intended for
        /// scenarios where a sequence of instructions needs to be presented as a distinct block.</remarks>
        public sealed record _Instructions() : PartType("Instructions", "Block of instruction screens", typeof(InstructionBlock), typeof(InstructionBlock), "text/xml+" + typeof(InstructionBlock).ToString())
        {
        }

        /// <summary>
        /// Represents a stimulus part type, such as a word or image, used in the system.
        /// </summary>
        /// <remarks>This type is used to identify and describe stimulus items within the application. It
        /// is a sealed record and cannot be inherited.</remarks>
        public sealed record _Trial() : PartType("Stimulus", "A stimulus item (word or image)", typeof(Trial), typeof(Trial), "text/xml+" + typeof(Trial).ToString())
        {
        }

        /// <summary>
        /// Represents a group of items that alternate during a trial.
        /// </summary>
        /// <remarks>Use this type to define a set of items where only one is active at a time,
        /// alternating according to trial logic. This is typically used in scenarios where mutually exclusive options
        /// or states are required within a trial context.</remarks>
        public sealed record _AlternationGroup() : PartType("AlternationGroup", "A group of items that alternate during a trial", typeof(Models.AlternationGroup), typeof(Models.AlternationGroup), "text/xml+" + typeof(Models.AlternationGroup).ToString())
        {
        }

        /// <summary>
        /// Represents a stimulus image part type used for defining and handling stimulus image items within the system.
        /// </summary>
        /// <remarks>This type is typically used to identify and work with stimulus image data in
        /// workflows that process or display such items. It is a sealed record and cannot be inherited.</remarks>
        public sealed record _StimulusImage() : PartType("StimulusImage", "A stimulus image item", typeof(DIStimulusImage), typeof(DIBase), "text/xml+" + typeof(DIStimulusImage).ToString())
        {
        }

        /// <summary>
        /// Represents a part type for a stimulus text item used in the system.
        /// </summary>
        /// <remarks>This type is used to identify and describe stimulus text items, typically for use in
        /// assessment or content delivery scenarios. It provides metadata and type information for integration with
        /// other components that process or render stimulus text content.</remarks>
        public sealed record _StimulusText() : PartType("StimulusText", "A stimulus text item", typeof(DTStimulusText), typeof(DIBase), "text/xml+" + typeof(DIStimulusText).ToString())
        {
        }

        /// <summary>
        /// Represents an entry in the save file that records information about previous usage or actions.
        /// </summary>
        /// <remarks>This type is used to track historical data within the save file, such as user actions
        /// or state changes. It is intended for scenarios where maintaining a record of past events is necessary for
        /// auditing, undo functionality, or user history features.</remarks>
        public sealed record _HistoryEntry() : PartType("HistoryEntry", "A entry in the save file regarding previous use", typeof(HistoryEntry), typeof(HistoryEntry), "text/xml+" + typeof(HistoryEntry).ToString())
        {
        }

        /// <summary>
        /// Represents a stimulus part type, such as a word or image, for use in content models.
        /// </summary>
        /// <remarks>This type is used to identify and describe stimulus items within a part-based content
        /// structure. It is typically used in scenarios where content is composed of multiple parts, and a stimulus is
        /// a distinct, addressable element. This record is sealed and not intended for inheritance.</remarks>
        public sealed record _Stimulus() : PartType("Stimulus", "A stimulus item (word or image)", typeof(Stimulus), typeof(Stimulus), "text/xml+" + typeof(Stimulus).ToString())
        {
        }
    }
}