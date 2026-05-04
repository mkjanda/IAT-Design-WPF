using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the NoActivationsCommand, which is triggered when the server indicates that there are no activations remaining for the transaction.
    /// </summary>
    public class NoActivationsHandler : IRequestHandler<NoActivationsCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;

        public NoActivationsHandler(IWebSocketService webSocketService, IDialogService dialogService)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
        }

        public async Task<TransactionResult> Handle(NoActivationsCommand request, CancellationToken cancellationToken)
        {
            await _dialogService.ShowNotificationAsync(TransactionResult.NoActivationsRemaining.Message, TransactionResult.NoActivationsRemaining.Title);
            await _webSocketService.CloseSocketAsync();
            return TransactionResult.NoActivationsRemaining;
        }
    }
}
