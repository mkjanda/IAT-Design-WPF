using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.ConfigFile;
using IAT.Core.Serializable;
using IAT.Core.Domain;

namespace IAT.Core.Services.Export
{
    /// <summary>
    /// Represents the context for exporting a test, including events, display items, and layout information.
    /// </summary>
    public sealed class ExportContext
    {
        /// <summary>
        /// A list for adding events to the export context. This collection is used to store events that will be 
        /// included in the exported test package, such as instructions screens, trial events, and other relevant 
        /// actions that occur during the test. The events added to this list will be processed and included in the 
        /// final export output, allowing for a structured representation of the test's flow and content.
        /// </summary>
        public List<Event> Events { get; } = new List<Event>();

        /// <summary>
        /// Collection of display items.
        /// </summary>
        public List<DisplayItem> DisplayItems { get; } = new List<DisplayItem>();

        /// <summary>
        /// Gets the file manifest.
        /// </summary>
        public Manifest FileManifest { get; } = new Manifest();

        /// <summary>
        /// Maps unique identifiers to integer values.
        /// </summary>
        public Dictionary<Guid, int> IdDictionary { get; } = new Dictionary<Guid, int>();

        /// <summary>
        /// Gets or initializes the IAT test.
        /// </summary>
        public required IatTest Test { get; init; }

        /// <summary>
        /// The layout rectangles.
        /// </summary>
        public required LayoutRects LayoutRects { get; init; }

        /// <summary>
        /// Adds an event to the collection.
        /// </summary>
        /// <param name="evt">The event to add.</param>
        public void AddEvent(Event evt) => Events.Add(evt);

        /// <summary>
        /// Adds a display item to the collection.
        /// </summary>
        /// <param name="item">The display item to add.</param>
        public void AddDisplayItem(DisplayItem item) => DisplayItems.Add(item);

    }
}
