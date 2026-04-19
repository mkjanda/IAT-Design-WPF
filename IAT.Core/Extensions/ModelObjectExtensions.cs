using IAT.Core.Domain;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using sun.awt.image;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        /// Verifies that the decrypted ciphertext from the handshake matches the specified plain text using the
        /// provided RSA key.
        /// </summary>
        /// <remarks>This method performs a fixed-time comparison to help prevent timing attacks. The
        /// comparison is case-sensitive and uses UTF-8 encoding for the plain text.</remarks>
        /// <param name="handshake">The handshake containing the base64-encoded ciphertext to verify. The CipherText property must not be null
        /// or empty.</param>
        /// <param name="rsa">The RSA key used to decrypt the ciphertext. Must be initialized with the appropriate private key.</param>
        /// <param name="PlainText">The plain text string to compare against the decrypted ciphertext. Encoded as UTF-8 for comparison.</param>
        /// <returns>true if the decrypted ciphertext matches the specified plain text; otherwise, false.</returns>
        public static bool Verify(this Handshake handshake)
        {
            var cipherBytes = Convert.FromBase64String(handshake.CipherText ?? string.Empty);
            var plainBytes = Convert.FromBase64String(handshake.PlainText ?? string.Empty);
            var decryptedBytes = Handshake.RSA.Decrypt(cipherBytes, RSAEncryptionPadding.Pkcs1);
            return CryptographicOperations.FixedTimeEquals(decryptedBytes, plainBytes);
        }

        /// <summary>
        /// Initializes the specified handshake with new cryptographic parameters, including a random plaintext, its
        /// encrypted ciphertext, and the public key information.
        /// </summary>
        /// <remarks>This method generates a new 32-byte random plaintext, encrypts it using the
        /// handshake's RSA public key, and sets the handshake's modulus and public key properties. The method
        /// overwrites any existing values in the handshake instance.</remarks>
        /// <param name="handshake">The handshake object to populate with generated cryptographic values. Cannot be null.</param>
        public static void Formulate(this Handshake handshake)
        {
            handshake.Modulus = Convert.ToBase64String(Handshake.RSA.ExportParameters(false).Modulus?.ToArray<byte>() ?? new byte[] { 1 });
            handshake.PublicKey = Convert.ToBase64String(Handshake.RSA.ExportRSAPublicKey());
            handshake.PlainText = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
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
                byte[] decryptedKey = ResultPacket.rsa.Decrypt(key, RSAEncryptionPadding.Pkcs1);
                byte[] decryptedIv = ResultPacket.rsa.Decrypt(iv, RSAEncryptionPadding.Pkcs1);
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
    }
}


