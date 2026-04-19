using System;
using System.Windows;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Xml.Serialization;
using IAT.Core.Extensions;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;

namespace IAT.Core.Services
{

    public enum WebSocketUse { ActivateProduct, VerifyEmail, ResendEmailVerification };


    /// <summary>
    /// Provides functionality for activating a product using a WebSocket-based service. Manages the activation
    /// workflow, including communication with the activation server and handling activation results.
    /// </summary>
    /// <remarks>This service coordinates the product activation process by establishing a WebSocket
    /// connection, sending activation requests, and processing responses from the server. It relies on several
    /// supporting services for local storage, user notifications, string resources, and XML deserialization. The class
    /// is intended to be used as part of a larger activation workflow and is not thread-safe.</remarks>
    public class WebSocketService
    {
        /// <summary>
        /// Gets the result of the product activation attempt.
        /// </summary>
        public TransactionResult Result { get; private set; } = TransactionResult.Unset;
        private ManualResetEvent ResetEvent = new(false);
        private Handshake OutHandshake = new();
        private object transmissionLock = new();
        private ClientWebSocket WebSocket = new();
        private CancellationToken AbortToken = new(false);
        private ArraySegment<byte> ReceiveBuffer = new();
        private ILocalStorageService _localStorageService;
        private IStringResourceService _stringResourceService;
        private IUserNotificationService _userNotificationService;
        private IXmlDeserializationService _xmlDeserializationService;
        private MemoryStream MessageBuffer = new();
        private WebSocketUse Use;
        /// <summary>
        /// Initializes a new instance of the WebSocketService class with the specified dependencies.
        /// </summary>
        /// <param name="localStorageService">The service used to access local storage for persisting and retrieving data.</param>
        /// <param name="stringResourceService">The service used to provide localized string resources.</param>
        /// <param name="userNotificationService">The service used to display notifications to the user.</param>
        /// <param name="xmlDeserializationService">The service used to deserialize XML data.</param>
        public WebSocketService(ILocalStorageService localStorageService, IStringResourceService stringResourceService,
            IUserNotificationService userNotificationService, IXmlDeserializationService xmlDeserializationService) 
        {
            _localStorageService = localStorageService;
            _userNotificationService = userNotificationService;
            _stringResourceService = stringResourceService;
            _xmlDeserializationService = xmlDeserializationService;
        }

        /// <summary>
        /// Sends the specified message object over the active WebSocket connection using binary serialization.
        /// </summary>
        /// <remarks>The message is serialized to XML and transmitted as a binary WebSocket message.
        /// Ensure that the receiving endpoint can deserialize the message using the same XML schema. This method is
        /// intended for use with an established WebSocket connection.</remarks>
        /// <param name="message">The object to send. The object must be serializable by the XmlSerializer corresponding to its runtime type.
        /// Cannot be null.</param>
        private void SendMessage(object message)
        {
            var ser = new XmlSerializer(message.GetType());
            var memStream = new MemoryStream();
            ser.Serialize(memStream, message);
            WebSocket.SendAsync(new ArraySegment<byte>(memStream.ToArray()), WebSocketMessageType.Binary, true, AbortToken);
        }



        /// <summary>
        /// Initiates the product activation process using the specified user and product information.
        /// </summary>
        /// <remarks>If the activation server cannot be reached, the activation result is set to indicate
        /// failure and an error notification is displayed to the user. The method will rethrow any exception
        /// encountered during the connection attempt.</remarks>
        /// <param name="fName">The first name of the user requesting activation. Cannot be null.</param>
        /// <param name="lName">The last name of the user requesting activation. Cannot be null.</param>
        /// <param name="eMail">The email address associated with the activation request. Cannot be null.</param>
        /// <param name="title">The title or honorific of the user (e.g., Mr., Ms., Dr.).</param>
        /// <param name="productKey">The product key to be activated. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous activation operation.</returns>
        public async Task Activate(string fName, string lName, string eMail, string title, string productKey)
        {
            Use = WebSocketUse.ActivateProduct;
            _localStorageService[Field.UserName] = String.Format("{0} {1} {2}", title, fName, lName);
            _localStorageService[Field.UserEmail] = eMail;
            _localStorageService[Field.ProductKey] = productKey;
            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                await StartMessageReceiver();
                var outTrans = new TransactionRequest(TransactionType.RequestConnection, _localStorageService);
                SendMessage(outTrans);
                ResetEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Result = TransactionResult.CannotConnect;
                _userNotificationService.ShowError(new ErrorNotificationMessage("Cannot Activate Product", 
                    "An error occurred while attempting to connect to the activation server. Please check your internet connection and try again.", ex));
                ExceptionDispatchInfo.Capture(ex).Throw();
                WebSocket.Dispose();
            }

        }

