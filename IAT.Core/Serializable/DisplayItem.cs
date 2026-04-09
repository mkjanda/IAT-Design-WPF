using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Windows;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Media.Converters;
using IAT.Core.Models;
using System.Runtime.ExceptionServices;
using System.Net.Mime;
using System.Formats.Nrbf;


namespace IAT.Core.Serializable
{
    public class DisplayItem : IPackagePart
    {
        /// <summary>
        /// THe URI of the objectin the package. This property is used to identify the location of the display item within the package structure.
        /// </summary>
        public Uri Uri => PackUriHelper.CreatePartUri(new Uri($"{typeof(DisplayItem).ToString()}/{Id.ToString()}.xml", UriKind.Relative));

        public PartType PackagePartType => PartType.DisplayItem;

        public Guid Id { get; set; } = Guid.Empty;

        public LayoutItem ItemType { get; set; }

        private readonly object _lock = new object();

        private readonly Debouncer cacheDebouncer;

        public LayoutItem LayoutItem { get; set; }

        public PackagePart? OriginalImagePart { get; set; } = null;

        public PackagePart? WorkingImagePart { get; set; } = null;
        public PackagePart? ThumbnailPart { get; set; } = null;

        // In-memory byte data for images (not serialized)
        [XmlIgnore]
        private MemoryStream? OriginalImageBytes = null;
        [XmlIgnore]
        private MemoryStream? WorkingImageBytes = null;
        [XmlIgnore]
        private MemoryStream? ThumbnailBytes = null;


        private BitmapEncoder FromType(string imageType) =>
            imageType switch {
                MediaTypeNames.Image.Jpeg => new JpegBitmapEncoder(),
                MediaTypeNames.Image.Gif => new GifBitmapEncoder(),
                MediaTypeNames.Image.Png => new PngBitmapEncoder(),
                MediaTypeNames.Image.Tiff => new TiffBitmapEncoder(),
                MediaTypeNames.Image.Bmp => new BmpBitmapEncoder(),
                _ => throw new ArgumentException($"Unsupported image type: {imageType}")
            };
        

        /// <summary>
        /// Gets or sets the original image associated with this item as a BitmapImage.
        /// </summary>
        /// <remarks>Accessing this property loads the image data from the underlying stream and returns a
        /// frozen BitmapImage for thread safety. Setting this property encodes and stores the provided image. The
        /// property is thread-safe. The image is cached in memory and may be flushed after a delay.</remarks>
        public BitmapImage OriginalImage
        {
            get
            {
                if (OriginalImagePart == null)
                    throw new InvalidOperationException("OriginalImagePart is not set.");
                try
                {
                    lock (_lock)
                    {
                        if (OriginalImageBytes == null)
                        {
                            OriginalImageBytes = new MemoryStream();
                            using (Stream stream = OriginalImagePart.GetStream())
                            {
                                stream.CopyTo(OriginalImageBytes);
                                OriginalImageBytes.Position = 0; // Reset position after copying
                            }
                        }
                        var bitmap = new BitmapImage();
                        BitmapDecoder decoder = new JpegBitmapDecoder(OriginalImageBytes, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        decoder.Frames[0].Freeze(); // Freeze the frame to make it cross-thread accessible
                        bitmap = new BitmapImage()
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = OriginalImageBytes;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Freeze to make it cross-thread accessible
                        cacheDebouncer.Refresh(); // Schedule cache flush after delay
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null;
                }
            } set
            {
                lock (_lock)
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value), "OriginalImage cannot be set to null.");
                    OriginalSize = new Size(value.PixelWidth, value.PixelHeight);
                    if (OriginalImageBytes != null)
                        OriginalImageBytes.Dispose();
                    OriginalImageBytes = new MemoryStream();
                    BitmapEncoder encoder = FromType(LayoutItem.ImageType);
                    encoder.Frames.Add(BitmapFrame.Create(value));
                    encoder.Frames[0].Freeze(); 
                    encoder.Save(OriginalImageBytes);
                    OriginalImageBytes.Position = 0; // Reset position after writing
                    cacheDebouncer.Refresh(); // Schedule cache flush after delay
                }
            }
        }

        /// <summary>
        /// Gets or sets the working image associated with the current item as a BitmapImage.
        /// </summary>
        /// <remarks>Accessing this property retrieves the image from the underlying data stream and
        /// returns a frozen BitmapImage instance for thread safety. Setting this property updates the underlying image
        /// data. The property is thread-safe. The image is cached in memory and may be refreshed or flushed based on
        /// internal cache management.</remarks>
        public BitmapImage WorkingImage
        {
            get
            {
                if (WorkingImagePart == null)
                    throw new InvalidOperationException("WorkingImagePart is not set.");
                try
                {
                    lock (_lock)
                    {
                        if (WorkingImageBytes == null)
                        {
                            WorkingImageBytes = new MemoryStream();
                            using (Stream stream = WorkingImagePart.GetStream())
                            {
                                stream.CopyTo(WorkingImageBytes);
                                WorkingImageBytes.Position = 0; // Reset position after copying
                            }
                        }
                        var bitmap = new BitmapImage();
                        BitmapDecoder decoder = new JpegBitmapDecoder(WorkingImageBytes, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        decoder.Frames[0].Freeze(); // Freeze the frame to make it cross-thread accessible
                        bitmap = new BitmapImage()
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = WorkingImageBytes;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Freeze to make it cross-thread accessible
                        cacheDebouncer.Refresh(); // Schedule cache flush after delay
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value), "WorkingImage cannot be set to null.");
                    if (WorkingImageBytes != null)
                        WorkingImageBytes.Dispose();
                    WorkingImageBytes = new MemoryStream();
                    BitmapEncoder encoder = FromType(LayoutItem.ImageType);
                    encoder.Frames.Add(BitmapFrame.Create(value));
                    encoder.Frames[0].Freeze();
                    encoder.Save(WorkingImageBytes);
                    WorkingImageBytes.Position = 0; // Reset position after writing
                    cacheDebouncer.Refresh(); // Schedule cache flush after delay
                }
            }
        }

