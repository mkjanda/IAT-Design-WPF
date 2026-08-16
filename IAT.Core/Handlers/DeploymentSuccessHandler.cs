using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using System.Transactions;

namespace IAT.Core.Handlers
{
    internal class DeploymentSuccessHandler : IRequestHandler<DeploymentSuccessCommand, TransactionResult>
    {
        private readonly TransactionState _transactionState;

        public DeploymentSuccessHandler(TransactionState transactionState)
        {
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(DeploymentSuccessCommand request, CancellationToken cancellationToken)
        {
            _transactionState.SetResult(TransactionResult.Success);
            return TransactionResult.Success;
        }
    }
}
