using IAT.Core.Enumerations;
using IAT.Core.Models;
using IAT.Core.Serializable;
using IAT.Core.Handlers;
using System;
using System.Collections.Generic;
using System.Text;

namespace IAT.Core.Services.Network
{
    /// <summary>
    /// Defines operations for retrieving item slides.
    /// </summary>
    public interface IItemSlideRetriever
    {
        /// <summary>
        /// Retrieves the item slides for the specified IAT and product.
        /// </summary>
        /// <param name="iatName">The IAT name.</param>
        /// <param name="password">The password for authentication.</param>
        /// <param name="productKey">The product key.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the transaction result.</returns>
        Task<TransactionResult> GetItemSlides(string iatName, string password, string productKey);
    }


    /// <summary>
    /// Retrieves item slides from a server using web socket communication.
    /// </summary>
    public class ItemSlideRetriever : IItemSlideRetriever
    {
        private readonly IWebSocketService _webSocketService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemSlideRetriever"/> class.
        /// </summary>
        /// <param name="webSocketService">The web socket service used for transaction communication.</param>
        /// <param name="transactionState">The transaction state.</param>
        public ItemSlideRetriever(IWebSocketService webSocketService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _transactionState = transactionState;
            _webSocketService.TransactionCommands[TransactionType.RequestItemSlides] = (request) => new RequestTransmissionItemSlideRetrievalCommand(request);
        }

        /// <summary>
        /// Sends the request to connect to the server to initiate the item 
        /// slide retrieval process and then waits for the process to complete or fail before returning.
        /// </summary>
        /// <param name="iatName"></param>
        /// <param name="password"></param>
        /// <param name="productKey"></param>
        /// <returns></returns>
        public async Task<TransactionResult> GetItemSlides(string iatName, string password, string productKey)
        {
            _transactionState.Clear();
            _transactionState.IATName = iatName;
            _transactionState.ProductKey = productKey;
            await _webSocketService.SendMessage(new TransactionRequest()
            {
                Transaction = TransactionType.RequestConnection,
                IATName = iatName,
                ProductKey = productKey,
            });
            _transactionState.Event.WaitOne();
            return TransactionResult.Unset;
        }


    }
}
