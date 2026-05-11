using IAT.Core.ConfigFile;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models
{
    /// <summary>
    /// A model object that encapsulates all the necessary components of a test package, including configuration settings, 
    /// file and slide manifests, events, and display items. This class serves as a container for all the data required to 
    /// define and execute a test within the IAT framework. It allows for easy access and management of the various elements 
    /// that make up a test package, facilitating the organization and execution of tests in a structured manner.
    /// </summary>
    internal class TestPackage
    {
        public IATConfigFile ConfigFile { get; set; } = new();
        public Manifest FileManifest { get; set; } = new();
        public Manifest SlideManifest { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<DisplayItem> DisplayItems { get; set; } = new();
    }
}
