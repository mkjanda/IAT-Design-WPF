using java.awt;
using sun.java2d.pipe;
using sun.security.x509;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace IAT.Core.Models
{
    /// <summary>
    /// Represents a block of items within an IAT (Implicit Association Test), containing presentations, instructions,
    /// response keys, and related configuration for a test segment.
    /// </summary>
    /// <remarks>A block manages a collection of items (stimuli or presentations) and their associated
    /// metadata, such as instructions and response keys. It supports alternation with other blocks, dynamic keying, and
    /// provides methods for validation, serialization, and previewing. Blocks are typically managed as part of an IAT's
    /// structure and can be added, removed, or reordered within the test. Thread safety is not guaranteed; external
    /// synchronization is required for concurrent access.</remarks>
    [XmlRoot("Block")]
    public class Block
    {
        /// <summary>
        /// Gets or sets the number of trials to perform in the operation.
        /// </summary>
        [XmlElement("NumTrials")] 
        required public int NumTrials{ get; set; } = 0;

        /// <summary>
        /// Gets or sets the alternated width value used for layout or rendering purposes.
        /// </summary>
        /// <remarks>A value of -1 typically indicates that no alternated width is specified. The meaning
        /// and usage of this property may depend on the context in which it is used.</remarks>
        [XmlElement("AlternatedWidth")] 
        required public int AlternatedWith { get; set; } = -1;

        /// <summary>
        /// Gets or sets the number of the block in the sequence.
        /// </summary>
        [XmlElement("BlockNumber")] 
        required public int BlockNumber { get; set; }

        /// <summary>
        /// Gets the number of stimul contained in the collection.
        /// </summary>
        [XmlElement("NumItems")] 
        public int NumItems {
            get {
                return Stimuli.Count;
            }
        }

        /// <summary>
        /// Gets or sets the URI where instructions for completing the process can be accessed.
        /// </summary>
        [XmlElement("InstructionsDisplayUri", IsNullable = true)]
        public Uri InstructionsDisplayUri { get; set; }

        /// <summary>
        /// Gets or sets the URI used to display the left-side response in a comparison or review scenario.
        /// </summary>
        [XmlElement("LeftResponseDisplayUri", IsNullable = true)] 
        public Uri LeftResponseDisplayUri { get; set; }

        /// <summary>
        /// Gets or sets the URI used to display the right-side response in the user interface.
        /// </summary>
        [XmlElement("RightResponseDisplayUri", IsNullable = true)] 
        public Uri RightResponseDisplayUri { get; set; }

        /// <summary>
        /// Gets or sets the collection of stimuli presentations associated with this instance.
        /// </summary>
        /// <remarks>Each item in the collection represents a single stimulus to be presented. The order
        /// of items in the list determines the sequence in which stimuli are processed or displayed.</remarks>
        [XmlArray("StimuliUris")]
        [XmlArrayItem("StimulsUri")]
        public List<Stimulus> Stimuli { get; set; } = new();


        [XmlElement("AlterationGroup", IsNullable = true)]
        protected AlternationGroup AltGroup = null;
        private List<Tuple<string, Uri>> ItemTuples = new();
        protected int _IndexInContents = -1;
        public delegate int BlockIndexRetriever(Block block);
        public BlockIndexRetriever GetIndex;
        public Uri InstructionsUri { get; private set; }
        public Uri URI { get; set; }
        public readonly Enumerations.PartType partType = Enumerations.PartType.Block;
        public readonly String MimeType => "text/xml";
        public readonly bool IsHeaderItem => true;
        public readonly bool IsSurvey => false;

        public CIATKey Key
        {
            get
            {
                
                return CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).FirstOrDefault();
            }
            set
            {
                try
                {
                    CIATKey oldKey = Key;
                    if (oldKey != null)
                        if (oldKey.URI.Equals(value.URI))
                            return;
                    if (value == null)
                        return;
                    if (oldKey != null)
                    {
                        CIAT.SaveFile.DeleteRelationship(this.URI, oldKey.URI);
                        CIAT.SaveFile.DeleteRelationship(oldKey.URI, this.URI);
                        CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Detached, oldKey.URI, URI);
                    }
                    CIATKey newKey = value;
                    CIAT.SaveFile.CreateRelationship(BaseType, value.BaseType, this.URI, value.URI);
                    CIAT.SaveFile.CreateRelationship(value.BaseType, BaseType, value.URI, this.URI);
                    CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Attached, value.URI, URI);
                    DIPreview preview = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                    bool suspended = preview.LayoutSuspended;
                    preview.SuspendLayout();
                    preview.RemoveComponent(LayoutItem.LeftResponseKey, false);
                    preview.RemoveComponent(LayoutItem.RightResponseKey, false);
                    foreach (CIATItem i in ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)))
                    {
                        i.GetPreview(URI).RemoveComponent(LayoutItem.LeftResponseKey, false);
                        i.GetPreview(URI).RemoveComponent(LayoutItem.RightResponseKey, false);
                    }
                    if (!suspended)
                        preview.ResumeLayout(true);
                    preview.AddComponent(newKey.LeftValue.IUri, LayoutItem.LeftResponseKey);
                    preview.AddComponent(newKey.RightValue.IUri, LayoutItem.RightResponseKey);
                    foreach (CIATItem i in ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)))
                    {
                        DIPreview itemPreview = i.GetPreview(URI);
                        suspended = itemPreview.LayoutSuspended;
                        i.GetPreview(URI).AddComponent(newKey.LeftValue.IUri, LayoutItem.LeftResponseKey);
                        i.GetPreview(URI).AddComponent(newKey.RightValue.IUri, LayoutItem.RightResponseKey);
                        if (!suspended)
                            itemPreview.ResumeLayout(false);
                    }
                }
                catch (Exception ex) { }
            }
        }

        public int NumItems
        {
            get
            {
                return ItemTuples.Count;
            }
        }

        public bool IsDynamicallyKeyed { get; set; } = false;

        public CIATBlock AlternateBlock
        {
            get
            {
                if (!HasAlternateItem)
                    return null;
                if (AlternationGroup.GroupMembers[0] == this)
                    return (CIATBlock)AlternationGroup.GroupMembers[1];
                else
                    return (CIATBlock)AlternationGroup.GroupMembers[0];
            }
        }

        public int NumPresentations
        {
            get
            {
                if (CIAT.SaveFile.IAT.RandomizationType == ConfigFile.ERandomizationType.SetNumberOfPresentations)
                {
                    if (_NumPresentations == -1)
                        _NumPresentations = ItemTuples.Count;
                    return _NumPresentations;
                }
                else
                    return ItemTuples.Count;
            }
            set
            {
                _NumPresentations = value;
            }
        }


        public CIATItem this[int ndx]
        {
            get
            {

                if ((ndx < 0) || (ndx >= ItemTuples.Count))
                    return null;
                return ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)).ToList()[ndx];
            }
        }

        public bool Contains(CIATItem i)
        {
            try
            {
                ItemTuples.Where(tup => tup.Item2.Equals(i.URI)).First();
                return true;
            }
            catch (ArgumentException)
            {
                return false;
            }

        }

        private void GeneratePreviewOverlay(Bitmap bmp)
        {
            Graphics g = Graphics.FromImage(bmp);
            Brush br = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            g.FillRectangle(br, 0, 0, CIAT.SaveFile.Layout.InteriorSize.Width, CIAT.SaveFile.Layout.InteriorSize.Height);
            br.Dispose();
            br = new SolidBrush(CIAT.SaveFile.Layout.BorderColor);
            Font f = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 18F);
            String str;
            if (ItemTuples.Count == 0)
                str = "No Stimuli";
            else if (ItemTuples.Count == 1)
                str = "1 Stimulus";
            else
                str = String.Format("{0} Stimuli", ItemTuples.Count);
            Size szStr = TextRenderer.MeasureText(str, f);
            float ar = CIAT.SaveFile.Layout.InteriorSize.Width / CIAT.SaveFile.Layout.InteriorSize.Height;
            PointF ptDraw = new PointF((CIAT.SaveFile.Layout.InteriorSize.Width - szStr.Width) / 2, (CIAT.SaveFile.Layout.InteriorSize.Height - szStr.Height) / 2 - (2 * szStr.Height) + (ar > 1 ? (szStr.Height / ar) : (-ar * szStr.Height)));
            g.DrawString(str, f, br, ptDraw);
            br.Dispose();
            g.Dispose();
            f.Dispose();
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public CIATBlock(CIAT iat)
        {
            this.URI = CIAT.SaveFile.Register(this);
            rIatId = CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CIATBlock), iat.URI, this.URI);
            Key = null;
            InstructionsUri = new DIIatBlockInstructions().URI;
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), this.URI, InstructionsUri);
            _NumPresentations = -1;
            GetRandomizationType = new RandomizationTypeResolver(iat.GetRandomizationType);
            IAT = iat;
            DIPreview preview = new DIPreview();
            BlockPreviewLambda = new DILambdaGenerated(GeneratePreviewOverlay);
            preview.AddComponent(BlockPreviewLambda.IUri);
            preview.AddComponent(CIAT.SaveFile.GetDI(InstructionsUri).IUri, LayoutItem.BlockInstructions);
            BlockPreviewLambda.ScheduleInvalidation();
            PreviewUri = preview.URI;
            CIAT.SaveFile.CreateRelationship(BaseType, preview.BaseType, this.URI, PreviewUri);
            Name = String.Format("IAT Block #{0}", CIAT.SaveFile.IAT.Blocks.Count + 1);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public CIATBlock(CIAT iat, Uri uri)
        {
            IAT = iat;
            this.URI = uri;
            rIatId = CIAT.SaveFile.GetRelationshipsByType(iat.URI, typeof(CIAT), BaseType).Where(pr => pr.TargetUri.Equals(uri)).Select(pr => pr.Id).First();
            CIAT.SaveFile.Register(this);
            Load();
            BlockPreviewLambda = new DILambdaGenerated(GeneratePreviewOverlay);
            DIPreview preview = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
            preview.AddComponent(BlockPreviewLambda.IUri);
        }

        public void ClearThumbnailDisplay()
        {
            foreach (Images.IImage i in ItemTuples.Select(tup => CIAT.SaveFile.GetDI(tup.Item2).IImage))
                i.Thumbnail.ClearChanged();
        }

        public List<Tuple<IUri, LayoutItem>> GetPreviewComponents()
        {
            var components = new List<Tuple<IUri, LayoutItem>>();
            if (Key != null)
            {
                components.Add(new Tuple<IUri, LayoutItem>(Key.LeftValue.IUri, LayoutItem.LeftResponseKey));
                components.Add(new Tuple<IUri, LayoutItem>(Key.RightValue.IUri, LayoutItem.RightResponseKey));
            }
            components.Add(new Tuple<IUri, LayoutItem>(CIAT.SaveFile.GetDI(InstructionsUri).IUri, LayoutItem.BlockInstructions));
            return components;
        }

        public void Validate()
        {
            if (Key == null)
                throw new CValidationException(Properties.Resources.sNoKeyAssignedToBlockException);
            for (int ctr = 0; ctr < ItemTuples.Count; ctr++)
                CIAT.SaveFile.GetIATItem(ItemTuples[ctr].Item2).Validate(ctr, this);
        }

        public void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary)
        {
            CLocationDescriptor loc = new CItemLocationDescriptor(this, null);
            if (ItemTuples.Count == 0)
                ErrorDictionary[this] = new CValidationException(EValidationException.BlockHasNoItems, loc);
            else if (Key == null)
                ErrorDictionary[this] = new CValidationException(EValidationException.BlockResponseKeyUndefined, loc);
            foreach (CIATItem i in ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)))
                i.ValidateItem(ErrorDictionary);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            String rInstructionsId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(rel => rel.TargetUri.Equals(InstructionsUri)).Select(rel => rel.Id).First();
            String rPreviewId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(rel => rel.TargetUri.Equals(PreviewUri)).Select(rel => rel.Id).First();
            xDoc.Add(new XElement("IATBlock", new XAttribute("Name", Name), new XAttribute("IndexInContents", IndexInContents)));
            xDoc.Root.Add(new XElement("rInstructionsId", rInstructionsId));
            xDoc.Root.Add(new XElement("rPreviewId", rPreviewId));
            xDoc.Root.Add(new XElement("IsDynamicallyKeyed", IsDynamicallyKeyed.ToString()));
            foreach (String rId in ItemTuples.Select(tup => tup.Item1))
                xDoc.Root.Add(new XElement("rItemId", rId));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Name = xDoc.Root.Attribute("Name").Value;
            _IndexInContents = Convert.ToInt32(xDoc.Root.Attribute("IndexInContents").Value);
            InstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rInstructionsId").Value).TargetUri;
            PreviewUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rPreviewId").Value).TargetUri;
            IsDynamicallyKeyed = Convert.ToBoolean(xDoc.Root.Element("IsDynamicallyKeyed").Value);
            ItemTuples.Clear();
            foreach (XElement elem in xDoc.Root.Elements("rItemId"))
            {
                String rId = elem.Value;
                Uri u = CIAT.SaveFile.GetRelationship(this, rId).TargetUri;
                ItemTuples.Add(new Tuple<String, Uri>(rId, u));
            }
        }

        public ContentsItemType Type
        {
            get
            {
                return ContentsItemType.IATBlock;
            }
        }

        public bool HasAlternateItem
        {
            get
            {
                return (AltGroup != null);
            }
        }

        public AlternationGroup AlternationGroup
        {
            get
            {
                return AltGroup;
            }
            set
            {
                if ((AltGroup != null) && (value != null))
                    if (!AltGroup.URI.Equals(value.URI))
                    {
                        MessageBox.Show("Dispose of the alternation group and instantiate a new one.");
                        return;
                    }
                AltGroup = value;
            }
        }

        public String Name { get; private set; }

        public int IndexInContainer
        {
            get
            {
                return IAT.Blocks.IndexOf(this);
            }
        }

        public void DeleteFromIAT()
        {
            IAT.DeleteIATBlock(this);
        }

        public void AddToIAT(int InsertionNdx)
        {
            int containerNdx = 0;
            for (int ctr = 0; ctr < InsertionNdx; ctr++)
                if (IAT.Contents[ctr].Type == Type)
                    containerNdx++;
            Name = "IAT Block #" + (containerNdx + 1).ToString();
            IAT.InsertIATBlock(this, InsertionNdx);
        }

        public int IndexInContents
        {
            get
            {
                if (IAT.Contents.Contains(this))
                    return IAT.Contents.IndexOf(this);
                return _IndexInContents;
            }
        }

        public void AddItem(CIATItem i, KeyedDirection kd)
        {
            String rId = CIAT.SaveFile.CreateRelationship(BaseType, i.BaseType, this.URI, i.URI);
            ItemTuples.Add(new Tuple<String, Uri>(rId, i.URI));
            i.AddParentBlock(this, kd);
            if (!CIAT.SaveFile.IAT.Is7Block)
                return;
            int blockNum = CIAT.SaveFile.IAT.Blocks.IndexOf(this) + 1;
            CIATBlock b = null;
            if ((blockNum == 1) || (blockNum == 2))
            {
                b = CIAT.SaveFile.IAT.Blocks[2];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, i.BaseType, b.URI, i.URI), i.URI));
                i.AddParentBlock(CIAT.SaveFile.IAT.Blocks[2], kd);
                b = CIAT.SaveFile.IAT.Blocks[3];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, i.BaseType, b.URI, i.URI), i.URI));
                i.AddParentBlock(CIAT.SaveFile.IAT.Blocks[3], kd);
            }
            if (blockNum == 1)
            {
                b = CIAT.SaveFile.IAT.Blocks[5];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, i.BaseType, b.URI, i.URI), i.URI));
                i.AddParentBlock(CIAT.SaveFile.IAT.Blocks[5], kd);
                b = CIAT.SaveFile.IAT.Blocks[6];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, i.BaseType, b.URI, i.URI), i.URI));
                i.AddParentBlock(CIAT.SaveFile.IAT.Blocks[6], kd);
            }
            else if (blockNum == 2)
            {
                b = CIAT.SaveFile.IAT.Blocks[4];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, i.BaseType, b.URI, i.URI), i.URI));
                i.AddParentBlock(CIAT.SaveFile.IAT.Blocks[4], kd.Opposite);
                b = CIAT.SaveFile.IAT.Blocks[5];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, i.BaseType, b.URI, i.URI), i.URI));
                i.AddParentBlock(CIAT.SaveFile.IAT.Blocks[5], kd.Opposite);
                b = CIAT.SaveFile.IAT.Blocks[6];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, i.BaseType, b.URI, i.URI), i.URI));
                i.AddParentBlock(CIAT.SaveFile.IAT.Blocks[6], kd.Opposite);
            }
        }

        public void MoveItem(int startNdx, int endNdx)
        {
            Tuple<String, Uri> tup = ItemTuples[startNdx];
            ItemTuples.RemoveAt(startNdx);
            if (startNdx < endNdx)
                ItemTuples.Insert(endNdx, tup);
            else
                ItemTuples.Insert(endNdx, tup);
        }


        public void InsertItem(int ndx, CIATItem item)
        {
            String rId = CIAT.SaveFile.CreateRelationship(BaseType, item.BaseType, this.URI, item.URI);
            ItemTuples.Insert(ndx, new Tuple<String, Uri>(rId, item.URI));
            KeyedDirection kd = IsDynamicallyKeyed ? KeyedDirection.DynamicNone : KeyedDirection.None;
            item.AddParentBlock(this, kd);
            CIAT.SaveFile.CreateRelationship(BaseType, item.BaseType, URI, item.URI);
            int blockNum = CIAT.SaveFile.IAT.Blocks.IndexOf(this) + 1;
            CIATBlock b = null;
            if ((blockNum == 1) || (blockNum == 2))
            {
                b = CIAT.SaveFile.IAT.Blocks[2];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, item.BaseType, b.URI, item.URI), item.URI));
                item.AddParentBlock(CIAT.SaveFile.IAT.Blocks[2], kd);
                b = CIAT.SaveFile.IAT.Blocks[3];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, item.BaseType, b.URI, item.URI), item.URI));
                item.AddParentBlock(CIAT.SaveFile.IAT.Blocks[3], kd);
            }
            if (blockNum == 1)
            {
                b = CIAT.SaveFile.IAT.Blocks[5];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, item.BaseType, b.URI, item.URI), item.URI));
                item.AddParentBlock(CIAT.SaveFile.IAT.Blocks[5], kd);
                b = CIAT.SaveFile.IAT.Blocks[6];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, item.BaseType, b.URI, item.URI), item.URI));
                item.AddParentBlock(CIAT.SaveFile.IAT.Blocks[6], kd);
            }
            else if (blockNum == 2)
            {
                b = CIAT.SaveFile.IAT.Blocks[4];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, item.BaseType, b.URI, item.URI), item.URI));
                item.AddParentBlock(CIAT.SaveFile.IAT.Blocks[4], kd.Opposite);
                b = CIAT.SaveFile.IAT.Blocks[5];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, item.BaseType, b.URI, item.URI), item.URI));
                item.AddParentBlock(CIAT.SaveFile.IAT.Blocks[5], kd.Opposite);
                b = CIAT.SaveFile.IAT.Blocks[6];
                b.ItemTuples.Add(new Tuple<String, Uri>(CIAT.SaveFile.CreateRelationship(b.BaseType, item.BaseType, b.URI, item.URI), item.URI));
                item.AddParentBlock(CIAT.SaveFile.IAT.Blocks[6], kd.Opposite);
            }
        }

        public void RemoveItem(CIATItem i)
        {
            Tuple<String, Uri> tup = ItemTuples.Where(t => t.Item2.Equals(i.URI)).FirstOrDefault();
            if (tup != null)
            {
                ItemTuples.Remove(tup);
                CIAT.SaveFile.DeleteRelationship(this.URI, tup.Item1);
            }
        }

        public int GetItemIndex(CIATItem item)
        {
            return ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)).ToList().IndexOf(item);
        }


        Action<Images.ImageChangedEventArgs> UpdateBlockPreview = null;

        public void Preview(IImageDisplay previewPanel)
        {
            if (previewPanel.Tag != null)
                (previewPanel.Tag as IPreviewableItem).EndPreview(previewPanel);
            previewPanel.Tag = this;
            DIPreview dip = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
            dip.ResumeLayout(true);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, URI);
            dip.PreviewPanel = previewPanel;
        }

        public void EndPreview(IImageDisplay p)
        {
            if (p.Tag == this)
            {
                (CIAT.SaveFile.GetDI(PreviewUri) as DIPreview).PreviewPanel = null;
                p.Tag = null;
            }
        }


        public Button GUIButton { get; set; }

        public void OpenItem(IATConfigMainForm mainForm)
        {
            mainForm.ActiveItem = this;
            mainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
            if (ItemTuples.Count > 0)
                mainForm.SetActiveIATItem(CIAT.SaveFile.GetIATItem(ItemTuples[0].Item2));
        }

        public List<IPreviewableItem> SubContentsItems
        {
            get
            {
                List<IPreviewableItem> result = new List<IPreviewableItem>();
                foreach (CIATItem item in ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)))
                    result.Add(new CIATItemPreview(this.URI, item.URI));
                return result;
            }
        }

        public String PreviewText
        {
            get
            {
                return Name;
            }
        }

        public void SuspendPreviewLayouts()
        {
            List<CIATItem> items = ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)).ToList();
            foreach (CIATItem i in items)
                i.SuspendPreviewLayout(URI);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
            if (CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel != null)
                EndPreview(CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel);
            if (Key != null)
            {
                Uri keyUri = Key.URI;
                CIAT.SaveFile.DeleteRelationship(Key.URI, URI);
                CIAT.SaveFile.DeleteRelationship(URI, Key.URI);
            }
            CIAT.SaveFile.GetDI(PreviewUri).Dispose();
            BlockPreviewLambda.Dispose();
            List<CIATItem> items = ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)).ToList();
            foreach (CIATItem i in items)
            {
                RemoveItem(i);
                i.DetachParentBlock(this);
            }
            CIAT.SaveFile.GetDI(InstructionsUri).Dispose();
            if (AlternationGroup != null)
                AlternationGroup.Remove(this);
            CIAT.SaveFile.DeletePart(this.URI);
            IsDisposed = true;
        }

        public bool IsDisposed { get; private set; }

        private Button _GUIButton = null;

        public void InvalidateKeys()
        {
            DIPreview dip = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
            bool suspended = dip.LayoutSuspended;
            if (!suspended)
                dip.SuspendLayout();
            dip.RemoveComponent(LayoutItem.LeftResponseKey, false);
            dip.RemoveComponent(LayoutItem.RightResponseKey, false);
            dip.AddComponent(Key.LeftValue.IUri, LayoutItem.LeftResponseKey);
            dip.AddComponent(Key.RightValue.IUri, LayoutItem.RightResponseKey);
            if (!suspended)
                dip.ResumeLayout(true);
            foreach (DIPreview p in ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2).GetPreview(URI)))
            {
                suspended = p.LayoutSuspended;
                p.SuspendLayout();
                p.RemoveComponent(LayoutItem.LeftResponseKey, false);
                p.RemoveComponent(LayoutItem.RightResponseKey, false);
                p.AddComponent(Key.LeftValue.IUri, LayoutItem.LeftResponseKey);
                p.AddComponent(Key.RightValue.IUri, LayoutItem.RightResponseKey);
                if (!suspended)
                    p.ResumeLayout(true);
            }
        }

        public List<CFontFile.FontItem> UtilizedStimuliFonts
        {
            get
            {
                List<CFontFile.FontItem> fontItems = new List<CFontFile.FontItem>();
                var textStimuli = ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2)).Select((i, n) => new { ndx = n + 1, stimulus = i.Stimulus }).Where(stim => stim.stimulus != null).Where(stim => stim.stimulus.Type == DIType.StimulusText);
                var stimuliFonts = from ff in textStimuli.Select(nStim => nStim.stimulus as DIStimulusText).Select(tdi => tdi.PhraseFontFamily).Distinct()
                                   select new { familyName = ff, indicies = textStimuli.Where(nStim => (nStim.stimulus as DIStimulusText).PhraseFontFamily == ff).Select(ts => ts.ndx) };
                foreach (var stimFont in stimuliFonts)
                {
                    var tdis = textStimuli.Where(tdi => stimFont.indicies.Contains(tdi.ndx)).Select(tdi => tdi.stimulus).Cast<DIStimulusText>();
                    CFontFile.FontItem fItem = new CFontFile.FontItem(stimFont.familyName, " is used for stimuli in IAT Blocks #" + ((IndexInContainer + 1 == 1) ? "1, 3, 4, 6, and 7." : "2, 3, 4, 5, 6, and 7."), null, tdis);
                    fontItems.Add(fItem);
                }
                return fontItems;
            }
        }
    }


}
}
