using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using IAT.Core.Enumerations;
using IAT.Core.Services.Network;
using System.Collections;
using com.sun.org.apache.bcel.@internal.generic;
using System.ComponentModel.DataAnnotations;
using sun.awt.image;

namespace IAT.Core.Services
{
    /// <summary>
    /// The interface ILocalStorageService defines a contract for a service that provides access to local storage, allowing for the retrieval and storage of string 
    /// values associated with specific fields. This service can be used to manage application settings, user preferences, or any other data that needs to be persisted 
    /// locally. The implementation of this interface may vary depending on the platform (e.g., file system, database, in-memory storage) and should handle cases where 
    /// fields may not exist gracefully.
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>
        /// Gets or sets the value associated with the specified field.
        /// </summary>
        /// <remarks>If the specified field does not exist, getting the value may return null or throw an
        /// exception, depending on the implementation. Setting a value for a non-existent field may add a new entry or
        /// update an existing one.</remarks>
        /// <param name="field">The field for which to get or set the value.</param>
        /// <returns>The value associated with the specified field.</returns>
        string this[Field field] { get; set; }

        /// <summary>
        /// Attempts to retrieve the stored password for a previously deployed IAT.
        /// </summary>
        /// <param name="iatName">Deployed IAT name.</param>
        /// <returns>The decrypted password, or <c>null</c> if no password is stored or decryption fails.</returns>
        string? TryGetIATPassword(string iatName);
    }


    /// <summary>
    /// LocalStorageService is a concrete implementation of the ILocalStorageService interface that provides access to local storage using XML files. It 
    /// manages activation data, including product keys and activation keys, and interacts with a web socket service for email verification. The service 
    /// uses AES encryption to securely store sensitive information and ensures that activation data is properly loaded or created as needed. Additionally, 
    /// it provides methods for managing IAT passwords and recording errors in a structured XML format. This implementation is designed to be thread-safe and 
    /// handles various activation states through the ActivationStatus enumeration.
    /// </summary>
    public class LocalStorageService : ILocalStorageService
    {
        /// <summary>
        /// Specifies the activation status of a user or entity.
        /// </summary>
        /// <remarks>Use this enumeration to represent and check the current activation state, such as
        /// whether email verification is required or if there is a version inconsistency.</remarks>
        public enum ActivationStatus { 
            /// <summary>
            /// The user or entity has not been activated.
            /// </summary>
            NotActivated,
            /// <summary>
            /// The user's email has not been verified.
            /// </summary>
            EMailNotVerified,
            /// <summary>
            /// The user or entity is activated.
            /// </summary>
            Activated,
            /// <summary>
            /// There is a version inconsistency.
            /// </summary>
            InconsistentVersion
        };
        private static readonly byte[] key = { 59, 207,  78,  40, 237, 240, 82, 223, 61, 99, 218, 147, 77, 174, 189, 80,
                                                240, 128, 216, 112, 182, 247, 222, 212, 104, 30, 54, 76, 56, 193, 227, 140 };
        private static readonly byte[] storageKey = { 49, 132, 90, 177, 63, 214, 120, 45, 173, 200, 34, 167, 88, 155, 201, 114,
                                                        200, 56, 173, 241, 93, 162, 205, 149, 67, 218, 132, 19, 253, 110, 244, 175 };
        private static readonly int NonceBytes = 12;
        private static readonly int TagBytes = 16;
        private static readonly Random random = new Random();
        private IEmailVerificationService _emailVerificationService;
        private readonly XDocument ActivationDocument;

        /// <summary>
        /// Initializes a new instance of the LocalStorageService class and ensures that activation data is loaded or
        /// created as needed.
        /// </summary>
        /// <remarks>If activation data exists in the registry, it is copied to local storage. If
        /// activation data exists in the expected file location, it is loaded; otherwise, a new activation document is
        /// created and saved. This constructor ensures that the local storage is properly initialized for subsequent
        /// operations.</remarks>
        public LocalStorageService(IEmailVerificationService emailVerificationService)
        {
            _emailVerificationService = emailVerificationService ?? throw new ArgumentNullException(nameof(emailVerificationService));
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
            byte[] bytes = Encoding.UTF8.GetBytes(ActivationKey);
            byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; byte[] ciphertext = new byte[bytes.Length - NonceBytes - TagBytes];
            byte[] plaintext = new byte[ciphertext.Length];
            var memStream = new MemoryStream(bytes);
            memStream.Read(nonce); memStream.Read(ciphertext); memStream.Read(tag);
            AesGcm aes = new AesGcm(key, TagBytes);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            if (CryptographicOperations.FixedTimeEquals(plaintext, Encoding.UTF8.GetBytes(productKey)))
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
                var transResult = _emailVerificationService.VerifyEmail(this[Field.ProductKey], this[Field.UserEmail]).Result;
                if (transResult != TransactionResult.Success)
                    return ActivationStatus.EMailNotVerified;
                if (_emailVerificationService.ActivationKey != string.Empty)
                {
                    this[Field.ActivationKey] = _emailVerificationService.ActivationKey;
                    if (IsActivatedCode(this[Field.ProductKey], this[Field.ActivationKey]))
                        return ActivationStatus.Activated;
                    else
                        return ActivationStatus.NotActivated;
                }
                return ActivationStatus.EMailNotVerified;
            }
        }


        /// <summary>
        /// Indexer on LocalStorageService to access and modify values associated with specific fields. 
        /// The indexer allows for getting and setting string values based on the provided Field key.
        /// </summary>
        /// <param name="field">The field key to access or modify.</param>
        /// <returns>The string value associated with the specified field key.</returns>
        public string this[Field field]
        {
            get
            {

                String value = ActivationDocument?.Root?.Elements()?.Where(elem => elem.Name == field.Name)?.Select(elem => elem.Value)?.FirstOrDefault() ?? string.Empty;
                if (value == string.Empty)
                    return string.Empty;
                if (field.Encrypted)
                {
                    var memStream = new MemoryStream(Convert.FromBase64String(value));
                    byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; byte[] ciphertext = new byte[memStream.Length - NonceBytes - TagBytes];
                    memStream.Read(nonce); memStream.Read(ciphertext); memStream.Read(tag);
                    var aes = new AesGcm(key, TagBytes);
                    var plaintext = new byte[ciphertext.Length];
                    aes.Decrypt(nonce, ciphertext, tag, plaintext);
                    return Encoding.UTF8.GetString(plaintext);
                }
                return value;
            }
            set
            {
                if (value == null)
                {
                    var elems = ActivationDocument?.Root?.Elements()?.Where(elem => field.Name == elem.Name) ?? [];
                    foreach (var elem in elems)
                        elem.Remove();
                    ActivationDocument?.Save(ActivationFilePath);
                    return;
                }
                String storedValue = value;
                if (field.Encrypted)
                {
                    var aes = new AesGcm(key, TagBytes);
                    byte[] plaintext = Encoding.UTF8.GetBytes(value);
                    byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; byte[] ciphertext = new byte[plaintext.Length];
                    random.NextBytes(nonce); 
                    aes.Encrypt(nonce, plaintext, ciphertext, tag);
                    var memStream = new MemoryStream();
                    memStream.Write(nonce); memStream.Write(ciphertext); memStream.Write(tag);  
                    storedValue = Convert.ToBase64String(memStream.ToArray());
                }
                if (ActivationDocument.Root?.Element(field.Name) != null)
                    ActivationDocument.Root?.Element(field.Name)?.SetValue(storedValue);
                else
                    ActivationDocument.Root?.Add(new XElement(field.Name, storedValue));
                ActivationDocument.Save(ActivationFilePath);
            }
        }

        /// <summary>
        /// Retrieves the password for a specified IAT (Implicit Association Test) from the activation document. The password is stored in an encrypted format and is decrypted using AES-GCM encryption with a predefined storage key. If the specified IAT does not exist or if the password cannot be found, 
        /// an InvalidOperationException is thrown. The method returns the decrypted password as a string.
        /// </summary>
        /// <param name="IAT">The name of the IAT (Implicit Association Test) for which to retrieve the password.</param>
        /// <returns>The decrypted password as a string.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the password doesn't exist or if the IAT cannot be found.</exception>
        public string GetIATPassword(String IAT)
        {
            if (ActivationDocument?.Root?.Element("Tests")?.Elements(IAT) == null)
                throw new InvalidOperationException("IAT not found");
            String passwordString = ActivationDocument?.Root?.Element("Tests")?.Element(IAT)?.Attribute("Password")?.Value ?? throw new InvalidOperationException("Password not found");
            if (!passwordString.StartsWith("secret:"))
                throw new InvalidOperationException("Password not found");
            var bytes = passwordString.Substring(7).Split("-").Select(s => Convert.ToByte(s, 16)).ToArray();
            byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; byte[] ciphertext = new byte[bytes.Length - TagBytes - NonceBytes];
            var memStream = new MemoryStream(bytes);
            memStream.Seek(0L, SeekOrigin.Begin);
            memStream.Read(nonce); memStream.Read(ciphertext); memStream.Read(tag);
            memStream.Dispose();
            var plaintext = new byte[ciphertext.Length];
            var aes = new AesGcm(storageKey, TagBytes);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);
            return Encoding.UTF8.GetString(plaintext);
        }


        /// <summary>
        /// Attempts to retrieve the password for a specified IAT (Implicit Association Test) from the activation document. If the IAT does not exist or if the password cannot be found, the method returns null instead of throwing an exception.
        /// </summary>
        /// <param name="iatName">The name of the IAT (Implicit Association Test) for which to retrieve the password.</param>
        /// <returns>The decrypted password as a string, or null if the IAT or password cannot be found.</returns>
        public string? TryGetIATPassword(string iatName)
        {
            if (string.IsNullOrWhiteSpace(iatName))
                return null;
            try
            {
                return GetIATPassword(iatName);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        /// <summary>
        /// Sets the password for a specified IAT (Implicit Association Test) in the activation document. The password is encrypted using AES-GCM encryption with a predefined storage key before being stored.
        /// </summary>
        /// <param name="iatName">The name of the IAT (Implicit Association Test) for which to set the password.</param>
        /// <param name="password">The password to set for the specified IAT.</param>
        public void SetIATPassword(String iatName, String password)
        {
            var aes = new AesGcm(storageKey, 16); byte[] plaintext = Encoding.UTF8.GetBytes(password); 
            byte[] nonce = new byte[NonceBytes]; byte[] tag = new byte[TagBytes]; 
            byte[] ciphertext = new byte[plaintext.Length]; random.NextBytes(nonce);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);
            var memStream = new MemoryStream();
            memStream.Write(nonce); memStream.Write(ciphertext); memStream.Write(tag);
            var secretString = "secret:";
            foreach (byte b in memStream.ToArray())
                secretString += b.ToString("{X2}-");
            secretString = secretString.TrimEnd('-');
            if (ActivationDocument.Root?.Element("Tests") == null)
                ActivationDocument.Root?.Add(new XElement("Tests", new XElement(iatName, new XAttribute("Password", secretString                                        ))));
            else if ((ActivationDocument.Root?.Element("Tests")?.Elements() ?? throw new InvalidOperationException()).Select(elem => elem.Name).Contains(iatName))
            {
                foreach (XAttribute attr in ActivationDocument.Root?.Element("Tests")?.Element(iatName)?.Attributes() ?? throw new InvalidOperationException())
                    attr.Remove();
                ActivationDocument.Root?.Element("Tests")?.Element(iatName)?.Add(new XAttribute("Password", secretString));
            }
            else
                ActivationDocument.Root?.Element("Tests")?.Add(new XElement(iatName, new XAttribute("Password", secretString)));
            ActivationDocument.Save(ActivationFilePath);
        }

        /// <summary>
        /// Deletes the specified IAT (Implicit Association Test) from the activation document, removing its associated password and any other related data. If the IAT does not exist, the method does nothing. 
        /// After deletion, the activation document is saved to persist the changes.
        /// </summary>
        /// <param name="iatName">The name of the IAT (Implicit Association Test) to delete.</param>
        public void DeleteIAT(string iatName)
        {
            if (ActivationDocument?.Root?.Element("Tests") == null)
                return;
            if (ActivationDocument.Root?.Element("Tests")?.Element(iatName) == null)
                return;
            ActivationDocument.Root?.Element("Tests")?.Element(iatName)?.Remove();
            ActivationDocument.Save(ActivationFilePath);
        }

        /// <summary>
        /// Records an error in the local error log by adding the provided error object to an XML file. If the error log file does not exist, a new XML document is created with a root element named "Errors". The error object is added as a child element to the root, and the updated document is saved to the specified error file path. 
        /// The method returns the total count of error elements currently recorded in the log.
        /// </summary>
        /// <param name="error">The error object to record in the local error log.</param>
        /// <returns>The total count of error elements currently recorded in the log.</returns>
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
            xDoc.Root?.Add(error);
            xDoc.Save(ErrorFilePath);
            return xDoc.Root?.Elements().Count() ?? 0;
        }

        /// <summary>
        /// Gets the directory path where activation files are stored. The path is constructed using the local application data folder and 
        /// appending "IATSoftware" to it. If the directory does not exist, it will be created when needed. This property provides a centralized location for storing activation-related files, ensuring that they are kept in a consistent and accessible location across different environments.
        /// </summary>
        public static string ActivationFileDirectory =>Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create) + Path.DirectorySeparatorChar + "IATSoftware";

        /// <summary>
        /// Gets the full file path for the error log file, which is named "Errors.xml" and is located within the activation file directory. This property provides a convenient way to access the error log's location, allowing for reading from or writing to the file as needed. The error log contains structured information about errors that have occurred within the application, facilitating debugging and issue tracking.
        /// </summary>
        public static string ErrorFilePath => ActivationFileDirectory + Path.DirectorySeparatorChar + "Errors.xml";

        /// <summary>
        /// Gets the full file path for the activation file, which is named "IATDesign.xml" and is located within the activation file directory. This property provides a convenient way to access the activation file's location, allowing for reading from or writing to the file as needed. The activation file contains important data related to the application's activation status and configuration.
        /// </summary>
        public static string ActivationFilePath => ActivationFileDirectory + Path.DirectorySeparatorChar + "IATDesign.xml";

        /// <summary>
        /// Gets a value indicating whether the activation data exists in the Windows registry. This property checks for the presence of a specific subkey ("IATSoftware") under the "Software" key in the current user's registry hive. If the subkey is found, it returns true, indicating that activation data is present in the registry; otherwise, it returns false. This check is useful for determining whether the application has been previously activated or if activation data needs to be created or restored from the registry.
        /// </summary>
        private static bool ActivationDataExistsInRegistry => (Registry.CurrentUser?.OpenSubKey("Software") ?? throw new InvalidOperationException()).GetSubKeyNames().Contains("IATSoftware");

        /// <summary>
        /// Gets a value indicating whether the activation data exists in the local storage. This property checks for the existence of the activation file directory and the activation file itself. If both the directory and the file are present, it returns true, indicating that activation data is available; otherwise, it returns false. This check is useful for determining whether the application has been previously activated or if activation data needs to be created or restored.
        /// </summary>
        private static bool ActivationDataExists => Directory.Exists(ActivationFileDirectory) && File.Exists(ActivationFilePath);

        /// <summary>
        /// Deactivates the application by deleting the activation file from the local storage. If the activation file exists at the specified path, it will be removed, effectively deactivating the application. This method does not check for the existence of the file before attempting to delete it, so it is safe to call even if the activation file may not be present. After calling this method, the application will no longer be considered activated until a new activation process is completed.
        /// </summary>
        public static void Deactivate()
        {
            if (File.Exists(ActivationFilePath))
                File.Delete(ActivationFilePath);
        }
    }
}



