using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using sun.awt.image;
using sun.tools.tree;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        /// <returns>The D-score representing the strength of associations in the IAT response.</returns>
        public static double Score(this IATResponse response)
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
                return double.NaN;
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

            return (((mean6 - mean3) / sd3_6) + ((mean7 - mean4) / sd4_7)) / 2;
        }
    }
}


