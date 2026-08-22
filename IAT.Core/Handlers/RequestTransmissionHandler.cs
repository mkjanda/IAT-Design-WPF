using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for processing the RequestTransmissionCommand, which is responsible for sending an activation request
    /// </summary>
    public class RequestTransmissionHandler : IRequestHandler<RequestTransmissionCommand, TransactionResult>
    {
        readonly private IWebSocketService _webSocketService;
        readonly private TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the RequestTransmissionHandler class with the specified WebSocket
        /// service and transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage WebSocket communications for request transmission.</param>
        /// <param name="transactionState">The transaction state object that tracks the current state of the transaction.</param>
        public RequestTransmissionHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Processes a request to activate a transmission and sends the activation data using a WebSocket service.
        /// </summary>
        /// <param name="request">The command containing the details required to initiate the transmission activation.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the activation request.</returns>
        public async Task<TransactionResult> Handle(RequestTransmissionCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ClientId = request.transaction.ClientId;
            object o;
            if (_transactionState.Operation == OperationType.Activation) {
                o = new ActivationRequest()
                {
                    FirstName = _transactionState.UserName.Split(' ')[1],
                    LastName = _transactionState.UserName.Split(' ')[2],
                    EMail = _transactionState.Email,
                    ProductKey = _transactionState.ProductKey,
                    Title = _transactionState.UserName.Split(' ')[0]
                };
            } else if (_transactionState.Operation == OperationType.TestDeployment)
            {
                o = new TransactionRequest()
                {
                    Type = TransactionType.RequestIATUpload,
                    IATName = _transactionState.IATName
                };
            }
            else if (_transactionState.Operation == OperationType.EMailVerification)
            {
                o = new TransactionRequest()
                {
                    Type = TransactionType.RequestEMailVerification,
                    Email = _transactionState.Email,
                    ProductKey = _transactionState.ProductKey
                };
            } else if (_transactionState.Operation == OperationType.ResendEmail)
            {
                o = new TransactionRequest()
                {
                    Type = TransactionType.RequestNewVerificationEMail,
                    Email = _transactionState.Email,
                    ProductKey = _transactionState.ProductKey
                };
            } else if (_transactionState.Operation == OperationType.RetrieveResults ||
                    _transactionState.Operation == OperationType.DeleteResults ||
                    _transactionState.Operation == OperationType.DeleteTest ||
                    _transactionState.Operation == OperationType.RetrieveItemSlides)
            {
                o = new TransactionRequest()
                {
                    Type = TransactionType.RequestRSAKey,
                    IATName = _transactionState.IATName,
                    ProductKey = _transactionState.ProductKey
                };
            }
            else if (_transactionState.Operation == OperationType.ServerReport)
            {
                o = new TransactionRequest()
                {
                    ProductKey = _transactionState.ProductKey,
                    Type = TransactionType.RequestServerReport
                };
            }
            else
            {
                _transactionState.SetResult(TransactionResult.InvalidRequest);
                return TransactionResult.InvalidRequest;
            }
            await _webSocketService.SendMessage(o);
            return TransactionResult.Unset;
        }
    }
}
