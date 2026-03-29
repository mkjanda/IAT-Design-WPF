using java.util;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using IAT.Core.Models.Enumerations;

namespace IAT.Core.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        public enum ActivationStatus { NotActivated, EMailNotVerified, Activated, InconsistentVersion };
        private static readonly int[] ProductKeyModuli = { 22, 6, 17, 30, 5, 15, 24, 2, 19, 35, 27, 2, 19, 23, 25, 20, 12, 4, 19, 31 };
        private static readonly byte[] DESData = { 0xAD, 0x81, 0x56, 0x1F, 0x59, 0xE1, 0x33, 0x85 };
        private static readonly byte[] IVData = { 0x01, 0x03, 0x05, 0x03, 0x01, 0x09, 0x07, 0x05 };
        private static readonly byte[] IatDESData = { 0x78, 0xA6, 0xB5, 0x0A, 0x89, 0x5D, 0x88, 0x5E };
        private static readonly byte[] IatIVData = { 0xFB, 0x64, 0xBC, 0x6C, 0x21, 0x62, 0x20, 0x32 };
        private XDocument ActivationDocument;
        private Dictionary<String, String> ActivationFileContents = new Dictionary<String, String>();


        public LocalStorageService()
        {
            if (ActivationDataExistsInRegistry)
                CopyLocalStorageDataFromRegistry();
            if (ActivationDataExists)
                ActivationDocument = XDocument.Load(ActivationFilePath);
            else
            {
                ActivationDocument = new XDocument(new XElement("IATDesign"));
                if (!Directory.Exists(ActivationFileDirectory))
                    Directory.CreateDirectory(ActivationFileDirectory);
                ActivationDocument.Save(ActivationFilePath);
            }
        }

        private static int FromBase36(String str)
        {
            int n = 0;
            for (int ctr = str.Length - 1; ctr >= 0; ctr--)
            {
                if ((str[ctr] >= '0') && (str[ctr] <= '9'))
                    n += (int)Math.Pow(36, (str.Length - ctr - 1)) * (int)(str[ctr] - '0');
                else
                    n += (int)Math.Pow(36, (str.Length - ctr - 1)) * (int)(str[ctr] - 'A' + 10);
            }
            return n;
        }

        static public bool IsActivatedCode(String productKey, String ActivationKey)
        {
            for (int ctr = 0; ctr < productKey.Length; ctr += 4)
            {
                int n = FromBase36(ActivationKey.Substring(ctr, 4));
                int m = FromBase36(productKey.Substring(ctr >> 1, 2));
                if (n % m != ProductKeyModuli[ctr >> 2])
                    return false;
            }
            return true;
        }

        public ActivationStatus Activated
        {
            get
            {
                if (this[Field.ProductKey] == null)
                    return ActivationStatus.NotActivated;
                if (this[Field.ActivationKey] != null)
                {
                    if (IsActivatedCode(this[Field.ProductKey], this[Field.ActivationKey]))
                        return ActivationStatus.Activated;
                    else
                        return ActivationStatus.NotActivated;
                }
                EMailConfirmationService emailConfirm = new EMailConfirmationService();
                Task<EMailConfirmaion.EConfirmResult> emailConfirmationCheck = Task<EMailConfirmationService.EConfirmResult>.Run(() => emailConfirm.ConfirmEMailVerification());
                emailConfirmationCheck.Wait();
                if (emailConfirmationCheck.IsFaulted)
                    return ActivationStatus.EMailNotVerified;
                else if (emailConfirmationCheck.Result != EMailConfirmationService.EConfirmResult.success)
                    return ActivationStatus.EMailNotVerified;
                return ActivationStatus.Activated;
            }
        }


        private object activationDocumentLockObj = new object();
        public String this[Field key]
        {
            get
            {
                lock (activationDocumentLockObj)
                {

                    String value;
                    if ((value = ActivationDocument.Root.Elements().Where(elem => elem.Name == key.Name).Select(elem => elem.Value).FirstOrDefault()) == null)
                        return null;
                    if (key.Encrypted)
                    {
                        MemoryStream memStream = new MemoryStream();
                        CryptoStream cStream = new CryptoStream(memStream, DESCrypt.CreateDecryptor(DESData, IVData), CryptoStreamMode.Write);
                        cStream.Write(Convert.FromBase64String(value), 0, Convert.FromBase64String(value).Length);
                        cStream.FlushFinalBlock();
                        String result = System.Text.Encoding.UTF8.GetString(memStream.ToArray());
                        cStream.Dispose();
                        memStream.Dispose();
                        return result;
                    }
                    return value;
                }
            }
            set
            {
                lock (activationDocumentLockObj)
                {
                    if (value == null)
                    {
                        var elems = ActivationDocument.Root.Elements().Where(elem => key.Name == elem.Name);
                        foreach (var elem in elems)
                            elem.Remove();
                        ActivationDocument.Save(ActivationFilePath);
                        return;
                    }
                    String storedValue = value;
                    if (key.Encrypted)
                    {
                        MemoryStream memStream = new MemoryStream();
                        CryptoStream cStream = new CryptoStream(memStream, DESCrypt.CreateEncryptor(DESData, IVData), CryptoStreamMode.Write);
                        cStream.Write(System.Text.Encoding.UTF8.GetBytes(value), 0, System.Text.Encoding.UTF8.GetBytes(value).Length);
                        cStream.FlushFinalBlock();
                        storedValue = Convert.ToBase64String(memStream.ToArray());
                        cStream.Dispose();
                        memStream.Dispose();
                    }
                    if (ActivationDocument.Root.Elements().Select(elem => elem.Name).Contains(key.Name))
                        ActivationDocument.Root.Element(key.Name).SetValue(storedValue);
                    else
                        ActivationDocument.Root.Add(new XElement(key.Name, storedValue));
                    ActivationDocument.Save(ActivationFilePath);
                }
            }
        }

        public static String GetIATPassword(String IAT)
        {
            if (Activation.ActivationDocument.Root.Element("Tests") == null)
                return null;
            if (Activation.ActivationDocument.Root.Element("Tests").Element(IAT) == null)
                return null;
            byte[] encPass = Convert.FromBase64String(Activation.ActivationDocument.Root.Element("Tests").Element(IAT).Attribute("Password").Value);
            MemoryStream passStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(passStream, DESCrypt.CreateDecryptor(IatDESData, IatIVData), CryptoStreamMode.Write);
            cStream.Write(encPass, 0, encPass.Length);
            cStream.Flush();
            cStream.FlushFinalBlock();
            String password = System.Text.Encoding.UTF8.GetString(passStream.ToArray());
            cStream.Dispose();
            passStream.Dispose();
            return password;
        }

        public static void SetIATPassword(String iatName, String password)
        {
            MemoryStream encPassStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(encPassStream, DESCrypt.CreateEncryptor(IatDESData, IatIVData), CryptoStreamMode.Write);
            cStream.Write(Encoding.UTF8.GetBytes(password), 0, Encoding.UTF8.GetBytes(password).Length);
            cStream.Flush();
            cStream.FlushFinalBlock();
            String encPass = Convert.ToBase64String(encPassStream.ToArray());
            if (Activation.ActivationDocument.Root.Element("Tests") == null)
                Activation.ActivationDocument.Root.Add(new XElement("Tests", new XElement(iatName, new XAttribute("Password", encPass))));
            else if (Activation.ActivationDocument.Root.Element("Tests").Elements().Select(elem => elem.Name).Contains(iatName))
            {
                foreach (XAttribute attr in Activation.ActivationDocument.Root.Element("Tests").Element(iatName).Attributes())
                    attr.Remove();
                Activation.ActivationDocument.Root.Element("Tests").Element(iatName).Add(new XAttribute("Password", encPass));
            }
            else
                Activation.ActivationDocument.Root.Element("Tests").Add(new XElement(iatName, new XAttribute("Password", encPass)));
            Activation.ActivationDocument.Save(ActivationFilePath);
        }

        public static void DeleteIAT(String iatName)
        {
            if (Activation.ActivationDocument.Root.Element("Tests") == null)
                return;
            if (Activation.ActivationDocument.Root.Element("Tests").Element(iatName) == null)
                return;
            Activation.ActivationDocument.Root.Element("Tests").Element(iatName).Remove();
            Activation.ActivationDocument.Save(ActivationFilePath);
        }

        public static int RecordError(Object error)
        {
            XDocument xDoc;
            if (File.Exists(ErrorFilePath))
                xDoc = XDocument.Load(ErrorFilePath);
            else
            {
                xDoc = new XDocument();
                xDoc.Add(new XElement("Errors"));
            }
            xDoc.Root.Add(error);
            xDoc.Save(ErrorFilePath);
            return xDoc.Root.Elements().Count();
        }

        public static String ActivationFileDirectory
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create) + Path.DirectorySeparatorChar + "IATSoftware";
            }
        }

        public static String ErrorFilePath
        {
            get
            {
                return ActivationFileDirectory + Path.DirectorySeparatorChar + "Errors.xml";
            }
        }

        public static String ActivationFilePath
        {
            get
            {
                return ActivationFileDirectory + Path.DirectorySeparatorChar + "IATDesign.xml";
            }
        }

        private static bool ActivationDataExistsInRegistry
        {
            get
            {
                return Registry.CurrentUser.OpenSubKey("Software").GetSubKeyNames().Contains("IATSoftware");
            }
        }

        private static bool ActivationDataExists
        {
            get
            {
                if (!Directory.Exists(ActivationFileDirectory))
                    return false;
                if (!File.Exists(ActivationFilePath))
                    return false;
                return true;
            }
        }

        private static void CopyLocalStorageDataFromRegistry()
        {
            if (!Directory.Exists(ActivationFileDirectory))
                Directory.CreateDirectory(ActivationFileDirectory);
            XDocument xDoc;
            if (File.Exists(ActivationFilePath))
                xDoc = XDocument.Load(ActivationFilePath);
            else
            {
                xDoc = new XDocument();
                xDoc.Add(new XElement("IATDesign"));
            }
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("IATSoftware").OpenSubKey("IATClient");
            foreach (var valueName in key.GetValueNames())
            {
                String name = (valueName == "UserKey") ? "VerificationCode" : valueName;
                if (xDoc.Root.Elements().Select(elem => elem.Name).Contains(name))
                    continue;
                Field field = Field.Parse(name);
                if (field == null)
                    continue;
                if (field.Encrypted)
                    xDoc.Root.Add(new XElement(name, key.GetValue(valueName) as String));
                else
                {
                    MemoryStream memStream = new MemoryStream();
                    CryptoStream cryptoStream = new CryptoStream(memStream, DESCrypt.CreateDecryptor(DESData, IVData), CryptoStreamMode.Write);
                    cryptoStream.Write(Convert.FromBase64String(key.GetValue(valueName) as String), 0, Convert.FromBase64String(key.GetValue(valueName) as String).Length);
                    cryptoStream.Flush();
                    cryptoStream.FlushFinalBlock();
                    xDoc.Root.Add(new XElement(name, Encoding.UTF8.GetString(memStream.ToArray())));
                }
            }
            XElement testElem;
            if (!xDoc.Root.Elements().Select(elem => elem.Name).Contains("Tests"))
            {
                testElem = new XElement("Tests");
                xDoc.Root.Add(testElem);
            }
            else
                testElem = xDoc.Root.Element("Tests");
            foreach (var subKeyName in key.GetSubKeyNames())
            {
                if (Encoding.UTF8.GetString(Convert.FromBase64String(subKeyName)).Contains(" "))
                    continue;
                var subKey = key.OpenSubKey(subKeyName);
                byte[] encPass = Convert.FromBase64String(subKey.GetValue("Value") as String);
                MemoryStream keyData = new MemoryStream(Convert.FromBase64String(subKey.GetValue("Key") as String));
                byte[] des = new byte[8];
                byte[] iv = new byte[8];
                keyData.Read(des, 0, 8);
                keyData.Read(iv, 0, 8);
                MemoryStream passStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(passStream, DESCrypt.CreateDecryptor(des, iv), CryptoStreamMode.Write);
                cryptoStream.Write(encPass, 0, encPass.Length);
                cryptoStream.Flush();
                cryptoStream.FlushFinalBlock();
                cryptoStream.Dispose();
                MemoryStream encPassStream = new MemoryStream();
                cryptoStream = new CryptoStream(encPassStream, DESCrypt.CreateEncryptor(IatDESData, IatIVData), CryptoStreamMode.Write);
                cryptoStream.Write(passStream.ToArray(), 0, passStream.ToArray().Length);
                cryptoStream.Flush();
                cryptoStream.FlushFinalBlock();
                if (testElem.Elements().Select(elem => elem.Name).Contains(Encoding.UTF8.GetString(Convert.FromBase64String(subKeyName))))
                    testElem.Element(Encoding.UTF8.GetString(Convert.FromBase64String(subKeyName))).Remove();
                testElem.Add(new XElement(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(subKeyName)),
                    new XAttribute("Password", Convert.ToBase64String(encPassStream.ToArray()))));
                cryptoStream.Dispose();
                passStream.Dispose();
                encPassStream.Dispose();
            }
            xDoc.Save(ActivationFilePath);
        }

        public static void Deactivate()
        {
            if (File.Exists(ActivationFilePath))
                File.Delete(ActivationFilePath);
            try
            {
                Registry.CurrentUser.OpenSubKey("Software", true).DeleteSubKeyTree("IATSoftware");
            }
            catch (Exception whoCaresEx) { }
        }
    }
    class ActivationObject
    {
        private ManualResetEvent TransactionComplete = new ManualResetEvent(false), TransactionFailed = new ManualResetEvent(false);
        private object transmissionLock = new object();
        private ActivationResponse actResponse = null;
        public ActivationResponse.EResult ActivationResult { get; private set; } = ActivationResponse.EResult.Unset;
        private ClientWebSocket ActivationWebSocket;
        private CancellationToken AbortToken = new CancellationToken();
        private String FName, LName, EMail, Title, ProductKey;
        private ActivationConfirmation _Confirmation = null;
        private Envelope IncomingMessage;
        private Control InvokeTarget;
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8012]);
        private IATConfigMainForm MainForm
        {
            get
            {
                return Application.OpenForms[Properties.Resources.sMainFormName] as IATConfigMainForm;
            }
        }

        private ActivationDialog ActivationDlg
        {
            get
            {
                return Application.OpenForms[Properties.Resources.sActivationDialogName] as ActivationDialog;
            }
        }

        public ActivationConfirmation Confirmation
        {
            get
            {
                return _Confirmation;
            }
        }

        public ActivationObject(Control invokeTarget)
        {
            InvokeTarget = invokeTarget;
        }

        public void Activate(String fName, String lName, String eMail, String title, String productKey)
        {
            TransactionComplete.Reset();
            TransactionFailed.Reset();
            FName = fName;
            LName = lName;
            EMail = eMail;
            Title = title;
            ProductKey = productKey;
            (null as ManualResetEvent).Set();
            LocalStorageService.Activation[LocalStorageService.Field.UserEmail] = EMail;
            LocalStorageService.Activation[LocalStorageService.Field.ProductKey] = ProductKey;
            LocalStorageService.Activation[LocalStorageService.Field.UserName] = Title + " " + FName + " " + LName;
            Envelope.ClearMessageMap();
            Envelope.OnReceipt[Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(OnHandshake);
            Envelope.OnReceipt[Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnTransaction);
            Envelope.OnReceipt[Envelope.EMessageType.ActivationResponse] = new Action<INamedXmlSerializable>(OnActivationResponse);
            Envelope.OnReceipt[Envelope.EMessageType.ServerException] = (ex) =>
            {
                ActivationResult = ActivationResponse.EResult.ServerFailure;
                TransactionFailed.Set();
            };
            TransactionComplete.Reset();
            TransactionFailed.Reset();
            ActivationWebSocket = new ClientWebSocket();
            bool connected = false;
            if (!ActivationWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), AbortToken).ContinueWith((t) =>
            {
                if (!t.IsFaulted)
                {
                    connected = true;
                    StartMessageReceiver();
                }
                else
                {
                    ActivationResult = ActivationResponse.EResult.ServerFailure;
                    WebException webException = null;
                    Exception innerEx;
                    foreach (Exception ex in t.Exception.InnerExceptions)
                    {
                        innerEx = ex;
                        while (!(innerEx is WebException))
                        {
                            innerEx = ex.InnerException;
                            if (innerEx == null)
                                break;
                        }
                        if (innerEx != null)
                            if (innerEx is WebException)
                            {
                                webException = innerEx as WebException;
                                break;
                            }
                    }
                    if (webException.Response != null)
                    {
                        HttpStatusCode code = (webException.Response as HttpWebResponse).StatusCode;
                        if ((code == HttpStatusCode.BadGateway) || (code == HttpStatusCode.InternalServerError))
                            MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), Properties.Resources.sServerDown, Properties.Resources.sServerDownCaption);
                    }
                    else
                        MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), Properties.Resources.sConnectionError, Properties.Resources.sConnectionErrorCaption);
                }
            }).Wait(15000))
            {
                MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), Properties.Resources.sConnectionTimeoutMessage, Properties.Resources.sConnectionTimeoutCaption);
                ActivationWebSocket.Dispose();
                ActivationResult = ActivationResponse.EResult.ServerFailure;
            }
            if (!connected)
            {
                ActivationResult = ActivationResponse.EResult.ServerFailure;
                ActivationWebSocket.Dispose();
                return;
            }
            TransactionRequest outTrans = new TransactionRequest();
            outTrans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            outTrans.ProductKey = productKey;
            Envelope env = new Envelope(outTrans);
            env.SendMessage(ActivationWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionComplete, TransactionFailed });
            if (nTrigger == 1)
            {
                LocalStorageService.Activation[LocalStorageService.Field.ProductKey] = null;
                LocalStorageService.Activation[LocalStorageService.Field.UserName] = null;
                LocalStorageService.Activation[LocalStorageService.Field.UserEmail] = null;
            }
        }

        private void StartMessageReceiver()
        {
            Task<WebSocketReceiveResult> receiveTask = ActivationWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
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
                            ActivationWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", new CancellationToken()).ContinueWith((t) =>
                            {
                                if (t.IsCompleted)
                                    ActivationWebSocket.Dispose();
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

                            CReportableException reportable = new CReportableException(ex.Message, ex);
                            ActivationDlg.ShowErrorReport("Error receiving internet transmission", reportable);
                            ActivationResult = ActivationResponse.EResult.Unset;
                            TransactionFailed.Set();
                        }
                    }
                }
                if ((ActivationWebSocket.State == WebSocketState.Open) || (ActivationWebSocket.State == WebSocketState.CloseSent))
                {
                    Task<WebSocketReceiveResult> receiveTask = ActivationWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                    receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
                }
            }
            catch (Exception ex)
            {
                TransactionFailed.Set();
            }
        }

        private void OnHandshake(INamedXmlSerializable inHand)
        {
            HandShake outHand = HandShake.CreateResponse((HandShake)inHand);
            Envelope env = new Envelope(outHand);
            env.SendMessage(ActivationWebSocket, AbortToken);
        }

        private void OnTransaction(INamedXmlSerializable transaction)
        {
            TransactionRequest inTrans = (TransactionRequest)transaction;
            if (inTrans.Transaction == TransactionRequest.ETransaction.RequestTransmission)
            {
                ActivationRequest actRequest = new ActivationRequest();
                actRequest.ActivationCode = ProductKey;
                actRequest.EMail = EMail;
                actRequest.FName = FName;
                actRequest.LName = LName;
                actRequest.Title = Title;
                Envelope env = new Envelope(actRequest);
                env.SendMessage(ActivationWebSocket, AbortToken);
            }
            else if (inTrans.Transaction == TransactionRequest.ETransaction.NoSuchClient)
            {
                ActivationResult = ActivationResponse.EResult.InvalidProductCode;
                TransactionFailed.Set();
            }
            else if (inTrans.Transaction == TransactionRequest.ETransaction.TransactionSuccess)
            {
                ActivationResult = ActivationResponse.EResult.Success;
                TransactionComplete.Set();
            }
            else if (inTrans.Transaction == TransactionRequest.ETransaction.NoActivationsRemain)
            {
                ActivationResult = ActivationResponse.EResult.NoActivationsRemaining;
                TransactionFailed.Set();
            }
            else if (inTrans.Transaction == TransactionRequest.ETransaction.EMailAlreadyVerified)
            {
                LocalStorageService.Activation[LocalStorageService.Field.UserEmail] = EMail;
                LocalStorageService.Activation[LocalStorageService.Field.ProductKey] = ProductKey;
                LocalStorageService.Activation[LocalStorageService.Field.UserName] = Title + " " + FName + " " + LName;
                LocalStorageService.Activation[LocalStorageService.Field.ActivationKey] = inTrans.ActivationKey;
                ActivationResult = ActivationResponse.EResult.EmailAlreadyVerified;
                TransactionComplete.Set();
            }

        }

        private void OnActivationResponse(INamedXmlSerializable transaction)
        {
            actResponse = (ActivationResponse)transaction;
            if (actResponse.Result == ActivationResponse.EResult.Success)
            {
                LocalStorageService.Activation[LocalStorageService.Field.UserEmail] = EMail;
                LocalStorageService.Activation[LocalStorageService.Field.ProductKey] = ProductKey;
                LocalStorageService.Activation[LocalStorageService.Field.UserName] = Title + " " + FName + " " + LName;
                ActivationResult = ActivationResponse.EResult.Success;
                TransactionComplete.Set();
            }
            else
            {
                ActivationResult = actResponse.Result;
                TransactionFailed.Set();
            }
        }

    }

    class ActivationRequest : INamedXmlSerializable
    {
        private String _ActivationCode;
        private String _FName = String.Empty, _LName = String.Empty, _EMail = String.Empty, _Title = String.Empty;

        public String ActivationCode
        {
            get
            {
                return _ActivationCode;
            }
            set
            {
                _ActivationCode = value;
            }
        }

        public String EMail
        {
            get
            {
                return _EMail;
            }
            set
            {
                _EMail = value;
            }
        }

        public String FName
        {
            get
            {
                return _FName;
            }
            set
            {
                _FName = value;
            }
        }

        public String LName
        {
            get
            {
                return _LName;
            }
            set
            {
                _LName = value;
            }
        }

        public String Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
            }
        }


        public ActivationRequest()
        {
            _ActivationCode = String.Empty;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            _ActivationCode = reader.ReadElementString();
            _EMail = reader.ReadElementString();
            _FName = reader.ReadElementString();
            _LName = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ActivationRequest");
            writer.WriteElementString("ProductCode", ActivationCode);
            writer.WriteElementString("EMail", EMail);
            writer.WriteElementString("Title", Title);
            writer.WriteElementString("FName", FName);
            writer.WriteElementString("LName", LName);
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "ActivationRequest";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }

    public class ActivationResponse : INamedXmlSerializable
    {
        public enum EResult
        {
            Unset, NoSuchClient, InvalidRequest, ServerFailure, NoActivationsRemaining, InvalidProductCode, ClientFrozen, ClientDeleted, CannotConnect, Success,
            EmailAlreadyVerified
        };
        private EResult _Result;
        private String _VerificationCode, _ProductKey, _Province, _PostalCode, _Phone, _Name, _EMail, _Country, _City, _Address1, _Address2;
        private int _UserNum;

        public ActivationResponse() { }

        public String VerificationCode
        {
            get
            {
                return _VerificationCode;
            }
        }

        public String ProductKey
        {
            get
            {
                return _ProductKey;
            }
        }

        public String ClientProvince
        {
            get
            {
                return _Province;
            }
        }

        public String ClientPostalCode
        {
            get
            {
                return _PostalCode;
            }
        }

        public String ClientPhone
        {
            get
            {
                return _Phone;
            }
        }

        public String ClientName
        {
            get
            {
                return _Name;
            }
        }

        public String ClientEMail
        {
            get
            {
                return _EMail;
            }
        }

        public String ClientCity
        {
            get
            {
                return _City;
            }
        }

        public String ClientCountry
        {
            get
            {
                return _Country;
            }
        }

        public String ClientAddress1
        {
            get
            {
                return _Address1;
            }
        }

        public String ClientAddress2
        {
            get
            {
                return _Address2;
            }
        }

        public EResult Result
        {
            get
            {
                return _Result;
            }
            set
            {
                _Result = value;
            }
        }

        public String GetName()
        {
            return "ActivationResponse";
        }

        public void WriteXml(XmlWriter xWriter)
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement("ActivationResponse");
            _Result = (EResult)Enum.Parse(typeof(EResult), xReader.ReadElementString("ActivationResult"));
            _ProductKey = xReader.ReadElementString("ProductKey");
            _VerificationCode = xReader.ReadElementString("VerificationCode");
            _Name = xReader.ReadElementString("ClientName");
            _EMail = xReader.ReadElementString("ClientEMail");
            _Phone = xReader.ReadElementString("Phone");
            _Address1 = xReader.ReadElementString("Address1");
            _Address2 = xReader.ReadElementString("Address2");
            _City = xReader.ReadElementString("City");
            _Province = xReader.ReadElementString("Province");
            _PostalCode = xReader.ReadElementString("PostalCode");
            _Country = xReader.ReadElementString("Country");
            xReader.ReadEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}



