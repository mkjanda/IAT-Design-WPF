using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for processing the RequestTransmissionActivationCommand, which is responsible for sending an activation request
    /// </summary>
    public class RequestTransmissionActivationHandler : IRequestHandler<RequestTransmissionActivationCommand, TransactionResult>
    {
        readonly private IWebSocketService _webSocketService;
        readonly private TransactionState _transactionState;

        public RequestTransmissionActivationHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }
        
        public async Task<TransactionResult> Handle(RequestTransmissionActivationCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new ActivationRequest() 
            {
                FirstName = _transactionState.UserName.Split(' ')[1],
                LastName = _transactionState.UserName.Split(' ')[2],
                EMail = _transactionState.Email,
                ProductCode = _transactionState.ProductKey,
                Title = _transactionState.UserName.Split(' ')[0]
            });
            return TransactionResult.Unset;
        }
    }
}
