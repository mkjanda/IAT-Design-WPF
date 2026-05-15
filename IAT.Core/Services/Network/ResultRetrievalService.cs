using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace IAT.Core.Services.Network
{
    /// <summary>
    /// This interface defines a service for retrieving test results from a remote source, such as a web socket connection. 
    /// It provides an asynchronous method for fetching results based on a product key, IAT name, and password. The service is 
    /// designed to handle the communication and data retrieval process, returning the results as an XML document (XDocument). 
    /// Implementations of this interface are responsible for managing the connection, sending requests, and processing responses 
    /// to obtain the desired test results securely and efficiently.</summary>
    public interface IResultRetrievalService
    {
        /// <summary>
        /// Asynchronously retrieves the results document for the specified product and IAT using the provided
        /// credentials.
        /// </summary>
        /// <param name="productKey">The unique key identifying the product for which to retrieve results. Cannot be null or empty.</param>
        /// <param name="iatName">The name of the IAT (Implicit Association Test) associated with the results. Cannot be null or empty.</param>
        /// <param name="password">The password used to authenticate the request. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an XDocument with the results
        /// data for the specified product and IAT.</returns>
        Task<XDocument> GetResults(string productKey, string iatName, string password);
    }

    /// <summary>
    /// Provides services for retrieving test result documents by managing WebSocket-based transaction commands and
    /// handling result retrieval operations.
    /// </summary>
    /// <remarks>This service coordinates the registration of transaction command handlers with the underlying
    /// WebSocket service to support various result retrieval scenarios. It is designed to work with transaction state
    /// tracking and supports asynchronous retrieval of test results. Thread safety and correct usage of credentials are
    /// important for successful operation.</remarks>
    public class ResultRetrievalService : IResultRetrievalService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the ResultRetrievalService class with the specified WebSocket service and
        /// transaction state.
        /// </summary>
        /// <remarks>This constructor registers specific transaction command handlers with the provided
        /// WebSocket service to support result retrieval scenarios. The service is configured to handle various
        /// transaction types related to result processing.</remarks>
        /// <param name="webSocketService">The WebSocket service used to manage transaction commands for result retrieval operations. Cannot be null.</param>
        /// <param name="transactionState">The transaction state object that tracks the current state of transactions. Cannot be null.</param>
        public ResultRetrievalService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.IATExists] = (request) => new IATExistsRetrievalCommand(request);
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionRetrieveResultsCommand(request);
            _webSocketService.TransactionCommands[TransactionType.PasswordValid] = (request) => new PasswordValidResultsCommand(request);
            _webSocketService.TransactionCommands[TransactionType.NoSuchIAT] = (request) => new NoSuchIATResultRetrievalCommand(request);
        }

        /// <summary>
        /// Establishes a connection using the specified credentials and retrieves the test results document
        /// asynchronously.
        /// </summary>
        /// <remarks>This method initiates a connection and waits for the test results to become
        /// available. The method blocks until the results are received or the operation is otherwise completed. Ensure
        /// that the provided credentials are valid to avoid authentication errors.</remarks>
        /// <param name="productKey">The product key used to authenticate the connection request. Cannot be null or empty.</param>
        /// <param name="iatName">The name of the IAT instance to connect to. Cannot be null or empty.</param>
        /// <param name="password">The password associated with the specified product key and IAT instance. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains an XDocument with the test
        /// results. The document may be empty if no results are available.</returns>
        public async Task<XDocument> GetResults(string productKey, string iatName, string password)
        {
            _webSocketService.Start();
            _transactionState.ProductKey = productKey;
            _transactionState.IATName = iatName;
            _transactionState.Password = password;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestConnection,
                ProductKey = productKey,
                IATName = iatName
            });
            _transactionState.Event.WaitOne();
            return _transactionState.TestResultsDocument;
        }
    }
}
