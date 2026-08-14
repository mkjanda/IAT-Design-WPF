using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using System.Threading;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Models;

namespace IAT.Core.Handlers
{
    internal class ServerReportHandler : IRequestHandler<ServerReportCommand, TransactionResult>
    {
        private readonly TransactionState _transactionState;

        public ServerReportHandler(TransactionState transactionState)
        {
            _transactionState = transactionState;
        }

        public Task<TransactionResult> Handle(ServerReportCommand request, CancellationToken cancellationToken)
        {
            _transactionState.ServerReport = request.report;
            _transactionState.SetResult(TransactionResult.Success);
            return Task.FromResult(TransactionResult.Success);
        }
    }
}
