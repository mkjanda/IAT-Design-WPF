using IAT.Core.Enumerations;
using IAT.Core.Handlers;
using IAT.Core.Models;
using IAT.Core.Serializable;


namespace IAT.Core.Services.Network
{
    /// <summary>
    /// The interface for the service responsible for resending email verification messages. It defines a method that takes a product 
    /// key and an email address, and returns a transaction result indicating the success or failure of the resend operation. This service 
    /// is typically used in scenarios where a user needs to have their email verification message resent, such as when they did not receive 
    /// the initial email or if they need to verify their email address again for any reason.
    /// </summary>
    public interface IResendEmailVerificationService
    {
        /// <summary>
        /// Resends an email verification message.
        /// </summary>
        /// <param name="productKey">The product key.</param>
        /// <param name="email">The email address to send the verification message to.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the transaction result.</returns>
        Task<TransactionResult> ResendEmailVerification(string productKey, string email);
    }

    /// <summary>
    /// Reissues verification email for a given product key and email address. This service is used when the user has not received 
    /// the initial verification email or needs to have it sent again for any reason. It initiates the resend process by sending a 
    /// request to the server and waits for the result of the transaction.
    /// </summary>
    public class ResendEmailVerificationService : IResendEmailVerificationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResendEmailVerificationService"/> class.
        /// </summary>
        /// <param name="webSocketService">The web socket service used for communication.</param>
        /// <param name="transactionState">The transaction state.</param>
        public ResendEmailVerificationService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionResendEMailCommand(request);
        }

        /// <summary>
        /// Resends an email verification for the specified product key and email address.
        /// </summary>
        /// <param name="productKey">The product key associated with the verification request.</param>
        /// <param name="email">The email address to send the verification to.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the transaction result.</returns>
        public async Task<TransactionResult> ResendEmailVerification(string productKey, string email)
        {
            _webSocketService.Start();
            _transactionState.Email = email;
            _transactionState.ProductKey = productKey;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestConnection,
                ProductKey = productKey
            });
            _transactionState.Event.WaitOne();
            return _transactionState.Result;
        }
    }
}
