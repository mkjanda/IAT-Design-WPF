using com.sun.xml.@internal.messaging.saaj.soap;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using IAT.Core.Models;

namespace IAT.Core.Services
{
    public class EMailConfirmationService
    {
        private LocalStorageService _localStorageService;
        private String ActivationKey = String.Empty;
        private ClientWebSocket EMailUtilityWebSocket = new();
        private CancellationToken AbortToken = new();
        private ManualResetEvent OpComplete = new(false), OpFailed = new(false);
        private object transmissionLock = new();
        private ArraySegment<byte> ReceiveBuffer = new(new byte[8192]);
        private Envelope IncomingMessage = null;
        public enum EConfirmResult { failed, emailMismatch, noSuchClient, success };
        private EConfirmResult Result = EConfirmResult.failed;
        public TransactionRequest FinalTransaction { get; private set; }

        private void ReportError(String caption, CReportableException rex)
        {
            try
            {
                HttpClient uploader = new();
                CClientException clientEx = new CClientException(caption, rex);
                uploader.Headers.Add("Content-type: text/xml");
                if (Encoding.UTF8.GetString(uploader.UploadData(Properties.Resources.sErrorReportURL, clientEx.GetXmlBytes())) == "success")
                {
                    MessageBox.Show(String.Format(Properties.Resources.sErrorReportedMessage, LocalStorage.Activation[LocalStorage.Field.ProductKey]), Properties.Resources.sErrorReportedCaption);
                    return;
                }
            }
            catch (Exception e) { }
            ErrorReportDisplay f = new ErrorReportDisplay(rex);
            f.ShowDialog();
        }

        public EConfirmResult ConfirmEMailVerification()
        {
            if (LocalStorage.Activation[LocalStorage.Field.UserEmail] == null)
                return EConfirmResult.failed;
            OpComplete.Reset();
            OpFailed.Reset();
            EMailUtilityWebSocket = new ClientWebSocket();
            Envelope.ClearMessageMap();
            Envelope.OnReceipt[Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(Confirmation_OnTransaction);
            Envelope.OnReceipt[Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(OnHandshake);
            Task connectTask = EMailUtilityWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), AbortToken);
            int nSecsWaited = 0;
            while ((nSecsWaited++ < 30) && !connectTask.IsCompleted)
                ((Func<Task>)(async () => await Task.Delay(1000)))().Wait();
            if ((nSecsWaited >= 30) || connectTask.IsFaulted)
            {
                MessageBox.Show(Properties.Resources.sConnectionTimeoutCaption, Properties.Resources.sConnectionTimeoutCaption);
                EMailUtilityWebSocket.Dispose();
                return EConfirmResult.failed;
            }
            StartMessageReceiver();
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            Envelope env = new Envelope(trans);
            env.SendMessage(EMailUtilityWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { OpComplete, OpFailed });
            return Result;
        }

        private void StartMessageReceiver()
        {
            Task<WebSocketReceiveResult> receiveTask = EMailUtilityWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
            receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
        }

