using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace IAT.Core.Handlers
{
    internal class RequestItemSlidesHandler : IRequestHandler<RequestItemSlidesCommand, TransactionResult>  
    {
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _stringResourceService;
        private readonly IWebSocketService _webSocketService;

        public RequestItemSlidesHandler(TransactionState transactionState, IStringResourceService stringResourceService, IWebSocketService webSocketService)
        {
            _transactionState = transactionState;
            _stringResourceService = stringResourceService;
            _webSocketService = webSocketService;
        }

        public async Task<TransactionResult> Handle(RequestItemSlidesCommand command, CancellationToken cancellationToken)
        {
            var client = new HttpClient();
            var urlString = $"{_stringResourceService.GetString("ItemSlideUploadUrl")}?DeploymentId={_transactionState.DeploymentId}";
            var memStream = new MemoryStream();
            foreach (var file in _transactionState.SlideManifest.Contents.Where(fe => fe.FileEntityType == FileEntity.EFileEntityType.File).Cast<ManifestFile>())
            {
                memStream.Write(file.Content);
            }
            var content = new ByteArrayContent(memStream.ToArray());
            await client.PostAsync(urlString, content);
            memStream.Dispose();
            client.Dispose();

             await _webSocketService.SendMessage(new TransactionRequest() {
                Type = TransactionType.DoIATDeploy,
                DeploymentId = _transactionState.DeploymentId
            });
            return TransactionResult.Unset;
        }
    }
}
