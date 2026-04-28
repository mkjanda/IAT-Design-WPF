using IAT.Core.ConfigFile;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using IAT.Core.Serializable;
using System.IO;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace IAT.Core.Services
{
    /// <summary>
    /// Provides functionality to retrieve and process test results, including survey and trial responses, from a remote
    /// source using web sockets and local storage.
    /// </summary>
    /// <remarks>The TestResultService coordinates the retrieval, decryption, and deserialization of test
    /// result data. It depends on services for web socket communication, local storage access, and XML deserialization.
    /// This class is intended for scenarios where secure retrieval and handling of test results are required, such as
    /// in assessment or survey applications.</remarks>
    public class TestResultService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly ILocalStorageService _localStorageService;
        private readonly IXmlDeserializationService _xmlDeserializationService;
        private TestConfig? ConfigFile { get; set; } = null;
        private List<ResultPacket> Results { get; set; } = new();

        private List<SurveyResponse?> SurveyResponses = new();
        private List<IATResponse?> TrialResults = new();

        /// <summary>
        /// Initializes a new instance of the TestResultService class with the specified dependencies.
        /// </summary>
        /// <param name="webSocketService">The service used to manage WebSocket communication for real-time updates.</param>
        /// <param name="localStorageService">The service used to access and manage local storage for persisting data.</param>
        /// <param name="xmlDeserializationService">The service used to deserialize XML data into application objects.</param>
        public TestResultService(IWebSocketService webSocketService, ILocalStorageService localStorageService,
            IXmlDeserializationService xmlDeserializationService)
        {
            _webSocketService = webSocketService;
            _localStorageService = localStorageService;
            _xmlDeserializationService = xmlDeserializationService;
        }

        /// <summary>
        /// Retrieves and decrypts test configuration and result data for the specified IAT using the provided
        /// credentials.
        /// </summary>
        /// <remarks>This method initiates an asynchronous operation to fetch and process encrypted test
        /// results. The retrieved data is deserialized and decrypted before being assigned to the corresponding
        /// properties. Ensure that the provided credentials are valid and that the local storage contains the required
        /// product key.</remarks>
        /// <param name="iatName">The name of the IAT (Implicit Association Test) for which to retrieve results.</param>
        /// <param name="password">The password required to authenticate and access the IAT results.</param>
        /// <exception cref="NullReferenceException">Thrown if the configuration or result data cannot be deserialized from the response.</exception>
        public void Retrieve(String iatName, String password)
        {
            _ = _webSocketService.GetResults(iatName, password, _localStorageService[Field.ProductKey]).ContinueWith(t =>
            {
                var xDoc = t.Result;
                var ser = new XmlSerializer(typeof(TestConfig), new XmlRootAttribute("ConfigFile"));
                ConfigFile = ser.Deserialize(xDoc.CreateReader()) as TestConfig ?? throw new NullReferenceException();
                ser = new XmlSerializer(typeof(List<ResultPacket>), new XmlRootAttribute("ResultSet"));
                Results = ser.Deserialize(xDoc.CreateReader()) as List<ResultPacket> ?? throw new NullReferenceException();
                var rsa = RSA.Create(_webSocketService.RSA.GetRSAParameters());
                foreach (var resultPacket in Results)
                {
                    var resultBytes = Convert.FromBase64String(resultPacket.ResultData);
                    foreach (var tocEntry in resultPacket.TOC)
                    {
                        var key = resultBytes.Skip((int)tocEntry.KeyOffset).Take((int)tocEntry.KeyLength).ToArray();
                        var iv = resultBytes.Skip((int)tocEntry.IVOffset).Take((int)tocEntry.IVLength).ToArray();
                        var data = resultBytes.Skip((int)tocEntry.DataOffset).Take((int)tocEntry.DataLength).ToArray();

                        using var des = DES.Create();
                        des.Key = rsa.Decrypt(key, RSAEncryptionPadding.Pkcs1);
                        des.IV = rsa.Decrypt(iv, RSAEncryptionPadding.Pkcs1);
                        using var decryptor = des.CreateDecryptor();
                        using var memStream = new MemoryStream();
                        using var cStream = new CryptoStream(memStream, decryptor, CryptoStreamMode.Write);
                        cStream.Write(data, 0, data.Length);
                        cStream.FlushFinalBlock();
                        memStream.Seek(0, SeekOrigin.Begin);
                        var resultElem = _xmlDeserializationService.DeserializeUnknownType(memStream);
                        if (resultElem is IATResponse)
                            TrialResults.Add(resultElem as IATResponse);
                        else if (resultElem is SurveyResponse)
                            SurveyResponses.Add(resultElem as SurveyResponse);
                    }
                }
            });
        }
    }
}
