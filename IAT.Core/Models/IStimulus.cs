using IAT.Core.Models.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Models
{
    internal interface IStimulus : IDisposable, IThumbnailPreviewable, IPackagePart
    {
        String Description { get; }
        IImage IImage { get; }
        DIType Type { get; }
        IUri IUri { get; }
        void ScheduleInvalidation();
    }
}
