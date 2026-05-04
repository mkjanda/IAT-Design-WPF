using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Services;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    public class PasswordInvalidHandler : IRequestHandler<PasswordInvalidCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;

        public PasswordInvalidHandler(IWebSocketService webSocketService)
        {
            _webSocketService = webSocketService;
        }

        public async Task<TransactionResult> Handle(PasswordInvalidCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.CloseSocketAsync();
            return TransactionResult.InvalidPassword;
        }
    }
}