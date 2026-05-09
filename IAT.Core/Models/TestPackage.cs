using IAT.Core.ConfigFile;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models
{
    internal class TestPackage
    {
        public IATConfigFile ConfigFile { get; set; } = new IATConfigFile();
        public Manifest FileManifest { get; set; } = new Manifest();
        public Manifest SlideManifest { get; set; } = new Manifest();
        public byte[] FileData { get; set; }
        public byte[] SlideData { get; set; }
    }
}
