using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;
using javax.xml.bind.annotation;
using IAT.Core.Serializable;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for processing the EncryptionKeyReceivedCommand, which is responsible for handling the event when an encryption key is received from the server.
    /// </summary>
    public class EncryptionKeyReceivedHandler : IRequestHandler<EncryptionKeyReceivedCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly TestPackage _testPackage;

        /// <summary>
        /// Initializes a new instance of the EncryptionKeyReceivedHandler class with the specified WebSocket service,
        /// transaction state, and test package.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to communicate with the client during the encryption key exchange process. Cannot
        /// be null.</param>
        /// <param name="transactionState">The current state of the transaction associated with the encryption key exchange. Cannot be null.</param>
        /// <param name="testPackage">The test package containing relevant test data for the encryption key handling operation. Cannot be null.</param>
        public EncryptionKeyReceivedHandler(IWebSocketService webSocketService, TransactionState transactionState, TestPackage testPackage)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _testPackage = testPackage;
        }

        /// <summary>
        /// Handles the receipt of an encryption key by serializing the test configuration and sending the updated file
        /// manifest over the WebSocket connection.S
        /// </summary>
        /// <remarks>The method updates the test package configuration and manifest before sending them.
        /// The returned TransactionResult is always Unset.</remarks>
        /// <param name="request">The command containing information about the received encryption key and related transaction details.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is a TransactionResult indicating the
        /// outcome of the transaction.</returns>
        public async Task<TransactionResult> Handle(EncryptionKeyReceivedCommand request, CancellationToken cancellationToken)
        {
            _testPackage.ConfigFile.ClientID = _transactionState.ClientId;
            _testPackage.ConfigFile.UploadTimeMillis = _transactionState.UploadTimeMillis;
            _testPackage.ConfigFile.Name = _transactionState.IATName;
            _testPackage.ConfigFile.EventList.AddRange(_testPackage.Events);
            _testPackage.ConfigFile.DisplayItemList.AddRange(_testPackage.DisplayItems);
            var ser = new XmlSerializer(typeof(ConfigFile.IATConfigFile));
            ser.Serialize(_testPackage.ConfigFileStream, _testPackage.ConfigFile);
            _testPackage.FileManifest.Contents.Insert(0, new ManifestFile()
            {
                ResourceType = ManifestFile.EResourceType.testConfiguration,
                ResourceId = 0,
                MimeType = "application/xml",
                Size = _testPackage.ConfigFileStream.Length,
            });
            _testPackage.FileManifest.Contents.AddRange(_testPackage.SlideManifest.Contents.Select((slide, index) => new ManifestFile()
            {
                ResourceType = ManifestFile.EResourceType.itemSlide,
                ResourceId = index,
                MimeType = "application/xml",
                Size = slide.Size,
            }));
            await _webSocketService.SendMessage(_testPackage.FileManifest);
            return TransactionResult.Unset;
        }
    }
}
