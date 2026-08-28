using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;
using MediatR;
using IAT.Core.Models;
using IAT.Core.Services.Network;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    internal class RequestEncryptionKeyHandler : IRequestHandler<RequestEncryptionKeyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocket;
        private readonly TransactionState _state;

        public RequestEncryptionKeyHandler(IWebSocketService webSocket, TransactionState state)
        {
            _webSocket = webSocket;
            _state = state;
        }

        public async Task<TransactionResult> Handle(RequestEncryptionKeyCommand command, CancellationToken cancellationToken)
        {
            var key = new EncryptedRSAKey();
            key.Generate(_state.IATName, _state.Password, true);
            key.ProductKey = _state.ProductKey;
            await _webSocket.SendMessage(key);
            return TransactionResult.Unset;
        }
    }
}
