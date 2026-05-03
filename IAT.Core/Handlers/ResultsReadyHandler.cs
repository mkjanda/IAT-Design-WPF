using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the ResultsReadyCommand, which is triggered when the server indicates that the test results are ready for download.
    /// </summary>
    internal class ResultsReadyHandler : IRequestHandler<ResultsReadyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IStringResourceService _stringResourceService;
        private readonly TransactionState _transactionState;

        public ResultsReadyHandler(IWebSocketService webSocketService, IStringResourceService stringResourceService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _stringResourceService = stringResourceService;
            _transactionState = transactionState;
        }

        public async Task<TransactionResult> Handle(ResultsReadyCommand request, CancellationToken cancellationToken)
        {
            var requestBody = JsonSerializer.Serialize(new
            {
                clientId = request.transaction.LongValues["ClientId"],
                testName = _transactionState.IATName,
                authToken = request.transaction.StringValues["AuthToken"]
            });
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            using var _httpClient = new HttpClient();
            var httpResponse = await _httpClient.PostAsync(_stringResourceService.GetString("ResultsDownloadUrl"), content);
            httpResponse.EnsureSuccessStatusCode();
            using var memStream = await httpResponse.Content.ReadAsStreamAsync();
            _transactionState.TestResultsDocument = XDocument.Load(memStream);
            await _webSocketService.CloseSocketAsync();
            return TransactionResult.Success;
        }
    }
}
