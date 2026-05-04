using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;

namespace IAT.Core.Handlers
{
    public class InvalidPasswordHandler : IRequestHandler<InvalidPasswordCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IDialogService _dialogService;
        public InvalidPasswordHandler(IWebSocketService webSocketService, IDialogService dialogService)
        {
            _webSocketService = webSocketService;
            _dialogService = dialogService;
        }
        public async Task<TransactionResult> Handle(InvalidPasswordCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.CloseSocketAsync();
            await _dialogService.ShowNotificationAsync(TransactionResult.InvalidPassword.Message, TransactionResult.InvalidPassword.Title);
            return TransactionResult.InvalidPassword;
        }
    }
}
