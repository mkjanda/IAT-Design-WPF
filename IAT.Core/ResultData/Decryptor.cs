using Konscious.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;
using IAT.Core.ResultData;
using IAT.Core.Models;
using System.Xml.Serialization;
using sun.security.util;
using System.Windows.Media.Converters;

namespace IAT.Core.ResultData
{
    /// <summary>
    /// Defines an interface for decrypting encrypted text using a specified key and returning a ResultSet object.
    /// </summary>
    public interface IDecryptor
    {
        /// <summary>
        /// Decrypts the provided encrypted text using the specified key and returns a ResultSet object.
        /// </summary>
        /// <param name="encryptedText">The encrypted text to decrypt.</param>
        /// <param name="key">The key to use for decryption.</param>
        /// <returns>A ResultSet object containing the decrypted data.</returns>
        ResultSet DecryptResultSet(EncryptedResultSet encryptedResultSet, RSA rsa);
        RSA GetRSA(string password);
        void Generate(string password, bool storePassword = false);
        bool TestPassword(string password);

    }


    /// <summary>
    /// Provides functionality to decrypt RSA keys and encrypted result sets using a password-based key derivation function (Argon2id) and AES-GCM encryption.
    /// </summary>
    public class Decryptor : IDecryptor
    {
        private readonly TransactionState _state;
        private readonly RsaParams _params;

        /// <summary>
        /// Initializes a new instance of the Decryptor class with the specified TransactionState.
        /// </summary>
        /// <param name="state"></param>
        public Decryptor(TransactionState state)
        {
            _state = state;
            _params = state.TestResults.Descriptor.RsaParams;
        }


        static byte[] DeriveAesKey(string password, byte[] salt, int keyBytes = 32)
        {
            var argon = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 4,   // lanes
                MemorySize = 64 * 1024,    // 64 MB — tune for your clients
                Iterations = 3
            };
            return argon.GetBytes(keyBytes);
        }

