using IAT.Core.Models;
using IAT.Core.Models.Enumerations;
using java.util;
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
using System.Xml.Linq;

namespace IAT.Core.Models
{
    public class KeyDIPosition : Enumeration
    {
        public readonly static KeyDIPosition Left = new KeyDIPosition(1, "Left");
        public readonly static KeyDIPosition Right = new KeyDIPosition(2, "Right");
        public readonly static KeyDIPosition UpperLeft = new KeyDIPosition(3, "UpperLeft");
        public readonly static KeyDIPosition CenterLeft = new KeyDIPosition(4, "CenterLeft");
        public readonly static KeyDIPosition LowerLeft = new KeyDIPosition(5, "LowerLeft");
        public readonly static KeyDIPosition UpperRight = new KeyDIPosition(6, "UpperRight");
        public readonly static KeyDIPosition CenterRight = new KeyDIPosition(7, "CenterRight");
        public readonly static KeyDIPosition LowerRight = new KeyDIPosition(8, "LowerRight");

        private static readonly KeyDIPosition[] All = new KeyDIPosition[] { Left, Right, UpperLeft, CenterLeft, LowerLeft, UpperRight, CenterRight, LowerRight };

        public static KeyDIPosition FromString(String str)
        {
            return All.Where(i => i.Name == str).FirstOrDefault();
        }

        public KeyDIPosition(int val, String name) : base(val, name) { }
    }

    public class IATKeyType : Enumeration
    {
        public readonly static IATKeyType None = new IATKeyType(0, "None", typeof(Nullable), new Func<Uri, CIATKey>((u) => null));
        public readonly static IATKeyType SimpleKey = new IATKeyType(1, "SimpleKey", typeof(CIATKey), new Func<Uri, CIATKey>((u) => new CIATKey(u)));
        public readonly static IATKeyType ReversedKey = new IATKeyType(2, "ReversedKey", typeof(CIATReversedKey), new Func<Uri, CIATKey>((u) => new CIATReversedKey(u)));
        public readonly static IATKeyType DualKey = new IATKeyType(3, "DualKey", typeof(CIATDualKey), new Func<Uri, CIATKey>((u) => new CIATDualKey(u)));

        public Type Type { get; private set; }
        public Func<Uri, CIATKey> Create { get; private set; }
        protected IATKeyType(int val, String name, Type t, Func<Uri, CIATKey> create)
            : base(val, name)
        {
            Type = t;
            Create = create;
        }


        private static IATKeyType[] All = new IATKeyType[] { None, SimpleKey, ReversedKey, DualKey };

        public static IATKeyType FromTypeName(String tName)
        {
            return All.FirstOrDefault((kt) => kt.Type.ToString() == tName);
        }

        public static IATKeyType FromString(String type)
        {
            return All.FirstOrDefault((kt) => kt.Name == type);
        }

        public static IATKeyType FromType(Type t)
        {
            return All.FirstOrDefault(kt => kt.Type == t);
        }

    }

