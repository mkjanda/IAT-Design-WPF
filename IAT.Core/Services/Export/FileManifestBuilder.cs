using System;
using System.Collections.Generic;
using System.Security.RightsManagement;
using System.Text;
using IAT.Core.Extensions;
using IAT.Core.Serializable;
using sun.awt.image;

namespace IAT.Core.Services.Export
{
    /// <summary>
    /// Interface for building a file manifest, which is a structured representation of files and their associated metadata. This interface provides 
    /// methods for adding files to the manifest, setting content for specific resources, and managing the overall manifest structure. The FileManifest 
    /// property allows access to the underlying Manifest object, which can be used to retrieve information about the files and their attributes. This 
    /// interface is essential for organizing and managing file resources within the application, particularly when exporting or handling file-related 
    /// operations.
    /// </summary>
    public interface IFileManifestBuilder
    {
        /// <summary>
        /// Adds a file resource to the collection with the specified path, resource identifier, type, MIME type, and
        /// optional content.
        /// </summary>
        /// <param name="path">The relative or absolute path where the file will be stored. Cannot be null or empty.</param>
        /// <param name="resourceType">The type of the file resource to add. Specifies how the file will be categorized.</param>
        /// <param name="mimeType">The MIME type of the file, such as "image/png" or "application/pdf". Cannot be null or empty.</param>
        /// <param name="content">The binary content of the file to add, or null to add a file reference without content.</param>
        void AddFile(Manifest manifest, string path, FileResourceType resourceType, string mimeType, byte[]? content = null);

        /// <summary>
        /// Sets the content for the specified resource identifier.
        /// </summary>
        /// <param name="resourceId">The identifier of the resource whose content is to be set.</param>
        /// <param name="content">The byte array containing the content to assign to the resource. Cannot be null.</param>
        void SetContent(Manifest manifest, int resourceId, byte[] content);
    }

    /// <summary>
    /// Provides methods to build and modify a file manifest by adding files and setting their content.
    /// </summary>
    /// <remarks>This class is intended for internal use to construct and update a Manifest instance with file
    /// entries and associated metadata. It implements the IFileManifestBuilder interface to support manifest creation
    /// workflows.</remarks>
    public class FileManifestBuilder : IFileManifestBuilder
    {
        /// <summary>
        /// Adds a file entry to the file manifest with the specified path, resource identifier, resource type, MIME
        /// type, and optional content.
        /// </summary>
        /// <param name="path">The relative or absolute path where the file will be stored in the manifest. Cannot be null or empty.</param>
        /// <param name="resourceType">The type of the file resource to add. Specifies how the file is categorized within the manifest.</param>
        /// <param name="mimeType">The MIME type of the file, such as "application/pdf" or "image/png". Cannot be null or empty.</param>
        /// <param name="content">The binary content of the file. If null, a default single-byte array is used.</param>
        public void AddFile(Manifest manifest, string path, FileResourceType resourceType, string mimeType, byte[]? content = null)
        {
            manifest.AddFile(new ManifestFile() { Path = path, 
                ResourceId = manifest.Contents.Where(fe => fe is ManifestFile).Count() + 1, 
                ResourceType = resourceType, 
                Size = content?.Length ?? 1, 
                MimeType = mimeType, 
                Content = content ?? new byte[1] { 0x00 }
            });
        }

        /// <summary>
        /// Sets the content and size for the file associated with the specified resource identifier.
        /// </summary>
        /// <remarks>If no file with the specified resource identifier exists, this method performs no
        /// action.</remarks>
        /// <param name="resourceId">The unique identifier of the resource whose file content is to be updated.</param>
        /// <param name="content">The byte array containing the new content to assign to the file. Cannot be null.</param>
        public void SetContent(Manifest manifest, int resourceId, byte[] content)
        {
            var file = manifest.Contents.Where(fe => fe is ManifestFile).Cast<ManifestFile>().Where(mf => mf.ResourceId == resourceId).FirstOrDefault();
            if (file != null)
            {
                file.Content = content;
                file.Size = content.Length;
            }
        }
    }
}
