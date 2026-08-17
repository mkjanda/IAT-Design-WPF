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
using com.sun.corba.se.spi.orbutil.fsm;

namespace IAT.Core.Handlers
{
    internal class RequestTransmissionServerReportHandler : IRequestHandler<RequestTransmissionServerReportCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _state;
        public RequestTransmissionServerReportHandler(IWebSocketService webSocketService, TransactionState state)
        {
            _webSocketService = webSocketService;
            _state = state;
        }

        public async Task<TransactionResult> Handle(RequestTransmissionServerReportCommand request, CancellationToken cancellationToken)
        {
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                ProductKey = _state.ProductKey,
                Type = TransactionType.RequestServerReport
            });
            return TransactionResult.Unset;
        }
    }
}
