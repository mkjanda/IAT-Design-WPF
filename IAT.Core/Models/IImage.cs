using IAT.Core.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace IAT.Core.Models
{
    public interface IImage : IImageMedia, ICloneable
    {
        void CreateThumbnail();
        void Resize(Size sz);
        Size OriginalSize { get; }
        Rectangle AbsoluteBounds { get; set; }
        IImageMedia OriginalImage { get; }
        IImageMedia Thumbnail { get; }
        void Load(Uri uri);
        DIType DIType { get; }
        new System.Drawing.Image Image { get; set; }
        ImageMetaData MetaData { get; }

    }
}

