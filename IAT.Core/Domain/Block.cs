using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Validation;
using System.Xml.Schema;
using System.Xml.Serialization;
using IAT.Core.Serializable;
using CommunityToolkit.Mvvm.ComponentModel;
using net.sf.saxon.style;

namespace IAT.Core.Domain
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
    public partial class Block : ObservableObject
    {

        /// <summary>
        /// Gets or sets the unique identifier for the object.
        /// </summary>
        [ObservableProperty]
        private Guid _id = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the number of trials to perform in the operation.
        /// </summary>
        [ObservableProperty]
        private int _numPresentations = 0;

        /// <summary>
        /// Gets or sets the alternation group associated with the element.
        /// </summary>
        /// <remarks>The alternation group defines a set of mutually exclusive options for the element. If set to null, no
        /// alternation group is applied.</remarks>
        [ObservableProperty]
        private Guid _alternationGroupId = Guid.Empty;

        /// <summary>
        /// Gets or sets the URI where instructions for completing the process can be accessed.
        /// </summary>
        [ObservableProperty]
        private Guid _instructionsId = Guid.Empty;

        /// <summary>
        /// Gets or sets the URI used to display the left-side response in a comparison or review scenario.
        /// </summary>
        [ObservableProperty]
        private Guid _leftResponseId = Guid.Empty;

        /// <summary>
        /// Gets or sets the URI used to display the right-side response in the user interface.
        /// </summary>
        [ObservableProperty]
        private Guid _rightResponseId = Guid.Empty;

        /// <summary>
        /// Gets the collection of unique identifiers for the associated trials.
        /// </summary>
        [ObservableProperty]
        private List<Guid> _trialIds = new();

        /// <summary>
        /// Indicates whether the item is a header item.
        /// </summary>
        public static bool IsHeaderItem => true;

        /// <summary>
        /// Determines if the representation of the item expands to show sub-items or details. For a block, 
        /// this is typically true, as it can contain multiple trials and instructions.
        /// </summary>
        public static bool IsExpandable => true;

        /// <summary>`
        /// Gets or sets the URI that identifies the cryptographic key.
        /// </summary>
        [ObservableProperty]
        private Guid _keyId = Guid.Empty;

        /// <summary>
        /// Gets the block number associated with this instance.
        /// </summary>
        [ObservableProperty]
        public int _blockNumber = 0;

        /// <summary>
        /// Initializes a new instance of the Block class.
        /// </summary>
        public Block() { }


        /// <summary>
        /// Gets a value indicating whether an alternate item is associated with this instance.
        /// </summary>
        public bool HasAlternateBlock
        {
            get
            {
                return AlternationGroupId != Guid.Empty;
            }
        }

        /// <summary>
        /// Gets the name associated with the current instance.
        /// </summary>
        [ObservableProperty]
        private string _name = String.Empty;


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

