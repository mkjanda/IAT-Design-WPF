using System;
using IAT.Core.Enumerations;
using IAT.Core.Extensions;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Windows;
using System.Xml.Serialization;
using System.Net;
using System.Xml.Linq;
using IAT.Core.ConfigFile;
using System.Net.Http;
using jdk.nashorn.@internal.ir;


namespace IAT.Core.Services
{

    public interface IWebSocketService
    {
        /// <summary>
        /// Gets the result of the product activation attempt.
        /// </summary>
        TransactionResult Result { get; }
        /// <summary>
        /// Initiates the product activation process using the specified user and product information.
        /// </summary>
        /// <returns>A task that represents the asynchronous activation operation.</returns>
        Task Activate(string productKey, string email, string userName);

        public EncryptedRSAKey RSA { get; }

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
        /// The activation key associated with the current activation or verification process. This property is updated upon successful email 
        /// verification or product activation and can be used to retrieve the activation key for storage or display purposes.
        /// </summary>
        public string ActivationKey { get; set; }
    }

    public enum WebSocketUse { ActivateProduct, VerifyEmail, ResendEmailVerification, GetResults };


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
        public TransactionResult Result { get; private set; } = TransactionResult.Unset;

        private ManualResetEvent ResetEvent = new(false);
        private Handshake OutHandshake = new();
        private object transmissionLock = new();
        private ClientWebSocket WebSocket = new();
        private CancellationToken AbortToken = new(false);
        private ArraySegment<byte> ReceiveBuffer = new();
        private IStringResourceService _stringResourceService;
        private IUserNotificationService _userNotificationService;
        private IXmlDeserializationService _xmlDeserializationService;
        private MemoryStream MessageBuffer = new();
        private WebSocketUse Use;
        private string _productKey = string.Empty;
        private string _email = string.Empty;
        private string _userName = string.Empty;
        private string _password = string.Empty;
        private string _testName = string.Empty;
        public string ActivationKey { get; set; } = String.Empty;
        public EncryptedRSAKey RSA { get; private set; }
        private XDocument _results;
        private static readonly byte[] AesKeyBytes = new byte[32] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE,
            0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x10, 0x32, 0x54, 0x76, 0x98, 0xBA, 0xDC, 0xFE };
        private static readonly int NonceBytes = 12;
        private static readonly int TagBytes = 16;


        /// <summary>
        /// Initializes a new instance of the WebSocketService class with the specified dependencies.
        /// </summary>
        /// <param name="stringResourceService">The service used to provide localized string resources.</param>
        /// <param name="userNotificationService">The service used to display notifications to the user.</param>
        /// <param name="xmlDeserializationService">The service used to deserialize XML data.</param>
        public WebSocketService(IStringResourceService stringResourceService, IUserNotificationService userNotificationService, IXmlDeserializationService xmlDeserializationService)
        {
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
            _productKey = productKey;
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
                    ProductKey = _productKey
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
            _productKey = productKey;
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
            _productKey = productKey;
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
                    ProductKey = _productKey
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
            _productKey = productKey;
            Use = WebSocketUse.GetResults;
            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                StartMessageReceiver();
                var outTrans = new TransactionRequest()
                {
                    Transaction = TransactionType.RequestConnection,
                    ProductKey = _productKey
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

        /// <summary>
        /// Begins asynchronously receiving a message from the activation WebSocket connection.
        /// </summary>
        /// <remarks>This method initiates a receive operation on the underlying WebSocket and processes
        /// the received message upon completion. The operation is canceled if the associated abort token is
        /// triggered.</remarks>
        /// <returns>A task that represents the asynchronous receive operation.</returns>
        private async void StartMessageReceiver()
        {
            var result = WebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
            await result;
            _ = ReceiveMessage(result);
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
                                    case EncryptedRSAKey key:
                                        ProcessMessage(key);
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
                                ProductCode = _productKey,
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
                    }
                    break;


                case TransactionType.IATExists:
                    SendMessage(new TransactionRequest()
                    {
                        Transaction = TransactionType.RequestEncryptionKey,
                        IATName = _testName,
                        ProductKey = _productKey
                    });
                    break;


                case TransactionType.VerifyPassword:
                    byte[] encData = Convert.FromBase64String(transactionRequest.StringValues["EncryptedTestString"]);
                    var rsa = RSA.Create(EncryptedRSAKey.GetRSAParameters());
                    byte[] decData = rsa.Decrypt(encData, RSAEncryptionPadding.Pkcs1);
                    SendMessage(new TransactionRequest()
                    {
                        Transaction = TransactionType.VerifyPassword,
                        StringValues = new Dictionary<string, string>()
                        {
                            { "DecryptedTestString", Convert.ToBase64String(decData) }
                        },
                         IATName = _testName,
                         ProductKey = _productKey
                    });
                    break;

                case TransactionType.PasswordValid:
                    SendMessage(new TransactionRequest()
                    {
                        IATName = _testName,
                        ProductKey = _productKey,
                    });
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

                case TransactionType.PasswordInvalid:
                    Result = TransactionResult.InvalidPassword;
                    ResetEvent.Set();
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
            EncryptedRSAKey = key;
            EncryptedRSAKey.DecryptKey(_password);
            SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestPasswordVerification,
                IATName = _testName,
                ProductKey = _productKey
            });
        }
    }
}
