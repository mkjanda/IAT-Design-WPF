using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services.Network;
using System.Net.Http;
using IAT.Core.Services;

namespace IAT.Core.Handlers
{
    internal class ResultSetDescriptorHandler : IRequestHandler<ResultSetDescriptorCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _stringResourceService;


        public ResultSetDescriptorHandler(IWebSocketService webSocketService, TransactionState transactionState,
            IStringResourceService stringResourceService)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _stringResourceService = stringResourceService;
        }   

        public async Task<TransactionResult> Handle(ResultSetDescriptorCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ConfigFile = request.response.ConfigFile;
            _transactionState.RSA = request.response.RSAKey;
            _transactionState.UserName = request.response.TestAuthor;
            _transactionState.NumResults = request.response.NumResults;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                IATName = _transactionState.IATName,
                AuthToken = _transactionState.AuthToken
            });
            return TransactionResult.Unset;
        }
    }
}
