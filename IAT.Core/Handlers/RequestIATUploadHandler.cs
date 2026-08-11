using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Domain;
using IAT.Core.Services;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using IAT.Core.Serializable;
using IAT.Core.Models;
using IAT.Core.Services.Export;
using System.Net.Http;
using System.Xml.Serialization;
using System.IO;

namespace IAT.Core.Handlers
{
    internal class RequestIATUploadHandler : IRequestHandler<RequestIATUploadCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _stringService;
        private readonly ITestExportService _testExportService;
        private readonly IatTest _test;

        public RequestIATUploadHandler(IWebSocketService webSocketService, TransactionState transactionState, IStringResourceService stringService, 
            ITestExportService testExportService, IatTest test)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _stringService = stringService;
            _testExportService = testExportService;
            _test = test;   
        }

        public async Task<TransactionResult> Handle(RequestIATUploadCommand request, CancellationToken cancellationToken)
        {
            _transactionState.AuthToken = request.transaction.AuthToken;
            var url = $"{_stringService.GetString("ManifestUploadUrl")}?TestName={_transactionState.IATName}"
                + $"&AuthToken={_transactionState.AuthToken}&ClientId={_transactionState.ClientId}";
            HttpClient client = new HttpClient();
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            var exporResult = await _testExportService.PrepareForServerUploadAsync(_test);
            var serializer = new XmlSerializer(typeof(ExportResult));
            StringWriter sWriter = new StringWriter();
            serializer.Serialize(sWriter, exporResult);
            requestMessage.Content = new StringContent(sWriter.ToString());
            await client.SendAsync(requestMessage);

            var memStream = new MemoryStream();
            foreach (ManifestFile file in exporResult.FileManifest.Contents)
            {
                memStream.Write(file.Content, 0, file.Content.Length);
            }
            foreach (ManifestFile file in exporResult.SlideManifest.Contents)
            {
                memStream.Write(file.Content, 0, file.Content.Length);
            }
            url = $"{_stringService.GetString("DeploymentUploadUrl")}?TestName={_transactionState.IATName}"
                + $"&AuthToken={_transactionState.AuthToken}&ClientId={_transactionState.ClientId}";
            requestMessage = new HttpRequestMessage(HttpMethod.Post, url);  
            requestMessage.Content = new ByteArrayContent(memStream.ToArray());
            await client.SendAsync(requestMessage);
            memStream.Dispose();
            return TransactionResult.Unset;
        }
    }
}
