using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using IAT.Core.Enumerations;

namespace IAT.Core.Services
{
    public class LocalStorageService : ILocalStorageService
    {
        /// <summary>
        /// Specifies the activation status of a user or entity.
        /// </summary>
        /// <remarks>Use this enumeration to represent and check the current activation state, such as
        /// whether email verification is required or if there is a version inconsistency.</remarks>
        public enum ActivationStatus { NotActivated, EMailNotVerified, Activated, InconsistentVersion };
        private static readonly byte[] key = { 59, 207,  78,  40, 237, 240, 82, 223, 61, 99, 218, 147, 77, 174, 189, 80,
                                                240, 128, 216, 112, 182, 247, 222, 212, 104, 30, 54, 76, 56, 193, 227, 140 };
        private static readonly byte[] storageKey = { 49, 132, 90, 177, 63, 214, 120, 45, 173, 200, 34, 167, 88, 155, 201, 114,
                                                        200, 56, 173, 241, 93, 162, 205, 149, 67, 218, 132, 19, 253, 110, 244, 175 };
        private static readonly int KeyBytes = 32;
        private static readonly int NonceBytes = 12;
        private static readonly int TagBytes = 16;

        private IWebSocketService _webSocketService;
        private XDocument? ActivationDocument { get; set; }
        private Dictionary<string, string> ActivationFileContents = new();

        /// <summary>
        /// Initializes a new instance of the LocalStorageService class and ensures that activation data is loaded or
        /// created as needed.
        /// </summary>
        /// <remarks>If activation data exists in the registry, it is copied to local storage. If
        /// activation data exists in the expected file location, it is loaded; otherwise, a new activation document is
        /// created and saved. This constructor ensures that the local storage is properly initialized for subsequent
        /// operations.</remarks>
        public LocalStorageService(IWebSocketService webSocketService)
        {
            _webSocketService = webSocketService;
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

        /// <summary>
        /// Given a product key and an activation key, determines whether the activation key is valid for the product key.
        /// </summary>
        /// <param name="productKey">The product key to validate.</param>
        /// <param name="ActivationKey">The activation key to validate against the product key.</param>
        /// <returns>True if the activation key is valid for the product key; otherwise, false.</returns>
        private bool IsActivatedCode(string productKey, string ActivationKey)
        {
            AesGcm aes = new AesGcm(key, 16);
            byte[] plaintext = Encoding.UTF8.GetBytes(productKey);
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceBytes);
            byte[] tag = new byte[TagBytes];
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
            if (CryptographicOperations.FixedTimeEquals(ciphertext, Convert.FromBase64String(ActivationKey)))
                return true;
            return false;
        }

        /// <summary>
        /// Gets the current activation status based on the presence and validity of the product key and activation key, as well as 
        /// the result of email verification through the web socket service. The method checks for the existence of the product key 
        /// and activation key, validates the activation key against the product key, and interacts with the web socket service to 
        /// verify email if necessary. The returned ActivationStatus indicates whether the user is not activated, has an unverified email, 
        /// is activated, or has an inconsistent version.
        /// </summary>
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
                _webSocketService.VerifyEmail(this[Field.ProductKey], this[Field.UserEmail]);
                if (_webSocketService.ActivationKey != string.Empty)
                {
                    this[Field.ActivationKey] = _webSocketService.ActivationKey;
                    if (IsActivatedCode(this[Field.ProductKey], this[Field.ActivationKey]))
                        return ActivationStatus.Activated;
                    else
                        return ActivationStatus.NotActivated;
                }
                return ActivationStatus.EMailNotVerified;
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
                        var aes = new AesGcm(storageKey, 16);
                        byte[] ciphertext = Convert.FromBase64String(value);
                        byte[] plaintext = new byte[ciphertext.Length]; byte[] nonce = RandomNumberGenerator.GetBytes(NonceBytes); byte[] tag = new byte[TagBytes];
                        aes.Decrypt(nonce, ciphertext, tag,  plaintext);
                        return Encoding.UTF8.GetString(plaintext);
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
                        var aes = new AesGcm(storageKey, 16);
                        byte[] plaintext = Encoding.UTF8.GetBytes(value);
                        byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; byte[] ciphertext = new byte[plaintext.Length];
                        aes.Encrypt(nonce, plaintext, ciphertext, tag);
                        storedValue = Convert.ToBase64String(ciphertext);
                    }
                    if (ActivationDocument.Root.Elements().Select(elem => elem.Name).Contains(key.Name))
                        ActivationDocument.Root.Element(key.Name).SetValue(storedValue);
                    else
                        ActivationDocument.Root.Add(new XElement(key.Name, storedValue));
                    ActivationDocument.Save(ActivationFilePath);
                }
            }
        }

        public string GetIATPassword(String IAT)
        {
            if (ActivationDocument?.Root?.Element("Tests")?.Elements(IAT) == null)
                throw new InvalidOperationException("IAT not found");
            byte[] encPass = Convert.FromBase64String(ActivationDocument.Root.Element("Tests").Element(IAT).Attribute("Password").Value);
            MemoryStream passStream = new MemoryStream();
            var aes = new AesGcm(storageKey, 16);
            byte[] ciphertext = Convert.FromBase64String(ActivationDocument.Root.Element("Tests").Element(IAT).Attribute("Password").Value);
            byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; byte[] plaintext = new byte[ciphertext.Length];
            aes.Decrypt(nonce, ciphertext, tag,  plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }

        public void SetIATPassword(String iatName, String password)
        {
            var aes = new AesGcm(storageKey, 16); byte[] plaintext = Encoding.UTF8.GetBytes(password); byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; byte[] ciphertext = new byte[plaintext.Length];
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
            var encPassword = Convert.ToBase64String(ciphertext);
            if (ActivationDocument.Root.Element("Tests") == null)
                ActivationDocument.Root.Add(new XElement("Tests", new XElement(iatName, new XAttribute("Password", encPassword))));
            else if (ActivationDocument.Root.Element("Tests").Elements().Select(elem => elem.Name).Contains(iatName))
            {
                foreach (XAttribute attr in ActivationDocument.Root.Element("Tests").Element(iatName).Attributes())
                    attr.Remove();
                ActivationDocument.Root.Element("Tests").Element(iatName).Add(new XAttribute("Password", encPassword));
            }
            else
                ActivationDocument.Root.Element("Tests").Add(new XElement(iatName, new XAttribute("Password", encPassword)));
            ActivationDocument.Save(ActivationFilePath);
        }

        public void DeleteIAT(string iatName)
        {
            if (ActivationDocument.Root.Element("Tests") == null)
                return;
            if (ActivationDocument.Root.Element("Tests").Element(iatName) == null)
                return;
            ActivationDocument.Root.Element("Tests").Element(iatName).Remove();
            ActivationDocument.Save(ActivationFilePath);
        }

        public int RecordError(object error)
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


        public static void Deactivate()
        {
            if (File.Exists(ActivationFilePath))
                File.Delete(ActivationFilePath);
        }
    }
}



