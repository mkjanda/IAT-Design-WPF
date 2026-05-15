using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using System.IO;
using System.Net.Http;
using System.Security.AccessControl;

namespace IAT.Core.Handlers
{
    public class DeploymentManifestReceivedHandler : IRequestHandler<DeploymentManifestReceivedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly StringResourceService _stringResourceService;
        private readonly TestPackage _test;        
        
        public DeploymentManifestReceivedHandler(IWebSocketService webSocketService, TransactionState state, 
            StringResourceService stringResourceService, TestPackage test)
        {
            _webSocketService = webSocketService;
            _transactionState = state;
            _stringResourceService = stringResourceService;
            _test = test;
        }

        public async Task<TransactionResult> Handle(DeploymentManifestReceivedCommand request, CancellationToken cancellationToken)
        {
            HttpClient http = new HttpClient();
            var content = new ByteArrayContent(_test.ConfigFileStream.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            var response = await http.PostAsync($"{_stringResourceService.GetString("DeploymentUploadUrl")}/configuration", content);
            response.EnsureSuccessStatusCode();

            var memStream = new MemoryStream();
            _test.FileManifest.Contents.Cast<ManifestFile>().Where(m => m.ResourceType == ManifestFile.EResourceType.errorMark || 
                m.ResourceType == ManifestFile.EResourceType.keyOutline || m.ResourceType == ManifestFile.EResourceType.image).ToList()
                .ForEach(ManifestFile => memStream.Write(ManifestFile.Content));
            content = new ByteArrayContent(memStream.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            response = await http.PostAsync($"{_stringResourceService.GetString("DeploymentUploadUrl")}/images", content);
            response.EnsureSuccessStatusCode();

            memStream.Dispose(); memStream = new MemoryStream();
            _test.FileManifest.Contents.Cast<ManifestFile>().Where(m => m.ResourceType == ManifestFile.EResourceType.itemSlide).ToList()
                .ForEach(ManifestFile => memStream.Write(ManifestFile.Content));
            content = new ByteArrayContent(memStream.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            response = await http.PostAsync($"{_stringResourceService.GetString("DeploymentUploadUrl")}/itemSlides", content);
            response.EnsureSuccessStatusCode();

            return TransactionResult.Unset;
        }
    }
}
