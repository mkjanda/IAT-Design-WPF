using IAT.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Serializable
{
    public class DIStimulusText : DIText, IStimulus
    {

        private void OnThumbnailChanged(ImageEvent evt, IImageMedia iMedia, object arg)
        {
            if (ThumbnailPreviewPanel == null)
                return;
            if (!ThumbnailPreviewPanel.IsHandleCreated)
                ThumbnailPreviewPanel.HandleCreated += (sender, args) => { ThumbnailPreviewPanel.SetImage(iMedia); };
            else
                ThumbnailPreviewPanel.SetImage(iMedia);
        }

        private IImageDisplay _ThumbnailPreviewPanel = null;
        public IImageDisplay ThumbnailPreviewPanel
        {
            get
            {
                return _ThumbnailPreviewPanel;
            }
            set
            {
                _ThumbnailPreviewPanel = value;
                if ((value != null) && (IImage != null))
                {
                    if (IImage.Thumbnail != null)
                        value.SetImage(IImage.Thumbnail);
                    else
                    {
                        IImage.CreateThumbnail();
                        IImage.Thumbnail.Changed += (evt, iMedia, arg) => OnThumbnailChanged(evt, iMedia, arg);
                    }
                }
            }
        }
        public DIStimulusText()
            : base(DIText.UsedAs.Stimulus)
        {
        }
        public DIStimulusText(Uri uri)
            : base(uri, DIText.UsedAs.Stimulus)
        {
        }
        public String Description
        {
            get
            {
                return Phrase;
            }
        }

        protected override void OnImageEvent(ImageEvent evt, IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if (IsDisposed)
                return;
            if ((evt == Images.ImageEvent.Updated) || (evt == Images.ImageEvent.Resized))
                CIAT.ImageManager.GenerateThumb(IImage);
        }

        public bool Equals(IStimulus stim)
        {
            if (Type != stim.Type)
                return false;
            DIStimulusText textStim = stim as DIStimulusText;
            if (PhraseFontColor.ToArgb() != textStim.PhraseFontColor.ToArgb())
                return false;
            if (PhraseFontFamily != textStim.PhraseFontFamily)
                return false;
            if (Phrase != textStim.Phrase)
                return false;
            if (PhraseFontSize != textStim.PhraseFontSize)
                return false;
            return true;
        }
        public override object Clone()
        {
            DIStimulusText di = new DIStimulusText();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => OnImageEvent(evt, img, arg);
            di.IImage.Thumbnail.Changed += (evt, img, args) => OnImageEvent(evt, img, args);
            di.ResumeLayout(true);
            return di;
        }
        public override void Dispose()
        {
            if (IsDisposed)
                return;
            if (ThumbnailPreviewPanel != null)
                ThumbnailPreviewPanel.ClearImage();
            base.Dispose();
        }
    }
}