        private void ReceiveMessage(Task<WebSocketReceiveResult> t)
        {
            if (t.IsCanceled)
                return;
            if (t.IsFaulted)
                return;
            try
            {
                if (t.Result.Count != 0)
                {
                    lock (transmissionLock)
                    {
                        WebSocketReceiveResult receipt = t.Result;
                        if (receipt.MessageType == WebSocketMessageType.Close)
                            EMailUtilityWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", new CancellationToken()).ContinueWith((t) =>
                            {
                                if (t.IsCompleted)
                                    EMailUtilityWebSocket.Dispose();
                            });
                        try
                        {
                            if (receipt.EndOfMessage)
                            {
                                if (IncomingMessage == null)
                                    IncomingMessage = new Envelope();
                                if (IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), true))
                                    IncomingMessage = null;
                            }
                            else
                            {
                                if (IncomingMessage == null)
                                    IncomingMessage = new Envelope();
                                IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), false);

                            }
                        }
                        catch (CXmlSerializationException ex)
                        {
                            ErrorReporter.ReportActivationError(ex);
                            OpFailed.Set();
                        }
                    }
                }
                if ((EMailUtilityWebSocket.State == WebSocketState.Open) || (EMailUtilityWebSocket.State == WebSocketState.CloseSent))
                {
                    Task<WebSocketReceiveResult> receiveTask = EMailUtilityWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                    receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
                }
            }
            catch (Exception ex)
            {
                OpFailed.Set();
            }
        }


        public void ResendEMailVerification()
        {
            OpComplete.Reset();
            OpFailed.Reset();
            Envelope.ClearMessageMap();
            Envelope.OnReceipt[Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(ResendConfirmationEMail_OnTransaction);
            Envelope.OnReceipt[Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(OnHandshake);
            EMailUtilityWebSocket = new ClientWebSocket();
            Task connectTask = EMailUtilityWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), AbortToken);
            int nSecsWaited = 0;
            while ((nSecsWaited++ < 30) && !connectTask.IsCompleted)
                ((Func<Task>)(async () => await Task.Delay(1000)))().Wait();
            if ((nSecsWaited >= 30) || connectTask.IsFaulted)
            {
                MessageBox.Show(Properties.Resources.sConnectionTimeoutMessage, Properties.Resources.sConnectionTimeoutMessage);
                EMailUtilityWebSocket.Dispose();
                FinalTransaction = null;
            }
            StartMessageReceiver();
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            Envelope env = new Envelope(trans);
            env.SendMessage(EMailUtilityWebSocket, AbortToken);
            WaitHandle.WaitAny(new WaitHandle[] { OpComplete, OpFailed });
        }


        private void Confirmation_OnTransaction(INamedXmlSerializable transaction)
        {
            TransactionRequest inTrans = (TransactionRequest)transaction;
            switch (inTrans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    TransactionRequest outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEMailVerification;
                    outTrans.StringValues["email"] = LocalStorage.Activation[LocalStorage.Field.UserEmail];
                    Envelope env = new Envelope(outTrans);
                    env.SendMessage(EMailUtilityWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.TransactionSuccess:
                    ActivationKey = inTrans.ActivationKey;
                    LocalStorage.Activation[LocalStorage.Field.ActivationKey] = ActivationKey;
                    Result = EConfirmResult.success;
                    OpComplete.Set();
                    break;

                case TransactionRequest.ETransaction.TransactionFail:
                    Result = EConfirmResult.failed;
                    OpFailed.Set();
                    break;

                case TransactionRequest.ETransaction.NoSuchClient:
                    Result = EConfirmResult.noSuchClient;
                    OpFailed.Set();
                    break;

                case TransactionRequest.ETransaction.EmailVerificationMismatch:
                    Result = EConfirmResult.emailMismatch;
                    OpFailed.Set();
                    break;

                default:
                    throw new CUnexpectedServerMessage("Unexpected message from server while confirming email activation", transaction);
                    break;
            }
        }

        private void ResendConfirmationEMail_OnTransaction(INamedXmlSerializable transaction)
        {
            TransactionRequest inTrans = (TransactionRequest)transaction;
            switch (inTrans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    TransactionRequest outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestNewVerificationEMail;
                    outTrans.StringValues["email"] = LocalStorage.Activation[LocalStorage.Field.UserEmail];
                    Envelope env = new Envelope(outTrans);
                    env.SendMessage(EMailUtilityWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.TransactionSuccess:
                    FinalTransaction = inTrans;
                    OpComplete.Set();
                    break;

                case TransactionRequest.ETransaction.TransactionFail:
                    FinalTransaction = inTrans;
                    OpComplete.Set();
                    break;

                case TransactionRequest.ETransaction.EMailAlreadyVerified:
                    FinalTransaction = inTrans;
                    OpComplete.Set();
                    break;
            }
        }

        private void OnHandshake(INamedXmlSerializable inHand)
        {
            HandShake outHand = HandShake.CreateResponse((HandShake)inHand);
            Envelope env = new Envelope(outHand);
            env.SendMessage(EMailUtilityWebSocket, AbortToken);
        }


    }
}
