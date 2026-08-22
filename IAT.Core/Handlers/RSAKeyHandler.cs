using MediatR;
using IAT.Core.Serializable;
using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handles the server-supplied encrypted RSA key: verifies the transaction password against it,
    /// stores the key on success, and signals <see cref="TransactionResult.InvalidPassword"/> on failure
    /// without leaving half-decrypted key material in <see cref="TransactionState"/>.
    /// </summary>
    public class RSAKeyHandler : IRequestHandler<RSAKeyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public RSAKeyHandler(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService ?? throw new ArgumentNullException(nameof(webSocketService));
            _transactionState = transactionState ?? throw new ArgumentNullException(nameof(transactionState));
        }

        /// <summary>
        /// Decrypts and validates the RSA key with the current transaction password.
        /// On failure: clears RSA state, completes the transaction with InvalidPassword, and does not
        /// send PasswordValid (so the pipeline stops cleanly with no further handlers touching a null key).
        /// </summary>
        public async Task<TransactionResult> Handle(RSAKeyCommand request, CancellationToken cancellationToken)
        {
            if (request?.Key is null)
            {
                _transactionState.RSA = new EncryptedRSAKey();
                _transactionState.SetResult(TransactionResult.InvalidPassword);
                return TransactionResult.InvalidPassword;
            }

            _transactionState.RSA = request.Key;

            var password = _transactionState.Password ?? string.Empty;
            if (!request.Key.TestPassword(password))
            {
                // TestPassword already resets decrypted state on failure; drop the key from state
                // so no later consumer can call GetRSAParameters on a bad instance.
                _transactionState.RSA = new EncryptedRSAKey();
                _transactionState.SetResult(TransactionResult.InvalidPassword);
                // Do not close the socket here — Deploy tab owns connection lifetime and will
                // reconnect on the next action via Start(). Closing would race with the UI await.
                return TransactionResult.InvalidPassword;
            }

            await _webSocketService.SendMessage(new TransactionRequest
            {
                Type = TransactionType.PasswordValid
            }).ConfigureAwait(false);

            // Pipeline continues; Completion is signaled by a terminal handler (ResultsReady, etc.).
            return TransactionResult.Unset;
        }
    }
}
