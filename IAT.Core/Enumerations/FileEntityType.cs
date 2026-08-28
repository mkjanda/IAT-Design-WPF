using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace IAT.Core.Enumerations
{

    /// <summary>
    /// Specifies the type of a file system entity, such as a file or a directory.
    /// </summary>
    /// <remarks>Use this enumeration to distinguish between files and directories when working with
    /// file system operations. This can help determine the appropriate handling or processing logic based on the
    /// entity type.</remarks>
    public enum FileEntityType
    {
        /// <summary>
        /// The entity is a file
        /// </summary>
        [Description("File")]
        File,

        /// <summary>
        /// The entity is a directory
        /// </summary>
        [Description("Directory")]
        Directory
    }
}
