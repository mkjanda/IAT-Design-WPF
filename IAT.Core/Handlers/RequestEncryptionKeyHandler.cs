using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;
using MediatR;
using IAT.Core.Models;
using IAT.Core.Services.Network;
using IAT.Core.Serializable;
using IAT.Core.Services;


namespace IAT.Core.Handlers
{
    internal class RequestEncryptionKeyHandler : IRequestHandler<RequestEncryptionKeyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocket;
        private readonly TransactionState _state;
        private readonly ICryptoService _decryptor;

        public RequestEncryptionKeyHandler(IWebSocketService webSocket, TransactionState state, ICryptoService decryptor)
        {
            _webSocket = webSocket;
            _state = state;
            _decryptor = decryptor;
        }

        public async Task<TransactionResult> Handle(RequestEncryptionKeyCommand command, CancellationToken cancellationToken)
        {
            _decryptor.Generate(_state.Password, true);
            var p = _decryptor.CurrentParams;
            p.ProductKey = _state.ProductKey;
            await _webSocket.SendMessage(p);
            return TransactionResult.Unset;
        }
    }
}
