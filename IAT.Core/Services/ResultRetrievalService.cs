using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace IAT.Core.Services
{
    public interface IResultRetrievalService
    {
        Task<XDocument> GetResults(string productKey, string iatName, string password);
    }
    public class ResultRetrievalService : IResultRetrievalService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public ResultRetrievalService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionRetrieveResultsCommand(request);
            _webSocketService.TransactionCommands[TransactionType.PasswordValid] = (request) => new PasswordValidResultsCommand(request);
        }

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
