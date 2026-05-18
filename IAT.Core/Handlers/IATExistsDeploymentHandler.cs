using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the IATExistsDeploymentCommand, which is triggered when an IAT deployment already exists. 
    /// It shows a confirmation dialog to the user and either sends a redeploy request or closes the WebSocket 
    /// connection based on the user's choice.
    /// </summary>
    public class IATExistsDeploymentHandler : IRequestHandler<IATExistsDeploymentCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        private readonly IStringResourceService _stringResourceService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the IATExistsDeploymentHandler with the necessary dependencies, including the WebSocket service for communication, 
        /// the dialog service for user interactions, the string resource service for localized messages, and the transaction state to manage the transaction process.   
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage the connection.</param>
        /// <param name="dialogService">The dialog service used to show confirmation dialogs to the user.</param>
        /// <param name="stringResourceService">The string resource service used to retrieve localized messages.</param>
        /// <param name="transactionState">The transaction state used to manage the transaction process.</param>
        public IATExistsDeploymentHandler(IWebSocketService webSocketService, IDialogService dialogService, IStringResourceService stringResourceService, 
            TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
            _stringResourceService = stringResourceService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the deployment command when an IAT with the specified name already exists, prompting the user for
        /// confirmation to redeploy or abort the operation.
        /// </summary>
        /// <remarks>If the user confirms the redeployment, a redeploy request is sent and the operation
        /// continues. If the user cancels, the WebSocket connection is closed and the operation is aborted.</remarks>
        /// <param name="request">The deployment command containing information about the IAT to be redeployed.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A TransactionResult value indicating whether the redeployment was initiated or the operation was aborted.</returns>
        public async Task<TransactionResult> Handle(IATExistsDeploymentCommand request, CancellationToken cancellationToken)
        {
            if (await _dialogService.ShowConfirmationAsync(_stringResourceService.GetString("DeploymentIATExists")))
            {
                await _webSocketService.SendMessage(new TransactionRequest()
                {
                    Transaction = TransactionType.AbortDeployment,
                    IATName = _transactionState.IATName,
                    ProductKey = _transactionState.ProductKey
                });
                _transactionState.Result = TransactionResult.Unset;
                _transactionState.Event.Set();
                return TransactionResult.Unset;
            }
            else
            {
                await _webSocketService.CloseSocketAsync();
                _transactionState.Result = TransactionResult.Aborted;
                _transactionState.Event.Set();
                return TransactionResult.Aborted;
            }
        }
    }
}
