using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Models.Enumerations;

namespace IAT.Core.Models
{
    public interface IPackagePart : IDisposable
    {
        /// <summary>
        /// Gets or sets the URI that uniquely identifies this package part within the package.
        /// </summary>
        Uri URI { get; set; }

        PartType PackagePartType { get; }

        String MimeType { get; } 
    }
}
