using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.ExceptionServices;
using System.Net.Http;
using System.IO;
using MediatR;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using IAT.Core.Services;
using IAT.Core.Models;
using IAT.Core.Services.Network;

namespace IAT.Core.Handlers
{
    /// <summary>
    /// Handles the ItemSlidesReadyCommand by downloading item slide data and updating the transaction state accordingly.
    /// </summary>
    public class ItemSlidesReadyHandler : IRequestHandler<ItemSlidesReadyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IStringResourceService _stringResourceService;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _transactionState;

        /// <summary>
        /// The constructor initializes the ItemSlidesReadyHandler with the necessary dependencies, including the WebSocket service 
        /// for communication, the string resource service for accessing string resources, the dialog service for displaying notifications, 
        /// and the transaction state for managing the transaction process. This setup allows the handler to effectively manage the item 
        /// slides ready scenario by downloading the item slide data, processing it, and providing user feedback through notifications if 
        /// any errors occur during the download process. The handler is designed to ensure that the WebSocket connection is properly 
        /// closed in case of errors, and that the user is informed of any issues that arise during the item slide data retrieval process.
        /// </summary>
        /// <param name="webSocketService">The WebSocket service used to manage the connection.</param>
        /// <param name="stringResourceService">The string resource service used to retrieve localized messages.</param>
        /// <param name="dialogService">The dialog service used to show notifications to the user.</param>
        /// <param name="transactionState">The transaction state used to manage the transaction process.</param>
        public ItemSlidesReadyHandler(IWebSocketService webSocketService, IStringResourceService stringResourceService,
            IDialogService dialogService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _stringResourceService = stringResourceService;
            _dialogService = dialogService;
            _transactionState = transactionState;
        }

        /// <summary>
        /// Handles the ItemSlidesReadyCommand by downloading item slide data and updating the transaction state
        /// accordingly.
        /// </summary>
        /// <remarks>If an error occurs during the download process, a notification is displayed to the
        /// user and the WebSocket connection is closed before returning a failure result. On success, the slide data is
        /// processed and the WebSocket connection is closed.</remarks>
        /// <param name="request">The command containing information required to process the item slides readiness operation.</param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a TransactionResult indicating
        /// the outcome of the operation.</returns>
        public async Task<TransactionResult> Handle(ItemSlidesReadyCommand request, CancellationToken cancellationToken)
        {

            using var httpClient = new HttpClient();
            var retVal = await httpClient.GetByteArrayAsync(_stringResourceService.GetString("sItemSlideDownloadURL")
                    + $"IATName={_transactionState.IATName}&ClientID={_transactionState.ClientId}&DownloadKey={request.transaction.StringValues["DownloadKey"]}")
                    .ContinueWith(async t =>
            {
                if (t.IsFaulted)
                {
                    await _dialogService.ShowNotificationAsync("An error occurred while downloading item slide data. Please try again.", "Error Downloading Item Slides");
                    await _webSocketService.CloseSocketAsync();
                    ExceptionDispatchInfo.Capture(t.Exception).Throw();
                    return TransactionResult.ServerFailure;
                }
                else
                {
                    using var receipt = new MemoryStream(t.Result);
                    var slideData = new List<byte[]>();

                    if (_transactionState.SlideManifest != null)
                    {
                        var fileList = _transactionState.SlideManifest.Contents.Where(fe => fe.FileEntityType == FileEntity.EFileEntityType.File).Cast<ManifestFile>()
                            .Where(mf => mf.ResourceType == FileResourceType.itemSlide).ToList();
                        foreach (var file in fileList)
                        {
                            file.Content = new byte[file.Size];
                            receipt.Read(file.Content, 0, (int)file.Size);
                        }
                    }
                    await _webSocketService.CloseSocketAsync();
                    return TransactionResult.Success;
                }
            }).Result;
            _transactionState.Result = retVal;
            _transactionState.Event.Set();
            return retVal;
        }
    }
}
