using java.util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IAT.Core.Models.Enumerations
{
    
    public class DIType : Enumeration
    {
        public static DIType Null = new DIType(0, "Null")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DINull(uri); }),
            Type = typeof(DINull),
            GetBoundingSize = new Func<Size>(() => new Size(1, 1)),
            GetBoundingRectangle = new Func<Rectangle>(() => new Rectangle(0, 0, 1, 1)),
            InvalidationInterval = 0,
            HasPreviewPanel = false,
            IsGenerated = false,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType StimulusImage = new DIType(1, "StimulusImage")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIStimulusImage(uri); }),
            Type = typeof(DIStimulusImage),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.StimulusSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.StimulusRectangle),
            InvalidationInterval = 250,
            HasPreviewPanel = false,
            IsGenerated = false,
            HasThumbnail = true,
            StoreOriginalImage = true,
            IsResponseKeyComponent = false
        };
        public static DIType ResponseKeyImage = new DIType(2, "ResponseKeyImage")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIResponseKeyImage(uri); }),
            Type = typeof(DIResponseKeyImage),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 250,
            HasPreviewPanel = true,
            IsGenerated = false,
            HasThumbnail = false,
            StoreOriginalImage = true,
            IsResponseKeyComponent = true
        };
        public static DIType StimulusText = new DIType(3, "StimulusText")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIStimulusText(uri); }),
            Type = typeof(DIStimulusText),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.StimulusSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.StimulusRectangle),
            InvalidationInterval = 50,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType ContinueInstructions = new DIType(4, "ContinueInstructions")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIContinueInstructions(uri); }),
            Type = typeof(DIContinueInstructions),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.ContinueInstructionsRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType ResponseKeyText = new DIType(5, "ResponseKeyText")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIResponseKeyText(uri); }),
            Type = typeof(DIResponseKeyText),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 150,
            HasPreviewPanel = true,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = true
        };
        public static DIType Conjunction = new DIType(6, "Conjunction")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIConjunction(uri); }),
            Type = typeof(DIConjunction),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 150,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = true
        };
        public static DIType MockItemInstructions = new DIType(7, "MockItemInstructions")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIMockItemInstructions(uri); }),
            Type = typeof(DIMockItemInstructions),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.MockItemInstructionsRectangle),
            InvalidationInterval = 300,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType IatBlockInstructions = new DIType(8, "IatBlockInstructions")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIIatBlockInstructions(uri); }),
            Type = typeof(DIIatBlockInstructions),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.InstructionsSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.InstructionsRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType TextInstructionsScreen = new DIType(9, "TextInstructionsScreen")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DITextInstructionsScreen(uri); }),
            Type = typeof(DITextInstructionsScreen),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType KeyedInstructionsScreen = new DIType(10, "KeyedInstructionsScreen")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIKeyedInstructionsScreen(uri); }),
            Type = typeof(DIKeyedInstructionsScreen),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType Preview = new DIType(11, "Preview")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIPreview(uri); }),
            Type = typeof(DIPreview),
            GetBoundingSize = new Func<Size>(() => Images.ImageMediaType.FullWindow.ImageSize),
            GetBoundingRectangle = new Func<Rectangle>(() => new Rectangle(new Point(0, 0), Images.ImageMediaType.FullWindow.ImageSize)),
            InvalidationInterval = 150,
            HasPreviewPanel = true,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType DualKey = new DIType(12, "DualKey")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIDualKey(uri); }),
            Type = typeof(DIDualKey),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 150,
            HasPreviewPanel = true,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType ErrorMark = new DIType(13, "ErrorMark")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIErrorMark(uri); }),
            Type = typeof(DIErrorMark),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.ErrorSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.ErrorRectangle),
            InvalidationInterval = 500,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType LeftKeyValueOutline = new DIType(14, "LeftKeyValueOutline")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIKeyValueOutline(uri); }),
            Type = typeof(DIKeyValueOutline),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle),
            InvalidationInterval = 150,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType RightKeyValueOutline = new DIType(15, "RightKeyValueOutline")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIKeyValueOutline(uri); }),
            Type = typeof(DIKeyValueOutline),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.RightKeyValueOutlineRectangle),
            InvalidationInterval = 150,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType LambdaGenerated = new DIType(16, "LambdaGenerated")
        {
            Create = new Func<Uri, DIBase>((uri) => { throw new NotImplementedException(); }),
            Type = typeof(DILambdaGenerated),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.InteriorSize),
            GetBoundingRectangle = new Func<Rectangle>(() => new Rectangle(0, 0, CIAT.SaveFile.Layout.InteriorSize.Width, CIAT.SaveFile.Layout.InteriorSize.Height)),
            InvalidationInterval = 100,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false,
            IsResponseKeyComponent = false
        };
        public static DIType SurveyImage = new DIType(17, "SurveyImage")
        {
            Create = new Func<Uri, DIBase>((uri) => new DISurveyImage(uri)),
            Type = typeof(DISurveyImage),
            GetBoundingSize = new Func<Size>(() => { throw new NotImplementedException(); }),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 200,
            HasPreviewPanel = true,
            IsGenerated = false,
            HasThumbnail = false,
            StoreOriginalImage = true,
            IsResponseKeyComponent = false
        };
        public Type Type { get; private set; }
        public String MimeType
        {
            get
            {
                return "text/xml+display-item+" + ToString();
            }
        }
        public Func<Uri, DIBase> Create { get; private set; }
        public Func<Size> GetBoundingSize { get; private set; }
        public Func<Rectangle> GetBoundingRectangle { get; private set; }
        public int InvalidationInterval { get; private set; }
        public bool HasPreviewPanel { get; private set; }
        public bool IsGenerated { get; private set; }
        public bool HasThumbnail { get; private set; }
        public bool StoreOriginalImage { get; private set; }
        public bool IsResponseKeyComponent { get; private set; }
        public bool IsText
        {
            get
            {
                return TextTypes.Where(t => t == this).FirstOrDefault() != null;
            }
        }
        public bool IsComposite
        {
            get
            {
                return CompositeTypes.Where(t => t == this).FirstOrDefault() != null;
            }
        }

        private DIType(int id, String name) : base(id, name) { }
        private DIType(int id, String name, int ii, Func<Uri, DIBase> f, Type t, Func<Size> bSz, Func<Rectangle> bRect)
            : base(id, name)
        {
            Type = t;
            Create = f;
            InvalidationInterval = ii;
            GetBoundingSize = bSz;
            GetBoundingRectangle = bRect;
        }
        private static readonly DIType[] CompositeTypes = new DIType[] { DualKey, Preview };
        private static readonly DIType[] TextTypes = new DIType[] { StimulusText, ContinueInstructions, ResponseKeyText, Conjunction, MockItemInstructions, IatBlockInstructions, TextInstructionsScreen };
        public static readonly IEnumerable<DIType> All = new DIType[]{ Null, StimulusImage, ResponseKeyImage, StimulusText, ContinueInstructions, ResponseKeyText, Conjunction, MockItemInstructions, IatBlockInstructions,
            TextInstructionsScreen, KeyedInstructionsScreen, Preview, DualKey, ErrorMark, LeftKeyValueOutline, RightKeyValueOutline, LambdaGenerated, SurveyImage };

        public static DIType FromString(String name)
        {
            return All.FirstOrDefault((diType) => diType.Name == name);
        }

        public static DIType FromTypeName(String tName)
        {
            return All.FirstOrDefault((diType) => diType.Type.ToString() == tName);
        }

        public static DIType FromType(Type t)
        {
            return All.FirstOrDefault((diType) => diType.Type == t);
        }

        public static DIType FromUri(Uri u)
        {
            Regex urlMatcher = new Regex(@"^/IATClient\.[^/]+/(IATClient\..*?)[1-9][0-9]*.*?(?!=\.rel)");
            if (!urlMatcher.IsMatch(u.ToString()))
                return null;
            String typeName = urlMatcher.Match(u.ToString()).Groups[1].Value;
            return All.FirstOrDefault((diType) => diType.Type.ToString() == typeName);
        }
    }

    internal class DIType
    {
    }
}
