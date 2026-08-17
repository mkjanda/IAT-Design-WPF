using com.sun.corba.se.spi.orbutil.fsm;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;
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
    public class HandshakeHandler : IRequestHandler<HandshakeCommand, TransactionResult>
    {
        private static byte[] AesKeyBytes = new byte[] {
        (byte)0x2c, (byte)0x5b, (byte)0xd5, (byte)0x54, (byte)0x33, (byte)0xa8, (byte)0x8a, (byte)0x1e,
        (byte)0xff, (byte)0xe7, (byte)0x1f, (byte)0x36, (byte)0xa7, (byte)0xe0, (byte)0xe4, (byte)0xae,
        (byte)0x76, (byte)0x78, (byte)0x12, (byte)0xb3, (byte)0x23, (byte)0x64, (byte)0x89, (byte)0x62,
        (byte)0xdc, (byte)0xfc, (byte)0x79, (byte)0x53, (byte)0x41, (byte)0x51, (byte)0x6a, (byte)0xeb
        };


        private static readonly int NonceBytes = 12;
        private static readonly int TagBytes = 16;
        private readonly IWebSocketService _wss;
        private readonly TransactionState _state;

        /// <summary>
        /// The constructor initializes the HandshakeHandler with the necessary dependencies, including the WebSocket service for managing the connection. This setup 
        /// allows the handler to effectively manage the handshake process by performing encryption and communication tasks required for establishing a secure 
        /// connection with the server. The handler will use AES-GCM encryption to securely exchange information during the handshake process, ensuring that sensitive 
        /// data is protected during transmission.
        /// </summary>
        /// <param name="wss">The WebSocket service used to manage the connection.</param>
        /// <param name="state">The transaction state</param>
        public HandshakeHandler(IWebSocketService wss, TransactionState state)
        {
            _wss = wss;
            _state = state;
        }

        /// <summary>
        /// Processes a handshake command by decrypting the provided message and sending the result over the WebSocket
        /// connection.
        /// </summary>
        /// <param name="request">The handshake command containing the encrypted message to be processed. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the handshake processing.</returns>
        public async Task<TransactionResult> Handle(HandshakeCommand request, CancellationToken cancellationToken)
        {
            var aes = new AesGcm(AesKeyBytes, TagBytes);
            var nonce = Convert.FromBase64String(request.inHand.Nonce);
            var tag = Convert.FromBase64String(request.inHand.Tag);
            var challenge = Convert.FromBase64String(request.inHand.Challenge);
            var reply = new byte[challenge.Length];
            aes.Decrypt(nonce, challenge, tag, reply);
            await _wss.SendMessage(new Handshake()
            {
                ProductKey = _state.ProductKey,
                Challenge = Convert.ToBase64String(reply),
                Nonce = request.inHand.Nonce,
                Tag = request.inHand.Tag
            });
            return TransactionResult.Unset;
        }
    }
}
    