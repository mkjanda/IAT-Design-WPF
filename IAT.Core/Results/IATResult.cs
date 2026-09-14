using IAT.Core.ConfigFile;
using IAT.Core.Serializable;
using IAT.Core.ResultData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IAT.Core.Results
{
    [XmlRoot("TestResults")]
    public class TestResults
    {
        [XmlElement("ResultSetDescriptor", Form = XmlSchemaForm.Unqualified)]
        public ResultSetDescriptor ResultSetDescriptor { get; set; } = new ResultSetDescriptor();

        [XmlElement("EncryptedResultSet", Form = XmlSchemaForm.Unqualified, Type = typeof(EncryptedResultSet))]
        public List<EncryptedResultSet> EncrypttedResultSet { get; set; } = new();

        [XmlIgnore]
        public ResultSet ResultSet { get; set; } = new ResultSet();
    }


    [XmlType("EncryptedResultSet")]
    public class EncryptedResultSet
    {
        [XmlElement("Results", Form = XmlSchemaForm.Unqualified)]
        public string Results { get; set; } = string.Empty;

        [XmlElement("Cipher", Form = XmlSchemaForm.Unqualified)]
        public string Cipher { get; set; } = string.Empty;

        [XmlElement("Tag", Form = XmlSchemaForm.Unqualified)]
        public string Tag { get; set; }

        [XmlElement("Nonce", Form = XmlSchemaForm.Unqualified)]
        public String Nonce { get; set; }

        [XmlElement("Salt", Form = XmlSchemaForm.Unqualified)]
        public string Salt { get; set; }
    }

    /// <summary>
    /// Represents a descriptor for a result set, containing metadata and configuration information related to the results of a test or assessment.
    /// </summary>
    [XmlType("ResultSetDescriptor")]
    public class ResultSetDescriptor : IWebSocketMessage
    {
        /// <summary>
        /// Gets or sets the product key associated with the result set descriptor. This key is used to identify the product or application for 
        /// which the results are being generated.
        /// </summary>
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
        public string ProductKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the test associated with the result set descriptor. This name is used to identify the specific test or assessment
        /// </summary>
        [XmlElement("TestAuthor", Form = XmlSchemaForm.Unqualified)]
        public string TestAuthor { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the author of the test associated with the result set descriptor. This information is used to 
        /// identify the individual or organization
        /// </summary>
        [XmlElement("ConfigFile", Form = XmlSchemaForm.Unqualified, Type = typeof(IATConfigFile))]
        public IATConfigFile ConfigFile { get; set; } = new IATConfigFile();

        /// <summary>
        /// Gets or sets the number of results in the result set.
        /// </summary>
        [XmlElement("NumResults", Form = XmlSchemaForm.Unqualified)]
        public int NumResults { get; set; } = 0;

        /// <summary>
        /// Gets or sets the RSA key used for encrypting the result set.
        /// </summary>
        [XmlElement("RSAKey", Form = XmlSchemaForm.Unqualified, Type = typeof(RSACryptoParams))]
        public RSACryptoParams RSAKey { get; set; } = new RSACryptoParams();

        [XmlAttribute("DataVersion", Form = XmlSchemaForm.Unqualified)]
        public string DataVersion { get; set; } = string.Empty;
    }


    [XmlType("ResultSet")]
    public class ResultSet
    {
        [XmlElement("SurveyResults", Form = XmlSchemaForm.Unqualified)]
        public List<SurveyResponse> SurveyResults { get; set; } = new();

        [XmlElement("IATResult", Form = XmlSchemaForm.Unqualified)]
        public List<IATResult> IATResult { get; set; } = new();
    }

    [XmlType("IATResult")]
    public class IATResult
    {
        [XmlElement("Fragment", Form = XmlSchemaForm.Unqualified)]
        public List<IATResultFragment> Fragment { get; set; } = new();
    }

    [XmlType("IATResultFragment")]
    public class IATResultFragment
    {
        [XmlElement("BlockNum", Form = XmlSchemaForm.Unqualified)]
        public int BlockNum { get; set; }

        [XmlElement("ItemNum", Form = XmlSchemaForm.Unqualified)]
        public int ItemNum { get; set; }

        [XmlElement("ResponseTime", Form = XmlSchemaForm.Unqualified)]
        public long ResponseTime { get; set; }

        [XmlElement("PresentationNum", Form = XmlSchemaForm.Unqualified)]
        public int PresentationNum { get; set; }

        [XmlElement("Error", Form = XmlSchemaForm.Unqualified)]
        public bool Error { get; set; }
    }

    /// <summary>
    /// Conntains the responses to a survey, represented as an array of strings. Each string in the array corresponds to an individual survey response.
    /// </summary>
    [XmlType("SurveyResults")]
    public class SurveyResponse
    {
        [XmlElement("ProductKey", Form = XmlSchemaForm.Unqualified)]
        public string ProductKey { get; set; } = string.Empty;

        [XmlElement("Answer", Form = XmlSchemaForm.Unqualified)]
        public List<string> Answer { get; set; } = new();

        [XmlAttribute("SurveyName", Form = XmlSchemaForm.Unqualified)]
        public string SurveyName { get; set; } = string.Empty;
    }
}
