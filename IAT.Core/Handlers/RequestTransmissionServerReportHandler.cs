using IAT.Core.Enumerations;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IAT.Core.Serializable;
using IAT.Core.Models;
using IAT.Core.Services;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    internal class RequestTransmissionServerReportHandler : IRequestHandler<RequestTransmissionServerReportCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;

        public RequestTransmissionServerReportHandler(IWebSocketService webSocketService)
        {
            _webSocketService = webSocketService;
        }

        public async Task<TransactionResult> Handle(RequestTransmissionServerReportCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.RequestServerReport
            });
            return TransactionResult.Unset;
        }
    }
}
