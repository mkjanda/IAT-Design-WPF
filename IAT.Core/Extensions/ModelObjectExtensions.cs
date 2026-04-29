using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using javax.activation;
using sun.awt.image;
using sun.tools.tree;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static IAT.Core.Serializable.FileEntity;
using static javax.xml.ws.soap.AddressingFeature;

namespace IAT.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for working with alternation groups, blocks, trials, and related model objects.
    /// </summary>
    /// <remarks>This static class contains utility methods that extend core model types such as
    /// AlternationGroup, Block, and Trial. The methods facilitate common operations including membership checks,
    /// retrieval of alternates, block and trial management, and keyed direction evaluation. These extensions are
    /// intended to simplify and standardize interactions with model objects in scenarios involving alternation and
    /// trial management.</remarks>
    public static class ModelObjectExtensions
    {

        /// <summary>
        /// Determines whether the specified block contains the given trial.
        /// </summary>
        /// <param name="block">The block to search for the trial. Cannot be null.</param>
        /// <param name="trial">The trial to locate within the block. Cannot be null.</param>
        /// <returns>true if the block contains the specified trial; otherwise, false.</returns>
        public static bool Contains(this Block block, Trial trial) => block.TrialIds.Contains(trial.Id);

        /// <summary>
        /// Determines the effective keyed direction for the specified trial, based on the originating block and the
        /// current block context.
        /// </summary>
        /// <param name="trial">The trial for which to determine the keyed direction. Must not be null.</param>
        /// <param name="block">The current block context used to evaluate the keyed direction. Must not be null.</param>
        /// <returns>A KeyedDirection value representing the direction to use for the trial in the context of the specified
        /// block.</returns>
        public static KeyedDirection GetKeyedDirection(this Trial trial, Block block)
        {
            if (trial.OriginatingBlock == 1)
                return trial.KeyedDirection;
            if ((trial.OriginatingBlock == 2) && (block.BlockNumber >= 2) && (block.BlockNumber <= 4))
                return trial.KeyedDirection;
            return trial.KeyedDirection.Opposite;
        }



        /// <summary>
        /// Compares two version instances and determines their relative order based on release, major, minor, and
        /// trivial components.
        /// </summary>
        /// <remarks>Comparison is performed in descending order, prioritizing the release component,
        /// followed by major, minor, and trivial components. This method is useful for sorting or ordering version
        /// objects from highest to lowest.</remarks>
        /// <param name="v1">The first version to compare.</param>
        /// <param name="v2">The second version to compare.</param>
        /// <returns>A negative integer if v2 is greater than v1; zero if v1 and v2 are equal; a positive integer if v1 is
        /// greater than v2.</returns>
        static public int CompareTo(this Serializable.Version v1, Serializable.Version v2)
        {
            if (v1.Release != v2.Release)
                return v2.Release - v1.Release;
            if (v1.Major != v2.Major)
                return v2.Major - v1.Major;
            if (v1.Minor != v2.Minor)
                return v2.Minor - v1.Minor;
            if (v1.Trivial != v2.Trivial)
                return v2.Trivial - v1.Trivial;
            return 0;
        }

        /// <summary>
        /// Converts the specified version to its string representation in the format 'Release.Major.Minor.Trivial'.
        /// </summary>
        /// <param name="version">The version to convert to a string. Cannot be null.</param>
        /// <returns>A string that represents the version in 'Release.Major.Minor.Trivial' format.</returns>
        public static string ToString(this Serializable.Version version)
        {
            return $"{version.Release}.{version.Major}.{version.Minor}.{version.Trivial}";
        }

        /// <summary>
        /// Sets the RSA key for the specified ResultSet instance using the provided EncryptedRSAKey. The method
        /// initializes the static RSA field of the ResultSet class with the parameters from the provided key.
        /// </summary>
        /// <param name="resultSet">The ResultSet instance for which to set the RSA key.</param>
        /// <param name="key">The EncryptedRSAKey containing the RSA parameters.</param>
        public static void SetRSAKey(this ResultPacket resultSet, EncryptedRSAKey key)
        {
            ResultPacket.rsa = RSA.Create(key.GetRSAParameters());
        }

        /// <summary>
        /// Decrypts and assembles the data contained in the specified result set.
        /// </summary>
        /// <remarks>This method processes each entry in the result set's table of contents, decrypting
        /// the associated data blocks using RSA and the provided metadata. The method returns the combined decrypted
        /// data as a single byte array.</remarks>
        /// <param name="resultSet">The result set containing encrypted data and associated metadata to be processed. Cannot be null.</param>
        /// <returns>A byte array containing the fully decrypted and assembled data from the result set.</returns>
        public static byte[] Process(this ResultPacket resultSet)
        {
            byte[] resultData = Convert.FromBase64String(resultSet.ResultData);
            var memStream = new MemoryStream();
            foreach (var entry in resultSet.TOC)
            {
                byte[] key = new byte[entry.KeyLength];
                byte[] iv = new byte[entry.IVLength];
                Array.Copy(resultData, entry.KeyOffset, key, 0, entry.KeyLength);
                Array.Copy(resultData, entry.IVOffset, iv, 0, entry.IVLength);
                byte[] decryptedKey = ResultPacket.rsa?.Decrypt(key, RSAEncryptionPadding.Pkcs1) ?? Array.Empty<byte>();
                byte[] decryptedIv = ResultPacket.rsa?.Decrypt(iv, RSAEncryptionPadding.Pkcs1) ?? Array.Empty<byte>();
                byte[] encryptedData = new byte[entry.DataLength];
                Array.Copy(resultData, entry.DataOffset, encryptedData, 0, entry.DataLength);
                memStream.Write(resultSet.DecryptData(encryptedData, decryptedKey, decryptedIv));
            }
            return memStream.ToArray();
        }

        /// <summary>
        /// Decrypts the specified cipher data using the DES algorithm and the provided key and initialization vector
        /// (IV).
        /// </summary>
        /// <remarks>The caller is responsible for ensuring that the key and IV are valid for DES
        /// decryption. Supplying an invalid key or IV will result in a cryptographic exception.</remarks>
        /// <param name="resultSet">The result set instance on which this extension method is called. This parameter is not used in the
        /// decryption process.</param>
        /// <param name="cipherBytes">The encrypted data to decrypt, as a byte array.</param>
        /// <param name="key">The secret key to use for DES decryption. Must be 8 bytes in length.</param>
        /// <param name="iv">The initialization vector to use for DES decryption. Must be 8 bytes in length.</param>
        /// <returns>A byte array containing the decrypted data.</returns>
        public static byte[] DecryptData(this ResultPacket resultSet, byte[] cipherBytes, byte[] key, byte[] iv)
        {
            using var des = DES.Create();
            des.Key = key;
            des.IV = iv;
            using var decryptor = des.CreateDecryptor();
            return decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        }

        /// <summary>
        /// Scores the specified IAT response using the D-score algorithm, which calculates a standardized score based on
        /// the response times of the trials. The D-score is commonly used in Implicit Association Tests (IAT) to measure
        /// the strength of associations between concepts.
        /// </summary>
        /// <param name="response">The IAT response to be scored.</param>
        public static void Score(this IATResponse response)
        {
            long LatencySum = 0;
            int nLT300 = 0;
            for (int ctr = 0; ctr < response.Responses.Count; ctr++)
            {
                LatencySum += response.Responses[ctr].ResponseTime;
                if (response.Responses[ctr].ResponseTime < 300)
                    nLT300++;
            }
            if (nLT300 * 10 >= response.Responses.Count)
            {
                response.Score = 0m;
                return;
            }

            var Block3 = new List<TrialResponse>();
            var Block4 = new List<TrialResponse>();
            var Block6 = new List<TrialResponse>();
            var Block7 = new List<TrialResponse>();

            for (int ctr = 0; ctr < response.Responses.Count; ctr++)
            {
                if (response.Responses[ctr].ResponseTime > 10000)
                    continue;

                switch (response.Responses[ctr].BlockNumber)
                {
                    case 3:
                        Block3.Add(response.Responses[ctr]);
                        break;

                    case 4:
                        Block4.Add(response.Responses[ctr]);
                        break;

                    case 6:
                        Block6.Add(response.Responses[ctr]);
                        break;

                    case 7:
                        Block7.Add(response.Responses[ctr]);
                        break;
                }
            }

            var InclusiveSDList1 = new List<TrialResponse>();
            var InclusiveSDList2 = new List<TrialResponse>();

            InclusiveSDList1.AddRange(Block3);
            InclusiveSDList1.AddRange(Block6);
            double mean = InclusiveSDList1.Select(r => r.ResponseTime).Average();
            double sd3_6 = Math.Sqrt(InclusiveSDList1.Select(r => (double)r.ResponseTime).Aggregate<double, double>(0, (sd, rt) => sd + Math.Pow(rt - mean, 2)) / (double)(InclusiveSDList1.Count - 1));

            InclusiveSDList2.AddRange(Block4);
            InclusiveSDList2.AddRange(Block7);
            mean = InclusiveSDList2.Select(r => r.ResponseTime).Average();
            double sd4_7 = Math.Sqrt(InclusiveSDList2.Select(r => (double)r.ResponseTime).Aggregate<double, double>(0, (sd, rt) => sd + Math.Pow(rt - mean, 2)) / (double)(InclusiveSDList2.Count - 1));

            double mean3 = Block3.Select(r => r.ResponseTime).Average();
            double mean4 = Block4.Select(r => r.ResponseTime).Average();
            double mean6 = Block6.Select(r => r.ResponseTime).Average();
            double mean7 = Block7.Select(r => r.ResponseTime).Average();

            response.Score = (decimal)((((mean6 - mean3) / sd3_6) + ((mean7 - mean4) / sd4_7)) / 2);
        }

        /// <summary>
        /// Sets the name of the manifest file and updates its MIME type based on the file extension.   
        /// </summary>
        /// <remarks>If the file extension is recognized as ".jpg", ".jpeg", ".png", ".xml", or ".txt",
        /// the corresponding MIME type is set. If the extension is not recognized or missing, the MIME type defaults to
        /// "text/xml".</remarks>
        /// <param name="mf">The manifest file instance whose name and MIME type are to be set.</param>
        /// <param name="name">The new name to assign to the manifest file. The file extension determines the MIME type. Cannot be null.</param>
        public static void SetName(this ManifestFile mf, string name)
        {
            int ndx = name.LastIndexOf('.');
            if (ndx > 0)
            {
                switch (name.Substring(ndx + 1))
                {
                    case "jpg":
                        mf.MimeType = "image/jpeg";
                        break;

                    case "jpeg":
                        mf.MimeType = "image/jpeg";
                        break;

                    case "png":
                        mf.MimeType = "image/png";
                        break;

                    case "xml":
                        mf.MimeType = "text/xml";
                        break;

                    case "txt":
                        mf.MimeType = "text/plain";
                        break;
                }
            }
            else
                mf.MimeType = "text/xml";
            mf._Name = name;
        }

        /// <summary>
        /// Adds a file to the specified manifest directory, updating the path of the added file    
        /// </summary>
        /// <param name="md">The manifest directory the file is to be added to</param>
        /// <param name="mf">The manifest file to add</param>
        public static void AddFile(this ManifestDirectory md, ManifestFile mf)
        {
            mf.Path = md.Path + Path.PathSeparator + mf._Name;
            md.Contents.Add(mf);
        }

        /// <summary>
        /// Adds a collection of manifest files to the specified manifest directory.
        /// </summary>
        /// <param name="md">The manifest directory to which the files will be added. Cannot be null.</param>
        /// <param name="files">The collection of manifest files to add. Cannot be null and must not contain null elements.</param>
        public static void AddFiles(this ManifestDirectory md, IEnumerable<ManifestFile> files)
        {
            foreach (var file in files)
            {
                AddFile(md, file);
            }
        }

        /// <summary>
        /// Adds a child directory to the specified parent directory and updates the child's path accordingly.
        /// </summary>
        /// <remarks>The child's Path property is updated to reflect its new location within the parent
        /// directory before it is added to the parent's Contents collection.</remarks>
        /// <param name="parent">The parent directory to which the child directory will be added. Cannot be null.</param>
        /// <param name="child">The child directory to add to the parent directory. Cannot be null. The child's Path and Name properties
        /// must be set appropriately.</param>
        public static void AddDirectory(this ManifestDirectory parent, ManifestDirectory child)
        {
            child.Path = parent.Path + Path.PathSeparator + child.Name;
            parent.Contents.Add(child);
        }

        /// <summary>
        /// Removes the first file with the specified filename from the directory or any of its subdirectories.
        /// </summary>
        /// <remarks>This method searches recursively through all subdirectories of the specified
        /// directory. Only the first matching file encountered is removed. If multiple files share the same filename in
        /// different locations, only the first match is affected.</remarks>
        /// <param name="parent">The root directory to search for the file to remove. Must not be null.</param>
        /// <param name="filename">The path or name of the file to remove. Comparison is case-sensitive.</param>
        /// <returns>true if a file matching the specified filename was found and removed; otherwise, false.</returns>
        public static bool RemoveFile(this ManifestDirectory parent, string filename)
        {
            for (int ctr = 0; ctr < parent.Contents.Count; ctr++)
            {
                if (parent.Contents[ctr].FileEntityType == FileEntity.EFileEntityType.File)
                {
                    if (parent.Contents[ctr].Path == filename)
                    {
                        parent.Contents.RemoveAt(ctr);
                        return true;
                    }
                }
                else
                {
                    if (parent.Contents[ctr].Path == filename)
                    {
                        parent.Contents.RemoveAt(ctr);
                        return true;
                    }
                    return ((ManifestDirectory)parent.Contents[ctr]).RemoveFile(filename);
                }
            }
            return false;
        }

        /// <summary>
        /// Recursively counts the total number of file entities contained within the specified file entity, including
        /// all nested entities.
        /// </summary>
        /// <remarks>If the specified file entity is a directory, this method includes all entities in its
        /// hierarchy. For non-directory entities, the method returns 1.</remarks>
        /// <param name="fe">The file entity to count. If the entity is a directory, all contained entities are included in the count.</param>
        /// <returns>The total number of file entities contained within the specified file entity. Returns 1 if the entity is not
        /// a directory.</returns>
        public static int GetNumEntities(this FileEntity fe)
        {
            int nEntities = 0;
            if (fe.FileEntityType == EFileEntityType.Directory)
            {
                (fe as ManifestDirectory)?.Contents.ForEach(fe2 =>
                {
                    nEntities += fe2.GetNumEntities();
                });
                return nEntities;
            } else 
                return 1;
        }

    }
}


