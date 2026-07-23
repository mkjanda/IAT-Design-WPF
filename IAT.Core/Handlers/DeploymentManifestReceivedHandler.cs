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
using System.Xml.Serialization; 
using System.Net.Http;
using System.Security.AccessControl;
using IAT.Core.ConfigFile;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handles the DeploymentManifestReceivedCommand by processing the deployment manifest and sending it to the 
    /// appropriate endpoints.
    /// </summary>
    public class DeploymentManifestReceivedHandler : IRequestHandler<DeploymentManifestReceivedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _stringResourceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentManifestReceivedHandler"/> class with the specified services.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage the connection.</param>
        /// <param name="state">The state of the current transaction.</param>
        /// <param name="stringResourceService">The service used to retrieve string resources.</param>
        public DeploymentManifestReceivedHandler(IWebSocketService webSocketService, TransactionState state, 
            IStringResourceService stringResourceService)
        {
            _webSocketService = webSocketService;
            _transactionState = state;
            _stringResourceService = stringResourceService;
        }

        /// <summary>
        /// Handles the DeploymentManifestReceivedCommand by serializing the configuration file and sending it, along with 
        /// the image and slide manifests, to the appropriate endpoints.
        /// </summary>
        /// <param name="request">The command containing information required to process the deployment manifest.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the operation.</param>
        /// <returns>A TransactionResult indicating the outcome of the operation.</returns>
        public async Task<TransactionResult> Handle(DeploymentManifestReceivedCommand request, CancellationToken cancellationToken)
        {
            HttpClient http = new HttpClient();

            var ser = new XmlSerializer(typeof(IATConfigFile));
            var memStream = new MemoryStream();
            ser.Serialize(memStream, _transactionState.ConfigFile);
            var content = new ByteArrayContent(memStream.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            var response = await http.PostAsync($"{_stringResourceService.GetString("DeploymentUploadUrl")}/configuration", content);
            response.EnsureSuccessStatusCode();

            memStream.Dispose(); memStream = new MemoryStream();

            if (_transactionState.FileManifest != null)
            {
                _transactionState.FileManifest.Contents.Cast<ManifestFile>().Where(m => m.ResourceType == FileResourceType.errorMark || 
                    m.ResourceType == FileResourceType.keyOutline || m.ResourceType == FileResourceType.image).ToList()
                    .ForEach(ManifestFile => memStream.Write(ManifestFile.Content));
            }
            content = new ByteArrayContent(memStream.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            response = await http.PostAsync($"{_stringResourceService.GetString("DeploymentUploadUrl")}/images", content);
            response.EnsureSuccessStatusCode();

            memStream.Dispose(); memStream = new MemoryStream();
            if (_transactionState.SlideManifest != null)
            {
                _transactionState.SlideManifest.Contents.Cast<ManifestFile>().ToList()
                    .ForEach(ManifestFile => memStream.Write(ManifestFile.Content));
            }
            content = new ByteArrayContent(memStream.ToArray());
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
            response = await http.PostAsync($"{_stringResourceService.GetString("DeploymentUploadUrl")}/itemSlides", content);
            response.EnsureSuccessStatusCode();

            return TransactionResult.Unset;
        }
    }
}
