using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.Models.Enumerations;

namespace IAT.Core.Models.Serializable
{
    /// <summary>
    /// Specifies the possible positions of a key relative to a reference point or object.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the spatial orientation or placement of a key, such as in
    /// graphical layouts, user interfaces, or mapping scenarios. The values represent common directional positions and
    /// can be used to determine alignment or navigation logic.</remarks>
    public enum KeyPosition
    {
        None,
        Left,
        Right,
        UpperLeft,
        UpperRight,
        LowerLeft,
        LowerRight
    }

    /// <summary>
    /// Specifies the type of key used for an operation or entity.
    /// </summary>
    /// <remarks>Use this enumeration to indicate the key structure or behavior required by a component or
    /// algorithm. The meaning of each value depends on the context in which it is used.</remarks>
    public enum KeyType
    {
        None,
        Simple,
        Reversed,
        Dual
    }

    public class Key : IPackagePart
    {

        /// <summary>
        /// Gets or sets the URI associated with this instance.
        /// </summary>
        [XmlElement("Uri", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public Uri? Uri { get; set; }

        /// <summary>
        /// Gets the type of the package part represented by this instance.
        /// </summary>
        [XmlIgnore]
        public PartType PackagePartType => PartType.Key;

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the name associated with the current instance.
        /// </summary>
        [XmlElement("Name", Form = XmlSchemaForm.Unqualified)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the Key class.
        /// </summary>
        public Key() { }

        [XmlIgnore]
        public virtual KeyType KeyType => KeyType.Simple;

        /// <summary>
        /// Gets or sets the unique identifier of the left display item.
        /// </summary>
        [XmlElement("LeftDisplayItemId", Form = XmlSchemaForm.Unqualified)]
        public Guid LeftDisplayItemId { get;set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the unique identifier of the right display item.
        /// </summary>
        [XmlElement("RightDisplayItemId", Form = XmlSchemaForm.Unqualified)]
        public Guid RightDisplayItemId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the display item shown on the left side.
        /// </summary>
        [XmlIgnore]
        public virtual DIBase? LeftDisplayItem { get; set; } = null;

        /// <summary>
        /// Gets or sets the display item shown on the right side.
        /// </summary>
        [XmlIgnore]
        public virtual DIBase? RightDisplayItem{ get; set; } = null;

        /// <summary>
        /// Gets or sets the collection of key identifiers to include in the operation.
        /// </summary>
        /// <remarks>Each identifier in the collection represents a unique key to be processed. The order
        /// of the identifiers is preserved. The collection may be empty if no specific keys are to be
        /// included.</remarks>
        [XmlArray("IncludingKeyIds", Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("KeyId", Form = XmlSchemaForm.Unqualified)]
        public required List<Guid> IncludingKeyIds { get; init; } = [];

        /// <summary>
        /// Gets the collection of keys to include in the operation.
        /// </summary>
        public required List<Key> IncludingKeys { get; init; } = [];

/*
        public void InvalidateBlockPreviews()
        {
            List<Uri> BlockUris = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATBlock)).Select(pr => pr.TargetUri).ToList();
            List<Uri> owningKeyUris = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATKey), "owned-by").Select(pr => pr.TargetUri).ToList();
            foreach (Uri u in owningKeyUris)
                BlockUris.AddRange(CIAT.SaveFile.GetRelationshipsByType(u, typeof(CIATKey), typeof(CIATBlock)).Select(pr => pr.TargetUri).ToList());
            foreach (CIATBlock b in BlockUris.Select(u => CIAT.SaveFile.GetIATBlock(u)))
                b.InvalidateKeys();
            List<Uri> InstructionScreenUris = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CInstructionScreen)).Select(pr => pr.TargetUri).ToList();
            foreach (CInstructionScreen scrn in InstructionScreenUris.Select(u => CIAT.SaveFile.GetInstructionScreen(u)))
                scrn.ResponseKeyUri = this.URI;
            foreach (Uri u in owningKeyUris)
            {
                InstructionScreenUris.Clear();
                InstructionScreenUris.AddRange(CIAT.SaveFile.GetRelationshipsByType(u, typeof(CIATKey), typeof(CInstructionScreen)).Select(pr => pr.TargetUri).ToList());
                foreach (CInstructionScreen scrn in InstructionScreenUris.Select(scrnUri => CIAT.SaveFile.GetInstructionScreen(scrnUri)))
                    scrn.ResponseKeyUri = u;
            }
        }

        public virtual void Load(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument doc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Name = doc.Root.Element("Name").Value;
            String rId = doc.Root.Element("LeftRId").Value;
            if (rId != String.Empty)
                _LeftValueUri = CIAT.SaveFile.GetRelationship(this, rId).TargetUri;
            else
                _LeftValueUri = null;
            rId = doc.Root.Element("RightRId").Value;
            if (rId != String.Empty)
                _RightValueUri = CIAT.SaveFile.GetRelationship(this, rId).TargetUri;
            else
                _RightValueUri = null;
        }

        public virtual void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement("IATKey", new XAttribute("Type", KeyType.ToString()), new XElement("Name", Name),
                new XElement("LeftRId", CIAT.SaveFile.GetRelationship(this, _LeftValueUri)), new XElement("RightRId", CIAT.SaveFile.GetRelationship(this, _RightValueUri))));
            Task.Run(() =>
            {
                Stream s = CIAT.SaveFile.GetWriteStream(this);
                xDoc.Save(s);
                s.Dispose();
                CIAT.SaveFile.ReleaseWriteStreamLock();
            });
        }

        private bool IsDisposed { get; set; } = false;
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if (LeftValueUri != null)
                (LeftValue as IResponseKeyDI).ReleaseKeyOwner(this);
            if (RightValueUri != null)
                (RightValue as IResponseKeyDI).ReleaseKeyOwner(this);
            foreach (var pp in KeyOwners)
                ReleaseOwner(pp);
            foreach (Uri u in CIAT.SaveFile.GetPartsOfType(CIATBlock._MimeType))
            {
                String rId;
                CIATBlock b = CIAT.SaveFile.GetIATBlock(u);
                if ((rId = CIAT.SaveFile.GetRelationship(b, this)) != null)
                    CIAT.SaveFile.DeleteRelationship(u, rId);
            }
            CIAT.SaveFile.DeletePart(this.URI);
            CIAT.SaveFile.DeleteRelationship(CIAT.SaveFile.IAT.URI, this.URI);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
        }

        public static List<CFontFile.FontItem> UtilizedFontFamilies
        {
            get
            {
                List<CFontFile.FontItem> resultList = new List<CFontFile.FontItem>();
                String fontFamLeft, fontFamRight;
                List<DIText> tdis = new List<DIText>();
                foreach (CIATKey key in CIAT.SaveFile.GetAllIATKeyUris().Select(u => CIAT.SaveFile.GetIATKey(u)))
                {
                    tdis.Clear();
                    if (key.KeyType == IATKeyType.SimpleKey)
                    {
                        DIBase leftVal = key.LeftValue, rightVal = key.RightValue;
                        fontFamLeft = String.Empty;
                        fontFamRight = String.Empty;
                        if (leftVal.Type == DIType.ResponseKeyText)
                        {
                            fontFamLeft = (leftVal as DIText).PhraseFontFamily;
                            tdis.Add(leftVal as DIText);
                        }
                        if (rightVal.Type == DIType.ResponseKeyText)
                        {
                            fontFamRight = (rightVal as DIText).PhraseFontFamily;
                            tdis.Add(rightVal as DIText);
                        }
                        if ((fontFamLeft == fontFamRight) && (tdis.Count > 0))
                        {
                            resultList.Add(new CFontFile.FontItem(fontFamLeft, String.Format("is used by left and right response values in {0}", key.Name), null, tdis));
                        }
                        else if (tdis.Count > 0)
                        {
                            if (fontFamLeft != String.Empty)
                                resultList.Add(new CFontFile.FontItem(fontFamLeft, String.Format("is used by left response value in {0}", key.Name), null, tdis.Where(tdi => tdi.PhraseFontFamily == fontFamLeft)));
                            if (fontFamRight != String.Empty)
                                resultList.Add(new CFontFile.FontItem(fontFamRight, String.Format("is used by right response value in {0}", key.Name), null, tdis.Where(tdi => tdi.PhraseFontFamily == fontFamRight)));
                        }

                    }
                }
                return resultList;
            }
        }

        */
    }
}
