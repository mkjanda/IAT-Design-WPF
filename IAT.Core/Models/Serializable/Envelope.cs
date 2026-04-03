using com.sun.xml.@internal.ws.api.message;
using IAT.Core.Services;
using java.rmi;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Xml.Serialization


using static System.Net.Mime.MediaTypeNames;

namespace IAT.Core.Models.Serializable
{
    [XmlRoot("Envelope")]
    public class Envelope
    {
            private static object syncObj = new();
            private static bool IsShutdown;
            private static int packetNum = 0;
            private MemoryStream IncomingData = new MemoryStream();
            private int NumPacketInEnvelope = 0;
            private INamedXmlSerializable _Message;
            public enum EMessageType
            {
                ResultSetDescriptor, ConfigFile, ActivationRequest, ActivationResponse, DeploymentProgress, Handshake, IATList, Manifest, ItemSlideManifest, ItemSlideData,
                Packet, RSAKeyPair, QueryIATExists, TransactionRequest, ServerReport, ServerException, ResultPacket, UploadRequest, UploadProgress, GenericException, ClientException
            };
            private EMessageType _MessageType;
            private static Dictionary<EMessageType, Action<INamedXmlSerializable>> MessageMap = new Dictionary<EMessageType, Action<INamedXmlSerializable>>();

            static public Dictionary<EMessageType, Action<INamedXmlSerializable>> OnReceipt
            {
                get
                {
                    return MessageMap;
                }
            }

            private static IATConfigMainForm MainForm
            {
                get
                {
                    return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                }
            }

            static public Dictionary<EMessageType, Action<INamedXmlSerializable>> GetMessageMap()
            {
                Dictionary<EMessageType, Action<INamedXmlSerializable>> result = new Dictionary<EMessageType, Action<INamedXmlSerializable>>();
                foreach (EMessageType type in MessageMap.Keys)
                    result[type] = MessageMap[type];
                return result;
            }

            static public void Shutdown()
            {
                IsShutdown = true;
            }

            static public void ClearMessageMap()
            {
                IsShutdown = false;
                MessageMap.Clear();
            }

            static public void LoadMessageMap(Dictionary<EMessageType, Action<INamedXmlSerializable>> map)
            {
                MessageMap = map;
            }

            public EMessageType MessageType
            {
                get
                {
                    return _MessageType;
                }
            }

            public INamedXmlSerializable Message
            {
                get
                {
                    return _Message;
                }
            }

            public Envelope()
            {
                _Message = null;
            }

            public bool QueueByteData(byte[] bData, bool lastPacket)
            {
                IncomingData.Write(bData, 0, bData.Length);
                if (lastPacket)
                {
                    String str = System.Text.Encoding.UTF8.GetString(IncomingData.ToArray());
                    StringReader sReader = new StringReader(str);
                    XmlReader xReader = new XmlTextReader(sReader);
                    ReadXml(xReader);
                    return true;
                }
                return false;
            }


            public void SendMessage(ClientWebSocket websocket, CancellationToken abort)
            {
                StringWriter sWriter = new StringWriter();
                XmlWriter xWriter = new XmlTextWriter(sWriter);
                WriteXml(xWriter);
                xWriter.Flush();
                ArraySegment<byte> packet = new ArraySegment<byte>(System.Text.Encoding.UTF8.GetBytes(sWriter.ToString() + "\r\n"));
                try
                {
                    websocket.SendAsync(packet, WebSocketMessageType.Text, true, abort);
                }
                catch (InvalidOperationException)
                {
                    websocket.Dispose();
                    throw;
                }
                catch (System.Net.Sockets.SocketException)
                {
                    websocket.Dispose();
                }
            }

            public Envelope(INamedXmlSerializable msg)
            {
                _Message = msg;
            }

            public void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                String str = reader.Name;
                _MessageType = (EMessageType)Enum.Parse(typeof(EMessageType), str);
                switch (_MessageType)
                {
                    case EMessageType.ActivationRequest:
                        _Message = new ActivationRequest();
                        break;

                    case EMessageType.ActivationResponse:
                        _Message = new ActivationResponse();
                        break;

                    case EMessageType.ConfigFile:
                        _Message = IATConfig.ConfigFile.GetConfigFile();
                        break;

                    case EMessageType.DeploymentProgress:
                        _Message = new DeploymentProgressUpdate();
                        break;

                    case EMessageType.Handshake:
                        _Message = new HandShake();
                        break;

                    case EMessageType.IATList:
                        _Message = new IATList();
                        break;

                    case EMessageType.Manifest:
                        _Message = new Manifest();
                        break;

                    case EMessageType.ItemSlideManifest:
                        _Message = new ItemSlideManifest();
                        break;

                    case EMessageType.Packet:
                        _Message = new Packet();
                        break;

                    case EMessageType.ResultSetDescriptor:
                        _Message = new ResultSetDescriptor();
                        break;

                    case EMessageType.TransactionRequest:
                        _Message = new TransactionRequest();
                        break;

                    case EMessageType.ServerReport:
                        _Message = new CServerReport();
                        break;

                    case EMessageType.RSAKeyPair:
                        _Message = new CRSAKeyPair();
                        break;

                    case EMessageType.ServerException:
                        _Message = new CServerException();
                        break;

                    case EMessageType.UploadRequest:
                        _Message = new CUploadRequest();
                        break;
                }
                _Message.ReadXml(reader);
                if (_MessageType == EMessageType.Packet)
                    ((Packet)_Message).PacketNum = ++packetNum;
                else
                    packetNum = 0;
                try
                {
                    if (MessageMap.ContainsKey(MessageType))
                        Task.Run(() => MessageMap[MessageType](_Message));
                    if (MessageType == EMessageType.ServerException)
                        ErrorReporter.ReportError(_Message as CServerException);
                }
                catch (CUnexpectedServerMessage ex)
                {
                    ErrorReporter.ReportError(ex.ReportableException);
                }
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Envelope");
                Message.WriteXml(writer);
                writer.WriteEndElement();
            }

            public String GetName()
            {
                return "Envelope";
            }
        }

    }
}