        /// <summary>
        /// Begins asynchronously receiving a message from the activation WebSocket connection.
        /// </summary>
        /// <remarks>This method initiates a receive operation on the underlying WebSocket and processes
        /// the received message upon completion. The operation is canceled if the associated abort token is
        /// triggered.</remarks>
        /// <returns>A task that represents the asynchronous receive operation.</returns>
        private async Task StartMessageReceiver()
        {
            var result =  WebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
            await result;
            ReceiveMessage(result);
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
        private async Task ReceiveMessage(Task<WebSocketReceiveResult> t)
        {
            if (t.IsCanceled)
                return;
            if (t.IsFaulted)
                return;
            try
            {
                if (t.Result.Count != 0)
                {
                    lock (transmissionLock)
                    {
                        try
                        {
                            if (t.Result.EndOfMessage)
                            {
                                MessageBuffer.Write(ReceiveBuffer.ToArray());
                                MessageBuffer.Seek(0, SeekOrigin.Begin);
                                var message = _xmlDeserializationService.DeserializeUnknownType(MessageBuffer);
                                switch (message)
                                {
                                    case Handshake handshake:
                                        ProcessMessage(handshake);
                                        break;
                                    case TransactionRequest transactionRequest:
                                        ProcessMessage(transactionRequest);
                                        break;
                                }
                            }
                            else
                            {
                                MessageBuffer.Write(ReceiveBuffer.ToArray());
                            }
                            if (WebSocket.State == WebSocketState.Open)
                                WebSocket.ReceiveAsync(ReceiveBuffer, AbortToken).ContinueWith(t => ReceiveMessage(t));
                            if (WebSocket.State != WebSocketState.Open)
                                WebSocket.Dispose();
                        }
                        catch (Exception ex)
                        {
                            _userNotificationService.ShowError(new ErrorNotificationMessage("Error Receiving Data", "An error occurred while receiving data from the server. Please try again.", ex));
                            ExceptionDispatchInfo.Capture(ex).Throw();
                            Result = TransactionResult.CannotConnect;
                            ResetEvent.Set();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _userNotificationService.ShowError(new ErrorNotificationMessage("Error Receiving Data", "An error occurred while receiving data from the server. Please try again.", ex));
                ExceptionDispatchInfo.Capture(ex).Throw();
                Result = TransactionResult.CannotConnect;
                ResetEvent.Set();
            }
        }


        /// <summary>
        /// Processes the specified handshake message and initiates an activation request based on the handshake data.
        /// </summary>
        /// <param name="handshake">The handshake message containing the cipher text to be verified and processed.</param>
        /// <exception cref="InvalidOperationException">Thrown if the handshake verification fails.</exception>
        private void ProcessMessage(Handshake handshake)
        {
            OutHandshake.CipherText = handshake.CipherText;
            if (!OutHandshake.Verify())
                throw new InvalidOperationException("Handshake verification failed.");
            switch (Use) {
                case WebSocketUse.VerifyEmail:
                    var requestEmailVerification = new TransactionRequest();
                    requestEmailVerification.Transaction = TransactionType.RequestEMailVerification;
                    requestEmailVerification.StringValues["email"] = _localStorageService[Field.UserEmail];
                    SendMessage(requestEmailVerification);
                    break;

                case WebSocketUse.ResendEmailVerification:
                    var transmission = new TransactionRequest();
                    transmission.Transaction = TransactionType.RequestNewVerificationEMail;
                    transmission.StringValues["email"] = _localStorageService[Field.UserEmail];
                    SendMessage(transmission);
                    break;

                case WebSocketUse.ActivateProduct:
                    SendMessage(new ActivationRequest()
                    {
                        FirstName = _localStorageService[Field.UserName].Split(' ')[1],
                        LastName = _localStorageService[Field.UserName].Split(' ')[2],
                        EMail = _localStorageService[Field.UserEmail],
                        ProductCode = _localStorageService[Field.ProductKey],
                        Title = _localStorageService[Field.UserName].Split(' ')[0]
                    });
                    break;
            }
        }

        /// <summary>
        /// Processes the specified transaction request and updates the activation result or sends a handshake message
        /// as appropriate.
        /// </summary>
        /// <param name="transactionRequest">The transaction request to process. Must not be null. The type of transaction determines the action taken.</param>
        private void ProcessMessage(TransactionRequest transactionRequest)
        {
            switch (transactionRequest.Transaction)
            {
                case TransactionType.RequestHandshake:
                    OutHandshake.Formulate();
                    SendMessage(OutHandshake);
                    break;

                case TransactionType.NoSuchClient:
                    Result = TransactionResult.NoSuchClient;
                    break;

                case TransactionType.TransactionSuccess:
                    if (Use == WebSocketUse.VerifyEmail)
                        _localStorageService[Field.ActivationKey] = transactionRequest.ActivationKey;
                    Result = TransactionResult.Success;
                    ResetEvent.Set();
                    break;

                case TransactionType.NoActivationsRemain:
                    Result = TransactionResult.NoActivationsRemaining;
                    ResetEvent.Set();
                    break;

                case TransactionType.EMailAlreadyVerified:
                    _localStorageService[Field.ActivationKey] = transactionRequest.ActivationKey;
                    ResetEvent.Set();
                    break;

                case TransactionType.EmailVerificationMismatch:
                    Result = TransactionResult.EmailMismatch;
                    break;
            }
        }
    }
}
