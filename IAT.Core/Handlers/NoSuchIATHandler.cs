using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;

namespace IAT.Core.Handlers
{
    public class NoSuchIATHandler : IRequestHandler<NoSuchIATCommand, TransactionResult>
    {
        private readonly IWebSocketService _wss;
        private readonly IDialogService _dialogService;
        public NoSuchIATHandler(IWebSocketService wss, IDialogService dialogService)
        {
            _wss = wss;
            _dialogService = dialogService;
        }
        public async Task<TransactionResult> Handle(NoSuchIATCommand request, CancellationToken cancellationToken)
        {
            await _dialogService.ShowNotificationAsync(TransactionResult.NoSuchIAT.Message, TransactionResult.NoSuchIAT.Title);
            await _wss.CloseSocketAsync();
            return TransactionResult.NoSuchIAT;
        }
}
