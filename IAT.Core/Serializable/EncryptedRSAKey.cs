using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using MediatR;
using IAT.Core.Enumerations;

namespace IAT.Core.Serializable;

/// <summary>
/// Represents a command to process an encrypted RSA key as part of a request that returns a transaction result.
/// </summary>
/// <param name="Key">The encrypted RSA key to be processed. Cannot be null.</param>
public record RSAKeyCommand(EncryptedRSAKey Key) : IRequest<TransactionResult>;


/// <summary>
/// Contains the encrypted RSA key information, including the modulus (n), exponent (e), private exponent (d), prime factors (p and q), and other related parameters.
/// </summary>
public class EncryptedRSAKey 
{


    /// <summary>
    /// Gets or sets the string value associated with the NString XML element.
    /// </summary>
    [XmlElement("NString", Form = XmlSchemaForm.Unqualified)]
    public string NString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the value of the EString XML element.
    /// </summary>
    [XmlElement("EString", Form = XmlSchemaForm.Unqualified)]
    public string EString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the encrypted key value.
    /// </summary>
    [XmlElement("EncryptedKey", Form = XmlSchemaForm.Unqualified)]
    public string EncryptedKey { get; set; } = string.Empty;

    [XmlIgnore]
    private byte[]? d, e, p, q, n, dp, dq, inverseQ;

    [XmlIgnore]
    private bool IsDecrypted { get; set; } = false;

    [XmlIgnore]
    private byte[] IV = new byte[] { (byte)0xFA, (byte)0x64, (byte)0x92, (byte)0x21, (byte)0x4A, (byte)0x74, (byte)0x41, (byte)0xE9 };

    /// <summary>
    /// Initializes a new instance of the EncryptedRSAKey class.
    /// </summary>
    public EncryptedRSAKey() { }


    /// <summary>
    /// Generates an 8-byte DES cipher key from the specified input string.
    /// </summary>
    /// <remarks>The generated key is derived deterministically from the input string using a custom
    /// algorithm. The same input will always produce the same key. This method does not perform any validation to
    /// ensure the key meets DES parity requirements.</remarks>
    /// <param name="input">The input string to be converted into a DES cipher key. Cannot be null.</param>
    /// <returns>A byte array containing the generated 8-byte DES cipher key.</returns>
    public static byte[] stringToDESCipherKey(String input)
    {
        byte[] productHex = System.Text.Encoding.Unicode.GetBytes(input);
        uint[] productNums = new uint[12];
        for (int ctr = 0; ctr < 12; ctr++)
            productNums[ctr] = 0;
        int ndx = 0;


        for (int ctr = 0; ctr < productHex.Length; ctr++)
        {
            productNums[ndx] ^= (uint)(productHex[ctr] & 0xFF);
            productNums[11 - ndx] ^= (uint)(productHex[ctr] << 8) & 0xFF00;
            ndx++;
            if (ndx >= 12)
                ndx = 0;
        }
        ulong[] cipherNums = new ulong[14];
        cipherNums[0] = productNums[6] * productNums[11];
        cipherNums[5] = productNums[Math.Abs((int)(cipherNums[0] % 12))] * productNums[2];
        cipherNums[11] = productNums[Math.Abs((int)(cipherNums[5] % 12))] * productNums[Math.Abs((int)(cipherNums[0] % 12))];
        cipherNums[2] = productNums[Math.Abs((int)(cipherNums[5] % 12))] * productNums[Math.Abs((int)(cipherNums[5] % 12))];
        cipherNums[13] = productNums[Math.Abs((int)(cipherNums[11] % 12))] * productNums[Math.Abs((int)(cipherNums[2] % 12))];
        cipherNums[1] = productNums[Math.Abs((int)(cipherNums[13] % 12))] * productNums[Math.Abs((int)(cipherNums[0] % 12))];
        cipherNums[7] = productNums[Math.Abs((int)(cipherNums[1] % 12))] * productNums[Math.Abs((int)(cipherNums[11] % 12))];
        cipherNums[3] = productNums[Math.Abs((int)(cipherNums[7] % 12))] * productNums[Math.Abs((int)(cipherNums[5] % 12))];
        cipherNums[9] = productNums[Math.Abs((int)(cipherNums[2] % 12))] * productNums[Math.Abs((int)(cipherNums[2] % 12))];
        cipherNums[4] = productNums[Math.Abs((int)(cipherNums[13] % 12))] * productNums[Math.Abs((int)(cipherNums[1] % 12))];
        cipherNums[6] = productNums[Math.Abs((int)(cipherNums[5] % 12))] * productNums[Math.Abs((int)(cipherNums[2] % 12))];
        cipherNums[8] = productNums[Math.Abs((int)(cipherNums[6] % 12))] * productNums[Math.Abs((int)(cipherNums[4] % 12))];
        cipherNums[10] = productNums[Math.Abs((int)(cipherNums[3] % 12))] * productNums[Math.Abs((int)(cipherNums[9] % 12))];
        cipherNums[12] = productNums[Math.Abs((int)(cipherNums[10] % 12))] * productNums[Math.Abs((int)(cipherNums[13] % 12))];


        byte[] cipher = new byte[8];
        for (int ctr = 0; ctr < 8; ctr++)
            cipher[ctr] = 0;
        for (int ctr = 0; ctr < 7; ctr++)
        {
            ulong val = ((ulong)cipherNums[ctr] << 32) + (ulong)cipherNums[7 + ctr];
            cipher[0] ^= (byte)(0xFF & (val >> 56));
            cipher[1] ^= (byte)(0xFF & (val >> 48));
            cipher[2] ^= (byte)(0xFF & (val >> 40));
            cipher[3] ^= (byte)(0xFF & (val >> 32));
            cipher[4] ^= (byte)(0xFF & (val >> 24));
            cipher[5] ^= (byte)(0xFF & (val >> 16));
            cipher[6] ^= (byte)(0xFF & (val >> 8));
            cipher[7] ^= (byte)(0xFF & (val));
        }
        return cipher;
    }

