using IAT.Core.ConfigFile;
using IAT.Core.Models;
using IAT.Core.Enumerations;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace IAT.Core.Services
{
    public class TestResultService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly ILocalStorageService _localStorageService;
        private TestConfig ConfigFile { get; set; }
        private List<ResultPacket> Results { get; set; } = new();

        List<List<String>> SurveyResponses;
        List<IATResult> TrialResults;

        public TestResultService(IWebSocketService webSocketService, ILocalStorageService localStorageService)
        {
            _webSocketService = webSocketService;
            _localStorageService = localStorageService;
        }

        public void Retrieve(String iatName, String password)
        {
            _ = _webSocketService.GetResults(iatName, password, _localStorageService[Field.ProductKey]).ContinueWith(t =>
            {
                var xDoc = t.Result;
                var ser = new XmlSerializer(typeof(TestConfig), new XmlRootAttribute("ConfigFile"));
                ConfigFile = ser.Deserialize(xDoc.CreateReader()) as TestConfig ?? throw new NullReferenceException();
                ser = new XmlSerializer(typeof(List<ResultPacket>), new XmlRootAttribute("ResultSet"));
                Results = ser.Deserialize(xDoc.CreateReader()) as List<ResultPacket> ?? throw new NullReferenceException();
                ResultPacket.rsa = RSA.Create(_webSocketService.RSA.GetRSAParameters());

            });
        }
    }
}
