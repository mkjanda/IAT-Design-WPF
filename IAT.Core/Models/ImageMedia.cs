using IAT.Core.Enumerations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Reflection.Emit;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace IAT.Core.Models
{
    public class ImageMedia : IImageMedia
    {
        [XmlElement("Uri", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public Uri? Uri { get; set; } = null;

        [XmlElement("PackagePartType", Form = XmlSchemaForm.Unqualified)]
        public PartType PackagePartType => PartType.ImageMedia;

        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.Empty;

        [XmlIgnore]
        public const long CacheDelay = 30000;

        [XmlIgnore]
        public bool IsDisposed { get; protected set; } = false;

        [XmlIgnore]
        protected bool ChangeEventsPaused { get; set; } = false;

        [XmlElement("Size", Form = XmlSchemaForm.Unqualified)]
        public Size Size { get; protected set; }

        [XmlElement("ImageFormat", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public ImageFormat? ImageFormat { get; protected set; } = null;


        [XmlIgnore]
        public bool IsCached
        {
            get
            {
                return ((image != null) && (CacheEntryTime != DateTime.MaxValue));
            }
        }

        public virtual void PauseChangeEvents()
        {
            ChangeEventsPaused = true;
        }

        public virtual void ResumeChangeEvents()
        {
            ChangeEventsPaused = false;
            RenderTargetBitmap rtb = new RenderTargetBitmap(1, 1, 96, 96, PixelFormats.Default);
        }


        private readonly ConcurrentQueue<System.Drawing.Image> ImagesQueuedForCache = new ConcurrentQueue<System.Drawing.Image>();
        public Uri URI { get; set; } = null;
        private Size _Size = Size.Empty;
        private ImageFormat _ImageFormat = null;
        public ImageFormat ImageFormat
        {
            get
            {
                if (_ImageFormat == null)
                    _ImageFormat = ImageFormat.FromMimeType(MimeType);
                return _ImageFormat;
            }
            set
            {
                _ImageFormat = value;
            }
        }
        protected System.Drawing.Image image = null;
        public bool PendingFetch { get; private set; } = false;
        public bool PendingWrite { get; private set; } = false;
        public virtual bool PendingResize { get { return false; } protected set { } }
        public readonly object lockObj = new object();
        public readonly object imageLock = new object();
        public readonly object cacheLock = new object();
        public ManualResetEventSlim pendingFetchEvent = new ManualResetEventSlim(true), pendingWriteEvent = new ManualResetEventSlim(true);
        public ManualResetEventSlim CachedEvent { get; private set; } = new ManualResetEventSlim(false);
        public String MimeType { get { return ImageFormat.MimeType; } }
        public event Action<ImageEvent, IImageMedia, object> Changed = null;
        public DateTime CacheEntryTime { get; set; } = DateTime.MaxValue;
        protected static Action<IntPtr, byte, int> Memset;
        public ImageMediaType ImageMediaType { get; protected set; }
        static ImageMedia()
        {
            DynamicMethod dynMethod = new DynamicMethod("Memset", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                null, new[] { typeof(IntPtr), typeof(byte), typeof(int) }, typeof(ImageMedia), true);
            var generator = dynMethod.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_2);
            generator.Emit(OpCodes.Initblk);
            generator.Emit(OpCodes.Ret);
            Memset = dynMethod.CreateDelegate(typeof(Action<IntPtr, byte, int>)) as Action<IntPtr, byte, int>;
        }

        protected ImageMedia()
        {
        }

        public ImageMedia(Uri uri, Size sz, ImageFormat format, ImageMediaType type)
        {
            URI = uri;
            this.Size = sz;
            ImageFormat = format;
            ImageMediaType = type;
        }

        public ImageMedia(ImageFormat format, ImageMediaType type)
        {
            this.Size = type.ImageSize;
            ImageMediaType = type;
            ImageFormat = format;
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + ImageFormat.Format.ToString());
        }

        public ImageMedia(System.Drawing.Image img, ImageFormat format, ImageMediaType type)
        {
            ImageFormat = format;
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + format.Format.ToString());
            this.Size = img.Size;
            ImageMediaType = type;
            Img = img;
        }

        public virtual Type BaseType
        {
            get
            {
                return typeof(ImageMedia);
            }
        }

        public String FileExtension
        {
            get
            {
                return ImageFormat.Extension;
            }
        }

        public Size Size
        {
            get
            {
                if (_Size.IsEmpty || (_Size.Width == 0) || (_Size.Height == 0))
                    return ImageMediaType.ImageSize;
                return _Size;
            }
            protected set
            {
                _Size = value;
            }
        }


        protected virtual System.Drawing.Image CreateCopy(System.Drawing.Image img)
        {
            lock (imageLock)
            {
                if (img == null)
                    return null;
                CacheEntryTime = DateTime.Now;
                System.Drawing.Image clone = img.Clone() as System.Drawing.Image;
                clone.Tag = img.Tag;
                return clone;
            }
        }

        public void RemoveFromCache()
        {
            lock (imageLock)
            {
                if (image != null)
                {
                    //WriteImage();
                    CIAT.ImageManager.ReleaseImage(Img);
                    CacheEntryTime = DateTime.MaxValue;
                    DisposeOfImage();
                }
            }
        }

        public void WriteImage(System.Drawing.Image val)
        {
            lock (imageLock)
            {
                if (URI == null)
                    URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + ImageFormat.ToString());
                Stream s = null;
                try
                {
                    s = CIAT.SaveFile.GetWriteStream(this);
                    val.Save(s, ImageFormat.Format);
                }
                finally
                {
                    s?.Dispose();
                    CIAT.SaveFile.ReleaseWriteStreamLock();
                }
            }
        }

        public void LoadImage()
        {
            lock (imageLock)
            {
                if (IsCached)
                    return;
                image = CIAT.ImageManager.FetchImageMedia(this);
                if (image == null)
                    return;
                CacheEntryTime = DateTime.Now;
                FireChanged(ImageEvent.LoadedFromDisk);
            }
        }

        public virtual System.Drawing.Image Img
        {
            get
            {
                System.Drawing.Image img;
                lock (imageLock)
                {
                    if (IsDisposed)
                        return null;
                    if (IsCached)
                        return CreateCopy(image);
                    img = CIAT.ImageManager.FetchImageMedia(this);
                    if (img == null)
                        return null;
                    img.Tag = ImageMediaType;
                    //       retVal = CreateCopy(image);
                }
                return img;
            }
            set
            {
                lock (imageLock)
                {
                    value.Tag = ImageMediaType;
                    this.Size = value.Size;
                    WriteImage(value);
                    //                      this.Size = value.Size;
                    //                        CIAT.SaveFile.ImageManager.AddImageToCache(this);
                    //                    CacheEntryTime = DateTime.Now;
                }
                FireChanged(ImageEvent.Updated);
                CIAT.ImageManager.ReleaseImage(value);
            }
        }


        protected virtual void FireChanged(ImageEvent evt)
        {
            if ((Changed != null) && !CIAT.ImageManager.IsDisposed && !CIAT.ImageManager.IsDisposing)
            {
                Delegate[] dels = Changed.GetInvocationList();
                foreach (Delegate d in dels)
                    d.DynamicInvoke(evt, this, null);
            }
        }

        protected virtual void FireChanged(ImageEvent evt, object arg)
        {
            if ((Changed != null) && !CIAT.ImageManager.IsDisposed && !CIAT.ImageManager.IsDisposing)
            {
                Delegate[] dels = Changed.GetInvocationList();
                foreach (Delegate d in dels)
                {
                    if (d.Target != null)
                        d.DynamicInvoke(evt, this, arg);
                }
            }
        }

        public virtual void Dispose()
        {
            lock (imageLock)
            {
                IsDisposed = true;
                Changed = null;
                DisposeOfImage();
                CIAT.SaveFile.DeletePart(this.URI);
                PendingWrite = PendingFetch = false;
            }
        }

        public void DisposeOfImage()
        {
            lock (imageLock)
            {
                if (image != null)
                {
                    CIAT.ImageManager.ReleaseImage(image);
                    image = null;
                }
            }
        }

        public virtual object Clone()
        {
            return new ImageMedia(Img, ImageFormat, ImageMediaType);
        }

        public void ClearChanged()
        {
            Changed = null;
        }
    }
}
