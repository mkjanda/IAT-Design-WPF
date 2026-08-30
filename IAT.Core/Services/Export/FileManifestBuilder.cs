using IAT.Core.Enumerations;
using IAT.Core.Serializable;

namespace IAT.Core.Services.Export
{
    /// <summary>
    /// Builds a file manifest by adding files and setting their content.
    /// </summary>
    public interface IFileManifestBuilder
    {
        /// <summary>
        /// Adds a file resource with an explicit resource identifier so display-item IDs and GFile.ResourceId stay aligned.
        /// </summary>
        void AddFile(Manifest manifest, string path, int resourceId, ResourceType resourceType, string mimeType, byte[]? content = null);

        /// <summary>
        /// Sets the content for the specified resource identifier.
        /// </summary>
        void SetContent(Manifest manifest, int resourceId, byte[] content);
    }

    /// <summary>
    /// Constructs and updates a <see cref="Manifest"/> with file entries and associated metadata.
    /// </summary>
    public class FileManifestBuilder : IFileManifestBuilder
    {
        /// <inheritdoc />
        public void AddFile(Manifest manifest, string path, int resourceId, ResourceType resourceType, string mimeType, byte[]? content = null)
        {
            ArgumentNullException.ThrowIfNull(manifest);
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path is required.", nameof(path));

            var bytes = content ?? new byte[] { 0x00 };
            manifest.Files.Add(new ManifestFile
            {
                Name = path,
                Path = path,
                ResourceId = resourceId,
                ResourceType = resourceType,
                Size = bytes.Length,
                MimeType = mimeType,
                Content = bytes
            });
        }

        /// <inheritdoc />
        public void SetContent(Manifest manifest, int resourceId, byte[] content)
        {
            ArgumentNullException.ThrowIfNull(manifest);
            ArgumentNullException.ThrowIfNull(content);
            var file = manifest.Files.FirstOrDefault(mf => mf.ResourceId == resourceId);
            if (file != null)
            {
                file.Content = content;
                file.Size = content.Length;
            }
        }
    }
}
