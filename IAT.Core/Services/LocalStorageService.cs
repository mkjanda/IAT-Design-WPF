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
using java.io;
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
        private static readonly int KeyBytes = 32;
        private static readonly int NonceBytes = 12;
        private static readonly int TagBytes = 16;

        private XDocument? ActivationDocument { get; set; }
        private Dictionary<string, string> ActivationFileContents = []; 


        /// <summary>
        /// Initializes a new instance of the LocalStorageService class and ensures that activation data is loaded or
        /// created as needed.
        /// </summary>
        /// <remarks>If activation data exists in the registry, it is copied to local storage. If
        /// activation data exists in the expected file location, it is loaded; otherwise, a new activation document is
        /// created and saved. This constructor ensures that the local storage is properly initialized for subsequent
        /// operations.</remarks>
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


}