        private byte[] EncryptKey(string password, byte[] plaintext)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);   // store with ciphertext
            byte[] key = DeriveAesKey(password, salt);
            byte[] nonce = RandomNumberGenerator.GetBytes(12);  // AES-GCM
            byte[] tag = new byte[16];
            byte[] ciphertext = new byte[plaintext.Length];

            var aesGcm = new AesGcm(key, 16);

            aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
            var memStream = new MemoryStream();
            memStream.Write(salt);
            memStream.Write(nonce);
            memStream.Write(ciphertext);
            memStream.Write(tag);

            _params.Salt = Convert.ToBase64String(salt);
            _params.Nonce = Convert.ToBase64String(nonce);
            _params.Tag = Convert.ToBase64String(tag);
            _params.EncRSAParams = Convert.ToBase64String(ciphertext);
            return memStream.ToArray();
        }

        private RSAParameters DecryptKey(byte[] aesKey)
        {
            byte[] salt = Convert.FromBase64String(_params.Salt);
            byte[] ciphertext = Convert.FromBase64String(_params.EncRSAParams);
            byte[] nonce = Convert.FromBase64String(_params.Nonce);
            byte[] tag = Convert.FromBase64String(_params.Tag);
            byte[] plainText = new byte[ciphertext.Length];
            byte[] cipherbytes = aesKey;
            var aes = new AesGcm(cipherbytes, 16);
            byte[] plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            var memStream = new MemoryStream(plaintext);
            BinaryReader bReader = new BinaryReader(memStream);
            int len = bReader.ReadInt32();
            byte[] n = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            byte[] e = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            byte[] d = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            byte[] p = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            byte[] q = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            byte[] dp = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            byte[] dq = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            byte[] inverseQ = bReader.ReadBytes(len);
            return new RSAParameters()
            {
                Modulus = n,
                Exponent = e,
                D = d,
                P = p,
                Q = q,
                DP = dp,
                DQ = dq,
                InverseQ = inverseQ
            };
        }

        /// <summary>
        /// Generates and encrypts a new RSA key pair using the specified password as the encryption key.
        /// </summary>    /// <remarks>The generated RSA key parameters are encrypted with DES using the key and IV derived from the
        /// provided password. The encrypted key and public key components are stored in corresponding fields. The password
        /// must be in the expected format; otherwise, the method may fail.</remarks>
        /// <param name="test">A string parameter for testing purposes.</param>
        /// <param name="password">A string containing the password in the format 'secret:XX-XX-...-XX', where each 'XX' is a hexadecimal byte.
        /// Used to derive the DES encryption key and initialization vector.</param>
        /// <param name="storePassword">A boolean indicating whether to store the password in local storage.</param>
        public void Generate(string password, bool storePassword = false)
        {
            RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
            RSAParameters rsaParams = rsaCrypt.ExportParameters(true);
            byte[] n = rsaParams.Modulus ?? throw new InvalidOperationException("Null RSA Parameter");
            byte[] e = rsaParams.Exponent ?? throw new InvalidOperationException("Null RSA Parameter");
            byte[] d = rsaParams.D ?? throw new InvalidOperationException("Null RSA Parameter");
            byte[] p = rsaParams.P ?? throw new InvalidOperationException("Null RSA Parameter");
            byte[] q = rsaParams.Q ?? throw new InvalidOperationException("Null RSA Parameter");
            byte[] dp = rsaParams.DP ?? throw new InvalidOperationException("Null RSA Parameter");
            byte[] dq = rsaParams.DQ ?? throw new InvalidOperationException("Null RSA Parameter");
            byte[] inverseQ = rsaParams.InverseQ ?? throw new InvalidOperationException("Null RSA Parameter");
            _params.Modulus = Convert.ToBase64String(n);
            _params.Exponent = Convert.ToBase64String(e);
            MemoryStream memStream = new MemoryStream();
            BinaryWriter bWriter = new BinaryWriter(memStream);
            bWriter.Write(n?.Length ?? 0);
            bWriter.Write(n ?? Array.Empty<byte>());
            bWriter.Write(e?.Length ?? 0);
            bWriter.Write(e ?? Array.Empty<byte>());
            bWriter.Write(d?.Length ?? 0);
            bWriter.Write(d ?? Array.Empty<byte>());
            bWriter.Write(p?.Length ?? 0);
            bWriter.Write(p ?? Array.Empty<byte>());
            bWriter.Write(q?.Length ?? 0);
            bWriter.Write(q ?? Array.Empty<byte>());
            bWriter.Write(dp?.Length ?? 0);
            bWriter.Write(dp ?? Array.Empty<byte>());
            bWriter.Write(dq?.Length ?? 0);
            bWriter.Write(dq ?? Array.Empty<byte>());
            bWriter.Write(inverseQ?.Length ?? 0);
            bWriter.Write(inverseQ ?? Array.Empty<byte>());
            bWriter.Flush();
            _params.EncRSAParams = Convert.ToBase64String(EncryptKey(password, memStream.ToArray()));
        }


        /// <summary>
        /// Tests whether the provided password can successfully decrypt the RSA key and perform a test encryption/decryption operation.
        /// </summary>
        /// <param name="password">The password to test for decrypting the RSA key.</param>
        /// <returns>True if the password can successfully decrypt the RSA key and perform the test operation; otherwise, false.</returns>
        public bool TestPassword(string password)
        {
            try
            {
                BigInteger modulus = new BigInteger(Convert.FromBase64String(_params.Modulus));
                BigInteger exponent = new BigInteger(Convert.FromBase64String(_params.Exponent));
                byte[] encRsaBytes = Convert.FromBase64String(_params.EncRSAParams);
                var Rsa = RSA.Create(DecryptKey(DeriveAesKey(password, Convert.FromBase64String(_params.Salt))));
                var dataStream = new MemoryStream();
                dataStream.Write(new byte[1] { 0 });
                byte[] testData = System.Text.Encoding.UTF8.GetBytes("ZippyTheWorldTortoise");
                dataStream.Write(testData, 0, testData.Length);
                BigInteger data = new BigInteger(dataStream.ToArray());
                BigInteger encryptedData = BigInteger.ModPow(data, exponent, modulus);
                var encryptedBytes = data.ToByteArray();
                Rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
                return Convert.ToBase64String(encryptedBytes) == Convert.ToBase64String(testData);
            }
            catch
            {
                // Wrong password (or corrupt key material) often decrypts to garbage that still
                // parses as bytes. Never leave that state visible to the rest of the pipeline.
                return false;
            }
        }

        /// <summary>
        /// Decrypts the RSA key using the provided password and returns an RSA object initialized with the decrypted key parameters.
        /// </summary>
        /// <param name="password">The password to use for decrypting the RSA key.</param>
        /// <returns>An RSA object initialized with the decrypted key parameters.</returns>
        public RSA GetRSA(string password)
        {
            byte[] salt = Convert.FromBase64String(_params.Salt);
            var aesBytes = DeriveAesKey(password, salt);
            var aes = new AesGcm(aesBytes, 16);
            return RSA.Create(DecryptKey(aesBytes));
        }

        /// <summary>
        /// Decrypts the provided EncryptedResultSet using the specified RSA object and returns a ResultSet object containing the decrypted data.
        /// </summary>
        /// <param name="encResults">The encrypted result set to decrypt.</param>
        /// <param name="rsa">The RSA object to use for decryption.</param>
        /// <returns>A ResultSet object containing the decrypted data.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the deserialized ResultSet is null.</exception>
        public ResultSet DecryptResultSet(EncryptedResultSet encResults, RSA rsa)
        {
            var cipherBytes = Convert.FromBase64String(encResults.Results);
            var nonce = Convert.FromBase64String(encResults.Nonce);
            var tag = Convert.FromBase64String(encResults.Tag);
            var aesKey = rsa.Decrypt(Convert.FromBase64String(encResults.EncryptedCipher), RSAEncryptionPadding.Pkcs1);
            var aes = new AesGcm(aesKey, 16);
            var plainText = new byte[cipherBytes.Length];
            aes.Decrypt(nonce, cipherBytes, tag, plainText);
            var deserializer = new XmlSerializer(typeof(ResultSet));
            using (var memStream = new MemoryStream(plainText))
                return (ResultSet)(deserializer.Deserialize(memStream) ?? throw new InvalidOperationException("Null encountered while deserializing ResultSet"));
        }


    }
}
