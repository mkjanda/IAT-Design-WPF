using IAT.Core.Enumerations;
using Konscious.Security.Cryptography;
using MediatR;
using sun.security.util;
using System;
using System.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;
using com.sun.beans.editors;
using IAT.Core.Services;

namespace IAT.Core.Serializable;

/// <summary>
/// Represents a command to process an encrypted RSA key as part of a request that returns a transaction result.
/// </summary>
/// <param name="Key">The encrypted RSA key to be processed. Cannot be null.</param>
public record RSAKeyCommand(EncryptedRSAKey Key) : IRequest<TransactionResult>;

/// <summary>
/// Contains the encrypted RSA key information, including the modulus (n), exponent (e), private exponent (d), prime factors (p and q), and other related parameters.
/// </summary>
public class EncryptedRSAKey : IWebSocketMessage
{
    /// <summary>
    /// The product key
    /// </summary>
    [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
    public string ProductKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the string value associated with the NString XML element.
    /// </summary>
    [XmlElement("Modulus", Form = XmlSchemaForm.Unqualified)]
    public string Modulus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the Exponent XML element.
    /// </summary>
    [XmlElement("Exponent", Form = XmlSchemaForm.Unqualified)]
    public string Exponent { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted key value.
    /// </summary>
    [XmlElement("EncryptedKey", Form = XmlSchemaForm.Unqualified)]
    public string EncryptedKey { get; set; } = string.Empty;

    [XmlIgnore]
    private byte[]? d, e, p, q, n, dp, dq, inverseQ;


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
        return memStream.ToArray();
    }

    private RSAParameters DecryptKey(byte[] cipherbytes, string password)
    {
        var memoryStream = new MemoryStream(cipherbytes);
        byte[] salt = new byte[16], nonce = new byte[12], tag = new byte[16];
        byte[] ciphertext = new byte[cipherbytes.Length - 44];
        var memStream = new MemoryStream(cipherbytes);
        memStream.Read(salt);
        memStream.Read(nonce);
        memStream.Read(ciphertext, 0, cipherbytes.Length - 44);
        memStream.Read(tag);
        byte[] key = DeriveAesKey(password, salt);
        var aes = new AesGcm(key, 16);
        byte[] plaintext = new byte[ciphertext.Length];
        aes.Decrypt(nonce, ciphertext, tag, plaintext);
        memStream.Dispose(); memStream = new MemoryStream(plaintext);
        BinaryReader bReader = new BinaryReader(memStream);
        int len = bReader.ReadInt32();
        n = bReader.ReadBytes(len);
        len = bReader.ReadInt32();
        e = bReader.ReadBytes(len);
        len = bReader.ReadInt32();
        d = bReader.ReadBytes(len);
        len = bReader.ReadInt32();
        p = bReader.ReadBytes(len);
        len = bReader.ReadInt32();
        q = bReader.ReadBytes(len);
        len = bReader.ReadInt32();
        dp = bReader.ReadBytes(len);
        len = bReader.ReadInt32();
        dq = bReader.ReadBytes(len);
        len = bReader.ReadInt32();
        inverseQ = bReader.ReadBytes(len);
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
    /// </summary>
    /// <remarks>The generated RSA key parameters are encrypted with DES using the key and IV derived from the
    /// provided password. The encrypted key and public key components are stored in corresponding fields. The password
    /// must be in the expected format; otherwise, the method may fail.</remarks>
    /// <param name="test">A string parameter for testing purposes.</param>
    /// <param name="password">A string containing the password in the format 'secret:XX-XX-...-XX', where each 'XX' is a hexadecimal byte.
    /// Used to derive the DES encryption key and initialization vector.</param>
    /// <param name="storePassword">A boolean indicating whether to store the password in local storage.</param>
    public void Generate(string test, string password, bool storePassword = false)
    {
        RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
        RSAParameters rsaParams = rsaCrypt.ExportParameters(true);
        n = rsaParams.Modulus;
        e = rsaParams.Exponent;
        d = rsaParams.D;
        p = rsaParams.P;
        q = rsaParams.Q;
        dp = rsaParams.DP;
        dq = rsaParams.DQ;
        inverseQ = rsaParams.InverseQ;
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
        byte[] encryptedbytes = EncryptKey(password, memStream.ToArray());
        EncryptedKey = Convert.ToBase64String(encryptedbytes.ToArray());
        Modulus = Convert.ToBase64String(rsaParams.Modulus ?? throw new ArgumentNullException(nameof(rsaParams.Modulus)));
        Exponent = Convert.ToBase64String(rsaParams.Exponent ?? throw new ArgumentNullException(nameof(rsaParams.Exponent)));
    }


    /// <summary>
    /// Retrieves the RSA key parameters for the current instance.
    /// </summary>
    /// <remarks>The returned parameters may include private key information. Handle the result securely to
    /// prevent disclosure of sensitive key material.</remarks>
    /// <returns>An <see cref="RSAParameters"/> structure containing the RSA key parameters, including modulus, exponent, and
    /// private key components if available.</returns>
    public RSAParameters GetRSAParameters()
    {
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
    /// Sets the RSA key parameters for the current instance using the provided <see cref="RSAParameters"/> structure.
    /// </summary>
    /// <param name="parameters">The RSA parameters to set for the current instance.</param>
    public void SetRSAParameters(RSAParameters parameters)
    {
        n = parameters.Modulus;
        e = parameters.Exponent;
        d = parameters.D;
        p = parameters.P;
        q = parameters.Q;
        dp = parameters.DP;
        dq = parameters.DQ;
        inverseQ = parameters.InverseQ;
    }

    /// <summary>
    /// Clears any partially decrypted private-key material and marks the instance as not decrypted.
    /// Call after a failed password attempt so a later correct password can retry cleanly and so
    /// callers never see a half-initialized key that would throw <see cref="NullReferenceException"/>.
    /// </summary>
    public void ResetDecryptedState()
    {
        d = e = p = q = n = dp = dq = inverseQ = null;
    }

    /// <summary>
    /// Tests whether the provided password can successfully decrypt the RSA key and perform a test encryption/decryption operation.
    /// </summary>
    /// <param name="password">The password to test for decrypting the RSA key.</param>
    /// <returns>True if the password can successfully decrypt the RSA key and perform the test operation; otherwise, false.</returns>
    public bool TestPassword(string password)
    {
        if (string.IsNullOrEmpty(password) ||
            string.IsNullOrEmpty(Modulus) ||
            string.IsNullOrEmpty(Exponent) ||
            string.IsNullOrEmpty(EncryptedKey))
        {
            ResetDecryptedState();
            return false;
        }
        try
        {
            ResetDecryptedState();
            BigInteger modulus = new BigInteger(Convert.FromBase64String(Modulus));
            BigInteger exponent = new BigInteger(Convert.FromBase64String(Exponent));
            byte[] cipherbytes = Convert.FromBase64String(EncryptedKey);
            RSAParameters rsaParams = DecryptKey(cipherbytes, password);
            RSA rsa = RSACryptoServiceProvider.Create();
            rsa.ImportParameters(rsaParams);
            var dataStream = new MemoryStream();
            dataStream.Write(new byte[1] { 0 });
            byte[] testData = System.Text.Encoding.UTF8.GetBytes("ZippyTheWorldTortoise");
            dataStream.Write(testData, 0, testData.Length);
            BigInteger data = new BigInteger(dataStream.ToArray());
            BigInteger encryptedData = BigInteger.ModPow(data, exponent, modulus);
            var encryptedBytes = data.ToByteArray();
            var decryptedBytes = rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encryptedBytes) == Convert.ToBase64String(decryptedBytes);
        }
        catch
        {
            // Wrong password (or corrupt key material) often decrypts to garbage that still
            // parses as bytes. Never leave that state visible to the rest of the pipeline.
            ResetDecryptedState();
            return false;
        }
    }
}

