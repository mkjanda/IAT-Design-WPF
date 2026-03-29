using IAT.Core.Models.Enumerations;
using sun.reflect.generics.tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace IAT.Core.Models
{
    [XmlRoot("Stimulus")]
    public class Trial
    {
        /// <summary>
        /// Gets or sets the direction associated with the key input.
        /// </summary>
        [XmlElement("KeyedDirection")]
        public required KeyedDirection KeyedDirection { get; set; } = KeyedDirection.None;

        /// <summary>
        /// Gets or sets the block number associated with the current entity.
        /// </summary>
        [XmlElement("BlockNum")]
        public int BlockNum { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the block from which the entity originated.
        /// </summary>
        [XmlElement("OriginatingBlock")]
        public int OriginatingBlock { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the stimulus display associated with this instance.
        /// </summary>
        [XmlIgnore]
        public Uri StimulusUri { get; set; }

        /// <summary>
        /// Gets the string representation of the stimulus URI.
        /// </summary>
        [XmlElement("StimulusUri")]
        public string StimulusUriString => StimulusUri.ToString() ??  String.Empty;


        public void AddParentBlock(Block parentBlock, KeyedDirection keyedDir)
        {
            List<Tuple<IUri, LayoutItem>> previewComponents = new List<Tuple<IUri, LayoutItem>>();
            previewComponents.Add(new Tuple<IUri, LayoutItem>(CIAT.SaveFile.GetDI(StimulusUri).IUri, LayoutItem.Stimulus));
            if (keyedDir == this.KeyedDirection.Left)
                previewComponents.Add(new Tuple<IUri, LayoutItem>(CIATLayout.ILeftKeyValueOutlineUri, LayoutItem.LeftResponseKeyOutline));
            else if (keyedDir == this.KeyedDirection.Right)
                previewComponents.Add(new Tuple<IUri, LayoutItem>(CIATLayout.IRightKeyValueOutlineUri, LayoutItem.RightResponseKeyOutline));
            previewComponents.AddRange(parentBlock.GetPreviewComponents());
            DIPreview preview = new DIPreview(previewComponents);
            preview.SuspendLayout();
            ParentBlockUris[parentBlock.URI] = new Tuple<KeyedDirection, Uri>(keyedDir, preview.URI);
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATBlock), this.URI, parentBlock.URI);
            CIAT.SaveFile.CreateRelationship(BaseType, preview.BaseType, this.URI, preview.URI);
        }

        public void DetachParentBlock(CIATBlock block)
        {
            DIBase preview = CIAT.SaveFile.GetDI(ParentBlockUris[block.URI].Item2);
            CIAT.SaveFile.DeleteRelationship(this.URI, preview.URI);
            CIAT.SaveFile.DeleteRelationship(this.URI, block.URI);
            preview.Dispose();
            ParentBlockUris.Remove(block.URI);
            if (ParentBlockUris.Count == 0)
                Dispose();
        }

        public CIATItem()
        {
            this.URI = CIAT.SaveFile.Register(this);
            _StimulusUri = DIBase.DINull.URI;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public CIATItem(Uri uri)
        {
            this.URI = uri;
            CIAT.SaveFile.Register(this);
            Load(uri);
        }

        public void Validate(int itemNdx, CIATBlock parent)
        {
            if (Stimulus.Type == DIType.Null)
                throw new CValidationException(String.Format(Properties.Resources.sNoStimulusForItemException, itemNdx + 1));
            else if (Stimulus.Type == DIType.StimulusText)
            {
                DIStimulusText stim = Stimulus as DIStimulusText;
                if (stim.Phrase.Trim() == String.Empty)
                    throw new CValidationException(String.Format(Properties.Resources.sNoStimulusForItemException, itemNdx + 1));
            }
            else if (Stimulus.Type == DIType.StimulusImage)
                if (Stimulus.IImage == null)
                    throw new CValidationException(String.Format(Properties.Resources.sNoStimulusForItemException, itemNdx + 1));
            if (ParentBlockUris[parent.URI].Item1 == this.KeyedDirection.None)
                throw new CValidationException(string.Format(Properties.Resources.sNoKeyedDirAssignedToStimulusException, itemNdx + 1));
        }




    }
}
