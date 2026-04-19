using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Text.RegularExpressions;

namespace IAT.Core.Models;

public class EncryptedRSAKey 
{
    private string nString = string.Empty, eString = string.Empty;
    private byte[] d, e, p, q, n, dp, dq, inverseQ;
    private String encryptedKey;
    public bool IsDecrypted { get; private set; } = false;

    public byte[] IV;// = new byte[] { (byte)0xFA, (byte)0x64, (byte)0x92, (byte)0x21, (byte)0x4A, (byte)0x74, (byte)0x41, (byte)0xE9 };
    public EncryptedRSAKey() { }

    public static EncryptedRSAKey CreateNullKey()
    {
        EncryptedRSAKey key = new EncryptedRSAKey();
        key.nString = "NULL";
        key.eString = "NULL";
        key.encryptedKey = "NULL";
        return key;
    }

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
        MemoryStream memStream = new MemoryStream(Convert.FromBase64String(encryptedKey));
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
        bWriter.Write((Int32)n.Length);
        bWriter.Write(n);
        bWriter.Write((Int32)e.Length);
        bWriter.Write(e);
        bWriter.Write((Int32)d.Length);
        bWriter.Write(d);
        bWriter.Write((Int32)p.Length);
        bWriter.Write(p);
        bWriter.Write((Int32)q.Length);
        bWriter.Write(q);
        bWriter.Write((Int32)dp.Length);
        bWriter.Write(dp);
        bWriter.Write((Int32)dq.Length);
        bWriter.Write(dq);
        bWriter.Write((Int32)inverseQ.Length);
        bWriter.Write(inverseQ);
        bWriter.Flush();
        MemoryStream encryptedKeyBytes = new MemoryStream();
        using var desCrypt = DES.Create();
        var desCipher = bytes.Where((b, ndx) => ndx < 8).ToArray();
        var iv = bytes.Where((b, ndx) => ndx >= 8).ToArray();
        CryptoStream cStream = new CryptoStream(encryptedKeyBytes, desCrypt.CreateEncryptor(desCipher, iv), CryptoStreamMode.Write);
        memStream.Seek(0, SeekOrigin.Begin);
        cStream.Write(memStream.ToArray(), 0, (int)memStream.Length);
        cStream.FlushFinalBlock();
        encryptedKey = Convert.ToBase64String(encryptedKeyBytes.ToArray());
        nString = Convert.ToBase64String(rsaParams.Modulus);
        eString = Convert.ToBase64String(rsaParams.Exponent);
    }


    public RSAParameters GetRSAParameters()
    {
        RSAParameters rsaParams = new RSAParameters();
        rsaParams.Modulus = N;
        rsaParams.Exponent = E;
        rsaParams.D = D;
        rsaParams.P = P;
        rsaParams.Q = Q;
        rsaParams.DP = DP;
        rsaParams.DQ = DQ;
        rsaParams.InverseQ = InverseQ;
        return rsaParams;
    }
}

