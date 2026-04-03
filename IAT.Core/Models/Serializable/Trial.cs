using IAT.Core.Models.Enumerations;
using sun.reflect.generics.tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Models.Serializable
{
    [XmlRoot("Stimulus")]
    public class Trial : IValidatedItem
    {

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the unique identifier for the associated stimulus.
        /// </summary>
        [XmlElement("StimulusId", Form = XmlSchemaForm.Unqualified)]  
        public Guid StimulusId { get; set; } = Guid.Empty;

        [XmlIgnore]
        public IStimulus Stimulus { get; set; }

        /// <summary>
        /// Gets or sets the item number associated with this instance.
        /// </summary>
        [XmlElement("TrialNumber", Form = XmlSchemaForm.Unqualified)]
        public int TrialNumber { get; set; }

        /// <summary>
        /// Gets or sets the direction associated with the key input.
        /// </summary>
        [XmlElement("KeyedDirection", Form = XmlSchemaForm.Unqualified)]
        public required KeyedDirection KeyedDirection { get; set; } = KeyedDirection.None;

        /// <summary>
        /// Gets or sets the block number associated with the current entity.
        /// </summary>
        [XmlElement("BlockNumber", Form = XmlSchemaForm.Unqualified)]
        public int BlockNumber { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the block from which the entity originated.
        /// </summary>
        [XmlElement("OriginatingBlock", Form = XmlSchemaForm.Unqualified)]
        public int OriginatingBlock { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the stimulus display associated with this instance.
        /// </summary>
        [XmlIgnore]
        public Uri StimulusUri { get; set; }

        public void Validate()


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
            ParentBlockUris[parentBlock.Uri] = new Tuple<KeyedDirection, Uri>(keyedDir, preview.URI);
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATBlock), this.URI, parentBlock.Uri);
            CIAT.SaveFile.CreateRelationship(BaseType, preview.BaseType, this.URI, preview.URI);
        }

        public void DetachParentBlock(CIATBlock block)
        {
            DIBase preview = CIAT.SaveFile.GetDI(ParentBlockUris[block.URI].Item2);
            CIAT.SaveFile.DeleteRelationship(this.URI, preview.Uri);
            CIAT.SaveFile.DeleteRelationship(this.URI, block.URI);
            preview.Dispose();
            ParentBlockUris.Remove(block.URI);
            if (ParentBlockUris.Count == 0)
                Dispose();
        }

        public CIATItem()
        {
            this.URI = CIAT.SaveFile.Register(this);
            _StimulusUri = DIBase.DINull.Uri;
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
