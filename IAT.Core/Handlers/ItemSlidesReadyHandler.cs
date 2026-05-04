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

namespace IAT.Core.Handlers
{
    public class ItemSlidesReadyHandler : IRequestHandler<ItemSlidesReadyCommand, TransactionResult>
    {
        private readonly IWebSocketService _webSocketService;
        private readonly IStringResourceService _stringResourceService;
        private readonly IDialogService _dialogService;
        private readonly TransactionState _transactionState;

        public ItemSlidesReadyHandler(IWebSocketService webSocketService, IStringResourceService stringResourceService,
            IDialogService dialogService, TransactionState transactionState)
        {
            _webSocketService = webSocketService;
            _stringResourceService = stringResourceService;
            _dialogService = dialogService;
            _transactionState = transactionState;
        }   
     
        public async Task<TransactionResult> Handle(ItemSlidesReadyCommand request, CancellationToken cancellationToken)
        {

            using var httpClient = new HttpClient();
            return await httpClient.GetByteArrayAsync(_stringResourceService.GetString("sItemSlideDownloadURL")).ContinueWith(async t =>
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

                    var fileList = _transactionState.SlideManifest.Contents.Where(fe => fe.FileEntityType == FileEntity.EFileEntityType.File).Cast<ManifestFile>().Where(mf => mf.ResourceType == ManifestFile.EResourceType.itemSlide).ToList();
                    foreach (var file in fileList)
                    {
                        file.Content = new byte[file.Size];
                        receipt.Read(file.Content, 0, (int)file.Size);
                    }
                    await _webSocketService.CloseSocketAsync();
                    return TransactionResult.Success;
                }
            }).Result;
        }
    }
}
