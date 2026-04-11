using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Enumerations
{
    public abstract record ContentsItemType(string Name, string Description, bool IsHeaderItem, bool IsExpandable, Type AssociatedType)
    {
        public static readonly ContentsItemType IATBlock = new _IATBlock();
        public static readonly ContentsItemType InstructionBlock = new _InstructionBlock();
        public static readonly ContentsItemType Trial = new _Trial();
        public static readonly ContentsItemType BlankInstructionScreen = new _BlankInstructionScreen();
        public static readonly ContentsItemType TextInstructionScreen = new _TextInstructionScreen();
        public static readonly ContentsItemType KeyedInstructionSreen = new _KeyedInstructionSreen();
        public static readonly ContentsItemType MockItemInstructionScreen = new _MockItemInstructionScreen();
        public static readonly ContentsItemType Survey = new _Survey();

        public static ContentsItemType FromName(String name) =>
            name.ToLowerInvariant() switch
            {
                "iatblock" => IATBlock,
                "instructionblock" => InstructionBlock,
                "trial" => Trial,
                "blankinstructionscreen" => BlankInstructionScreen,
                "textinstructionscreen" => TextInstructionScreen,
                "keyedinstructionscreen" => KeyedInstructionSreen,
                "mockiteminstructionscreen" => MockItemInstructionScreen,
                "survey" => Survey,
                _ => throw new ArgumentException($"Unknown ContentsItemType name: {name}")
            };


        private sealed record _IATBlock() : ContentsItemType("IATBlock", "A block containing trials for an Implicit Association Test.", true, true, typeof(Domain.Block));
        private sealed record _InstructionBlock() : ContentsItemType("InstructionBlock", "A block containing instructions for a segment of the IAT.", true, true, typeof(Domain.InstructionsBlock));
        private sealed record _Trial() : ContentsItemType("Trial", "A single trial within a block, representing a stimulus presentation and response.", false, false, typeof(Domain.Trial));
        private sealed record _BlankInstructionScreen() : ContentsItemType("BlankInstructionScreen", "An instruction screen with no content, used for pacing or breaks.", false, false, 
            typeof(Domain.InstructionsScreen));
        private sealed record _TextInstructionScreen() : ContentsItemType("TextInstructionScreen", "An instruction screen containing text content for participant guidance.", false, false, 
            typeof(Domain.TextInstructionsScreen));
        private sealed record _KeyedInstructionSreen() : ContentsItemType("KeyedInstructionScreen", "An instruction screen that includes response key information for participant guidance.", false, false,
            typeof(Domain.KeyedInstructionsScreen));
        private sealed record _MockItemInstructionScreen() : ContentsItemType("MockItemInstructionScreen", "An instruction screen that simulates a trial item to demonstrate the task to participants.", false, false,
            typeof(Domain.MockItemInstructionsScreen));
        private sealed record _Survey() : ContentsItemType("Survey", "A survey item used to collect participant responses or demographic information.", true, false,
            typeof(Domain.Survey));
    }
}
