using com.sun.xml.@internal.ws.resources;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handles the handshake process for a transaction request.
    /// </summary>
    internal class HandshakeHandler : IRequestHandler<HandshakeCommand, TransactionResult>
    {
        private static readonly byte[] AesKeyBytes = new byte[32] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE };
        private static readonly int NonceBytes = 12;
        private static readonly int TagBytes = 16;
        private readonly IWebSocketService _wss;

        public HandshakeHandler(IWebSocketService wss)
        {
            _wss = wss;
        }

        public async Task<TransactionResult> Handle(HandshakeCommand request, CancellationToken cancellationToken)
        {
            var aes = new AesGcm(AesKeyBytes, 16);
            var nonce = RandomNumberGenerator.GetBytes(NonceBytes);
            var tag = new byte[TagBytes];
            var plaintext = Convert.FromBase64String(request.inHand.Text);
            var ciphertext = new byte[plaintext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            _wss.SendMessage(new Handshake() { 
                Text = Convert.ToBase64String(ciphertext)
            });
            return TransactionResult.Unset;
        }
    }
}
