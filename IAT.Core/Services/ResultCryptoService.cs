// IResultCryptoService.cs
using IAT.Core.Serializable;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IAT.Core.Services;

/// <summary>
/// Legacy-compatible RSA private-key unwrap for result retrieval.
/// The password → DES key derivation is non-standard and must stay bit-identical
/// with historical server data.
/// </summary>
public interface IResultCryptoService
{
    /// <summary>
    /// Unwraps the encrypted RSA private key using the data-retrieval password.
    /// </summary>
    /// <param name="encryptedKey">The key blob received from the server (RSAKey / EncryptedRSAKey).</param>
    /// <param name="password">Plain password or "secret:XX-XX-..." form.</param>
    /// <returns>Fully populated RSAParameters (including private components).</returns>
    /// <exception cref="CryptographicException">Password is wrong or blob is corrupt.</exception>
    RSAParameters UnwrapPrivateKey(EncryptedRSAKey encryptedKey, string password);

    /// <summary>
    /// Returns true if the supplied password successfully decrypts the key
    /// (does not throw). Useful for UI validation before starting the full retrieval.
    /// </summary>
    bool TryUnwrapPrivateKey(EncryptedRSAKey encryptedKey, string password, out RSAParameters parameters);
}

/// <summary>
/// Legacy-compatible RSA private-key unwrap for result retrieval.
/// </summary>
public sealed class ResultCryptoService : IResultCryptoService
{
    // Fixed IV used by the original client when the password is not in "secret:" form.
    private static readonly byte[] DefaultIv =
    {
        0xFA, 0x64, 0x92, 0x21, 0x4A, 0x74, 0x41, 0xE9
    };

    /// <summary>
    /// Unwraps the encrypted RSA private key using the data-retrieval password.
    /// </summary>
    /// <param name="encryptedKey">The key blob received from the server (RSAKey / EncryptedRSAKey).</param>
    /// <param name="password">Plain password or "secret:XX-XX-..." form.</param>
    /// <returns>Fully populated RSAParameters (including private components).</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="encryptedKey"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="password"/> is null or empty.</exception>
    /// <exception cref="CryptographicException">Thrown if the encrypted key blob is empty or decryption fails.</exception>
    public RSAParameters UnwrapPrivateKey(EncryptedRSAKey encryptedKey, string password)
    {
        if (encryptedKey is null) throw new ArgumentNullException(nameof(encryptedKey));
        if (string.IsNullOrEmpty(password)) throw new ArgumentException("Password required.", nameof(password));
        if (string.IsNullOrEmpty(encryptedKey.EncryptedKey))
            throw new CryptographicException("EncryptedKey blob is empty.");

        var (desKey, iv) = DeriveDesKeyAndIv(password);

        // Exact behaviour of the current EncryptedRSAKey.DecryptKey
        using var des = DES.Create();
        des.Mode = CipherMode.CBC;
        des.Padding = PaddingMode.None;          // must stay None – matches live data

        var cipherBytes = Convert.FromBase64String(encryptedKey.EncryptedKey);
        using var plain = new MemoryStream();
        using (var crypto = new CryptoStream(plain, des.CreateDecryptor(desKey, iv), CryptoStreamMode.Write))
        {
            crypto.Write(cipherBytes, 0, cipherBytes.Length);
            crypto.FlushFinalBlock();
        }
        plain.Position = 0;

        using var reader = new BinaryReader(plain);
        return new RSAParameters
        {
            Modulus = ReadLengthPrefixed(reader),
            Exponent = ReadLengthPrefixed(reader),
            D = ReadLengthPrefixed(reader),
            P = ReadLengthPrefixed(reader),
            Q = ReadLengthPrefixed(reader),
            DP = ReadLengthPrefixed(reader),
            DQ = ReadLengthPrefixed(reader),
            InverseQ = ReadLengthPrefixed(reader)
        };
    }

    /// <summary>
    /// Decrypts the encrypted RSA private key using the data-retrieval password, returning true if successful. 
    /// </summary>
    /// <param name="encryptedKey">The key blob received from the server (RSAKey / EncryptedRSAKey).</param>
    /// <param name="password">Plain password or "secret:XX-XX-..." form.</param>
    /// <param name="parameters">The decrypted RSA parameters if successful.</param>
    /// <returns>True if the decryption was successful; otherwise, false.</returns>
    public bool TryUnwrapPrivateKey(EncryptedRSAKey encryptedKey, string password, out RSAParameters parameters)
    {
        try
        {
            parameters = UnwrapPrivateKey(encryptedKey, password);
            return true;
        }
        catch
        {
            parameters = default;
            return false;
        }
    }

    private static (byte[] key, byte[] iv) DeriveDesKeyAndIv(string password)
    {
        if (password.StartsWith("secret:", StringComparison.Ordinal))
        {
            var hex = password["secret:".Length..]
                .Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(b => byte.Parse(b, NumberStyles.HexNumber))
                .ToArray();

            if (hex.Length < 16)
                throw new CryptographicException("secret: form must contain at least 16 bytes (key + IV).");

            return (hex.AsSpan(0, 8).ToArray(), hex.AsSpan(8, 8).ToArray());
        }

        return (StringToDesCipherKey(password), DefaultIv);
    }

    /// <summary>
    /// Bit-identical port of the original stringToDESCipherKey.
    /// Do not “improve” this algorithm – every existing result set depends on it.
    /// </summary>
    private static byte[] StringToDesCipherKey(string input)
    {
        var productHex = Encoding.Unicode.GetBytes(input);
        var productNums = new uint[12];
        int ndx = 0;

        foreach (var b in productHex)
        {
            productNums[ndx] ^= b;
            productNums[11 - ndx] ^= (uint)(b << 8);
            ndx = (ndx + 1) % 12;
        }

        var cipherNums = new ulong[14];
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

        var cipher = new byte[8];
        for (int i = 0; i < 7; i++)
        {
            ulong val = (cipherNums[i] << 32) + cipherNums[7 + i];
            cipher[0] ^= (byte)(val >> 56);
            cipher[1] ^= (byte)(val >> 48);
            cipher[2] ^= (byte)(val >> 40);
            cipher[3] ^= (byte)(val >> 32);
            cipher[4] ^= (byte)(val >> 24);
            cipher[5] ^= (byte)(val >> 16);
            cipher[6] ^= (byte)(val >> 8);
            cipher[7] ^= (byte)val;
        }
        return cipher;
    }

    private static byte[] ReadLengthPrefixed(BinaryReader reader)
    {
        int len = reader.ReadInt32();
        return reader.ReadBytes(len);
    }
}