using com.sun.org.apache.bcel.@internal.generic;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IKVM.ByteCode.Decoding;
using jdk.nashorn.@internal.ir;
using sun.java2d;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.Serializable
{
    /// <summary>
    /// This class defines the layout of various UI elements in a test block, including the stimulus area, key-value pairs, instructions, and error display. It provides properties 
    /// to get and set the sizes of these elements, as well as observable values to track their positions and dimensions. The class also includes methods to check for overlapping 
    /// rectangles and to calculate maximum allowable sizes for different components based on the current layout configuration. This layout information is crucial for ensuring that 
    /// UI elements are displayed correctly without overlap and that they fit within the designated areas of the test block.
    /// </summary>
    public class Layout 
    {
        /// <summary>
        /// A structure of constant values that defines bitmasks for rectangle overlap
        /// </summary>
        public struct Overlap
        {
            /// <summary>
            /// Represents an overlap involving the stimulus rectangle. This flag is set when the stimulus rectangle overlaps with any other defined rectangle in the layout.
            /// </summary>
            public const uint StimulusRectangle = 0x1;

            /// <summary>
            /// Represents the flag value for key-value rectangles in the associated API or enumeration.
            /// </summary>
            /// <remarks>Use this constant to specify or identify key-value rectangle data when
            /// interacting with APIs that support multiple data types or flags. The meaning and usage of this value may
            /// depend on the specific context in which it is used.</remarks>
            public const uint KeyValueRectangles = 0x2;

            /// <summary>
            /// Represents the error code for a rectangle-related error condition.
            /// </summary>
            public const uint ErrorRectangle = 0x4;

            /// <summary>
            /// Represents the instruction code for a rectangle operation.
            /// </summary>
            /// <remarks>Use this constant to identify rectangle instructions when working with
            /// instruction sets that support multiple operation types.</remarks>
            public const uint InstructionRectangle = 0x8;
        };

        /// <summary>
        /// Represents an observable value that is initialized to an empty rectangle and is used when no observation is
        /// required.
        /// </summary>
        [XmlIgnore]
        public readonly ObservableValue<Rect> NoneObservable = new ObservableValue<Rect>(Rect.Empty);

        /// <summary>
        /// Represents an observable value that tracks the current interior rectangle.
        /// </summary>
        /// <remarks>This field is used to monitor changes to the interior rectangle's dimensions and
        /// position. It is not serialized during XML serialization due to the XmlIgnore attribute.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> InteriorRectObservable = new ObservableValue<Rect>(new Rect(0, 0, _InteriorSize.Width, _InteriorSize.Height));

        /// <summary>
        /// Represents an observable value containing the rectangle used for displaying block instructions.
        /// </summary>
        /// <remarks>The rectangle is initialized to a default position and size based on the default
        /// interior and instructions dimensions. Changes to this value can be observed to update UI elements
        /// accordingly.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> BlockInstructionsRectObservable = new ObservableValue<Rect>(
            new Rect()
            {
                X = (_InteriorSize.Width - _InstructionsSize.Width) / 2,
                Y = _InteriorSize.Height - _InstructionsSize.Height / 2,
                Width = _InstructionsSize.Width,
                Height = _InstructionsSize.Height
            });

        /// <summary>
        /// Represents an observable value that tracks the current rectangle of the stimulus area.
        /// </summary>
        /// <remarks>This field is intended for internal use to monitor changes to the stimulus rectangle.
        /// The value is not serialized due to the XmlIgnore attribute.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> StimulusRectObservable = new ObservableValue<Rect>(
            new Rect()
            {
                X = _InteriorSize.Width - _StimulusSize.Width / 2,
                Y = (_InteriorSize.Height - _InstructionsSize.Height - _ErrorSize.Height - _KeyValueSize.Height) / 2 + _StimulusSize.Height,
                Width = _StimulusSize.Width,
                Height = _StimulusSize.Height
            });

        /// <summary>
        /// Represents an observable value that tracks the bounding rectangle of the left key-value pair.
        /// </summary>
        /// <remarks>This field is intended for internal use to monitor changes to the position and size
        /// of the left key-value rectangle. It is ignored during XML serialization.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> LeftKeyValueRectObservable = new ObservableValue<Rect>(
            new Rect() {
                X = 0, Y = 0, Width = _KeyValueSize.Width, Height = _KeyValueSize.Height
            });

        /// <summary>
        /// Represents an observable value containing the rectangle that defines the position and size of the right key
        /// value area.
        /// </summary>
        /// <remarks>The rectangle is initialized based on the default interior and key value sizes.
        /// Changes to this value can be observed to update UI elements or respond to layout changes.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> RightKeyValueRectObservable = new ObservableValue<Rect>(
            new Rect()
            {
                X = _InteriorSize.Width - _KeyValueSize.Width,
                Y = 0,
                Width = _KeyValueSize.Width,
                Height = _KeyValueSize.Height
            });

        /// <summary>
        /// Represents an observable value containing the bounding rectangle for displaying error messages.
        /// </summary>
        /// <remarks>The initial rectangle is centered horizontally and positioned above the instructions
        /// area, using default size values. This field is intended for internal use to track changes to the error
        /// display region.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> ErrorRectObservable = new ObservableValue<Rect>(
            new Rect()
            {
                X = (_InteriorSize.Width - _ErrorSize.Width) / 2,
                Y = _InteriorSize.Height - _InstructionsSize.Height - _ErrorSize.Height,
                Width = _ErrorSize.Width, 
                Height = _ErrorSize.Height
            });

        /// <summary>
        /// Represents an observable value that tracks the bounding rectangle for continue instructions.
        /// </summary>
        /// <remarks>This field is intended for internal use to monitor changes to the continue
        /// instructions area. It is ignored during XML serialization.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> ContinueInstructionsRectObservable = new ObservableValue<Rect>(
            new Rect()
            {
                X = 0,
                Y = 0,
                Width = _ContinueInstructionsSize.Width,
                Height = _InteriorSize.Height - _ContinueInstructionsSize.Height
            });

        /// <summary>
        /// Represents an observable value that tracks the bounding rectangle for the instruction text area on an instruction screen.
        /// </summary>
        [XmlIgnore]
        public readonly ObservableValue<Rect> TextInstructionScreenRectObservable = new ObservableValue<Rect>(new Rect(0, 0, _InteriorSize.Width, _InteriorSize.Height - _ContinueInstructionsSize.Height));

        /// <summary>
        /// Represents an observable value that tracks the bounding rectangle for the key instruction screen text area.
        /// </summary>
        /// <remarks>This field is intended for internal use to monitor changes to the layout region where
        /// key instructions are displayed. The value is updated as the UI layout changes.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> KeyInstructionScreenTextRectObservable = new ObservableValue<Rect>(
            new Rect(0, _KeyValueSize.Height, _InteriorSize.Width, _InteriorSize.Height - _ContinueInstructionsSize.Height - _KeyValueSize.Height));

        /// <summary>
        /// Represents an observable value that tracks the bounding rectangle for mock item instructions in the UI.
        /// </summary>
        /// <remarks>This field is intended for internal use to monitor changes to the instructions'
        /// display area. It is ignored during XML serialization.</remarks>
        [XmlIgnore]
        public readonly ObservableValue<Rect> MockItemInstructionsRectObservable = new ObservableValue<Rect>(
            new Rect()
            {
                X = (_InteriorSize.Width - _InstructionsSize.Width) / 2,
                Y = _InteriorSize.Height - _ContinueInstructionsSize.Height - _InstructionsSize.Height,
                Width = _InstructionsSize.Width,
                Height = _InstructionsSize.Height
            });


        private static Size _InteriorSize = new Size(600, 600);
        private static Size _StimulusSize = new Size(540, 300);
        private static Size _KeyValueSize = new Size(200, 120);
        private static Size _InstructionsSize = new Size(570, 80);
        private static Size _ErrorSize = new Size(50, 50);
        private static Size _ContinueInstructionsSize = new Size(570, 30);
        private const int BorderWidth = 10;
        private const int TextStimulusPaddingTop = 50;

        /// <summary>
        /// Gets the total size of the layout, including borders.
        /// </summary>
        public Size TotalSize
        {
            get
            {
                var rect = InteriorRectObservable.Value;
                rect.Inflate(BorderWidth, BorderWidth);
                return rect.Size;
            }
        }

        /// <summary>
        /// Gets or sets the size of the interior content area.
        /// </summary>
        /// <remarks>Setting this property updates the width and height of the interior rectangle, while
        /// the origin remains unchanged. The value reflects the current dimensions of the content area, excluding any
        /// borders or padding.</remarks>
        [XmlElement("InteriorSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        public Size InteriorSize
        {
            get
            {
                return InteriorRectObservable.Value.Size;
            }
            set
            {
                InteriorRectObservable.Value = new Rect(0, 0, value.Width, value.Height);
            }
        }

        /// <summary>
        /// Gets the interior rectangle of the element, excluding any borders or padding.
        /// </summary>
        [XmlIgnore]
        public Rect InteriorRect
        {
            get
            {
                return InteriorRectObservable.Value;
            }
        }

        /// <summary>
        /// gets or sets the size of the key value rectangles
        /// </summary>
        [XmlElement("KeyValueSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        public Size KeyValueSize
        {
            get
            {
                return LeftKeyValueRectObservable.Value.Size;
            }
            set
            {
                var rect = LeftKeyValueRectObservable.Value;
                rect.Size = value;
                LeftKeyValueRectObservable.Value = rect;
                rect = RightKeyValueRectObservable.Value;
                rect.Offset(-value.Width, 0);
                rect.Size = value;
                RightKeyValueRectObservable.Value = rect;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle for the left key-value pair in the layout.
        /// </summary>
        [XmlIgnore]
        public Rect LeftKeyValueRectangle
        {
            get
            {
                return LeftKeyValueRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets the outline rectangle for the left key-value area.
        /// </summary>
        [XmlIgnore]
        public Rect LeftKeyValueOutlineRectangle
        {
            get
            {
                return LeftKeyValueRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets the rectangle that defines the area for the right key-value pair.
        /// </summary>
        [XmlIgnore]
        public Rect RightKeyValueRectangle
        {
            get
            {
                return RightKeyValueRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle that outlines the right key-value pair.
        /// </summary>
        [XmlIgnore]
        public Rect RightKeyValueOutlineRectangle
        {
            get
            {
                return RightKeyValueRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the stimulus area.
        /// </summary>
        /// <remarks>Setting this property adjusts the dimensions of the stimulus rectangle while keeping
        /// its center position unchanged. The size is specified in pixels.</remarks>
        [XmlElement("StimulusSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))] 
        public Size StimulusSize
        {
            get
            {
                return StimulusRectObservable.Value.Size;   
            }
            set
            {
                var rect = StimulusRectObservable.Value;
                rect.Inflate(new Size((value.Width - rect.Width) / 2, (value.Height - rect.Height) / 2));
                StimulusRectObservable.Value = rect;
            }
        }

        /// <summary>
        /// Gets the current rectangle that defines the area of the stimulus.
        /// </summary>
        [XmlIgnore]
        public Rect StimulusRectangle
        {
            get
            {
                return StimulusRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the instructions area in device-independent pixels.
        /// </summary>
        /// <remarks>Setting this property adjusts the size of the instructions area by inflating or
        /// deflating its bounding rectangle. The size is centered relative to the current rectangle. The value should
        /// be a non-negative size.</remarks>
        [XmlElement("InstructionsSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))] 
        public Size InstructionsSize
        {
            get
            {
                return BlockInstructionsRectObservable.Value.Size;
            }
            set
            {
                var rect = BlockInstructionsRectObservable.Value;
                rect.Inflate(new Size((value.Width - rect.Width) / 2, (value.Height - rect.Height) / 2));
                BlockInstructionsRectObservable.Value = rect;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle that defines the area for displaying instructions.  
        /// </summary>
        [XmlIgnore]
        public Rect InstructionsRectangle
        {
            get
            {
                return BlockInstructionsRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the error area.
        /// </summary>
        /// <remarks>Setting this property adjusts the dimensions of the error rectangle while keeping it
        /// centered. The value represents the overall width and height of the error region.</remarks>
        [XmlElement("ErrorSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        public Size ErrorSize
        {
            get
            {
                return ErrorRectObservable.Value.Size;
            }
            set
            {
                var rect = ErrorRectObservable.Value; 
                rect.Inflate(new Size((value.Width - rect.Width) / 2, (value.Height - rect.Height) / 2));
                ErrorRectObservable.Value = rect;
            }
        }

        /// <summary>
        /// Gets the rectangle that defines the area where an error is displayed.
        /// </summary>
        [XmlIgnore]
        public Rect ErrorRectangle
        {
            get
            {
                return ErrorRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets or sets the size of the continue instructions area.    
        /// </summary>
        [XmlElement("ContinueInstructionsSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        public Size ContinueInstructionsSize
        {
            get
            {
                return ContinueInstructionsRectObservable.Value.Size;
            }
            set
            {
                var rect = ContinueInstructionsRectObservable.Value;
                rect.Inflate(new Size((value.Width - rect.Width) / 2, (value.Height - rect.Height) / 2));
                ContinueInstructionsRectObservable.Value = rect;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle for the continue instructions area.
        /// </summary>
        [XmlIgnore]
        public Rect ContinueInstructionsRectangle
        {
            get
            {
                return ContinueInstructionsRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets the rectangle that defines the area of the instruction screen text.
        /// </summary>
        [XmlIgnore]
        public Rect InstructionScreenTextAreaRectangle
        {
            get
            {
                return TextInstructionScreenRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle of the text area used for displaying key instructions on the screen.
        /// </summary>
        [XmlIgnore]
        public Rect KeyInstructionScreenTextAreaRectangle
        {
            get
            {
                return KeyInstructionScreenTextRectObservable.Value;
            }
        }

        /// <summary>
        /// Gets the bounding rectangle for the mock item instructions area.
        /// </summary>
        [XmlIgnore]
        public Rect MockItemInstructionsRectangle
        {
            get
            {
                return MockItemInstructionsRectObservable.Value;
            }
        }


        /// <summary>
        /// Initializes a new instance of the Layout class by copying the sizes from an existing Layout instance.
        /// </summary>
        /// <remarks>This constructor creates a deep copy of the size-related properties from the
        /// specified Layout instance. Other properties not shown may not be copied.</remarks>
        /// <param name="layout">The Layout instance from which to copy size values. Cannot be null.</param>
        public Layout(Layout layout)
        {
            this.InteriorSize = layout.InteriorSize;
            this.ErrorSize = layout.ErrorSize;
            this.StimulusSize = layout.StimulusSize;
            this.InstructionsSize = layout.InstructionsSize;
            this.KeyValueSize = layout.KeyValueSize;
            this.ContinueInstructionsSize = layout.ContinueInstructionsSize;
        }

        /// <summary>
        /// Determines which defined rectangles overlap and returns a bitwise combination of overlap flags.
        /// </summary>
        /// <remarks>Each bit in the returned value corresponds to a specific type of overlap between
        /// rectangles, as defined by the Overlap enumeration. This method can be used to detect layout conflicts or
        /// invalid arrangements among the rectangles.</remarks>
        /// <returns>A bitwise combination of values from the Overlap enumeration indicating which rectangles overlap. Returns 0
        /// if there are no overlaps.</returns>
        public uint FindOverlap()
        {
            uint returnValue = 0;

            // check for overlap between key value rectangles with each other
            if (Rect.Intersect(LeftKeyValueRectangle, RightKeyValueRectangle) != Rect.Empty)
                returnValue |= Overlap.KeyValueRectangles;

            // check for overlap between key value rectangles and stimulus rectangle
            if (Rect.Intersect(LeftKeyValueRectangle, StimulusRectangle) != Rect.Empty)
                returnValue |= Overlap.KeyValueRectangles | Overlap.StimulusRectangle;

            // check for overlap between key value rectangles and error rectangle
            if (Rect.Intersect(LeftKeyValueRectangle, ErrorRectangle) != Rect.Empty)
                returnValue |= Overlap.KeyValueRectangles | Overlap.ErrorRectangle;

            // check for overlap between key value rectangles and instruction rectangle
            if (Rect.Intersect(LeftKeyValueRectangle, InstructionsRectangle) != Rect.Empty)
                returnValue |= Overlap.KeyValueRectangles | Overlap.InstructionRectangle;

            // check for overlap between stimulus rectangle and error rectangle
            if (Rect.Intersect(StimulusRectangle, ErrorRectangle) != Rect.Empty)
                returnValue |= Overlap.StimulusRectangle | Overlap.ErrorRectangle;

            // check for overlap between stimulus rectangle and instruction rectangle
            if (Rect.Intersect(StimulusRectangle, InstructionsRectangle) != Rect.Empty)
                returnValue |= Overlap.StimulusRectangle | Overlap.InstructionRectangle;

            // check for overlap between error rectangle and instruction rectangle
            if (Rect.Intersect(InstructionsRectangle, ErrorRectangle) != Rect.Empty)
                returnValue |= Overlap.InstructionRectangle | Overlap.ErrorRectangle;

            return returnValue;
        }

        /// <summary>
        /// Gets the maximum allowable height for the stimulus area, adjusted based on the current layout constraints.
        /// </summary>
        /// <remarks>The maximum height is calculated by subtracting the heights of the error display and
        /// instructions area from the available interior space. If the stimulus width exceeds the available width after
        /// accounting for key value areas, the key value height is also subtracted. This property is useful for
        /// dynamically sizing UI elements to fit within the designated display area.</remarks>
        [XmlIgnore]
        public int MaxStimulusHeight
        {
            get
            {
                if (StimulusRectangle.Width > InteriorSize.Width - (KeyValueSize.Width * 2))
                    return (int)(InteriorSize.Height - ErrorSize.Height - InstructionsRectangle.Height - KeyValueSize.Height);
                return (int)(InteriorSize.Height - ErrorSize.Height - InstructionsRectangle.Height);
            }
        }

        /// <summary>
        /// Gets the maximum allowable width for the stimulus area, based on the current interior and key value sizes.
        /// </summary>
        /// <remarks>The maximum width is constrained to ensure that the stimulus does not overlap with
        /// the key value areas. If the stimulus size exceeds the available space, the value is reduced
        /// accordingly.</remarks>
        [XmlIgnore]
        public int MaxStimulusWidth
        {
            get
            {
                if (StimulusSize.Width > InteriorSize.Width - (KeyValueSize.Width * 2))
                    return (int)(InteriorSize.Width - (KeyValueSize.Width * 2));
                return (int)InteriorSize.Width;
            }
        }

        /// <summary>
        /// Gets the maximum allowable height, in pixels, for displaying instructions within the available layout area.
        /// </summary>
        /// <remarks>The returned value accounts for the current sizes of the stimulus, error message, and
        /// key value elements, ensuring that instructions do not overlap with these components. The calculation
        /// dynamically adjusts based on the layout and element sizes.</remarks>
        [XmlIgnore]
        public int MaxInstructionsHeight
        {
            get
            {
                if (StimulusSize.Width > InteriorSize.Width - (KeyValueSize.Width * 2))
                    return (int)(InteriorSize.Height - ErrorSize.Height - StimulusRectangle.Height - KeyValueSize.Height);
                return (int)(InteriorSize.Height - ErrorSize.Height - StimulusRectangle.Height);
            }
        }

        /// <summary>
        /// Gets the maximum width, in pixels, available for displaying instructions.
        /// </summary>
        [XmlIgnore]
        public int MaxInstructionsWidth
        {
            get 
            {
                return (int)InteriorRectObservable.Value.Width;
            }
        }

        /// <summary>
        /// Gets the maximum allowable height for the error mark area based on the current layout dimensions.
        /// </summary>
        /// <remarks>The returned value accounts for the sizes of the stimulus, instructions, and key
        /// value areas to ensure the error mark fits within the available space. The calculation adapts if the stimulus
        /// width exceeds the available interior width minus the combined key value widths.</remarks>
        [XmlIgnore]
        public int MaxErrorMarkHeight
        {
            get
            {
                if (StimulusSize.Width > InteriorSize.Width - (KeyValueSize.Width * 2))
                    return (int)(InteriorSize.Height - InstructionsSize.Height - StimulusRectangle.Height - KeyValueSize.Height);
                return (int)(InteriorSize.Height - InstructionsSize.Height - StimulusRectangle.Height);
            }
        }

        /// <summary>
        /// Gets the maximum width, in pixels, allowed for error marks.
        /// </summary>
        [XmlIgnore]
        public int MaxErrorMarkWidth
        {
            get
            {
                return MaxErrorMarkHeight;
            }
        }

        /// <summary>
        /// Gets the maximum allowable height, in pixels, for displaying a key-value pair within the current layout
        /// constraints.
        /// </summary>
        /// <remarks>The returned value accounts for the sizes of other UI elements, such as the stimulus,
        /// instructions, and error messages, to ensure that key-value pairs fit within the available interior
        /// space.</remarks>
        [XmlIgnore]
        public int MaxKeyValueHeight
        {
            get
            {
                if (StimulusSize.Width > InteriorSize.Width - (KeyValueSize.Width * 2))
                    return (int)(InteriorSize.Height - StimulusSize.Height - InstructionsSize.Height - ErrorSize.Height);
                return (int)(InteriorSize.Height - InstructionsSize.Height);
            }
        }

        /// <summary>
        /// Gets the maximum allowed width, in pixels, for a key-value pair within the current layout constraints.
        /// </summary>
        /// <remarks>The returned value is determined by the available interior width and the size of the
        /// stimulus. This property is useful for ensuring that key-value pairs do not exceed the space allocated in the
        /// layout.</remarks>
        [XmlIgnore]
        public int MaxKeyValueWidth
        {
            get
            {
                if (StimulusSize.Width > InteriorSize.Width - (KeyValueSize.Width * 2))
                    return (int)(InteriorSize.Width - StimulusSize.Width);
                return (int)(InteriorSize.Width / 2);
            }
        }

        /// <summary>
        /// Determines whether the current layout is valid by checking for overlapping elements.
        /// </summary>
        /// <returns>true if no overlaps are detected in the layout; otherwise, false.</returns>
        public bool IsValidLayout()
        {
            return FindOverlap() == 0;
        }
    }
}
