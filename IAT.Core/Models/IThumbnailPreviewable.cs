using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models
{
    internal interface IThumbnailPreviewable
    {
        IImage ThumbnailPreview { get; set; }
    }
}
