using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;

namespace IAT.Core.Serializable
{
    public abstract class DIText : DIGenerated
    {
        private Font phraseFont = null;
        public Font PhraseFont
        {
            get
            {
                lock (lockObject)
                {
                    return phraseFont;
                }
            }
            set
            {
                if (value == phraseFont)
                    return;
                lock (lockObject)
                {
                    if (phraseFont != null)
                        phraseFont.Dispose();
                    phraseFont = value;
                    ScheduleInvalidation();
                }
            }
        }

        private float phraseFontSize;
        public float PhraseFontSize
        {
            get
            {
                return phraseFontSize;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].FontSize = value;
                if (value == phraseFontSize)
                    return;
                phraseFontSize = value;
                PhraseFont = new Font(PhraseFontFamily, value);
            }
        }

        private String phraseFontFamily;
        public String PhraseFontFamily
        {
            get
            {
                return phraseFontFamily;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].FontFamily = value;
                if (value == phraseFontFamily)
                    return;
                phraseFontFamily = value;
                PhraseFont = new Font(value, PhraseFontSize);
            }
        }

        private Color phraseFontColor;
        public Color PhraseFontColor
        {
            get
            {
                return phraseFontColor;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].FontColor = value;
                phraseFontColor = value;
                ScheduleInvalidation();
            }
        }

        private String phrase = String.Empty;
        public String Phrase
        {
            get
            {
                return phrase;
            }
            set
            {
                if (value == phrase)
                    return;
                phrase = value;
                ScheduleInvalidation();
            }
        }


        private TextJustification justification;
        public TextJustification Justification
        {
            get
            {
                return justification;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].Justification = value;
                if (value == Justification)
                    return;
                justification = value;
                ScheduleInvalidation();
            }
        }

        private float lineSpacing;
        public float LineSpacing
        {
            get
            {
                return lineSpacing;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].LineSpacing = value;
                if (LineSpacing == value)
                    return;
                lineSpacing = value;
                ScheduleInvalidation();
            }
        }


        public void SetFont(DIText.UsedAs usedAs)
        {
            lock (lockObject)
            {
                FontPreference preference = CIAT.SaveFile.FontPreferences[usedAs];
                phraseFontFamily = preference.FontFamily;
                phraseFontColor = preference.FontColor;
                phraseFontSize = preference.FontSize;
                lineSpacing = preference.LineSpacing;
                justification = preference.Justification;
                phraseFont = new Font(phraseFontFamily, phraseFontSize);
            }
        }


        public override void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement(BaseType.ToString(),
                new XElement("PhraseFontFamily", PhraseFontFamily), new XElement("PhraseFontSize", PhraseFontSize.ToString()),
                new XElement("PhraseFontColor", NamedColor.GetNamedColor(PhraseFontColor).Name), new XElement("Justification", Justification.ToString()),
                new XElement("LineSpacing", LineSpacing.ToString()), new XElement("AbsoluteBounds", new XElement("Top", AbsoluteBounds.Top.ToString()),
                new XElement("Left", AbsoluteBounds.Left.ToString()), new XElement("Width", AbsoluteBounds.Width.ToString()),
                new XElement("Height", AbsoluteBounds.Height.ToString()))));
            if (this.IImage != null)
                xDoc.Root.Add(new XAttribute("rImageId", rImageId));
            foreach (var str in Phrase.Split(new String[] { "\r\n" }, StringSplitOptions.None))
                xDoc.Root.Add(new XElement("Phrase", str));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            phrase = String.Empty;
            foreach (XElement elem in xDoc.Root.Elements("Phrase"))
                phrase += elem.Value + "\r\n";
            phrase = phrase.TrimEnd();
            phraseFontFamily = xDoc.Root.Element("PhraseFontFamily").Value;
            phraseFontSize = Convert.ToSingle(xDoc.Root.Element("PhraseFontSize").Value);
            phraseFontColor = NamedColor.GetNamedColor(xDoc.Root.Element("PhraseFontColor").Value).Color;
            justification = TextJustification.FromString(xDoc.Root.Element("Justification").Value);
            lineSpacing = Convert.ToSingle(xDoc.Root.Element("LineSpacing").Value);
            AbsoluteBounds = new Rectangle(Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Left").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Top").Value), Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Width").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Height").Value));
            phraseFont = new Font(PhraseFontFamily, PhraseFontSize);
            if (xDoc.Root.Attribute("rImageId") != null)
            {
                var iStateAttr = xDoc.Root.Attribute("InvalidationState");
                if (iStateAttr != null)
                    InvalidationState = InvalidationStates.Parse(iStateAttr.Value);
                else
                    InvalidationState = InvalidationStates.InvalidationReady;
                rImageId = xDoc.Root.Attribute("rImageId").Value;
                SetImage(rImageId);
            }
        }

        private UsedAs usedAs;

        public DIText(UsedAs usedAs)
        {
            SetFont(usedAs);
            this.usedAs = usedAs;
        }

        public DIText(Uri uri, UsedAs usedAs)
            : base(uri)
        {
            this.usedAs = usedAs;
        }

        protected Bitmap GenerateText()
        {
            String str = Phrase;
            Size bSz = BoundingSize;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FromDIType(Type));
            Graphics g = Graphics.FromImage(bmp);
            Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            g.FillRectangle(backBr, new Rectangle(new Point(0, 0), bSz));
            backBr.Dispose();
            SizeF sz = g.MeasureString(str, PhraseFont);
            PointF ptDraw = new PointF();
            if (Justification == TextJustification.Left)
                ptDraw = new PointF(0, (bSz.Height - sz.Height) / 2);
            else if (Justification == TextJustification.Center)
                ptDraw = new PointF((bSz.Width - sz.Width) / 2, (bSz.Height - sz.Height) / 2);
            else if (Justification == TextJustification.Right)
                ptDraw = new PointF(bSz.Width - sz.Width, (bSz.Height - sz.Height) / 2);
            Brush br = new SolidBrush(PhraseFontColor);
            g.DrawString(Phrase, PhraseFont, br, ptDraw);
            g.Dispose();
            return bmp;
        }

        protected override Bitmap Generate()
        {
            if (Broken || IsDisposed)
                return null;
            Bitmap bmp = GenerateText();
            CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
            return bmp;
        }
        public override void Dispose()
        {
            base.Dispose();
            phraseFont.Dispose();
        }
    }
}
