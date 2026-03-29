using System;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using System.Text;
using sun.awt.image;

namespace IAT.Core.Models.Enumerations
{
    public abstract record ImageFormat(String name, String mimeType, System.Drawing.Imaging.ImageFormat format)
    {
        public static readonly ImageFormat Jpeg = new ImageFormat("jpeg", "image/jpeg", System.Windows.);
        public static readonly ImageFormat Jpg = new ImageFormat("jpg", "image/jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        public static readonly ImageFormat Tiff = new ImageFormat("tiff", "image/tiff", System.Drawing.Imaging.ImageFormat.Tiff);
        public static readonly ImageFormat Tif = new ImageFormat("tif", "image/tiff", System.Drawing.Imaging.ImageFormat.Tiff);
        public static readonly ImageFormat Png = new ImageFormat("png", "image/png", System.Drawing.Imaging.ImageFormat.Png);
        public static readonly ImageFormat Bmp = new ImageFormat("bmp", "image/bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        private ImageFormat(String ext, String mimeType, System.Drawing.Imaging.ImageFormat format)
        {
            Extension = ext;
            MimeType = mimeType;
            Format = format;
        }
        public String Extension { get; private set; }
        public String MimeType { get; private set; }
        public System.Drawing.Imaging.ImageFormat Format { get; private set; }
        private static IEnumerable<ImageFormat> All = new ImageFormat[] { Jpeg, Jpg, Tiff, Tif, Png, Bmp };
        public static ImageFormat FromExtension(String ext)
        {
            try
            {
                return All.Where(f => f.Extension == ext.ToLower()).First();
            }
            catch (InvalidOperationException ex)
            {
                return Png;
            }
        }
        public static ImageFormat FromMimeType(String mimeType)
        {
            try
            {
                return All.Where(mt => mimeType == mt.MimeType).First();
            }
            catch (InvalidOperationException)
            {
                return Png;
            }
        }
    }
}
