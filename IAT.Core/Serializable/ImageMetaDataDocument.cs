using IAT.Core.Models;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Enumerations;

namespace IAT.Core.Serializable
{

    /// <summary>
    /// Represents a package part that stores image metadata entries for a document package.
    /// </summary>
    /// <remarks>This class manages a collection of image metadata entries, providing methods to load, save,
    /// and remove entries as part of a document packaging system. It implements the IPackagePart interface to integrate
    /// with package management workflows. Thread safety is not guaranteed; callers should ensure appropriate
    /// synchronization if accessed concurrently.</remarks>
    public class ImageMetaDataDocument : IPackagePart
    {
        public Uri? Uri { get; set; } = PackUriHelper.CreatePartUri(new Uri("ImageMetaData.xml", UriKind.Relative));
        public PartType PackagePartType => PartType.ImageMetaDataDocument;
        public Guid Id { get; set; } = Guid.Empty;

        [XmlIgnore]
        public required Dictionary<string, ImageMetaData> Entries { get; init; } = new();


        struct 

        [XmlArray("Entries")]
        [XmlrrayItem("Emtry")]



        public ImageMetaDataDocument()
        {
        }

        public void CleanPackageForSave()
        {
            var l = Entries.Values.Where(kv => Images.ImageMediaType.FromDIType(kv.DIType) == Images.ImageMediaType.FullWindow).ToList();
            foreach (var md in l)
                md.Image.Dispose();
        }

        public void RemoveEntry(Images.IImage iImage)
        {
            var pr = CIAT.SaveFile.GetRelationship(this, iImage);
            if (pr == null)
                return;
            Entries.Remove(pr);
            CIAT.SaveFile.DeleteRelationship(URI, pr);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement(GetType().Name));
            foreach (var md in Entries.Values)
                md.Append(xDoc.Root);
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetWriteStream(this));
            try
            {
                xDoc.Save(s);
            }
            finally
            {
                s.Dispose();
                CIAT.SaveFile.ReleaseWriteStreamLock();
            }
        }

        public void Load()
        {
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetReadStream(this));
            XDocument xDoc;
            try
            {
                xDoc = XDocument.Load(s);
            }
            finally
            {
                s.Dispose();
                CIAT.SaveFile.ReleaseReadStreamLock();
            }
            foreach (var meta in xDoc.Root.Elements(typeof(ImageMetaDataDocument).Name))
            {
                var data = new ImageMetaData(this, meta);
                Entries[data.ImageRelId] = data;
            }
        }

        public void Dispose()
        {
            foreach (var md in Entries)
            {
                var img = md.Value.Image;
                CIAT.SaveFile.DeleteRelationship(URI, md.Key);
            }
            Entries.Clear();
            CIAT.SaveFile.DeletePackageLevelRelationship(BaseType);
        }
    }
}
