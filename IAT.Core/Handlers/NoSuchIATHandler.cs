using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;
using IAT.Core.Enumerations;

using MediatR;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// NoSuchIATHandler is responsible for handling the NoSuchIATCommand, which is triggered when there is no existing IAT found.
    /// </summary>
    public class NoSuchIATHandler : IRequestHandler<NoSuchIATCommand, TransactionResult> 
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// Initializes a new instance of the NoSuchIATHandler class with the specified WebSocket, dialog, and
        /// string resource services.
        /// </summary>
        /// <param name="webSocketService">The service used to manage WebSocket communication for this handler. Cannot be null.</param>
        /// <param name="transactionState">The state of the transaction. Cannot be null.</param>
        /// <param name="dialogService">The service used to display dialogs and notifications. Cannot be null.</param>
        public NoSuchIATHandler(IWebSocketService webSocketService, TransactionState transactionState, IDialogService dialogService)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _dialogService = dialogService;
        }   

        /// <summary>
        /// Handles a command indicating that no IAT exists by sending a transaction request for IAT upload.
        /// </summary>
        /// <param name="request">The command representing the absence of an IAT. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is an unset transaction result.</returns>
        public async Task<TransactionResult> Handle(NoSuchIATCommand request, CancellationToken cancellationToken)
        {
            object o;
            if (_transactionState.Operation == OperationType.TestDeployment)
            {
                o = new TransactionRequest()
                {
                    Type = TransactionType.RequestIATUpload
                };
            } else if (_transactionState.Operation == OperationType.RetrieveItemSlides ||
                _transactionState.Operation == OperationType.RetrieveResults ||
                _transactionState.Operation == OperationType.DeleteResults ||
                _transactionState.Operation == OperationType.DeleteTest)
            {
                await _dialogService.ShowNotificationAsync(TransactionResult.NoSuchIAT.Message, TransactionResult.NoSuchIAT.Title);
                _transactionState.SetResult(TransactionResult.NoSuchIAT);
                return TransactionResult.NoSuchIAT;
            }
            _transactionState.Result = TransactionResult.InvalidRequest;
            return TransactionResult.InvalidRequest;
        }
    }
}