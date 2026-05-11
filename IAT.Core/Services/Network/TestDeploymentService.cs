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
    public interface ITestDeploymentService
    {
    }

    internal class TestDeploymentService : ITestDeploymentService
    {
        private readonly IWebSocketService _webSocket;
        private readonly TransactionState _state;
        
        public TestDeploymentService(IWebSocketService webSocket, TransactionState state, TestPackage testPackage)
        {
            _webSocket = webSocket;
            _state = state;
            _state.Clear();
            _webSocket.TransactionCommands[TransactionType.IATExists] = (request) => new IATExistsDeploymentCommand(request);
            _webSocket.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionDeployTestCommand(request);
            _webSocket.TransactionCommands[TransactionType.NoSuchIAT] = (request) => new NoSuchIATDeploymentCommand(request);
        }

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
