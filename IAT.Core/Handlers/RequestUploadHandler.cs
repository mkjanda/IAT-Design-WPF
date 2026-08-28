using System;
using System.Collections.Generic;
using System.Text;
using IAT.Core.Services.Network;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System.Net.Http;
using MediatR;
using IAT.Core.Services;
using System.IO;
using IAT.Core.Enumerations;

namespace IAT.Core.Handlers
{
    internal class RequestUploadHandler : IRequestHandler<RequestUploadCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _strings;

        public RequestUploadHandler(IWebSocketService webSocketService, TransactionState transactionState, IStringResourceService strings)
        {
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
            _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
            _strings = strings ?? throw new ArgumentNullException(nameof(strings));
        }

        public async Task<TransactionResult> Handle(RequestUploadCommand request, CancellationToken cancellationToken)
        {
            var httpClient = new HttpClient();
            MemoryStream memStream = new MemoryStream();
            if (request.transaction.Type == TransactionType.RequestFiles)
            {
                var url = $"{_strings.GetString("DeploymentUploadUrl")}?" +
                     $"deploymentId={_transactionState.DeploymentId}";
                foreach (var f in _transactionState.FileManifest.Files)
                    memStream.Write(f.Content);
                var requestBody = new ByteArrayContent(memStream.ToArray());
                memStream.Dispose();
                await httpClient.PostAsync(url, requestBody, CancellationToken.None);
                await _webSocketService.SendMessage(new TransactionRequest()
                {
                    Type = TransactionType.DoIATDeploy,
                    DeploymentId = _transactionState.DeploymentId,
                    ProductKey = _transactionState.ProductKey
                });
                return TransactionResult.Unset;
            }
            else if (request.transaction.Type == TransactionType.RequestItemSlides)
            {
                var url = $"{_strings.GetString("ItemSlideUploadUrl")}?" +
                     $"deploymentId={_transactionState.DeploymentId}";
                foreach (var f in _transactionState.SlideManifest.Files)
                    memStream.Write(f.Content);
                var requestBody = new ByteArrayContent(memStream.ToArray());
                memStream.Dispose();
                await httpClient.PostAsync(url, requestBody, CancellationToken.None);
                return TransactionResult.Unset;
            }
            else
            {
                _transactionState.SetResult(TransactionResult.Failure);
                return TransactionResult.Failure;
            }
        }
    }
}
