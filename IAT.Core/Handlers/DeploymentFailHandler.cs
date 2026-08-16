using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;

namespace IAT.Core.Handlers
{
    internal class DeploymentFailHandler : IRequestHandler<DeploymentFailCommand, TransactionResult>
    {
        private readonly TransactionState _transactionState;

        public DeploymentFailHandler(TransactionState transactionState)
        {
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(DeploymentFailCommand request, CancellationToken cancellationToken)
        {
            _transactionState.SetResult(TransactionResult.Failure);
            return TransactionResult.Failure;
        }
    }
}
