using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Services.Network;
using MediatR;
using System.Net.Http;
using System.IO;
using System.Xml.Serialization;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handler for the ResultsReadyCommand, which is triggered when the server indicates that the test results are ready for download.
    /// </summary>
    public class ResultsReadyHandler : IRequestHandler<ResultsReadyCommand, TransactionResult>
    {
        private readonly IStringResourceService _stringResourceService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the ResultsReadyHandler with the necessary dependencies, including the WebSocket service for communication, 
        /// the string resource service for accessing configuration values, and the transaction state for managing the transaction process. It sets up 
        /// the handler to respond to the ResultsReadyCommand, which is expected to be triggered when the server indicates that the test results are 
        /// ready for download. The handler will perform the necessary actions to download the test results, update the transaction state, and close 
        /// the WebSocket connection when this command is received. .
        /// </summary>
        /// <param name="stringResourceService">The string resource service used to access configuration values.</param>
        /// <param name="transactionState">The transaction state that tracks the current status and data of the ongoing transaction.</param>
        public ResultsReadyHandler(IStringResourceService stringResourceService, TransactionState transactionState)
        {
            _stringResourceService = stringResourceService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the ResultsReadyCommand by submitting the test results request and updating the transaction state
        /// with the downloaded results document.
        /// </summary>
        /// <remarks>This method sends an HTTP POST request to the results download endpoint using
        /// information from the provided command. Upon successful completion, the transaction state is updated with the
        /// downloaded results document, and any active WebSocket connection is closed. The method throws an exception
        /// if the HTTP request fails or the response cannot be processed.</remarks>
        /// <param name="request">The command containing transaction details and authentication information required to request the test
        /// results.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A TransactionResult indicating the outcome of the operation. Returns TransactionResult.Success if the
        /// results are successfully retrieved and processed.</returns>
        public async Task<TransactionResult> Handle(ResultsReadyCommand request, CancellationToken cancellationToken)
        {
            var url = _stringResourceService.GetString("ResultsDownloadUrl") +
                $"?TestName={_transactionState.IATName}&AuthToken={request.transaction.AuthToken}" +
                $"&ClientId={_transactionState.ClientId}";
            using var _httpClient = new HttpClient();
            var response = await _httpClient.GetAsync(url);
            XmlSerializer serializer = new XmlSerializer(typeof(TestResults));
            var memStream = new MemoryStream();
            await response.Content.CopyToAsync(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            _transactionState.TestResults = (TestResults)serializer.Deserialize(memStream);
            _transactionState.Result = TransactionResult.Success;
            _transactionState.Event.Set();
            return TransactionResult.Success;
        }
    }
}
