using IAT.Core.Models.Enumerations;
using IAT.Core.Validation;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.Models.Serializable
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
    public class Block : IPackagePart, IValidatedItem
    {

        /// <summary>
        /// Gets or sets the unique identifier for the object.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.NewGuid();


        /// <summary>
        /// Gets or sets the Uniform Resource Identifier (URI) associated with this instance.
        /// </summary>
        [XmlIgnore]
        public Uri? Uri { get; set; } = null;

        /// <summary>
        /// Gets the part type associated with the package.
        /// </summary>
        [XmlIgnore]
        public PartType PackagePartType => PartType.Block;


        /// <summary>
        /// Gets the MIME type associated with the current block instance.  
        /// </summary>
        [XmlIgnore]
        public String MimeType => "text/xml+" + typeof(Block).ToString();


        /// <summary>
        /// Gets or sets the number of trials to perform in the operation.
        /// </summary>
        [XmlElement("NumPresentations", Form = XmlSchemaForm.Unqualified)]
        required public int NumPresentations { get; set; } = 0;

        /// <summary>
        /// Gets or sets the alternation group associated with the element.
        /// </summary>
        /// <remarks>The alternation group defines a set of mutually exclusive options for the element. If set to null, no
        /// alternation group is applied.</remarks>
        [XmlElement("AlternationGroupGuid", Form = XmlSchemaForm.Unqualified)]
        required public Guid AlternationGroupId { get; set; } = Guid.Empty;


        /// <summary>
        /// Gets or sets the alternation group associated with this element.
        /// </summary>
        [XmlIgnore]
        public AlternationGroup? AlternationGroup { get; set; } = null;


        /// <summary>
        /// Gets or sets the URI where instructions for completing the process can be accessed.
        /// </summary>
        [XmlElement("InstructionsId", Form = XmlSchemaForm.Unqualified)]
        public required Guid InstructionsId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the URI used to display the left-side response in a comparison or review scenario.
        /// </summary>
        [XmlElement("LeftResponseDisplayUri", Form = XmlSchemaForm.Unqualified)]
        public required Guid LeftResponseId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the URI used to display the right-side response in the user interface.
        /// </summary>
        [XmlElement("RightResponseDisplayUri", Form = XmlSchemaForm.Unqualified)]
        public required Guid RightResponseId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets the collection of unique identifiers for the associated trials.
        /// </summary>
        [XmlArray("TrialIds", Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("TrialId", Form = XmlSchemaForm.Unqualified)]
        public required List<Guid> TrialIds { get; init; } = [];

        /// <summary>
        /// Gets the collection of trials associated with this instance.
        /// </summary>
        /// <remarks>The returned list is initialized to an empty collection and is required to be set
        /// during object initialization. The property is read-only after initialization.</remarks>
        [XmlIgnore]
        public required List<Trial> Trials { get; init; } = [];

        /// <summary>
        /// Indicates whether the item is a header item.
        /// </summary>
        public static bool IsHeaderItem => true;

        /// <summary>
        /// Gets a value indicating whether the current context represents a survey.
        /// </summary>
        public static bool IsSurvey => false;


        /// <summary>
        /// Gets or sets the URI that identifies the cryptographic key.
        /// </summary>
        [XmlElement("KeyId", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public Guid KeyId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the key associated with this instance.
        /// </summary>
        [XmlIgnore]
        public Key? Key { get; set; } = null;

        /// <summary>
        /// Gets the block number associated with this instance.
        /// </summary>
        [XmlElement("BlockNumber", Form = XmlSchemaForm.Unqualified)]
        public required int BlockNumber { get; init; } = 0;




        /*
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
        */

        /// <summary>
        /// Initializes a new instance of the Block class.
        /// </summary>
        public Block() { }
        /*
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
        */

        /// <summary>
        /// Gets the item type that represents an IAT block content item.
        /// </summary>
        public static ContentsItemType ContentsItemType => ContentsItemType.IATBlock;

        /// <summary>
        /// Gets a value indicating whether an alternate item is associated with this instance.
        /// </summary>
        public bool HasAlternateItem
        {
            get
            {
                return (AlternationGroup != null);
            }
        }

        /// <summary>
        /// Gets the name associated with the current instance.
        /// </summary>
        [XmlElement("Name", Form = XmlSchemaForm.Unqualified)]
        public required string Name { get; set; } = String.Empty;


        /// <summary>
        /// Validates the current object and adds any validation errors to the specified dictionary.
        /// </summary>
        /// <remarks>Use this method to collect validation errors for the current object and its related
        /// items. Existing entries in the dictionary may be updated or new entries added, depending on the validation
        /// results.</remarks>
        /// <param name="ErrorDictionary">A dictionary to which validation errors are added. The key is the item being validated, and the value is the
        /// associated validation error. Cannot be null.</param>
        public void Validate(Dictionary<IValidatedItem, ValidationError> ErrorDictionary) => this.Validate(ErrorDictionary);



/*
        public void Preview(IImageDisplay previewPanel)
        {
            if (previewPanel.Tag != null)
                (previewPanel.Tag as IPreviewableItem).EndPreview(previewPanel);
            previewPanel.Tag = this;
            DIPreview dip = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
            dip.ResumeLayout(true);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, Uri);
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
                    result.Add(new CIATItemPreview(this.Uri, item.URI));
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
                i.SuspendPreviewLayout(Uri);
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, Uri);
            if (CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel != null)
                EndPreview(CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel);
            if (Key != null)
            {
                Uri keyUri = Key.URI;
                CIAT.SaveFile.DeleteRelationship(Key.URI, Uri);
                CIAT.SaveFile.DeleteRelationship(Uri, Key.URI);
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
            CIAT.SaveFile.DeletePart(this.Uri);
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
            foreach (DIPreview p in ItemTuples.Select(tup => CIAT.SaveFile.GetIATItem(tup.Item2).GetPreview(Uri)))
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
*/

    }
}

