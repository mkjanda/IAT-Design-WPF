using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using java.awt;
using java.util;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace IAT.Core.Models
{

    public abstract class DIBase : IDisposable, IPackagePart, ICloneable
    {
        /// <summary>
        /// Gets or sets the URI associated with this instance.
        /// </summary>
        [XmlElement("Uri", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public Uri? Uri { get; set; } = null;

        /// <summary>
        /// Gets the package part type associated with this instance.
        /// </summary>
        [XmlElement("PackagePartType", Form = XmlSchemaForm.Unqualified)]
        public PartType PackagePartType => PartType.DIBase;

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [XmlElement("Id", Form = XmlSchemaForm.Unqualified)]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The layout item associated with this display item, which defines its position and size within a layout. This property 
        /// is abstract and must be implemented by derived classes to provide the specific layout information for the display item.
        /// </summary>
        public abstract LayoutItem LayoutItem { get; }


        [XmlElement("ImageId", Form = XmlSchemaForm.Unqualified)]
        public required Guid ImageId { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the current invalidation state for the object.
        /// </summary>
        [XmlIgnore]
        protected InvalidationState State { get; set; } = InvalidationState.InvalidationReady;

        [XmlIgnore]
        protected bool IsDisposed { get; set; } = false;

        [XmlIgnore]
        private IImage? Image
        {

        }
        protected ImageDocument MetaDataDoc { get { return CIAT.SaveFile.ImageMetaDataDocument; } }
        public virtual Images.IImage IImage
        {
            get
            {
                return _IImage;
            }
            protected set
            {
                if (rImageId != null)
                {
                    CIAT.SaveFile.DeleteRelationship(Uri, rImageId);
                    IImage?.Dispose();
                }
                _IImage = value;
                if (value == null)
                    return;
                var pr = CIAT.SaveFile.GetRelationship(MetaDataDoc, value);
                _IImage = value;
                if (pr == null)
                    rImageId = CIAT.SaveFile.CreateRelationship(MetaDataDoc.BaseType, this.IImage.BaseType, MetaDataDoc.URI, this.IImage.URI);
                else
                    rImageId = pr;
                IImage.ClearChanged();
                IImage.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(OnImageEvent);
            }
        }
        public virtual bool IsObservable { get { return false; } }
        public long Expiration { get; set; }
        public virtual bool IsComposite { get { return false; } }
        private bool WaitingOnInvalidation { get; set; } = false;
        public virtual IUri IUri { get; private set; }

        public bool IsValid
        {
            get
            {
                if (IImage == null)
                    return false;
                if (!Interlocked.Equals(InvalidationState, InvalidationStates.InvalidationReady))
                    return false;
                return true;
            }
        }

        public static DIType GetDiType(Uri uri)
        {
            return DIType.FromTypeName(CIAT.SaveFile.GetTypeName(uri));
        }

        public String MimeType
        {
            get
            {
                return "text/xml+display-item+" + DIType.FromType(this.GetType()).ToString();
            }
        }

        public DIType Type { get { return DIType.FromType(this.GetType()); } }

        public DIBase(Uri URI)
        {
            InvalidationState = InvalidationStates.InvalidationReady;
            this.Uri = URI;
            IUri = new UriContainer(URI);
            GetBoundingSize = DIType.FromType(this.GetType()).GetBoundingSize;
            Load(URI);
            if (IImage == null)
            {
                IImage = new Images.ImageManager.Image(Images.ImageFormat.Png, Type);
                CIAT.SaveFile.CreateRelationship(BaseType, IImage.BaseType, URI, IImage.URI);
            }
            CIAT.SaveFile.Register(this);
            IImage.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(OnImageEvent);
        }

        public DIBase()
        {
            InvalidationState = InvalidationStates.InvalidationReady;
            if (Type.IsComposite)
                CompositeInvalidationEvents.Add(InvalidationEvent);
            else
                ComponentInvalidationEvents.Add(InvalidationEvent);
            Uri = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            IUri = new UriContainer(Uri);
            GetBoundingSize = DIType.FromType(this.GetType()).GetBoundingSize;
            IImage = new Images.ImageManager.Image(Images.ImageFormat.Png, Type);
            rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument.URI, IImage.URI);
            IImage.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(OnImageEvent);
            CIAT.SaveFile.Register(this);
        }

        public DIBase(Images.IImage imgObj)
        {
            InvalidationState = InvalidationStates.InvalidationReady;
            if (Type.IsComposite)
                CompositeInvalidationEvents.Add(InvalidationEvent);
            else
                ComponentInvalidationEvents.Add(InvalidationEvent);
            Uri = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            IUri = new UriContainer(Uri);
            GetBoundingSize = DIType.FromType(this.GetType()).GetBoundingSize;
            this.IImage = imgObj.Clone() as Images.IImage;
            CIAT.SaveFile.Register(this);
        }

        public virtual Size BoundingSize
        {
            get
            {
                return GetBoundingSize();
            }
        }
        public virtual Rectangle AbsoluteBounds { get; protected set; } = Rectangle.Empty;
        private static readonly List<ManualResetEventSlim> ComponentInvalidationEvents = new List<ManualResetEventSlim>();
        private static readonly List<ManualResetEventSlim> CompositeInvalidationEvents = new List<ManualResetEventSlim>();
        protected abstract void Invalidate();
        public virtual bool IsGenerated { get { return false; } }
        public WaitHandle InvalidationWaitHandle { get { return InvalidationEvent.WaitHandle; } }
        private readonly object InvalidationLock = new object();
        public readonly ManualResetEventSlim InvalidationEvent = new ManualResetEventSlim(true), BlockedInvalidationComplete = new ManualResetEventSlim(true);

        public bool LayoutSuspended { get; private set; }
        public void SuspendLayout()
        {
            LayoutSuspended = true;
        }
        public void ResumeLayout(bool immediate)
        {
            if (!LayoutSuspended)
                return;
            LayoutSuspended = false;
            if (immediate)
                ScheduleInvalidation();

        }

        public void ScheduleInvalidationSync()
        {
            if (IImage == null)
                return;
            try
            {
                lock (InvalidationLock)
                {
                    if (!IImage.IsCached && (PreviewPanel == null) && Type.HasPreviewPanel && (IImage.URI == null))
                    {
                        if (Interlocked.Equals(InvalidationState, InvalidationStates.CacheInvalidationQueued))
                            return;
                        Interlocked.Exchange(ref InvalidationState, InvalidationStates.CacheInvalidationQueued);
                        var owners = CIAT.SaveFile.GetRelationshipsByType(Uri, BaseType, typeof(DIBase), "owned-by").ToList();
                        foreach (Uri diUri in owners.Select(pr => pr.TargetUri))
                        {
                            var owner = CIAT.SaveFile.GetDI(diUri);
                            if (owner != null)
                                owner.ScheduleInvalidation();
                            else if (owner == null)
                                CIAT.SaveFile.DeleteRelationship(Uri, owner.URI);
                        }
                        return;
                    }
                }
                if (Interlocked.CompareExchange(ref InvalidationState, InvalidationStates.Invalidating, InvalidationStates.CacheInvalidationQueued).Equals(InvalidationStates.CacheInvalidationQueued))
                {
                    InvalidationEvent.Reset();
                    Invalidate();
                    return;
                }
                else if (Interlocked.CompareExchange(ref InvalidationState, InvalidationStates.Invalidating, InvalidationStates.InvalidationReady).Equals(InvalidationStates.InvalidationReady))
                {
                    InvalidationEvent.Reset();
                    Invalidate();
                    return;
                }
                else if (Interlocked.CompareExchange(ref InvalidationState, InvalidationStates.InvalidationQueued, InvalidationStates.Invalidating).Equals(InvalidationStates.Invalidating))
                {
                    InvalidationEvent.Wait();
                    InvalidationEvent.Reset();
                    Invalidate();
                    return;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error occurred in image generation", ex));
            }
        }

        public void ScheduleInvalidation()
        {
            Task.Run(() =>
            {
                ScheduleInvalidationSync();
            });
        }


        protected virtual void Validate()
        {
            Interlocked.CompareExchange(ref InvalidationState, InvalidationStates.InvalidationReady, InvalidationStates.Invalidating);
            Interlocked.CompareExchange(ref InvalidationState, InvalidationStates.Invalidating, InvalidationStates.InvalidationQueued);
            Interlocked.CompareExchange(ref InvalidationState, InvalidationStates.InvalidationReady, InvalidationStates.BlockedInvalidationQueued);
            InvalidationEvent.Set();
        }

        public virtual void LockValidation(ValidationLock validationLock) { }

        protected abstract void DoLoad(Uri uri);

        public void Load(Uri uri)
        {
            DoLoad(uri);
            if (!InvalidationState.Equals(InvalidationStates.InvalidationReady))
            {
                InvalidationState = InvalidationStates.InvalidationReady;
                ScheduleInvalidation();
            }
        }
        public abstract void Save();
        public void Save(Uri uri)
        {
            this.Uri = uri;
            Save();
            CIAT.SaveFile.Register(this);
        }

        public virtual bool ImageStale { get; protected set; } = false;
        public virtual bool ComponentStale
        {
            get
            {
                return ImageStale || (IImage == null);
            }
        }


        public virtual void SetImage(String rImageId)
        {
            lock (lockObject)
            {
                if (this.IImage != null)
                {
                    if (this.IImage.MetaData.ImageRelId != rImageId)
                    {
                        //                        CIAT.SaveFile.DeleteRelationship(URI, IImage.URI);
                        this.IImage.Dispose();
                    }
                    else
                        return;
                }
                this.rImageId = rImageId;
                this.IImage = CIAT.SaveFile.GetIImage(CIAT.SaveFile.ImageMetaDataDocument.Entries[rImageId].ImageUri);
                this.IImage.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(OnImageEvent);
            }
        }

        public virtual void Replace(DIBase target)
        {
            lock (lockObject)
            {
                Uri oldUri = Uri;
                CIAT.SaveFile.Replace(this, target);
                Save();
                target.Replaced = true;
                target.Dispose();
                CIAT.SaveFile.DeletePart(oldUri);
            }
        }

        protected virtual void OnImageEvent(Images.ImageEvent evt, Images.IImageMedia img, object arg)
        {
            if (IsDisposed)
                return;
            if ((evt == Images.ImageEvent.LoadedFromDisk) && Interlocked.Equals(InvalidationState, InvalidationStates.CacheInvalidationQueued))
                ScheduleInvalidation();
            if ((evt == Images.ImageEvent.Updated) || (evt == Images.ImageEvent.Initialized))
            {
                try
                {
                    var prs = CIAT.SaveFile.GetRelationshipsByType(Uri, BaseType, typeof(DIBase), "owned-by");
                    foreach (PackageRelationship pr in prs)
                    {
                        DIBase di = CIAT.SaveFile.GetDI(pr.TargetUri);
                        if (di != null)
                            di.ScheduleInvalidation();
                        else
                            CIAT.SaveFile.DeletePart(pr.TargetUri);
                    }
                }
                catch (InvalidOperationException) { }
            }
            else if (evt == Images.ImageEvent.Resized)
            {
                AbsoluteBounds = (Rectangle)arg;
            }
            Validate();
        }

        public virtual bool Resize()
        {
            if (this.IImage == null)
                return false;
            if (Type.IsGenerated)
                ScheduleInvalidation();
            else if (((this.IImage.OriginalSize.Width > BoundingSize.Width) || (this.IImage.OriginalSize.Height > BoundingSize.Height)) &&
                (this.IImage.Size.Width != BoundingSize.Width) || (this.IImage.Size.Height != BoundingSize.Height))
            {
                this.IImage.Resize(BoundingSize);
                return true;
            }
            return false;
        }


        public void AddOwner(Uri ownerUri)
        {
            foreach (PackageRelationship pr in CIAT.SaveFile.GetRelationshipsByType(Uri, BaseType, typeof(DIBase), "owned-by").ToList())
                if (pr.TargetUri.Equals(ownerUri))
                    return;
            foreach (PackageRelationship pr in CIAT.SaveFile.GetRelationshipsByType(Uri, BaseType, typeof(DIBase), "owns").ToList())
                if (pr.TargetUri.Equals(ownerUri))
                    return;
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), Uri, ownerUri, "owned-by");
            CIAT.SaveFile.CreateRelationship(typeof(DIBase), BaseType, ownerUri, Uri, "owns");
        }

        public void CopyOwners(Uri srcUri, Uri destUri)
        {
            var owningUris = CIAT.SaveFile.GetRelationshipsByType(srcUri, typeof(DIBase), typeof(DIBase), "owned-by").Select(pr => pr.SourceUri).ToList();
            foreach (var u in owningUris)
            {
                CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), destUri, u, "owned-by");
                CIAT.SaveFile.CreateRelationship(typeof(DIBase), BaseType, u, destUri, "owns");
            }
        }

        public void ReleaseOwner(Uri ownerUri)
        {
            if (!CIAT.SaveFile.PartExists(ownerUri))
                return;
            CIAT.SaveFile.DeleteRelationship(ownerUri, Uri);
            CIAT.SaveFile.DeleteRelationship(Uri, ownerUri);
        }

        public void ReleaseSubject(Uri subjectUri)
        {
            if (!CIAT.SaveFile.PartExists(subjectUri))
                return;
            CIAT.SaveFile.DeleteRelationship(Uri, subjectUri);
            CIAT.SaveFile.DeleteRelationship(subjectUri, Uri);
        }

        public abstract object Clone();

        public virtual void Dispose()
        {
            lock (lockObject)
            {
                if (IsDisposed)
                    return;
                if (!Replaced)
                {
                    List<Uri> owns = CIAT.SaveFile.GetRelationshipsByType(Uri, BaseType, typeof(DIBase), "owns").Select(pr => pr.TargetUri).ToList();
                    foreach (Uri u in owns)
                        ReleaseSubject(Uri);
                    List<Uri> owners = CIAT.SaveFile.GetRelationshipsByType(Uri, BaseType, typeof(DIBase), "owned-by").Select(pr => pr.TargetUri).ToList();
                    foreach (Uri u in owners)
                        ReleaseOwner(u);
                    CIAT.SaveFile.DeletePart(this.Uri);
                }
            }
            if (Type != DIType.Null)
            {
                if (this.IImage != null)
                    this.IImage.Dispose();
            }
            IsDisposed = true;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, Uri);
        }

        private static DIBase diNull = null;
        public static DIBase DINull
        {
            get
            {
                if (diNull == null)
                    diNull = new DINull();
                return diNull;
            }
        }
    }

    public class DINull : DIBase
    {
        public override Images.IImage IImage
        {
            get
            {
                if (base.IImage.Img == null)
                {
                    var bmp = new Bitmap(10, 10);
                    Color c = Color.Black;
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        Brush br = new SolidBrush(c);
                        g.FillRectangle(Brushes.Black, new Rectangle(new Point(0, 0), new Size(10, 10)));
                        br.Dispose();
                    }
                    bmp.MakeTransparent(c);
                    base.IImage.Img = bmp;
                }
                return base.IImage;
            }
            protected set
            {
                base.IImage = value;
            }
        }

        public Action ValidateData { get; set; }

        public DINull()
        {
        }

        public DINull(Uri uri)
            : base()
        {
        }

        protected override void Invalidate() { Validate(); }

        public void AddKeyOwner()
        {

        }

        public String Description
        {
            get
            {
                return String.Empty;
            }
        }

        public IImageDisplay ThumbnailPreviewPanel { get; set; }


        protected override void DoLoad(Uri uri)
        {
        }

        public override void Save()
        {
        }

        public override object Clone()
        {
            return this;
        }

        public override void Dispose()
        {
        }
    }
}