        /// <summary>
        /// Gets or sets the thumbnail image associated with this item.
        /// </summary>
        /// <remarks>Accessing this property retrieves the thumbnail as a frozen BitmapImage, making it
        /// safe for cross-thread operations. Setting this property updates the underlying thumbnail data. The property
        /// is thread-safe. An InvalidOperationException is thrown if the thumbnail source is not set when getting the
        /// value. An ArgumentNullException is thrown if attempting to set the value to null.</remarks>
        public BitmapImage Thumbnail
        {
            get
            {
                if (ThumbnailPart == null)
                    throw new InvalidOperationException("ThumbnailPart is not set.");
                try
                {
                    lock (_lock)
                    {
                        if (ThumbnailBytes == null)
                        {
                            ThumbnailBytes = new MemoryStream();
                            using (Stream stream = ThumbnailPart.GetStream())
                            {
                                stream.CopyTo(ThumbnailBytes);
                                ThumbnailBytes.Position = 0; // Reset position after copying
                            }
                        }
                        var bitmap = new BitmapImage();
                        BitmapDecoder decoder = new JpegBitmapDecoder(ThumbnailBytes, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                        decoder.Frames[0].Freeze(); // Freeze the frame to make it cross-thread accessible
                        bitmap = new BitmapImage()
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ThumbnailBytes;
                        bitmap.EndInit();
                        bitmap.Freeze(); // Freeze to make it cross-thread accessible
                        cacheDebouncer.Refresh(); // Schedule cache flush after delay
                        return bitmap;
                    }
                }
                catch (Exception ex)
                {
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    return null;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value), "Thumbnail cannot be set to null.");
                    if (ThumbnailBytes != null)
                        ThumbnailBytes.Dispose();
                    ThumbnailBytes = new MemoryStream();
                    BitmapEncoder encoder = FromType(LayoutItem.ImageType);
                    encoder.Frames.Add(BitmapFrame.Create(value));
                    encoder.Frames[0].Freeze();
                    encoder.Save(ThumbnailBytes);
                    ThumbnailBytes.Position = 0; // Reset position after writing
                    cacheDebouncer.Refresh(); // Schedule cache flush after delay
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the DisplayItem class.
        /// </summary>
        /// <remarks>This constructor sets up internal debouncing logic to manage the saving of image data
        /// streams to their respective package parts. The debouncer ensures that image data is written efficiently and
        /// resources are released appropriately. Thread safety is maintained during the save operation.</remarks>
        public DisplayItem()
        {
            cacheDebouncer = new Debouncer(TimeSpan.FromSeconds(1), () =>
            {
                lock (_lock)
                {
                    if (OriginalImagePart != null && OriginalImageBytes != null)
                    {
                        // Save OriginalImageBytes to package part
                        using (var stream = OriginalImagePart.GetStream(FileMode.Create, FileAccess.Write))
                        {
                            OriginalImageBytes.Position = 0; // Ensure we're at the beginning
                            OriginalImageBytes.CopyTo(stream);
                        }
                        OriginalImageBytes.Dispose();
                        OriginalImageBytes = null;
                    }
                    if (WorkingImagePart != null && WorkingImageBytes != null)              
                    {
                        // Save WorkingImageBytes to package part
                        using (var stream = WorkingImagePart.GetStream(FileMode.Create, FileAccess.Write))
                        {
                            WorkingImageBytes.Position = 0; // Ensure we're at the beginning
                            WorkingImageBytes.CopyTo(stream);
                        }
                        WorkingImageBytes.Dispose();
                        WorkingImageBytes = null;
                    }
                    if (ThumbnailPart != null && ThumbnailBytes != null)
                    {
                        // Save ThumbnailBytes to package part
                        using (var stream = ThumbnailPart.GetStream(FileMode.Create, FileAccess.Write))
                        {
                            ThumbnailBytes.Position = 0; // Ensure we're at the beginning
                            ThumbnailBytes.CopyTo(stream);
                        }
                        ThumbnailBytes.Dispose();
                        ThumbnailBytes = null;
                    }       
                }
            });
        }



        /// <summary>
        /// The original size of the image before any transformations or resizing. This field is used to store the initial dimensions of the image
        /// </summary>
        [XmlElement("OriginalSize", Form = XmlSchemaForm.Unqualified, Type = typeof(Size))]
        public Size OriginalSize { get; set; }  


        /// <summary>
        /// Represents the original format of the image as specified in the XML data.
        /// </summary>
        [XmlElement("OriginalFormat", Form = XmlSchemaForm.Unqualified, Type = typeof(ImageFormat))]
        private ImageFormat? _OriginalFormat = null;

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