using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Models;
using IAT.Core.Services;
using IAT.Core.Enumerations;
using System.Net.Http;
using System.IO;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    internal class RequestFilesHandler : IRequestHandler<RequestFilesCommand, TransactionResult>
    {
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _stringResourceService;

        public RequestFilesHandler(TransactionState transactionState, IStringResourceService stringResourceService)
        {
            _transactionState = transactionState;
            _stringResourceService = stringResourceService;
        }   

        public async Task<TransactionResult> Handle(RequestFilesCommand command, CancellationToken cancellationToken)
        {
            var client = new HttpClient();
            var urlString = $"{_stringResourceService.GetString("DeploymentUploadUrl")}?DeploymentId={_transactionState.DeploymentId}";
            var memStream = new MemoryStream();
            foreach (var file in _transactionState.FileManifest.Contents.Where(fe => fe.FileEntityType == FileEntity.EFileEntityType.File).Cast<ManifestFile>())
            {
                memStream.Write(file.Content);
            }
            var content = new ByteArrayContent(memStream.ToArray());
            await client.PostAsync(urlString, content);
            memStream.Dispose();
            client.Dispose();
            return TransactionResult.Unset;
        }
    }
}
