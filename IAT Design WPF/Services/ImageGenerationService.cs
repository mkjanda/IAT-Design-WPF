using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;

namespace IAT_Design_WPF.Services
{
    public class ImageGenerationService : IImageGenerationService
    {
        public BitmapSource RenderTextToBitmap(DIText di)
        { 
            var foreground = new SolidColorBrush(Color.FromArgb(di.PhraseFontColor.A, di.PhraseFontColor.R, di.PhraseFontColor.G, di.PhraseFontColor.B));
            var typeface = new Typeface(new FontFamily(di.PhraseFontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            var formattedText = new FormattedText(
                di.Phrase,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                di.PhraseFontSize * 96.0 / 72.0,   // convert from points to DIPs   
                foreground,
                VisualTreeHelper.GetDpi(new Window()).PixelsPerDip);  // critical for crisp rendering


            // Create the target bitmap (size based on measured text)
            var width = di.LayoutItem.BoundingRectangle.Width;  
            var height = di.LayoutItem.BoundingRectangle.Height; 
            var bmp = new RenderTargetBitmap((int)di.LayoutItem.BoundingRectangle.Width, (int)di.LayoutItem.BoundingRectangle.Height, 96, 96, PixelFormats.Pbgra32);

            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.DrawText(formattedText, new Point((int)((width - formattedText.Width) / 2), (int)((height - formattedText.Height) / 2)));  // offset for crisp edges
            }

            bmp.Render(visual);

            return bmp;  
        }


        public WriteableBitmap LoadEncodedBytesAsManipulableImage(byte[] encodedBytes)
        {
            if (encodedBytes == null || encodedBytes.Length == 0)
                throw new ArgumentException("Encoded byte array cannot be empty", nameof(encodedBytes));

            BitmapImage bitmapImage;
            using (var stream = new MemoryStream(encodedBytes, false))  // false = do not dispose the byte[] itself
            {
                bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = stream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;   // ← THIS is what makes disposal safe
                bitmapImage.EndInit();   // ← Decoding happens here; stream is now fully consumed

                // Stream can be disposed the moment we exit this block
            }

            // Freeze for thread-safety across your multithreaded service
            if (bitmapImage.CanFreeze)
                bitmapImage.Freeze();

            // Convert to manipulable WriteableBitmap (still BGRA32)
            var writable = new WriteableBitmap(bitmapImage);
            if (writable.Format != PixelFormats.Bgra32 && writable.Format != PixelFormats.Pbgra32)
            {
                writable = new WriteableBitmap(new FormatConvertedBitmap(writable, PixelFormats.Bgra32, null, 0));
            }

            return writable;   // ← Stream was already disposed; everything is now self-contained
        }
    }
}
