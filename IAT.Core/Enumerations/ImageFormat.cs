using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace IAT.Core.Enumerations
{
    /// <summary>
    /// Represents an image file format, including its name, MIME type, and a factory for creating an encoder instance.
    /// </summary>
    /// <remarks>Use the predefined static properties, such as Jpeg or Png, to access common image formats.
    /// The record provides methods to resolve an image format by file extension or MIME type. Custom formats can be
    /// created by deriving from this record if needed.</remarks>
    /// <param name="Name">The file extension or short name that identifies the image format (for example, "png" or "jpeg").</param>
    /// <param name="MimeType">The MIME type associated with the image format (for example, "image/png").</param>
    /// <param name="EncoderFactory">A factory function that creates a new encoder instance for this image format.</param>
    public abstract record ImageFormat(String Name, String MimeType, Func<BitmapEncoder> EncoderFactory)
    {
        /// <summary>
        /// Represents the JPEG image format with the MIME type "image/jpeg".
        /// </summary>
        /// <remarks>Use this field to specify or identify JPEG images when working with image processing
        /// APIs that require an explicit image format. JPEG is a commonly used lossy compression format suitable for
        /// photographs and web images.</remarks>
        public static readonly ImageFormat Jpeg = new _Jpeg("jpeg", "image/jpeg", () => new JpegBitmapEncoder());

        /// <summary>
        /// Represents the JPEG image format with the MIME type "image/jpeg".
        /// </summary>
        /// <remarks>Use this field to specify or identify JPEG images when working with image processing
        /// APIs that support multiple formats.</remarks>
        public static readonly ImageFormat Jpg = new _Jpg("jpg", "image/jpeg", () => new JpegBitmapEncoder());

        /// <summary>
        /// Represents the TIFF image format with the MIME type "image/tiff".
        /// </summary>
        /// <remarks>Use this field to specify or identify TIFF images when working with image encoding or
        /// decoding operations. TIFF is a flexible raster image format commonly used for high-quality graphics and
        /// document imaging.</remarks>
        public static readonly ImageFormat Tiff = new _Tiff("tiff", "image/tiff", () => new TiffBitmapEncoder());

        /// <summary>
        /// Gets the image format for Tagged Image File Format (TIFF) images.
        /// </summary>
        /// <remarks>Use this format to read or write images in the TIFF format, which supports multiple
        /// pages and high color depth. TIFF is commonly used for archiving and professional imaging
        /// applications.</remarks>
        public static readonly ImageFormat Tif = new _Tif("tif", "image/tiff", () => new TiffBitmapEncoder());

        /// <summary>
        /// Gets the image format for Portable Network Graphics (PNG) files.
        /// </summary>
        /// <remarks>Use this format to encode or decode images in the PNG format, which supports lossless
        /// compression and transparency. The associated MIME type is "image/png".</remarks>
        public static readonly ImageFormat Png = new _Png("png", "image/png", () => new PngBitmapEncoder());

        /// <summary>
        /// Represents the BMP (Bitmap) image format, identified by the MIME type "image/bmp".
        /// </summary>
        /// <remarks>Use this format to encode or decode images in the standard BMP file format. BMP is a
        /// widely supported, uncompressed raster image format commonly used for simple graphics and compatibility with
        /// legacy systems.</remarks>
        public static readonly ImageFormat Bmp = new _Bmp("bmp", "image/bmp", () => new BmpBitmapEncoder());

        /// <summary>
        /// Represents the JPEG image format, including its name, MIME type, and encoder factory.
        /// </summary>
        /// <remarks>This type is used internally to provide JPEG-specific information and encoder
        /// creation within the image processing framework. It is a sealed record and is not intended to be instantiated
        /// directly by consumers.</remarks>
        /// <param name="name">The name of the JPEG image format. This value is used to identify the format.</param>
        /// <param name="MimeType">The MIME type associated with the JPEG image format. For example, "image/jpeg".</param>
        /// <param name="EncoderFactory">A factory function that creates an encoder for the JPEG image format. The function should return a new
        /// instance of an encoder each time it is called.</param>
        private sealed record _Jpeg(string name, string MimeType, Func<BitmapEncoder> EncoderFactory) : ImageFormat(name, MimeType, EncoderFactory);
        
        /// <summary>
        /// Represents the JPEG image format, including its name, MIME type, and a factory for creating image encoders.
        /// </summary>
        /// <param name="name">The name of the image format. Typically set to "JPEG" or a similar identifier.</param>
        /// <param name="MimeType">The MIME type associated with the JPEG format, such as "image/jpeg".</param>
        /// <param name="EncoderFactory">A factory function that creates instances of an image encoder for the JPEG format.</param>
        private sealed record _Jpg(string name, string MimeType, Func<BitmapEncoder> EncoderFactory) : ImageFormat(name, MimeType, EncoderFactory);

        /// <summary>
        /// Represents the TIFF image format, including its name, MIME type, and a factory for creating image encoders.
        /// </summary>
        /// <param name="name">The name of the TIFF image format. This value is used to identify the format.</param>
        /// <param name="MimeType">The MIME type associated with the TIFF image format. For example, "image/tiff".</param>
        /// <param name="EncoderFactory">A factory function that creates instances of image encoders for the TIFF format.</param>
        private sealed record _Tiff(string name, string MimeType, Func<BitmapEncoder> EncoderFactory) : ImageFormat(name, MimeType, EncoderFactory);

        /// <summary>
        /// Represents the TIFF image format, including its name, MIME type, and encoder factory.
        /// </summary>
        /// <param name="name">The name of the image format. Typically 'TIFF' for this format.</param>
        /// <param name="MimeType">The MIME type associated with the TIFF image format. For example, 'image/tiff'.</param>
        /// <param name="EncoderFactory">A factory function that creates an encoder for the TIFF image format.</param>
        private sealed record _Tif(string name, string MimeType, Func<BitmapEncoder> EncoderFactory) : ImageFormat(name, MimeType, EncoderFactory);

        /// <summary>
        /// Represents the PNG image format, including its name, MIME type, and encoder factory.
        /// </summary>
        /// <param name="name">The name of the image format. Typically 'PNG'.</param>
        /// <param name="MimeType">The MIME type associated with the PNG format, such as 'image/png'.</param>
        /// <param name="EncoderFactory">A factory function that creates an encoder for PNG images.</param>
        private sealed record _Png(string name, string MimeType, Func<BitmapEncoder> EncoderFactory) : ImageFormat(name, MimeType, EncoderFactory);

        /// <summary>
        /// Represents the BMP image format, including its name, MIME type, and a factory for creating image encoders.
        /// </summary>
        /// <param name="name">The name of the image format. Typically set to "BMP".</param>
        /// <param name="MimeType">The MIME type associated with the BMP image format. For example, "image/bmp".</param>
        /// <param name="EncoderFactory">A factory function that creates an encoder for the BMP image format. Must not be null.</param>
        private sealed record _Bmp(string name, string MimeType, Func<BitmapEncoder> EncoderFactory) : ImageFormat(name, MimeType, EncoderFactory);

        private static ImageFormat FromExtension(String ext) =>
            ext.ToLowerInvariant() switch
            {
                "jpeg" => Jpeg,
                "jpg" => Jpg,
                "tiff" => Tiff,
                "tif" => Tif,
                "png" => Png,
                "bmp" => Bmp,
                _ => throw new ArgumentException($"Unsupported image format extension: {ext}")
            };

        private static ImageFormat FromMimeType(String mimeType) =>
            mimeType.ToLowerInvariant() switch
            {
                "image/jpeg" => Jpeg,
                "image/tiff" => Tiff,
                "image/png" => Png,
                "image/bmp" => Bmp,
                _ => throw new ArgumentException($"Unsupported image format MIME type: {mimeType}")
            };
    }
}
