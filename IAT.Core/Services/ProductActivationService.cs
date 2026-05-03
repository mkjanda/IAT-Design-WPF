using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using javax.sql;
using javax.xml.soap;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Runtime.ExceptionServices;
using System.Text;
using static javax.jws.soap.SOAPBinding;

namespace IAT.Core.Services
{
    interface IProductActivationService
    {
        Task Activate(string productKey, string email, string user);
    }

    public class ProductActivationService : IProductActivationService
    {
        private readonly IWebSocketService _wss;
        public ProductActivationService(IWebSocketService wss)
        {
            _wss = wss;
        }

        /// <summary>
        /// Initiates the product activation process using the specified user and product information.
        /// </summary>
        /// <remarks>If the activation server cannot be reached, the activation result is set to indicate
        /// failure and an error notification is displayed to the user. The method will rethrow any exception
        /// encountered during the connection attempt.</remarks>
        /// <returns>A task that represents the asynchronous activation operation.</returns>
        public async Task Activate(String productKey, string email, string userName)
        {
            Use = WebSocketUse.ActivateProduct;
            _productKey = productKey;
            _email = email;
            _userName = userName;

            WebSocket = new ClientWebSocket();
            try
            {
                await WebSocket.ConnectAsync(new Uri(_stringResourceService["WebSocketUri"]), new CancellationToken(false));
                StartMessageReceiver();
                var outTrans = new TransactionRequest()
                {
                    Transaction = TransactionType.RequestConnection,
                    ProductKey = _productKey
                };
                SendMessage(outTrans);
                ResetEvent.WaitOne();
            }
            catch (Exception ex)
            {
                Result = TransactionResult.CannotConnect;
                _userNotificationService.ShowError(new ErrorNotificationMessage("Cannot Activate Product",
                    "An error occurred while attempting to connect to the activation server. Please check your internet connection and try again.", ex));
                ExceptionDispatchInfo.Capture(ex).Throw();
                WebSocket.Dispose();
            }

        }
    }
}
