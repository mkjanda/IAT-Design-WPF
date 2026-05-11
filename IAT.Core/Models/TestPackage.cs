using IAT.Core.ConfigFile;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IAT.Core.Models
{
    /// <summary>
    /// A model object that encapsulates all the necessary components of a test package, including configuration settings, 
    /// file and slide manifests, events, and display items. This class serves as a container for all the data required to 
    /// define and execute a test within the IAT framework. It allows for easy access and management of the various elements 
    /// that make up a test package, facilitating the organization and execution of tests in a structured manner.
    /// </summary>
    public class TestPackage
    {
        /// <summary>
        /// Text configuration settings for the IAT test, including properties that define how the test should be structured and presented.
        /// </summary>
        public IATConfigFile ConfigFile { get; set; } = new();

        /// <summary>
        /// Gets or sets the manifest that describes the files included in the operation.
        /// </summary>
        public Manifest FileManifest { get; set; } = new();

        /// <summary>
        /// Gets or sets the manifest that defines the structure and metadata for slides.
        /// </summary>
        public Manifest SlideManifest { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of events associated with this instance.
        /// </summary>
        public List<Event> Events { get; set; } = new();

        /// <summary>
        /// Gets or sets the collection of items to be displayed in the user interface.
        /// </summary>
        public List<DisplayItem> DisplayItems { get; set; } = new();

        public MemoryStream ConfigFileStream { get; set; } = new MemoryStream();

        public void Clear()
        {
            ConfigFile = new IATConfigFile();
            FileManifest = new Manifest();
            SlideManifest = new Manifest();
            Events = new List<Event>();
            DisplayItems = new List<DisplayItem>();
            ConfigFileStream.Dispose();
            ConfigFileStream = new MemoryStream();
        }
    }
}
