using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// ManifestHandler is responsible for handling the ManifestCommand, which is triggered when a manifest is received from the 
    /// server. It updates the transaction state with the received manifest and sends a message to the server indicating that the 
    /// manifest has been received. The handler returns an unset transaction result, indicating that the transaction is still in 
    /// progress and awaiting further actions or responses.
    /// </summary>
    public class ManifestReceivedCommand : IRequestHandler<ManifestCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _stringService;

        /// <summary>
        /// Initializes a new instance of the ManifestHandler class with the specified WebSocket service and transaction
        /// state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send and receive messages for manifest operations. Cannot be null.</param>
        /// <param name="transactionState">The transaction state object that tracks the current state of transactions. Cannot be null.</param>
        /// <param name="stringService">The string resource service used to retrieve string resources. Cannot be null.</param>
        public ManifestReceivedCommand(IWebSocketService webSocketService, TransactionState transactionState, IStringResourceService stringService)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _stringService = stringService;
        }   

        /// <summary>
        /// Processes the specified manifest command and updates the transaction state accordingly.
        /// </summary>
        /// <param name="request">The manifest command containing the slide manifest data to be handled.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a TransactionResult indicating the
        /// outcome of the transaction.</returns>
        public async Task<TransactionResult> Handle(ManifestCommand request, CancellationToken cancellationToken)
        {
            HttpClient client = new HttpClient();
            string url = $"http://{_stringService.GetString("RemoteHost")}{_stringService.GetString("ItemSlideDownloadPath")}";
            url += $"?TestName={_transactionState.IATName}&ClientId={_transactionState.ClientId}&AuthToken={_transactionState.AuthToken}";
            var response = await client.GetAsync(url);
            var memStream = new MemoryStream();
            await response.Content.CopyToAsync(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            var manifest = new Manifest();
            request.manifest.Contents.ForEach(f =>
            {
                ManifestFile mf = new ManifestFile()
                {
                    ResourceType = FileResourceType.itemSlide,
                    ResourceId = request.manifest.Contents.IndexOf(f) + 1,
                    MimeType = "image/jpeg",
                    Content = new byte[f.Size]
                };
                memStream.Write(mf.Content, 0, (int)f.Size);
                manifest.Contents.Add(mf);
            });
            _transactionState.SlideManifest = manifest;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Type = TransactionType.RequestResultDescriptor,
                IATName = _transactionState.IATName
            });
            return TransactionResult.Unset;
        }
    }
}
