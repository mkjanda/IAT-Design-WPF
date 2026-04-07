using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;

namespace IAT.Core.Models
{
    /// <summary>
    /// Defines the contract for a part within a package, providing access to its unique identifier and type
    /// information.
    /// </summary>
    /// <remarks>Implementations of this interface represent individual parts within a package structure, such
    /// as files or resources. The interface extends IDisposable, indicating that implementations may hold unmanaged
    /// resources that should be released when no longer needed.</remarks>
    public interface IPackagePart 
    {
        /// <summary>
        /// Gets or sets the URI that uniquely identifies this package part within the package.
        /// </summary>
        Uri? Uri { get; }

        /// <summary>
        /// Gets the type of the package part represented by this instance.
        /// </summary>
        PartType PackagePartType { get; }

        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        Guid Id { get; set; }
    }
}
