using IAT.Core.Serializable;
using IAT.Core.Handlers;
using IAT.Core.Models;

namespace IAT.Core.Services
{
    interface IGetItemSlidesService
    {
        Task<Manifest> GetItemSlides(string productKey, string iatName, string password);
    }
    internal class GetItemSlidesService : IGetItemSlidesService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        public GetItemSlidesService(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestTransmission] = (request) => new RequestTransmissionRetrieveItemSlidesCommand(request);
            _webSocketService.TransactionCommands[TransactionType.PasswordValid] = (request) => new PasswordValidSlidesCommand(request);
        }

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
