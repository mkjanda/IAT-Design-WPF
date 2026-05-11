using IAT.Core.Serializable;
using IAT.Core.Handlers;
using IAT.Core.Models;

namespace IAT.Core.Services.Network
{
    /// <summary>
    /// The IGetItemSlidesService interface defines a contract for a service that retrieves item slides based on a product key, IAT name, and password.
    /// </summary>
    public interface IGetItemSlidesService
    {
        Task<Manifest> GetItemSlides(string productKey, string iatName, string password);
    }


    /// <summary>
    /// Provides functionality to retrieve slide manifests for items by communicating with a remote service using
    /// WebSocket transactions.
    /// </summary>
    /// <remarks>This service coordinates the process of requesting and retrieving item slide data, managing
    /// transaction state and command registration internally. It is intended to be used as part of a workflow that
    /// requires secure, transactional retrieval of slide manifests based on product credentials.</remarks>
    public class GetItemSlidesService : IGetItemSlidesService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the GetItemSlidesService class with the specified WebSocket service and
        /// transaction state.
        /// </summary>
        /// <remarks>This constructor clears the provided transaction state and sets up command handlers
        /// for specific transaction types. The service is ready for use immediately after construction.</remarks>
        /// <param name="webSocketService">The WebSocket service used to manage transaction commands and communication.</param>
        /// <param name="transactionState">The transaction state object used to track and manage the current transaction lifecycle.</param>
        public GetItemSlidesService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _transactionState.Clear();
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionRetrieveItemSlidesCommand(request);
            _webSocketService.TransactionCommands[TransactionType.PasswordValid] = (request) => new PasswordValidSlidesCommand(request);
        }

        /// <summary>
        /// Asynchronously retrieves the slide manifest for a specified product and IAT instance using the provided
        /// credentials.
        /// </summary>
        /// <remarks>This method initiates a connection and waits for the slide manifest to be received.
        /// The call will block until the manifest is available or the operation is otherwise completed.</remarks>
        /// <param name="productKey">The unique key identifying the product for which to retrieve slides.</param>
        /// <param name="iatName">The name of the IAT (Implicit Association Test) instance associated with the product.</param>
        /// <param name="password">The password used to authenticate the request for the specified IAT instance.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a Manifest object with the slide
        /// information for the specified product and IAT instance.</returns>
        public async Task<Manifest> GetItemSlides(string productKey, string iatName, string password)
        {
            _webSocketService.Start();
            _transactionState.ProductKey = productKey;
            _transactionState.IATName = iatName;
            _transactionState.Password = password;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestConnection,
                ProductKey = productKey,
            });
            _transactionState.Event.WaitOne();
            return _transactionState.SlideManifest;
        }
    }
}