    /// <summary>
    /// Decrypts the stored key using the specified password.
    /// </summary>
    /// <remarks>If the key has already been decrypted, this method performs no action. The method updates the
    /// internal state with the decrypted key components upon successful decryption.</remarks>
    /// <param name="password">The password used to decrypt the key. If the password starts with "secret:", it is interpreted as a
    /// hexadecimal-encoded key and IV; otherwise, it is used directly to derive the decryption key. Cannot be null.</param>
    public void DecryptKey(String password)
    {
        if (IsDecrypted)
            return;
        byte[] desCipher;
        if (password.StartsWith("secret:"))
        {
            password = password.Remove(0, "secret:".Length);
            var bytes = password.Split('-').Select(b => Byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
            desCipher = bytes.Where((b, ndx) => ndx < 8).ToArray();
            IV = bytes.Where((b, ndx) => ndx >= 8).ToArray();
        }
        else
            desCipher = stringToDESCipherKey(password);
        MemoryStream memStream = new MemoryStream(Convert.FromBase64String(EncryptedKey));
        using var des = DES.Create();
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.None;
        MemoryStream keyStream = new MemoryStream();
        var cStream = new CryptoStream(keyStream, des.CreateDecryptor(desCipher, IV), CryptoStreamMode.Write);
        memStream.Seek(0, SeekOrigin.Begin);
        cStream.Write(memStream.ToArray(), 0, (int)memStream.Length);
        cStream.FlushFinalBlock();
        keyStream.Position = 0;
        BinaryReader bReader = new BinaryReader(keyStream);
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
        IsDecrypted = true;
    }

    /// <summary>
    /// Generates and encrypts a new RSA key pair using the specified password as the encryption key.
    /// </summary>
    /// <remarks>The generated RSA key parameters are encrypted with DES using the key and IV derived from the
    /// provided password. The encrypted key and public key components are stored in corresponding fields. The password
    /// must be in the expected format; otherwise, the method may fail.</remarks>
    /// <param name="password">A string containing the password in the format 'secret:XX-XX-...-XX', where each 'XX' is a hexadecimal byte.
    /// Used to derive the DES encryption key and initialization vector.</param>
    public void Generate(String password)
    {
        Regex r = new Regex("secret:(.+)");
        var bytes = r.Match(password).Groups[1].Value.Split('-')
                        .Select(b => Byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
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
        MemoryStream encryptedKeyBytes = new MemoryStream();
        using var desCrypt = DES.Create();
        var desCipher = bytes.Where((b, ndx) => ndx < 8).ToArray();
        var iv = bytes.Where((b, ndx) => ndx >= 8).ToArray();
        CryptoStream cStream = new CryptoStream(encryptedKeyBytes, desCrypt.CreateEncryptor(desCipher, iv), CryptoStreamMode.Write);
        memStream.Seek(0, SeekOrigin.Begin);
        cStream.Write(memStream.ToArray(), 0, (int)memStream.Length);
        cStream.FlushFinalBlock();
        EncryptedKey = Convert.ToBase64String(encryptedKeyBytes.ToArray());
        NString = Convert.ToBase64String(rsaParams.Modulus);
        EString = Convert.ToBase64String(rsaParams.Exponent);
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
}

