using IAT.Core.ConfigFile;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models
{
    internal class TestPackage
    {
        public IATConfigFile ConfigFile { get; set; } = new();
        public Manifest FileManifest { get; set; } = new();
        public Manifest SlideManifest { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<DisplayItem> DisplayItems { get; set; } = new();
    }
}
