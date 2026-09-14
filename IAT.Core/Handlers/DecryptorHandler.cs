using MediatR;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;
using IAT.Core.Services;
using IAT.Core.Results;
using IAT.Core.ResultData;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handles the server-supplied encrypted RSA key: verifies the transaction password against it,
    /// stores the key on success, and signals <see cref="TransactionResult.InvalidPassword"/> on failure
    /// without leaving half-decrypted key material in <see cref="TransactionState"/>.
    /// </summary>
    public class DecryptorHandler : IRequestHandler<DecryptorCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;
        private readonly ILocalStorageService _localStorage;
        private readonly Decryptor _decryptor;

        public DecryptorHandler(IWebSocketService webSocketService, TransactionState transactionState, ILocalStorageService localStorage,
            Decryptor decryptor)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _localStorage = localStorage;
            _decryptor = decryptor;
        }

        /// <summary>
        /// Decrypts and validates the RSA key with the current transaction password.
        /// On failure: clears RSA state, completes the transaction with InvalidPassword, and does not
        /// send PasswordValid (so the pipeline stops cleanly with no further handlers touching a null key).
        /// </summary>
        public async Task<TransactionResult> Handle(DecryptorCommand request, CancellationToken cancellationToken)
        {
            _transactionState.Decryptor = request.decryptor;

            var password = _transactionState.Password ?? string.Empty;
            if (_transactionState.Operation == OperationType.DeleteTest ||
                _transactionState.Operation == OperationType.DeleteResults)
            {
                if (!_decryptor.TestPassword(password))
                {
                    // TestPassword already resets decrypted state on failure; drop the key from state
                    // so no later consumer can call GetRSAParameters on a bad instance.
                    _transactionState.Decryptor = new RsaParams();
                    _transactionState.SetResult(TransactionResult.InvalidPassword);
                    // Do not close the socket here — Deploy tab owns connection lifetime and will
                    // reconnect on the next action via Start(). Closing would race with the UI await.
                    return TransactionResult.InvalidPassword;
                }
                await _webSocketService.SendMessage(new TransactionRequest
                {
                    Type = TransactionType.PasswordValid,
                    ProductKey = _transactionState.ProductKey
                }).ConfigureAwait(false);
            }
            // Pipeline continues; Completion is signaled by a terminal handler (ResultsReady, etc.).
            return TransactionResult.Unset;
        }
    }
}
