using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
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
    internal class IATExistsDeploymentHandler : IRequestHandler<IATExistsDeploymentCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        private readonly IStringResourceService _stringResourceService;
        private readonly TransactionState _transactionState;

        public IATExistsDeploymentHandler(IWebSocketService webSocketService, IDialogService dialogService, IStringResourceService stringResourceService, 
            TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
            _stringResourceService = stringResourceService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(IATExistsDeploymentCommand request, CancellationToken cancellationToken)
        {
            if (await _dialogService.ShowConfirmationAsync(_stringResourceService.GetString("DeploymentIATExists")))
            {
                _webSocketService.SendMessage(new TransactionRequest()
                {
                    Transaction = TransactionType.RequestIATRedeploy,
                    IATName = _transactionState.IATName,
                    ProductKey = _transactionState.ProductKey
                });
                return TransactionResult.Unset;
            }
            else
            {
                await _webSocketService.CloseSocketAsync();
                return TransactionResult.Aborted;
            }
        }
    }
}
