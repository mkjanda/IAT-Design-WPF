using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace IAT.Core.Domain
{
    /// <summary>
    /// Represents the layout configuration for an Implicit Association Test (IAT), including the positions and sizes of various UI elements such as the 
    /// interior rectangle, stimulus area, response key areas, error mark, and instruction areas. This class provides properties for defining the layout 
    /// of the test interface, allowing for customization and adjustments to fit different screen sizes and configurations. The UserSizeOverrides dictionary 
    /// allows for specific size overrides for individual elements based on user preferences or requirements.
    /// </summary>
    public class LayoutConfiguration
    {
        /// <summary>
        /// The unique identifier for this layout configuration, allowing it to be referenced and managed within the context of an IAT test. This ID is 
        /// automatically generated when a new instance of LayoutConfiguration is created, ensuring that each layout can be uniquely identified and 
        /// associated with specific tests or configurations as needed.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the rectangle that defines the interior area available for content layout.
        /// </summary>
        public Rect InteriorRect { get; set; } = _defaultInteriorRect;

        /// <summary>
        /// Gets or sets the rectangular area in which the stimulus is displayed.
        /// </summary>
        public Rect StimulusRect { get; set; } = _defaultStimulusRect;

        /// <summary>
        /// Gets or sets the bounding rectangle for the left key area.
        /// </summary>
        public Rect LeftKeyRect { get; set; } = _defaultLeftKeyRect;

        /// <summary>
        /// Gets or sets the bounding rectangle for the right key area.
        /// </summary>
        public Rect RightKeyRect { get; set; } = _defaultRightKeyRect;

        /// <summary>
        /// Gets or sets the rectangle that defines the area used to display an error mark.
        /// </summary>
        public Rect ErrorMarkRect { get; set; } = _defaultErrorMarkRect;

        /// <summary>
        /// Gets  or sets the rectangle that defines the area for displaying block instructions.
        /// </summary>
        public Rect BlockInstructionsRect { get; set; } = _defaultBlockInstructionsRect;

        /// <summary>
        /// Gets or sets the bounding rectangle for displaying mock item instructions.
        /// </summary>
        public Rect MockItemInstructionsRect { get; set; } = _defaultMockItemInstructionsRect;

        /// <summary>
        /// Gets or sets the rectangular region used for displaying keyed instructions.
        /// </summary>
        public Rect KeyedInstructionsRect { get; set; } = _defaultKeyedInstructionsRect;

        /// <summary>
        /// Gets or sets the bounding rectangle that defines the area for displaying text instructions.
        /// </summary>
        public Rect TextInstructionsRect { get; set; } = _defaultTextInstructionsRect;

        /// <summary>
        /// Gets or sets the bounding rectangle for displaying continue instructions.
        /// </summary>
        public Rect ContinueInstructionsRect { get; set; } = _defaultContinueInstructionsRect;

        /// <summary>
        /// Contains the user overrides of the sizes of the various layout elements. The keys in the dictionary represent the names of the 
        /// layout elements (e.g., "InteriorRect", "StimulusRect", etc.), and the values are the corresponding Size objects that specify the 
        /// overridden dimensions for those elements. This allows users to customize the layout by providing specific size overrides for individual 
        /// elements, which can be applied during layout calculations to adjust the interface according to user preferences or requirements.
        /// </summary>
        public Dictionary<string, Size> UserSizeOverrides = new();

        private static readonly Rect _defaultInteriorRect = new Rect(0, 0, 600, 600);
        private static readonly Rect _defaultStimulusRect = new Rect(30, 140, 540, 300);
        private static readonly Rect _defaultLeftKeyRect = new Rect(0, 0, 200, 120);
        private static readonly Rect _defaultRightKeyRect = new Rect(400, 0, 200, 120);
        private static readonly Rect _defaultErrorMarkRect = new Rect(275, 450, 50, 50);
        private static readonly Rect _defaultBlockInstructionsRect = new Rect(15, 320, 570, 80);
        private static readonly Rect _defaultMockItemInstructionsRect = new Rect(15, 510, 570, 60);
        private static readonly Rect _defaultKeyedInstructionsRect = new Rect(15, 135, 570, 410);
        private static readonly Rect _defaultTextInstructionsRect = new Rect(15, 15, 570, 530);
        private static readonly Rect _defaultContinueInstructionsRect = new Rect(15, 570, 570, 30);

        /// <summary>
        /// Restores all configurable rectangles to their default values.
        /// </summary>
        /// <remarks>Call this method to reset the layout-related properties to their original default
        /// settings. This is useful for reverting any customizations made to the rectangles used for display or
        /// interaction areas.</remarks>
        public void RestoreDefaults()
        {
            InteriorRect = _defaultInteriorRect;
            StimulusRect = _defaultStimulusRect;
            LeftKeyRect = _defaultLeftKeyRect;
            RightKeyRect = _defaultRightKeyRect;
            ErrorMarkRect = _defaultErrorMarkRect;
            BlockInstructionsRect = _defaultBlockInstructionsRect;
            MockItemInstructionsRect = _defaultMockItemInstructionsRect;
            KeyedInstructionsRect = _defaultKeyedInstructionsRect;
            TextInstructionsRect = _defaultTextInstructionsRect;
            ContinueInstructionsRect = _defaultContinueInstructionsRect;
        }
    }
}
