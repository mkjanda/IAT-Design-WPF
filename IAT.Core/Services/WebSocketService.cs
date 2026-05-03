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

    public interface IWebSocketService
    {
        Dictionary<TransactionType, Func<TransactionRequest, IRequest<TransactionResult>>> TransactionCommands { get; set; }


        /// <summary>
        /// Closes the web socket connection gracefully by sending a close message to the server and disposing of the WebSocket instance. 
        /// If the WebSocket is already closed, it simply disposes of the instance.
        /// </summary>
        /// <returns>A task that represents the operation</returns>
        public Task CloseSocketAsync();

        /// <summary>
        /// Initiates the product activation process using the specified user and product information.
        /// </summary>
        /// <returns>A task that represents the asynchronous activation operation.</returns>
        Task Activate(string productKey, string email, string userName);

        /// <summary>
        /// Verifies the user's email address by sending a verification request to the server. The method will handle the communication with the server and update the 
        /// activation key in local storage if the verification is successful. If the email is already verified, it will set the activation key and update the result 
        /// accordingly. In case of an email mismatch or other errors, it will update the result to reflect the issue. This method relies on the WebSocketService to 
        /// manage the communication with the server and process the responses received during the verification process.
        /// </summary>
        /// <returns>A task that represents the asynchronous activation operation.</returns>
        Task VerifyEmail(string productKey, string email);

        /// <summary>
        /// Sends a new email verification message to the currently authenticated user.
        /// </summary>
        /// <remarks>Call this method to allow users to request a new verification email if they did not
        /// receive or have lost the original message. This method does not throw an error if the user is already
        /// verified.</remarks>
        /// <returns>A task that represents the asynchronous resend operation.</returns>
        Task ResendEmailVerification(string productKey, string email);

        /// <summary>
        /// Retrieves the results document for the specified IAT using the provided credentials and product key.
        /// </summary>
        /// <param name="iatName">The name of the IAT for which to retrieve results. Cannot be null or empty.</param>
        /// <param name="password">The password associated with the IAT. Cannot be null or empty.</param>
        /// <param name="productKey">The product key required to access the IAT results. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an XDocument with the IAT
        /// results data.</returns>
        Task<XDocument> GetResults(string iatName, string password, string productKey);

        /// <summary>
        /// Retrieves the manifest containing slides for the specified item, using the provided credentials and product
        /// key.
        /// </summary>
        /// <param name="iatName">The name of the item for which to retrieve slides. Cannot be null or empty.</param>
        /// <param name="password">The password used to authenticate the request. Cannot be null or empty.</param>
        /// <param name="productKey">The product key associated with the item. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Manifest object with the
        /// slides for the specified item.</returns>
        Task<Manifest> GetItemSlides(string iatName, string password, string productKey);

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
        private readonly IUserNotificationService _userNotificationService;
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
                { TransactionType.VerifyPassword, (request) => new VerifyPasswordCommand(request) } };
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

        /*
        /// <summary>
        /// Initiates a new request to resend the email verification message to the current user.
        /// </summary>
        /// <remarks>This method attempts to establish a WebSocket connection to the verification server
        /// and triggers the resend of the verification email. If the connection cannot be established, an error
        /// notification is displayed to the user. The method throws if an exception occurs during the connection
        /// process.</remarks>
        /// <returns>A task that represents the asynchronous resend operation.</returns>
        public async Task ResendEmailVerification(string productKey, string email)
        {
            Use = WebSocketUse.ResendEmailVerification;
            ProductKey = productKey;
            _email = email;
            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                StartMessageReceiver();
                RSAParameters para = new RSAParameters();
                var outTrans = new TransactionRequest()
                {
                    Transaction = TransactionType.RequestConnection,
                    ProductKey = ProductKey
                };
                SendMessage(outTrans);
                ResetEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Result = TransactionResult.CannotConnect;
                _userNotificationService.ShowError(new ErrorNotificationMessage("Cannot Resend Verification Email",
                    "An error occurred while attempting to connect to the verification server. Please check your internet connection and try again.", ex));
                ExceptionDispatchInfo.Capture(ex).Throw();
                WebSocket.Dispose();
            }
        }



        /// <summary>
        /// Initiates the email verification process by establishing a WebSocket connection and sending a verification
        /// request.
        /// </summary>
        /// <remarks>If the connection to the verification server cannot be established, an error
        /// notification is displayed and the exception is rethrown. This method must be awaited to ensure the
        /// verification process completes before proceeding.</remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task VerifyEmail(string productKey, string email)
        {
            Use = WebSocketUse.VerifyEmail;
            ProductKey = productKey;
            _email = email;
            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                StartMessageReceiver();
                var outTrans = new TransactionRequest()
                {
                    Transaction = TransactionType.RequestConnection,
                    ProductKey = productKey
                };
                SendMessage(outTrans);
                ResetEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Result = TransactionResult.CannotConnect;
                _userNotificationService.ShowError(new ErrorNotificationMessage("Cannot Verify Email",
                    "An error occurred while attempting to connect to the verification server. Please check your internet connection and try again.", ex));
                ExceptionDispatchInfo.Capture(ex).Throw();
                WebSocket.Dispose();
            }
        }

        /// <summary>
        /// Initiates the product activation process using the specified user and product information.
        /// </summary>
        /// <remarks>If the activation server cannot be reached, the activation result is set to indicate
        /// failure and an error notification is displayed to the user. The method will rethrow any exception
        /// encountered during the connection attempt.</remarks>
        /// <returns>A task that represents the asynchronous activation operation.</returns>
        public async Task Activate(String productKey, string email, string userName)
        {
            Use = WebSocketUse.ActivateProduct;
            ProductKey = productKey;
            _email = email;
            _userName = userName;

            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                StartMessageReceiver();
                var outTrans = new TransactionRequest()
                {
                    Transaction = TransactionType.RequestConnection,
                    ProductKey = ProductKey
                };
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
        /// Asynchronously retrieves a list of result packets for the specified IAT using the provided credentials and
        /// product key.
        /// </summary>
        /// <remarks>This method establishes a WebSocket connection to the activation server to request
        /// and retrieve results. If the connection cannot be established, an error notification is displayed and the
        /// method returns null. The caller should ensure that the provided credentials and product key are
        /// valid.</remarks>
        /// <param name="iatName">The name of the IAT for which to retrieve results.</param>
        /// <param name="password">The password associated with the IAT account. Cannot be null or empty.</param>
        /// <param name="productKey">The product key used to authenticate the request. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of result packets if the
        /// connection is successful; otherwise, null if the connection fails.</returns>
        public async Task<XDocument> GetResults(string iatName, string password, string productKey)
        {
            _testName = iatName;
            _password = password;
            ProductKey = productKey;
            Use = WebSocketUse.GetResults;
            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                StartMessageReceiver();
                var outTrans = new TransactionRequest()
                {
                    Transaction = TransactionType.RequestConnection,
                    ProductKey = ProductKey
                };
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
            return _results;
        }

        public async Task<Manifest> GetItemSlides(string iatName, string password, string productKey)
        {
            _testName = iatName;
            _password = password;
            ProductKey = productKey;
            Use = WebSocketUse.GetItemSlides;
            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                StartMessageReceiver();
                var outTrans = new TransactionRequest()
                {
                    Transaction = TransactionType.RequestConnection,
                    ProductKey = ProductKey
                };
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
            return _slideManifest;
        }






        /// <summary>
        /// Processes the specified handshake message and initiates an activation request based on the handshake data.
        /// </summary>
        /// <param name="handshake">The handshake message containing the cipher text to be verified and processed.</param>
        /// <exception cref="InvalidOperationException">Thrown if the handshake verification fails.</exception>
        private void ProcessMessage(Handshake handshake)
        {
            var aes = new AesGcm(AesKeyBytes, 16);
            var nonce = RandomNumberGenerator.GetBytes(NonceBytes);
            var tag = new byte[TagBytes];
            var plaintext = Convert.FromBase64String(handshake.Text);
            var ciphertext = new byte[plaintext.Length];
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            Handshake hs = new Handshake()
            {
                Text = Convert.ToBase64String(ciphertext)
            };
            SendMessage(hs);
        }


        private void ProcessMessage(Manifest manifest)
        {
            _slideManifest = manifest;
            SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.ItemSlideManifestReceived,
                IATName = _testName,
                ProductKey = ProductKey
            });
        }

        /// <summary>
        /// Processes the specified transaction request and updates the activation result or sends a handshake message
        /// as appropriate.
        /// </summary>
        /// <param name="transactionRequest">The transaction request to process. Must not be null. The type of transaction determines the action taken.</param>
        private async Task ProcessMessage(TransactionRequest transactionRequest)
        {
            switch (transactionRequest.Transaction)
            {
                case TransactionType.RequestTransmission:
                    switch (Use)
                    {
                        case WebSocketUse.VerifyEmail:
                            var requestEmailVerification = new TransactionRequest();
                            requestEmailVerification.Transaction = TransactionType.RequestEMailVerification;
                            requestEmailVerification.StringValues["email"] = _email;
                            SendMessage(requestEmailVerification);
                            break;

                        case WebSocketUse.ResendEmailVerification:
                            var transmission = new TransactionRequest();
                            transmission.Transaction = TransactionType.RequestNewVerificationEMail;
                            transmission.StringValues["email"] = _email;
                            SendMessage(transmission);
                            break;

                        case WebSocketUse.ActivateProduct:
                            SendMessage(new ActivationRequest()
                            {
                                FirstName = _userName.Split(' ')[1],
                                LastName = _userName.Split(' ')[2],
                                EMail = _email,
                                ProductCode = ProductKey,
                                Title = _userName.Split(' ')[0]
                            });
                            break;

                        case WebSocketUse.GetResults:
                            SendMessage(new TransactionRequest()
                            {
                                Transaction = TransactionType.IATExists,
                                IATName = _testName
                            });
                            break;

                        case WebSocketUse.GetItemSlides:
                            SendMessage(new TransactionRequest()
                            {
                                Transaction = TransactionType.IATExists,
                                IATName = _testName
                            });
                            break;

                        case WebSocketUse.DeployTest:
                            SendMessage(new TransactionRequest()
                            {
                                Transaction = TransactionType.RequestIATUpload,
                                IATName = _testName
                            });
                            break;

                    }
                    break;


                case TransactionType.IATExists:
                    switch (Use)
                    {
                        case WebSocketUse.GetResults:
                            SendMessage(new TransactionRequest()
                            {
                                Transaction = TransactionType.RequestEncryptionKey,
                                IATName = _testName,
                                ProductKey = ProductKey
                            });
                            break;

                        case WebSocketUse.GetItemSlides:
                            SendMessage(new TransactionRequest()
                            {
                                Transaction = TransactionType.RequestEncryptionKey,
                                IATName = _testName,
                                ProductKey = ProductKey
                            });
                            break;

                        case WebSocketUse.DeployTest:
                            if (await _dialogService.ShowConfirmationAsync(_stringResourceService.GetString("DeploymentIATExists")))
                            {
                                SendMessage(new TransactionRequest()
                                {
                                    Transaction = TransactionType.RequestIATRedeploy,
                                    IATName = _testName,
                                    ProductKey = ProductKey
                                });
                            }
                            else
                            {
                                SendMessage(new TransactionRequest()
                                {
                                    Transaction = TransactionType.AbortTransaction,
                                    ProductKey = ProductKey
                                });
                                Result = TransactionResult.Canceled;
                                ResetEvent.Set();
                            }
                            break;
                    }
                    break;

                case TransactionType.VerifyPassword:
                    byte[] encData = Convert.FromBase64String(transactionRequest.StringValues["EncryptedTestString"]);
                    var rsa = System.Security.Cryptography.RSA.Create(RSA.GetRSAParameters());
                    byte[] decData = rsa.Decrypt(encData, RSAEncryptionPadding.Pkcs1);
                    SendMessage(new TransactionRequest()
                    {
                        Transaction = TransactionType.VerifyPassword,
                        StringValues = new Dictionary<string, string>()
                        {
                            { "DecryptedTestString", Convert.ToBase64String(decData) }
                        },
                        IATName = _testName,
                        ProductKey = ProductKey
                    });
                    break;

                case TransactionType.PasswordValid:
                    SendMessage(new TransactionRequest()
                    {
                        Transaction = Use == WebSocketUse.GetResults ? TransactionType.RequestResults : TransactionType.RequestItemSlides,
                        IATName = _testName,
                        ProductKey = ProductKey,
                    });
                    break;

                case TransactionType.PasswordInvalid:
                    await _dialogService.ShowNotificationAsync(_stringResourceService.GetString("InvalidPassword"), "Invalid Password"); 
                    Result = TransactionResult.InvalidPassword;
                    ResetEvent.Set();
                    break;

                case TransactionType.ResultsReady:
                    var body = new
                    {
                        clientId = transactionRequest.LongValues["clientiD"],
                        testName = transactionRequest.IATName,
                        authToken = transactionRequest.StringValues["AuthToken"]
                    };
                    var requestBody = JsonSerializer.Serialize(body);
                    var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                    var _httpClient = new HttpClient();
                    var httpResponse = await _httpClient.PostAsync(_stringResourceService.GetString("ResultsDownloadUrl"), content);
                    httpResponse.EnsureSuccessStatusCode();
                    var memStream = await httpResponse.Content.ReadAsStreamAsync();
                    _results = XDocument.Load(memStream);
                    memStream.Dispose();
                    _httpClient.Dispose();
                    Result = TransactionResult.Success;
                    ResetEvent.Set();
                    break;

                case TransactionType.ItemSlideDownloadReady:
                    var httpClient = new HttpClient();
                    _ = httpClient.GetByteArrayAsync(_stringResourceService.GetString("sItemSlideDownloadURL")).ContinueWith(t =>
                    {
                        if (t.IsFaulted)
                        {
                            _userNotificationService.ShowError(new ErrorNotificationMessage("Error Downloading Item Slides", "An error occurred while downloading item slide data. Please try again.", t.Exception));
                            ExceptionDispatchInfo.Capture(t.Exception).Throw();
                            Result = TransactionResult.ServerFailure;
                            ResetEvent.Set();
                        }
                        else
                        {
                            var receipt = new MemoryStream(t.Result);
                            var slideData = new List<byte[]>();
                            var fileList = _slideManifest.Contents.Where(fe => fe.FileEntityType == FileEntity.EFileEntityType.File).Cast<ManifestFile>().Where(mf => mf.ResourceType == ManifestFile.EResourceType.itemSlide).ToList();
                            foreach (var file in fileList)
                            {
                                file.Content = new byte[file.Size];
                                receipt.Read(file.Content, 0, (int)file.Size);
                            }
                            receipt.Dispose();
                            httpClient.Dispose();
                            Result = TransactionResult.Success;
                            ResetEvent.Set();
                        }
                    });
                    break;

                case TransactionType.NoSuchIAT:
                    Result = TransactionResult.NoSuchIAT;
                    ResetEvent.Set();
                    break;

                case TransactionType.NoSuchClient:
                    Result = TransactionResult.NoSuchClient;
                    break;

                case TransactionType.TransactionSuccess:
                    if (Use == WebSocketUse.VerifyEmail)
                        ActivationKey = transactionRequest.ActivationKey;
                    Result = TransactionResult.Success;
                    ResetEvent.Set();
                    break;

                case TransactionType.NoActivationsRemain:
                    Result = TransactionResult.NoActivationsRemaining;
                    ResetEvent.Set();
                    break;

                case TransactionType.EMailAlreadyVerified:
                    ActivationKey = transactionRequest.ActivationKey;
                    ResetEvent.Set();
                    break;

                case TransactionType.EmailVerificationMismatch:
                    Result = TransactionResult.EmailMismatch;
                    break;
            }
        }

        private void ProcessMessage(EncryptedRSAKey key)
        {
            RSA = key;
            RSA.DecryptKey(_password);
            SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestPasswordVerification,
                IATName = _testName,
                ProductKey = ProductKey
            });
        }*/
    }
}
