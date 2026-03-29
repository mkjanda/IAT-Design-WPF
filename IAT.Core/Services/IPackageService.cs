using IAT.Core.Models;
using IAT.Core.Models.Enumerations;
using System.IO;
using System.Threading.Tasks;

namespace IAT.Core.Services
{
    /// <summary>
    /// Central service that owns the OPC Package (the .iat file) and all file I/O.
    /// This replaces the old static CIAT.SaveFile god-class. Thread-safe, injectable, testable.
    /// </summary>
    public interface IPackageService
    {
        /// <summary>
        /// Opens (or creates) the package file and returns the root CIAT model.
        /// </summary>
        Task<CIAT> LoadPackageAsync(string packagePath);

        /// <summary>
        /// Retrieves a block by URI with internal caching.
        /// </summary>
        Models.IATBlock GetBlock(Uri uri);

        /// <summary>
        /// Returns a synchronized read stream for any package part.
        /// </summary>
        Stream GetReadStream(IPackagePart part);

        /// <summary>
        /// Flushes all changes to disk.
        /// </summary>
        void SaveChanges();
    }
}