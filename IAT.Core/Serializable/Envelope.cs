using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml.Schema;
using IAT.Core.ConfigFile;

namespace IAT.Core.Serializable
{

    [Serializable]
    [XmlRoot("Envelope")]
    public class Envelope
    {
        public static Envelope getEnvelope(object o)
        {
            var envelope = new Envelope();
            if (o is ResultSetDescriptor resultSetDescriptor)
            {
                envelope.ResultSetDescriptor = resultSetDescriptor;
            }
            else if (o is IATConfigFile configFile)
            {
                envelope.ConfigFile = configFile;
            }
            else if (o is ActivationResponse activationResponse)
            {
                envelope.ActivationResponse = activationResponse;
            }
            else if (o is ActivationRequest activationRequest)
            {
                envelope.ActivationRequest = activationRequest;
            }
            else if (o is Handshake handshake)
            {
                envelope.Handshake = handshake;
            }
            else if (o is Manifest manifest)
            {
                envelope.Manifest = manifest;
            }
            else if (o is TransactionRequest transactionRequest)
            {
                envelope.TransactionRequest = transactionRequest;
            }
            else if (o is ServerReport serverReport)
            {
                envelope.ServerReport = serverReport;
            }
            return envelope;
        }

        public ResultSetDescriptor ResultSetDescriptor { get; set; }
        public IATConfigFile ConfigFile { get; set; }
        public ActivationResponse ActivationResponse { get; set; }
        public ActivationRequest ActivationRequest { get; set; }
        public Handshake Handshake { get; set; }
        public Manifest Manifest { get; set; }
        public TransactionRequest TransactionRequest { get; set; }
        public ServerReport ServerReport { get; set; }
    }
}
