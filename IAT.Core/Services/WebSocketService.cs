using IAT.Core.ConfigFile;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Handlers; 
using MediatR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace IAT.Core.Services
{
    /// <summary>
    /// Defines the contract for a service that manages WebSocket connections, message handling, and command dispatching
    /// for transaction processing.
    /// </summary>
    /// <remarks>Implementations of this interface provide mechanisms to send and receive messages over a
    /// WebSocket connection, associate transaction types with their handlers, and manage the connection lifecycle. This
    /// interface is intended for use in scenarios where custom handling of transaction-based communication over
    /// WebSockets is required.</remarks>
    public interface IWebSocketService
    {
        /// <summary>
        /// Gets or sets the mapping of transaction types to their corresponding command handlers.
        /// </summary>
        /// <remarks>Each entry associates a specific transaction type with a delegate that processes a
        /// transaction request and returns a transaction result. Modifying this dictionary allows customization of how
        /// different transaction types are handled.</remarks>
        Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>> TransactionCommands { get; set; }

        /// <summary>
        /// Closes the web socket connection gracefully by sending a close message to the server and disposing of the WebSocket instance. 
        /// If the WebSocket is already closed, it simply disposes of the instance.
        /// </summary>
        /// <returns>A task that represents the operation</returns>
        public Task CloseSocketAsync();

        /// <summary>
        /// Starts the message receiving loop for the WebSocket connection. This method continuously listens for incoming 
        /// messages from the server and processes them accordingly.
        /// </summary>
        void Start();

        /// <summary>
        /// Asynchronously sends the specified message object over the active WebSocket connection using binary serialization. 
        /// The message is serialized to XML and transmitted as a binary WebSocket message.
        /// </summary>
        /// <param name="message">The message object to be sent over the WebSocket connection.</param>
        Task SendMessage(object message);    
    }


    /// <summary>
    /// Provides functionality for activating a product using a WebSocket-based service. Manages the activation
    /// workflow, including communication with the activation server and handling activation results.
    /// </summary>
    /// <remarks>This service coordinates the product activation process by establishing a WebSocket
    /// connection, sending activation requests, and processing responses from the server. It relies on several
    /// supporting services for local storage, user notifications, string resources, and XML deserialization. The class
    /// is intended to be used as part of a larger activation workflow and is not thread-safe.</remarks>
    public class WebSocketService : IWebSocketService
    {
        /// <summary>
        /// Gets the result of the product activation attempt.
        /// </summary>
        private ClientWebSocket WebSocket = new();
        private readonly CancellationToken AbortToken = new(false);
        private readonly ArraySegment<byte> ReceiveBuffer = new();
        private readonly TransactionState _transactionState;
        private readonly IDialogService _dialogService;
        private readonly IStringResourceService _stringResourceService;
        private readonly IXmlDeserializationService _xmlDeserializationService;
        public Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>> TransactionCommands { get; set; }
        private readonly IMediator _mediator;
        private readonly MemoryStream MessageBuffer = new();


        /// <summary>
        /// Initializes a new instance of the WebSocketService class with the specified dependencies.
        /// </summary>
        /// <param name="stringResourceService">The service used to provide localized string resources.</param>
        /// <param name="xmlDeserializationService">The service used to deserialize XML data.</param>
        /// <param name="transactionState">The state object used to manage transaction state.</param>
        /// <param name="mediator">The mediator used for handling requests and notifications.</param>
        /// <param name="dialogService">The service used to display dialog notifications to the user.</param>
        public WebSocketService(IStringResourceService stringResourceService, IXmlDeserializationService xmlDeserializationService, 
            TransactionState transactionState, IMediator mediator, IDialogService dialogService)
        {
            _stringResourceService = stringResourceService;
            _xmlDeserializationService = xmlDeserializationService;
            _transactionState = transactionState;
            _dialogService = dialogService;
            _mediator = mediator;
            TransactionCommands = new Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>>() {
                { TransactionType.AbortTransaction, (request) => new AbortTransactionCommand(request) },
                { TransactionType.ClientDeleted, (request) => new ClientDeletedCommand(request)  },
                { TransactionType.ClientFrozen, (request) => new ClientFrozenCommand(request) },
                { TransactionType.EMailAlreadyVerified, (request) => new EMailAlreadyVerifiedCommand(request) },
                { TransactionType.PasswordInvalid, (request) => new InvalidPasswordCommand(request) },
                { TransactionType.ItemSlideDownloadReady, (request) => new ItemSlidesReadyCommand(request) },
                { TransactionType.NoActivationsRemain, (request) => new NoActivationsCommand(request) },
                { TransactionType.NoSuchClient, (request) => new NoSuchClientCommand(request) },
                { TransactionType.NoSuchIAT, (request) => new NoSuchIATCommand(request) },
                { TransactionType.ResultsReady, (request) => new ResultsReadyCommand(request) },
                { TransactionType.TransactionFail, (request) => new TransactionFailCommand(request) },
                { TransactionType.VerifyPassword, (request) => new VerifyPasswordCommand(request) },
                { TransactionType.TransactionSuccess, (request) => new TransactionSuccessCommand(request) }
            };
        }

        /// <summary>
        /// Sends the specified message object over the active WebSocket connection using binary serialization.
        /// </summary>
        /// <remarks>The message is serialized to XML and transmitted as a binary WebSocket message.
        /// Ensure that the receiving endpoint can deserialize the message using the same XML schema. This method is
        /// intended for use with an established WebSocket connection.</remarks>
        /// <param name="message">The object to send. The object must be serializable by the XmlSerializer corresponding to its runtime type.
        /// Cannot be null.</param>
        public async Task SendMessage(object message)
        {
            var ser = new XmlSerializer(message.GetType());
            var memStream = new MemoryStream();
            ser.Serialize(memStream, message);
            await WebSocket.SendAsync(new ArraySegment<byte>(memStream.ToArray()), WebSocketMessageType.Binary, true, AbortToken);
        }

        /// <summary>
        /// Closes the active WebSocket connection gracefully by sending a close message to the server and disposing of the WebSocket instance. 
        /// If the WebSocket is already closed, it simply disposes of the instance.
        /// </summary>
        /// <returns>A task that represents the asynchronous close operation.</returns>
        public async Task CloseSocketAsync()
        {
            if (WebSocket.State == WebSocketState.Open)
            {
                await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
            }
            WebSocket.Dispose();
        }

        /// <summary>
        /// Starts the asynchronous message receiving loop for the WebSocket connection. This method continuously listens 
        /// for incoming messages from the server and processes them accordingly.
        /// </summary>
        public void Start()
        {
            _ = ReceiveMessages();
        }

        /// <summary>
        /// Processes the result of an asynchronous WebSocket receive operation and handles incoming messages
        /// accordingly.
        /// </summary>
        /// <remarks>If the received message is complete, the method deserializes and processes it. If the
        /// WebSocket remains open, the method continues to receive additional messages. Errors encountered during
        /// message processing are reported to the user notification service.</remarks>
        /// <param name="t">A task representing the asynchronous WebSocket receive operation whose result contains the received data and
        /// status information.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        private async Task ReceiveMessages()
        {
            try
            {
                while (WebSocket.State == WebSocketState.Open)
                {
                    var result = await WebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                    if (result.Count != 0)
                    {
                        if (result.EndOfMessage)
                        {
                            MessageBuffer.Write(ReceiveBuffer.ToArray());
                            MessageBuffer.Seek(0, SeekOrigin.Begin);
                            var message = _xmlDeserializationService.DeserializeUnknownType(MessageBuffer);
                            var command = await (message switch
                            {
                                TransactionRequest tr => Task.FromResult(TransactionCommands[tr.Transaction](tr)),
                                Handshake hs => Task.FromResult<IRequest<TransactionResult>>(new HandshakeCommand(hs)),
                                EncryptedRSAKey key => Task.FromResult<IRequest<TransactionResult>>(new RSAKeyCommand(key)),
                                Manifest manifest => Task.FromResult<IRequest<TransactionResult>>(new ManifestCommand(manifest))
                            });
                            var transactionResult = await _mediator.Send(command);
                            if (transactionResult != TransactionResult.Unset)
                            {
                                _transactionState.Result = transactionResult;
                                _transactionState.Event.Set();
                                return;
                            }
                        }
                        MessageBuffer.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        MessageBuffer.Write(ReceiveBuffer.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowNotificationAsync("An error occurred while receiving data from the server. Please try again.", "Error Receiving Data");
                WebSocket.Dispose();
                ExceptionDispatchInfo.Capture(ex).Throw();
            }
        }
    }
}
