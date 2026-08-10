using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services.Network
{
    public interface IDeletionService
    {
        /// <summary>
        /// Deletes a test with the specified name and password by initiating a transaction request to the server.
        /// </summary>
        /// <param name="testName">The name of the test to delete. Cannot be null or empty.</param>
        /// <param name="password">The password associated with the test. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating the outcome of the deletion request.</returns>
        Task<TransactionResult> DeleteTest(string testName, String password);
    }

    public class DeletionService : IDeletionService
    {
        private readonly WebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the DeletionService class with the specified WebSocket service 
        /// and transaction state.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to send or receive messages related to deletion events. Cannot be null.</param>
        /// <param name="transactionState">The object that contains state information for the transaction.</param>
        public DeletionService(WebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
        }


        /// <summary>
        /// Initiates the activation process for a product using the specified product key and user information.
        /// </summary>
        /// <remarks>This method establishes a connection to the activation service and waits for the
        /// activation process to complete before returning the result. The method blocks until the activation response
        /// is received. Ensure that the calling context allows for potential blocking behavior.</remarks>
        /// <param name="productKey">The unique key identifying the product to activate. Cannot be null or empty.</param>
        /// <param name="userName">The name of the user requesting activation. Cannot be null or empty.</param>
        /// <param name="email">The email address associated with the user. Cannot be null or empty.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the activation request.</returns>
        public async Task<TransactionResult> DeleteTest(string testName, String password)
        {
            _transactionState.IATName = testName;
            _transactionState.Password = password;
            _webSocketService.TransactionCommands[TransactionType.NoSuchIAT] = (result) => new NoSuchIATErrorCommand(result);
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (result) => new VerifyPasswordCommand(result);
            _webSocketService.TransactionCommands[TransactionType.PasswordValid] = (result) => new PasswordValidDeleteCommand(result);
            _transactionState.Event.Set();
            var transaction = new TransactionRequest()
            {
                Type = TransactionType.RequestConnection
            };
            _transactionState.Clear();
            await _webSocketService.SendMessage(transaction);
            _transactionState.Event.WaitOne();
            return _transactionState.Result;
        }

        public async Task<TransactionResult> DeleteTestData(string testName, String password)
        {
            _transactionState.IATName = testName;
            _transactionState.Password = password;
            _webSocketService.TransactionCommands[TransactionType.NoSuchIAT] = (result) => new NoSuchIATErrorCommand(result);
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (result) => new VerifyPasswordCommand(result);
            _webSocketService.TransactionCommands[TransactionType.PasswordValid] = (result) => new PasswordValidDeleteDataCommand(result);
            _transactionState.Event.Set();
            var transaction = new TransactionRequest()
            {
                Type = TransactionType.RequestConnection,
            };
            _transactionState.Clear();
            await _webSocketService.SendMessage(transaction);
            _transactionState.Event.WaitOne();
            return _transactionState.Result;
        }
    }
}
