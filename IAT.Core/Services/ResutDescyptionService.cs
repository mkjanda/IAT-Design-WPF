using IAT.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace IAT.Core.Services
{
    /// <summary>
    /// Legacy-compatible RSA decryption of result packets.
    /// </summary>
    public interface IResultDecryptionService
    {
        /// <summary>
        /// Decrypts each chunk of the result packet into a memory stream.
        /// </summary>
        /// <param name="packet">The result packet containing encrypted chunks.</param>
        /// <param name="rsaParams">The RSA parameters used for decryption.</param>
        /// <returns>An enumerable of memory streams, each containing a decrypted chunk.</returns>
        IEnumerable<MemoryStream> DecryptToStreams(ResultPacket packet, RSAParameters rsaParams);

        /// <summary>
        /// Decrypts and deserializes each chunk using the supplied deserializer.
        /// </summary>
        /// <param name="packet">The result packet containing encrypted chunks.</param>
        /// <param name="rsaParams">The RSA parameters used for decryption.</param>
        /// <param name="deserialize">A function to deserialize each decrypted chunk.</param>
        /// <returns>An enumerable of deserialized objects.</returns>
        IEnumerable<object> DecryptAndDeserialize(
            ResultPacket packet,
            RSAParameters rsaParams,
            Func<Stream, object?> deserialize);
    }

    public class ResultDecryptionService : IResultDecryptionService
    {
        /// <summary>
        /// Decrypts each chunk of the result packet into a memory stream.
        /// </summary>
        /// <param name="packet">The result packet containing encrypted chunks.</param>
        /// <param name="rsaParams">The RSA parameters used for decryption.</param>
        /// <returns>An enumerable of memory streams, each containing a decrypted chunk.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IEnumerable<MemoryStream> DecryptToStreams(ResultPacket packet, RSAParameters rsaParams)
        {
            if (packet is null) throw new ArgumentNullException(nameof(packet));
            if (string.IsNullOrEmpty(packet.ResultData))
                yield break;

            var resultBytes = Convert.FromBase64String(packet.ResultData);

            using var rsa = RSA.Create(rsaParams);

            foreach (var toc in packet.TOC)
            {
                var encKey = resultBytes.AsSpan((int)toc.KeyOffset, toc.KeyLength).ToArray();
                var encIv = resultBytes.AsSpan((int)toc.IVOffset, toc.IVLength).ToArray();
                var encData = resultBytes.AsSpan((int)toc.DataOffset, toc.DataLength).ToArray();

                var desKey = rsa.Decrypt(encKey, RSAEncryptionPadding.Pkcs1);
                var desIv = rsa.Decrypt(encIv, RSAEncryptionPadding.Pkcs1);

                using var des = DES.Create();
                // Match the behaviour currently used in TestResultService
                des.Key = desKey;
                des.IV = desIv;

                var plain = new MemoryStream();
                using (var crypto = new CryptoStream(plain, des.CreateDecryptor(), CryptoStreamMode.Write, leaveOpen: true))
                {
                    crypto.Write(encData, 0, encData.Length);
                    crypto.FlushFinalBlock();
                }
                plain.Position = 0;
                yield return plain;
            }
        }

        /// <summary>
        /// Decrypts and deserializes each chunk using the supplied deserializer.   
        /// </summary>
        /// <param name="packet">The result packet containing encrypted chunks.</param>
        /// <param name="rsaParams">The RSA parameters used for decryption.</param>
        /// <param name="deserialize">A function to deserialize each decrypted chunk.</param>
        /// <returns>An enumerable of deserialized objects.</returns>
        public IEnumerable<object> DecryptAndDeserialize(
            ResultPacket packet,
            RSAParameters rsaParams,
            Func<Stream, object?> deserialize)
        {
            foreach (var stream in DecryptToStreams(packet, rsaParams))
            {
                using (stream)
                {
                    var obj = deserialize(stream);
                    if (obj is not null)
                        yield return obj;
                }
            }
        }
    }
}


