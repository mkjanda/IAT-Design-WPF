using java.awt.print;
using System;
using System.Collections.Generic;
using System.Drawing;
using IAT.Core.Enumerations;

namespace IAT.Core.Models
{
    public interface IImageMedia : IDisposable, IPackagePart, ICloneable
    {
        System.Drawing.Image Img { get; set; }
        Size Size { get; }
        ImageFormat ImageFormat { get; set; }
        String FileExtension { get; }
        event Action<ImageEvent, IImageMedia, object> Changed;
        void ClearChanged();
        bool IsCached { get; }
        void PauseChangeEvents();
        void ResumeChangeEvents();
        void LoadImage();
        void DisposeOfImage();
    }
}
