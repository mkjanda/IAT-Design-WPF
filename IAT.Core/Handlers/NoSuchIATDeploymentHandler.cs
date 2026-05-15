using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;
using IAT.Core.Enumerations;

using MediatR;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// NoSuchIATDeploymentHandler is responsible for handling the NoSuchIATDeploymentCommand, which is triggered when there is no existing IAT deployment found.
    /// </summary>
    public class NoSuchIATDeploymentHandler : IRequestHandler<NoSuchIATDeploymentCommand, TransactionResult> 
    {
        private readonly IWebSocketService _webSocketService;

        /// <summary>
        /// Initializes a new instance of the NoSuchIATDeploymentHandler class with the specified WebSocket, dialog, and
        /// string resource services.
        /// </summary>
        /// <param name="webSocketService">The service used to manage WebSocket communication for this handler. Cannot be null.</param>
        /// <param name="dialogService">The service used to display dialogs to the user. Cannot be null.</param>
        /// <param name="stringService">The service used to retrieve localized string resources. Cannot be null.</param>
        public NoSuchIATDeploymentHandler(IWebSocketService webSocketService, IDialogService dialogService, 
            StringResourceService stringService)
        {
            _webSocketService = webSocketService;
        }   

        /// <summary>
        /// Handles a command indicating that no IAT deployment exists by sending a transaction request for IAT upload.
        /// </summary>
        /// <param name="request">The command representing the absence of an IAT deployment. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is an unset transaction result.</returns>
        public async Task<TransactionResult> Handle(NoSuchIATDeploymentCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestIATUpload
            });
            return TransactionResult.Unset;
        }
    }
}