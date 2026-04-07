using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml.Linq;
using IAT.Core.Models;
using System.Security.Policy;


namespace IAT.Core.Serializable
{
    public class ImageMetaData : IPackagePart
    {
        public Uri? Uri { get; set; } = null;

        public PartType PackagePartType => PartType.ImageMetaData;

        public Guid Id { get; set; } = Guid.Empty;

        public string? rOriginalImageId { get; set; } = null;

        public string? rImageId { get; set; } = null;

        public string? rThumbnailId { get; set; } = null;

        /// <summary>
        /// Gets or sets the absolute bounding rectangle for the element.
        /// </summary>
        [XmlElement("AbsoluteBounds", Form = XmlSchemaForm.Unqualified, Type = typeof(Rect))]
        private Rect _AbsoluteBounds { get; set; } = Rect.Empty;

        /// <summary>
        /// Gets the absolute bounding rectangle for the element. If save file data is being supplied, returns the value 
        /// from the private field; otherwise, retrieves it from the associated image.
        /// </summary>
        [XmlIgnore]
        public Rect AbsoluteBounds
        {
            get
            {
                return SupplyingSaveFileData ? _AbsoluteBounds : Image.AbsoluteBounds;
            }
        }

        /// <summary>
        /// Gets or sets the size of the image. If save file data is being supplied, returns the value from the private field;
        /// </summary>
        [XmlElement("Size", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        private Size _Size = Size.Empty;

        /// <summary>
        /// Gets the dimensions of the image as a Size structure.
        /// </summary>
        /// <remarks>The returned value reflects either the current image size or a supplied value,
        /// depending on the state of the object. This property is ignored during XML serialization due to the XmlIgnore
        /// attribute.</remarks>
        [XmlIgnore]
        public Size Size
        {
            get
            {
                return SupplyingSaveFileData ? _Size : Image.Size;
            }
        }

        /// <summary>
        /// Represents the origin point associated with the object. This field is intended for internal use and may be
        /// null if the origin is not specified.
        /// </summary>
        [XmlElement("Origin", Form = XmlSchemaForm.Unqualified, IsNullable = true, Type = typeof(Point))]
        private Point? _Origin;

        /// <summary>
        /// Gets the origin point associated with the current context.
        /// </summary>
        /// <remarks>The returned value depends on whether save file data is being supplied. If so, the
        /// origin is taken from the internal state; otherwise, it reflects the origin of the associated
        /// image.</remarks>
        [XmlIgnore]
        public Point Origin
        {
            get
            {
                return SupplyingSaveFileData ? _Origin : Image.Origin;
            }
        }

        /// <summary>
        /// The original size of the image before any transformations or resizing. This field is used to store the initial dimensions of the image
        /// </summary>
        [XmlElement("OriginalSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        private Size _OriginalSize = Size.Empty;

        /// <summary>
        /// Gets the original dimensions of the image before any modifications or processing.
        /// </summary>
        [XmlIgnore]
        public Size OriginalSize
        {
            get
            {
                return SupplyingSaveFileData ? _OriginalSize : Image.OriginalSize;
            }
        }

        /// <summary>
        /// Represents the size of the thumbnail image to be used during XML serialization.
        /// </summary>
        /// <remarks>This field is intended for internal use and is serialized as the 'ThumbnailSize' XML
        /// element. The value is of type Size and defaults to Size.Empty if not set.</remarks>
        [XmlElement("ThumbnailSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        private Size _ThumbnailSize = Size.Empty;

        /// <summary>
        /// Gets the size of the thumbnail image associated with the current image.
        /// </summary>
        /// <remarks>If no thumbnail is available, the property returns <see cref="Size.Empty"/>. The
        /// returned value may depend on the current state of the object, such as whether save file data is being
        /// supplied.</remarks>
        public Size ThumbnailSize
        {
            get
            {
                if (SupplyingSaveFileData)
                    return _ThumbnailSize;
                if (Image.Thumbnail == null)
                    return Size.Empty;
                return Image.Thumbnail.Size;
            }
        }

        /// <summary>
        /// The format of the thumbnail image, represented as an ImageFormat object. This field is used to store 
        /// the image format information for the thumbnail during XML serialization.
        /// </summary>
        [XmlElement("ThumbnailFormat", Form = XmlSchemaForm.Unqualified, Type = typeof(ImageFormat))]
        private ImageFormat _ThumbnailFormat;

        /// <summary>
        /// Gets the image format used for the thumbnail associated with this instance.
        /// </summary>
        [XmlIgnore]
        public ImageFormat ThumbnailFormat
        {
            get
            {
                return SupplyingSaveFileData ? _ThumbnailFormat : (Image.Thumbnail?.ImageFormat);
            }
        }

        /// <summary>
        /// Represents the original format of the image as specified in the XML data.
        /// </summary>
        [XmlElement("OriginalFormat", Form = XmlSchemaForm.Unqualified, Type = typeof(ImageFormat))]
        private ImageFormat _OriginalFormat;

        /// <summary>
        /// Gets the image format of the original image before any modifications or processing.
        /// </summary>
        /// <remarks>This property returns the format of the image as it was initially loaded or supplied.
        /// If the image was created or modified in memory, the returned format may reflect the most recent original
        /// state available.</remarks>
        [XmlIgnore]
        public ImageFormat OriginalFormat
        {
            get
            {
                return SupplyingSaveFileData ? _OriginalFormat : Image?.OriginalImage?.ImageFormat;
            }
        }

        /// <summary>
        /// Represents the image format used for serialization or deserialization.
        /// </summary>
        [XmlElement("ImageFormat", Form = XmlSchemaForm.Unqualified, Type = typeof(ImageFormat))]
        private ImageFormat _ImageFormat;

        /// <summary>
        /// Gets the image format associated with the current instance.
        /// </summary>
        /// <remarks>The returned format may depend on whether save file data is being supplied. Use this
        /// property to determine the format in which the image will be processed or saved.</remarks>
        [XmlIgnore]
        public ImageFormat ImageFormat
        {
            get
            {
                return SupplyingSaveFileData ? _ImageFormat : Image.ImageFormat;
            }
        }

        /// <summary>
        /// Represents the DIType element for XML serialization. This field is used to store the value of the DIType
        /// element when deserializing or serializing XML data.
        /// </summary>
        /// <remarks>This field is typically managed by the XML serializer and should not be accessed
        /// directly in application code. Use the corresponding public property to interact with the DIType
        /// value.</remarks>
        [XmlElement("DIType", Form = XmlSchemaForm.Unqualified, Type = typeof(DIType), IsNullable = true)]
        private DIType? _DIType = null;

        /// <summary>
        /// Gets the DI type associated with the current context.
        /// </summary>
        [XmlIgnore]
        public DIType DIType
        {
            get
            {
                return SupplyingSaveFileData ? _DIType : Image.DIType;
            }
        }

        [XmlElement("ImageMediaType", Form = XmlSchemaForm.Unqualified, Type = typeof(ImageMediaType), IsNullable = true)]
        private ImageMediaType? _ImageMediaType = null;

        public ImageMediaType ImageMediaType
        {
            get
            {
                return SupplyingSaveFileData ? _ImageMediaType : Image.ImageMediaType;
            }
        }

        private Uri _ImageUri = null;

        public Uri ImageUri
        {
            get
            {
                if (SupplyingSaveFileData)
                    return _ImageUri == null ? CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, ImageRelId).TargetUri : _ImageUri;
                if (_Image == null)
                    return _ImageUri;
                return Image.URI;
            }
        }

        private Uri _OriginalImageUri = null;

        public Uri OriginalImageUri
        {
            get
            {
                if (SupplyingSaveFileData)
                    return _OriginalImageUri;
                return Image?.OriginalImage?.URI;
            }
        }

        private Uri _ThumbnailUri = null;

        public Uri ThumbnailUri
        {
            get
            {
                return SupplyingSaveFileData ? _ThumbnailUri : CIAT.SaveFile.GetRelationship(Image, ThumbnailRelId).TargetUri;
            }
        }

        public string ImageRelId { get; private set; } = string.Empty;
        private string ThumbnailRelId { get; set; } = string.Empty;
        private string OriginalImageRelId { get; set; } = string.Empty;

        private ImageManager.Image _Image = null;

        public ImageManager.Image Image
        {
            get
            {
                if (_Image == null)
                    _Image = CIAT.SaveFile.GetIImage(ImageUri) as ImageManager.Image;
                return _Image;
            }

            private set
            {
                _Image = value;
            }
        }

        // In-memory byte data for images (not serialized)
        [XmlIgnore]
        private byte[] _originalImageBytes;
        [XmlIgnore]
        private byte[] _workingImageBytes;
        [XmlIgnore]
        private byte[] _thumbnailBytes;

        public ImageMetaData(IImage img)
        {
            ImageRelId = CIAT.SaveFile.CreateRelationship(CIAT.SaveFile.ImageMetaDataDocument.GetType(), img.BaseType, CIAT.SaveFile.ImageMetaDataDocument.URI, img.URI);
            CIAT.SaveFile.ImageMetaDataDocument.Entries[ImageRelId] = this;
            Image = img;
            SupplyingSaveFileData = false;
        }

        public ImageMetaData(ImageMetaDataDocument mdd, XElement node)
        {
            Load(mdd, node);
            SupplyingSaveFileData = true;
        }

        public void SupplySaveFileData()
        {
            SupplyingSaveFileData = true;
        }

        public void SupplySourceData()
        {
            SupplyingSaveFileData = false;
        }

        public void Append(XElement root)
        {
            string absX = AbsoluteBounds.X.ToString(), absY = AbsoluteBounds.Y.ToString(), absWidth = AbsoluteBounds.Width.ToString(), absHeight = AbsoluteBounds.Height.ToString();
            XElement elem = new XElement(typeof(ImageMetaData).Name);
            elem.Add(new XAttribute("rImageId", ImageRelId));
            elem.Add(new XElement("AbsoluteBounds", new XElement("X", absX), new XElement("Y", absY), new XElement("Width", absWidth),
                new XElement("Height", absHeight)));
            string origX = Origin.X.ToString(), origY = Origin.Y.ToString(), sizeWidth = Size.Width.ToString(), sizeHeight = Size.Height.ToString();
            elem.Add(new XElement("Origin", new XElement("X", origX.ToString()), new XElement("Y", origY.ToString())));
            elem.Add(new XElement("Size", new XElement("Width", sizeWidth.ToString()), new XElement("Height", sizeHeight.ToString())));
            elem.Add(new XElement("MimeType", Image.MimeType));
            elem.Add(new XElement("DIType", Image.DIType.ToString()));
            if (Image.OriginalImage != null)
            {
                if (OriginalImageRelId == string.Empty)
                    OriginalImageRelId = CIAT.SaveFile.CreateRelationship(Image.BaseType, Image.OriginalImage.BaseType, Image.URI, Image.OriginalImage.URI);
                XElement origElem = new XElement("OriginalImage");
                origElem.Add(new XAttribute("rOriginalImageId", OriginalImageRelId),
                    new XElement("ImageFormat", Image.OriginalImage.MimeType),
                    new XElement("Size", new XElement("Width", Image.OriginalImage.Size.Width.ToString()),
                    new XElement("Height", Image.OriginalImage.Size.Height.ToString())));
                elem.Add(origElem);
            }

            if (Image.Thumbnail != null)
            {
                if (ThumbnailRelId == string.Empty)
                    ThumbnailRelId = CIAT.SaveFile.CreateRelationship(Image.BaseType, Image.Thumbnail.BaseType, Image.URI, Image.Thumbnail.URI);
                XElement thumbElem = new XElement("Thumbnail");
                thumbElem.Add(new XAttribute("rThumbnailImageId", ThumbnailRelId),
                    new XElement("ImageFormat", Image.Thumbnail.MimeType),
                    new XElement("Size", new XElement("Width", Image.Thumbnail.Size.Width.ToString()),
                    new XElement("Height", Image.Thumbnail.Size.Height.ToString())));
                elem.Add(thumbElem);
            }

            root.Add(elem);
        }

        public void Load(ImageMetaDataDocument mdd, XElement elem)
        {
            ImageRelId = elem.Attribute("rImageId").Value;
            _ImageUri = CIAT.SaveFile.GetRelationship(mdd, ImageRelId).TargetUri;
            int absX = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("X").Value);
            int absY = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("Y").Value);
            int absWidth = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("Width").Value);
            int absHeight = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("Height").Value);
            _AbsoluteBounds = new Rectangle(absX, absY, absWidth, absHeight);
            int ptX = Convert.ToInt32(elem.Element("Origin").Element("X").Value);
            int ptY = Convert.ToInt32(elem.Element("Origin").Element("Y").Value);
            _Origin = new Point(ptX, ptY);
            int szWidth = Convert.ToInt32(elem.Element("Size").Element("Width").Value);
            int szHeight = Convert.ToInt32(elem.Element("Size").Element("Height").Value);
            _Size = new Size(szWidth, szHeight);
            _ImageFormat = ImageFormat.FromMimeType(elem.Element("MimeType").Value);
            _DIType = DIType.FromString(elem.Element("DIType").Value);
            _ImageMediaType = ImageMediaType.FromDIType(_DIType);
            if (elem.Element("OriginalImage") != null)
            {
                XElement origElem = elem.Element("OriginalImage");
                OriginalImageRelId = origElem.Attribute("rOriginalImageId").Value;
                _OriginalImageUri = CIAT.SaveFile.GetRelationship(_ImageUri, OriginalImageRelId).TargetUri;
                _OriginalFormat = ImageFormat.FromMimeType(origElem.Element("ImageFormat").Value);
                _ImageFormat = _OriginalFormat;
                _OriginalSize = new Size(Convert.ToInt32(origElem.Element("Size").Element("Width").Value),
                    Convert.ToInt32(origElem.Element("Size").Element("Height").Value));
            }

            if (elem.Element("Thumbnail") != null)
            {
                XElement thumbElem = elem.Element("Thumbnail");
                ThumbnailRelId = thumbElem.Attribute("rThumbnailImageId").Value;
                _ThumbnailUri = CIAT.SaveFile.GetRelationship(_ImageUri, ThumbnailRelId).TargetUri;
                _ThumbnailFormat = ImageFormat.FromMimeType(thumbElem.Element("ImageFormat").Value);
                _ThumbnailSize = new Size(Convert.ToInt32(thumbElem.Element("Size").Element("Width").Value),
                    Convert.ToInt32(thumbElem.Element("Size").Element("Height").Value));
            }
        }

        // Methods for resizing and managing images
        public void ResizeWorkingImage(Size newSize)
        {
            if (_originalImageBytes == null) return;
            // Use ImageSharp or WPF to resize
            using var image = Image.Load(_originalImageBytes);
            image.Mutate(x => x.Resize((int)newSize.Width, (int)newSize.Height));
            _workingImageBytes = GetImageBytes(image, ImageFormat);
        }

        public void GenerateThumbnail(Size thumbSize)
        {
            if (_workingImageBytes == null) return;
            using var image = Image.Load(_workingImageBytes);
            image.Mutate(x => x.Resize((int)thumbSize.Width, (int)thumbSize.Height));
            _thumbnailBytes = GetImageBytes(image, ThumbnailFormat ?? ImageFormat.Png);
        }

        public void ReplaceOriginalImage(byte[] newOriginalBytes)
        {
            _originalImageBytes = newOriginalBytes;
            // Regenerate working and thumbnail
            ResizeWorkingImage(Size); // Assuming Size is the target working size
            GenerateThumbnail(ThumbnailSize);
        }

        public void FlushToPackage()
        {
            // Flush byte data to package parts and free memory
            if (_originalImageBytes != null)
            {
                // Save to package part for original
                // CIAT.SaveFile.SavePart(OriginalImageUri, _originalImageBytes);
                _originalImageBytes = null;
            }
            if (_workingImageBytes != null)
            {
                // Save to package part for working image
                // CIAT.SaveFile.SavePart(ImageUri, _workingImageBytes);
                _workingImageBytes = null;
            }
            if (_thumbnailBytes != null)
            {
                // Save to package part for thumbnail
                // CIAT.SaveFile.SavePart(ThumbnailUri, _thumbnailBytes);
                _thumbnailBytes = null;
            }
        }

        private byte[] GetImageBytes(Image image, ImageFormat format)
        {
            using var ms = new MemoryStream();
            // Assuming ImageSharp encoders
            var encoder = format.EncoderFactory();
            image.Save(ms, encoder);
            return ms.ToArray();
        }
    }
}