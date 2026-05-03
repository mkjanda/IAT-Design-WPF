using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using MediatR;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the IATExistsRetrievalCommand, which is triggered when checking if an IAT exists. It sends a request for the encryption key to the server.
    /// </summary>
    internal class IATExistsRetrievalHandler : IRequestHandler<IATExistsRetrievalCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        public IATExistsRetrievalHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }       

        public async Task<TransactionResult> Handle(IATExistsRetrievalCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestEncryptionKey,
                IATName = _transactionState.IATName,
                ProductKey = _transactionState.ProductKey
            });
            return TransactionResult.Unset;
        }

    }
}
