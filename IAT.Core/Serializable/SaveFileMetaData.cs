using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace IAT.Core.Models 
{
    public sealed class SaveFileMetaData : IPackagePart
    {
        /// <summary>
        /// Gets or sets the URI that identifies the part within the package.
        /// </summary>
        [XmlIgnore]
        public Uri? Uri { get; set; } = PackUriHelper.CreatePartUri(new Uri(typeof(SaveFileMetaData).ToString(), UriKind.Relative));

        /// <summary>
        /// Gets the package part type associated with save file metadata.
        /// </summary>
        [XmlIgnore]
        public PartType PackagePartType => PartType.SaveFileMetaData;

        /// <summary>
        /// Gets or sets the unique identifier associated with this instance.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.Empty;


    }
}