    public class Key : IPackagePart, IDisposable
    {
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(CIATKey); } }
        public String MimeType { get { return "text/xml+" + this.GetType().ToString(); } }
        public long Expiration { get; set; }
        public CIATKey()
        {
            this.URI = CIAT.SaveFile.Register(this);
            this.Name = String.Empty;
            CIAT iat = CIAT.SaveFile.IAT;
            CIAT.SaveFile.CreateRelationship(iat.BaseType, this.BaseType, iat.URI, this.URI);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public CIATKey(Uri uri)
        {
            this.URI = uri;
            Load(uri);
            CIAT.SaveFile.Register(this);
        }

        public virtual void ValueChanged()
        {
            InvalidateValues();
            foreach (var key in KeyOwners)
                (key as CIATKey).ValueChanged();
        }

        protected virtual void InvalidateValues()
        {
        }

        public IATKeyType KeyType
        {
            get
            {
                return IATKeyType.FromType(this.GetType());
            }
        }

        public String Name { get; set; }

        public virtual Size GetDISize(DIBase di)
        {
            if (di.IImage.OriginalImage == null)
                return di.IImage.AbsoluteBounds.Size;
            double arDI = (double)di.IImage.OriginalSize.Width / (double)di.IImage.OriginalSize.Height;
            double arBounds = (double)CIAT.SaveFile.Layout.KeyValueSize.Width / (double)CIAT.SaveFile.Layout.KeyValueSize.Height;
            if ((arDI > arBounds) && (di.IImage.OriginalSize.Width > CIAT.SaveFile.Layout.KeyValueSize.Width))
                return new Size(CIAT.SaveFile.Layout.KeyValueSize.Width, (int)((double)CIAT.SaveFile.Layout.KeyValueSize.Width / arDI));
            if ((arDI < arBounds) && (di.IImage.OriginalSize.Height > CIAT.SaveFile.Layout.KeyValueSize.Height))
                return new Size((int)((double)CIAT.SaveFile.Layout.KeyValueSize.Height * arDI), CIAT.SaveFile.Layout.KeyValueSize.Height);
            return di.IImage.OriginalSize;
        }
        public enum EType { simple, reversed, dual };

        // a list of constant strings to represent the key types
        protected const String sKey = "SingleKey";
        protected const String sReversedKey = "ReversedKey";
        protected const String sDualKey = "DualKey";
        private Uri _LeftValueUri = null, _RightValueUri = null;
        public virtual Uri LeftValueUri
        {
            get
            {
                return _LeftValueUri;
            }
            set
            {
                DIBase oldVal = (_LeftValueUri == null) ? null : CIAT.SaveFile.GetDI(_LeftValueUri);
                if (_LeftValueUri != null)
                    (CIAT.SaveFile.GetDI(_LeftValueUri) as IDisposable).Dispose();
                _LeftValueUri = value;
                (CIAT.SaveFile.GetDI(value) as IResponseKeyDI).AddKeyOwner(this);
                List<CIATDualKey> dkOwners = KeyOwners.Cast<CIATKey>().Where(ko => ko.KeyType == IATKeyType.DualKey).Cast<CIATDualKey>().ToList();
                foreach (CIATDualKey dk in dkOwners)
                    dk.GenerateKeyValues();
                oldVal?.Dispose();
            }
        }
        public virtual Uri RightValueUri
        {
            get
            {
                return _RightValueUri;
            }
            set
            {
                DIBase oldVal = (_RightValueUri == null) ? null : CIAT.SaveFile.GetDI(_RightValueUri);
                if (_RightValueUri != null)
                    (CIAT.SaveFile.GetDI(_RightValueUri) as IDisposable).Dispose();
                _RightValueUri = value;
                (CIAT.SaveFile.GetDI(value) as IResponseKeyDI).AddKeyOwner(this);
                List<CIATDualKey> dkOwners = KeyOwners.Cast<CIATKey>().Where(ko => ko.KeyType == IATKeyType.DualKey).Cast<CIATDualKey>().ToList();
                foreach (CIATDualKey dk in dkOwners)
                    dk.GenerateKeyValues();
                oldVal?.Dispose();
            }
        }

        public virtual DIBase LeftValue
        {
            get
            {
                if (LeftValueUri == null)
                    return null;
                DIBase di = CIAT.SaveFile.GetDI(LeftValueUri);
                if (di.Type == DIType.Null)
                    return null;
                return di;
            }
        }

        public virtual DIBase RightValue
        {
            get
            {
                if (RightValueUri == null)
                    return null;
                DIBase di = CIAT.SaveFile.GetDI(RightValueUri);
                if (di.Type == DIType.Null)
                    return null;
                return di;
            }
        }

        protected List<IPackagePart> KeyOwners
        {
            get
            {
                return CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATKey), "owned-by").Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).ToList<IPackagePart>();
            }
        }

        public virtual void AddOwner(IPackagePart p)
        {
            CIAT.SaveFile.CreateRelationship(BaseType, p.BaseType, this.URI, p.URI, "owned-by");
            CIAT.SaveFile.CreateRelationship(p.BaseType, BaseType, p.URI, URI, "owns");
        }

        public virtual void ReleaseOwner(IPackagePart p)
        {
            CIAT.SaveFile.DeleteRelationship(p.URI, URI);
            CIAT.SaveFile.DeleteRelationship(URI, p.URI);
        }

        virtual public bool IsValid()
        {
            if ((LeftValueUri == null) || (RightValue == null) || (Name == String.Empty))
                return false;
            return true;
        }

        public override string ToString()
        {
            return Name;
        }

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


        public static Uri GetKeyUriByName(String Name)
        {
            try
            {
                return CIAT.SaveFile.GetAllIATKeyUris().Select(u => new { u, n = CIAT.SaveFile.GetIATKey(u).Name }).Where(tup => tup.n == Name).Select(tup => tup.u).First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
