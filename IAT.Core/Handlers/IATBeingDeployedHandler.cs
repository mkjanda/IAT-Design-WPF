using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using MediatR;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// IATBeingDeployedHandler is responsible for handling the IATBeingDeployedCommand, which is triggered when the server indicates that an 
    /// IAT (Interactive Application Test) is currently being deployed. Upon receiving this command, the handler prompts the user with a confirmation 
    /// dialog to decide whether to halt the deployment or not. If the user confirms, a transaction request is sent to the server to halt the test 
    /// deployment. If the user cancels, the WebSocket connection is closed and the transaction is aborted. The handler returns an unset transaction 
    /// result if the user chooses to halt the deployment, indicating that further processing is needed, or an aborted result if the user cancels, 
    /// indicating that the transaction has been terminated.
    /// </summary>
    public class IATBeingDeployedHandler : IRequestHandler<IATBeingDeployedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _transactionState;
        private readonly StringResourceService _stringService;

        /// <summary>
        /// Initializes a new instance of the IATBeingDeployedHandler class with the specified services and transaction
        /// state.
        /// </summary>
        /// <param name="webSocketService">The service used to manage WebSocket communication for deployment events.</param>
        /// <param name="dialogService">The service used to display dialogs and user notifications during deployment.</param>
        /// <param name="transactionState">The current state of the transaction associated with the deployment process.</param>
        /// <param name="stringService">The service used to retrieve localized string resources for user interface elements.</param>
        public IATBeingDeployedHandler(IWebSocketService webSocketService, IDialogService dialogService, TransactionState transactionState, StringResourceService stringService)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
            _transactionState = transactionState;
            _stringService = stringService;
        }

        /// <summary>
        /// Handles the command to confirm and process the halting of a test deployment.
        /// </summary>
        /// <remarks>If the user confirms the halt operation, a halt request is sent and the operation
        /// completes without closing the connection. If the user does not confirm, the WebSocket connection is closed
        /// and the operation is aborted.</remarks>
        /// <param name="request">The command containing information about the test deployment to be halted. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A TransactionResult value indicating the outcome of the operation. Returns TransactionResult.Unset if the
        /// halt is confirmed; otherwise, returns TransactionResult.Aborted.</returns>
        public async Task<TransactionResult> Handle(IATBeingDeployedCommand request, CancellationToken cancellationToken)
        {
            if (await _dialogService.ShowConfirmationAsync(_stringService.GetString("IATBeingDeployed"), _stringService.GetString("IATBeingDeployedTitle")))
            {
                await _webSocketService.SendMessage(new TransactionRequest()
                {
                    Transaction = TransactionType.HaltTestDeployment,
                    LongValues = { ["DeploymentId"] = request.transaction.LongValues["DeploymentId"] }
                });
                return TransactionResult.Unset;
            }
            await _webSocketService.CloseSocketAsync();
            _transactionState.Result = TransactionResult.Aborted;
            _transactionState.Event.Set();
            return TransactionResult.Aborted;
        }
    }
}
