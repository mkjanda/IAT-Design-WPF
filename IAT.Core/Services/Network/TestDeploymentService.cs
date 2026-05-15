using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Handlers;
using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Transactions;
using org.omg.CORBA;

namespace IAT.Core.Services.Network
{
    /// <summary>
    /// Interface for a service that handles the deployment of tests in a networked environment. This service is responsible for managing 
    /// the deployment process, including sending requests to the server and handling responses related to test deployment. The Deploy 
    /// method initiates the deployment of a test with the specified name and password, and returns a TransactionResult indicating the 
    /// outcome of the deployment operation. Implementations of this interface should ensure proper communication with the server and handle 
    /// any necessary state management during the deployment process.
    /// </summary>
    public interface ITestDeploymentService
    {
        Task<TransactionResult> Deploy(string name, string password);
    }

    /// <summary>
    /// Provides services for deploying test packages using a WebSocket-based transaction protocol.
    /// </summary>
    /// <remarks>This service coordinates deployment operations by managing transaction state and handling
    /// communication with the deployment server. It is intended to be used in scenarios where test packages must be
    /// deployed and managed remotely via WebSocket transactions. The service is not thread-safe; concurrent usage
    /// should be externally synchronized if required.</remarks>
    public class TestDeploymentService : ITestDeploymentService
    {
        private readonly IWebSocketService _webSocket;
        private readonly TransactionState _state;
        
        /// <summary>
        /// Initializes a new instance of the TestDeploymentService class with the specified WebSocket service,
        /// transaction state, and test package.
        /// </summary>
        /// <remarks>This constructor registers deployment-specific transaction commands with the provided
        /// WebSocket service and clears any existing transaction state. The service is intended to be used as part of a
        /// deployment workflow that relies on WebSocket-based communication.</remarks>
        /// <param name="webSocket">The WebSocket service used to handle deployment-related transaction commands.</param>
        /// <param name="state">The transaction state object that tracks the current state of the deployment process.</param>
        /// <param name="testPackage">The test package containing the data and configuration required for deployment.</param>
        public TestDeploymentService(IWebSocketService webSocket, TransactionState state, TestPackage testPackage)
        {
            _webSocket = webSocket;
            _state = state;
            _state.Clear();
            _webSocket.TransactionCommands[TransactionType.IATExists] = (request) => new IATExistsDeploymentCommand(request);
            _webSocket.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionDeployTestCommand(request);
            _webSocket.TransactionCommands[TransactionType.NoSuchIAT] = (request) => new NoSuchIATDeploymentCommand(request);
        }

        /// <summary>
        /// Initiates a deployment operation by establishing a connection using the specified name and password.    
        /// </summary>
        /// <remarks>This method sends a connection request and waits for the deployment process to
        /// complete before returning the result. The method is asynchronous but blocks internally until the deployment
        /// operation finishes. Ensure that the calling context can tolerate potential blocking behavior.</remarks>
        /// <param name="name">The name to associate with the deployment operation. This value is used to identify the deployment session.</param>
        /// <param name="password">The password required to authenticate the deployment operation. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous deployment operation. The task result contains a TransactionResult
        /// indicating the outcome of the deployment.</returns>
        public async Task<TransactionResult> Deploy(string name, string password)
        {
            var evt = _state.Event;
            _state.Password = password;
            _state.IATName = name;
            _state.Event.Reset();
            await _webSocket.SendMessage(new TransactionRequest() { Transaction = TransactionType.RequestConnection });
            _state.Event.WaitOne();
            return _state.Result;
        }
    }
}
