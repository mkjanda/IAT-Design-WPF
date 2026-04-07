using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Serializable;

namespace IAT.Core.Services
{
    public interface ISaveFileService
    {

        public Serializable.Version CurrentVersion { get; }
    }
}
