using IAT.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging; 

namespace IAT.Core.Serializable
{
    /// <summary>
    /// Represents the result of extracting a rectangular region from an image, including pixel data and associated
    /// metadata.
    /// </summary>
    /// <param name="Pixels">The array of bytes containing the pixel data for the clipped region. The format and layout are determined by the
    /// specified pixel format and stride.</param>
    /// <param name="Width">The width, in pixels, of the clipped region.</param>
    /// <param name="Height">The height, in pixels, of the clipped region.</param>
    /// <param name="Format">The pixel format used to interpret the pixel data.</param>
    /// <param name="Stride">The number of bytes per row of pixel data, including any padding.</param>
    public record AbsoluteClipResult(byte[] Pixels, int Width, int Height, PixelFormat Format, int Stride);

    /// <summary>
    /// Provides an abstract base class for dynamically generated image objects that support validation, layout
    /// suspension, and resource management.
    /// </summary>
    /// <remarks>DIGenerated manages the lifecycle and validation of generated images, including support for
    /// suspending and resuming layout updates. Instances are tracked for resource management, and the class implements
    /// IDisposable to ensure proper cleanup. Thread safety is maintained for shared resources. Derived classes must
    /// implement the image generation logic by overriding the Generate method.</remarks>
    public abstract class DIGenerated : DIBase, IDisposable
    {
        private readonly static List<DIGenerated> GeneratedItems = new List<DIGenerated>();
        public bool Modified { get; protected set; } = false;
        protected abstract Bitmap Generate();
        private static readonly object GeneratedItemsLock = new object();

        public override bool IsGenerated { get { return true; } }

        public AbsoluteClipResult CalcAbsoluteBounds(BitmapSource source, byte alphaThreshold = 0)
        {
            // Ensure the source is in a known pixel format (BGRA32 or PBGRA32) for consistent processing
            if (source.Format != PixelFormats.Bgra32 && source.Format != PixelFormats.Pbgra32)
            {
                var converted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
                source = converted;
            }

            // Read pixel data into a byte array
            int stride = (source.PixelWidth * source.Format.BitsPerPixel + 7) / 8;
            int size = stride * source.PixelHeight;
            byte[] pixels = new byte[size];
            source.CopyPixels(pixels, stride, 0);

            // Analyze pixel data to find the bounding box of non-transparent pixels
            var bytesPerPixel = source.Format.BitsPerPixel / 8; 
            var minX = source.PixelWidth;
            var minY = source.PixelHeight;
            var maxX = 0;
            var maxY = 0;
            for (int y = 0; y < source.PixelHeight; y++)
            {
                for (int x = 0; x < source.PixelWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;
                    byte alpha = pixels[index + 3]; 
                    if (alpha > alphaThreshold)
                    {
                        if (x < minX) minX = x;
                        if (y < minY) minY = y;
                        if (x > maxX) maxX = x;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            // If no non-transparent pixels were found, return an empty result
            if (maxX < minX || maxY < minY)
            {
                AbsoluteBounds = Rect.Empty;
                return new AbsoluteClipResult(Array.Empty<byte>(), 0, 0, source.Format, stride);
            }

            // Extract the clipped pixel data based on the calculated bounds
            int clipppedWidth = maxX - minX + 1;
            int clipppedHeight = maxY - minY + 1;
            int clippedStride = (clipppedWidth * bytesPerPixel + 3) & ~3;
            byte[] clippedPixels = new byte[clippedStride * clipppedHeight];
            for (int y = 0; y < clipppedHeight; y++)
            {
                var srcRow = (minY + y)  * stride + minX * bytesPerPixel;
                Array.Copy(pixels, srcRow, clippedPixels, y * clippedStride, clipppedWidth * bytesPerPixel);
            }

            // Set the AbsoluteBounds property to the calculated rectangle
            AbsoluteBounds = new Rect(minX, minY, clipppedWidth, clipppedHeight);
            return new AbsoluteClipResult(clippedPixels, clipppedWidth, clipppedHeight, source.Format, clippedStride);
        }


        private ConcurrentQueue<ValidationLock> ValidationLockQueue = new ConcurrentQueue<ValidationLock>();
        public override void LockValidation(ValidationLock validationLock)
        {
            ValidationLockQueue.Enqueue(validationLock);
        }

        protected override void Validate()
        {
            if (ValidationLockQueue.TryDequeue(out ValidationLock result))
                result.Validate(this);
            base.Validate();
        }


        private readonly ManualResetEvent invalidationEntryEvt = new ManualResetEvent(true);
        protected override void Invalidate()
        {
            Task.Run(() =>
            {
                if (!Monitor.TryEnter(this))
                    return;
                try
                {
                    try
                    {
                        invalidationEntryEvt.WaitOne();
                        invalidationEntryEvt.Reset();
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                    if (LayoutSuspended)
                    {
                        Validate();
                        return;
                    }
                    if (ValidationLockQueue.TryPeek(out ValidationLock validationLock))
                        if (!validationLock.DoInvalidation(this))
                            return;
                    Bitmap bmp = Generate();
                    if ((bmp != null) && !IsDisposing && !IsDisposed)
                        IImage.Img = bmp;
                    else
                        Validate();
                }
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(new CReportableException("Error occurred in image generation", ex));
                }
                finally
                {
                    invalidationEntryEvt.Set();
                }
            });
        }

        public bool LayoutSuspended { get; protected set; } = false;
        public void SuspendLayout()
        {
            LayoutSuspended = true;
        }
        public virtual void ResumeLayout(bool invalidate)
        {
            if (!LayoutSuspended)
                return;
            LayoutSuspended = false;
            if (invalidate)
                ScheduleInvalidation();
        }

        public DIGenerated()
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Add(this);
            }
        }

        public DIGenerated(Uri uri) : base(uri)
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Add(this);
            }
        }

        public DIGenerated(Images.IImage img)
            : base(img)
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Add(this);
            }
        }

        protected void StopGenerating()
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Remove(this);
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Remove(this);
            }
            base.Dispose();
        }
    }
}
