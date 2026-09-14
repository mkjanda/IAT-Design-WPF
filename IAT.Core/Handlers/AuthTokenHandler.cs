using System;
using System.Collections.Generic;
using System.Text;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using IAT.Core.Models;
using IAT.Core.ResultData; 
using IAT.Core.Serializable;
using System.Net.Http;
using System.IO;
using IAT.Core.Services;
using System.Xml.Serialization;


namespace IAT.Core.Handlers
{
    internal class AuthTokenHandler : IRequestHandler<AuthTokenCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly IStringResourceService _strings;
        private readonly IDecryptor _decryptor;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthTokenHandler"/> class.
        /// </summary>
        /// <param name="webSocketService">The web socket service.</param>
        /// <param name="transactionState">The transaction state.</param>
        /// <param name="stringResourceService">The string resource service.</param>
        /// <param name="decryptor">The decryptor service.</param>
        public AuthTokenHandler(IWebSocketService webSocketService, TransactionState transactionState, IStringResourceService stringResourceService, IDecryptor decryptor)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _strings = stringResourceService;
            _decryptor = decryptor;
        }

        /// <summary>
        /// Handles the specified <see cref="AuthTokenCommand"/> request and returns a <see cref="TransactionResult"/>. 
        /// </summary>
        /// <param name="request">The <see cref="AuthTokenCommand"/> request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="TransactionResult"/> representing the result of the transaction.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the operation type is unsupported.</exception>
        public async Task<TransactionResult> Handle(AuthTokenCommand request, CancellationToken cancellationToken)
        {
            _transactionState.AuthToken = request.transaction.AuthToken;
            switch (_transactionState.Operation)
            {
                case OperationType.RetrieveResults:
                    var client = new HttpClient();
                    var requestBody = new HttpRequestMessage(HttpMethod.Get, $"{_strings.GetString("ResultDownloadUrl")}?" +
                        $"clientId={_transactionState.ClientId}&" +
                        $"iatName={_transactionState.IATName}&" +
                        $"authToken={_transactionState.AuthToken}");
                    var httpResponse = await client.SendAsync(requestBody);
                    var memStream = new MemoryStream();
                    await httpResponse.Content.CopyToAsync(memStream);
                    memStream.Seek(0L, SeekOrigin.Begin);
                    var serializer = new XmlSerializer(typeof(TestResults));
                    _transactionState.TestResults = serializer.Deserialize(memStream) as TestResults ?? 
                        throw new InvalidOperationException("Null value received at test results.");
                    _transactionState.SetResult(TransactionResult.Success);
                    var rsa = _decryptor.GetRSA(_transactionState.Password);
                    foreach (var encryptedResultSet in _transactionState.TestResults.EncryptedResultSets)
                    {
                        var resultSet = _decryptor.DecryptResultSet(encryptedResultSet, rsa);
                        if (resultSet is not null)
                            _transactionState.TestResults.ResultSets.Add(resultSet);
                    }
                    return TransactionResult.Success;
/*
                    await _webSocketService.SendMessage(new TransactionRequest()
                    {
                        Type = TransactionType.RequestItemSlideManifest,
                        IATName = _transactionState.IATName,
                        ProductKey = _transactionState.ProductKey
                    });*/

                case OperationType.DeleteTest:
                    await _webSocketService.SendMessage(new TransactionRequest()
                    {
                        Type = TransactionType.DeleteIAT,
                        IATName = _transactionState.IATName
                    });
                    break;
                case OperationType.DeleteResults:
                    await _webSocketService.SendMessage(new TransactionRequest()
                    {
                        Type = TransactionType.DeleteIATData,
                        IATName = _transactionState.IATName
                    });
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported operation type: {_transactionState.Operation}");
            }
            return TransactionResult.Unset;
        }
    }
}
