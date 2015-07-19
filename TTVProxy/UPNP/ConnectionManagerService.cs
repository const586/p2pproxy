using System;
using System.Reflection;
using System.Web;
using System.Xml;
using P2pProxy.UPNP;
using P2pProxy.Http;
using P2pProxy.Http.Server;

namespace P2pProxy.UPNP
{
    [UpnpServiceVariable("SourceProtocolInfo", "string", true)]
    [UpnpServiceVariable("SinkProtocolInfo", "string", true)]
    [UpnpServiceVariable("CurrentConnectionIDs", "string", true)]
    [UpnpServiceVariable("A_ARG_TYPE_ConnectionStatus", "string", false, "OK", "ContentFormatMismatch", "InsufficientBandwidth", "UnreliableChannel", "Unknown")]
    [UpnpServiceVariable("A_ARG_TYPE_ConnectionManager", "string", false)]
    [UpnpServiceVariable("A_ARG_TYPE_Direction", "string", false, "Input", "Output")]
    [UpnpServiceVariable("A_ARG_TYPE_ProtocolInfo", "string", false)]
    [UpnpServiceVariable("A_ARG_TYPE_ConnectionID", "i4", false)]
    [UpnpServiceVariable("A_ARG_TYPE_AVTransportID", "i4", false)]
    [UpnpServiceVariable("A_ARG_TYPE_RcsID", "i4", false)]
    public class ConnectionManagerService : UpnpService
    {
        private static readonly string sourceProtocolInfo = "http-get:*:image/jpeg:DLNA.ORG_PN=JPEG_TN," + 
            "http-get:*:image/jpeg:DLNA.ORG_PN=JPEG_SM," + 
            "http-get:*:audio/x-ms-wma:DLNA.ORG_PN=WMAFULL," + 
            "http-get:*:video/x-ms-wmv:DLNA.ORG_PN=WMVMED_BASE," + 
            "http-get:*:audio/L16;rate=44100;channels=2:DLNA.ORG_PN=LPCM," + 
            "http-get:*:video/x-ms-wmv:DLNA.ORG_PN=WMVSPML_MP3," + 
            "http-get:*:video/mpeg:DLNA.ORG_PN=MPEG_PS_PAL," + 
            "http-get:*:audio/x-ms-wma:DLNA.ORG_PN=WMABASE," + 
            "http-get:*:video/x-ms-wmv:DLNA.ORG_PN=WMVHIGH_FULL," +
            "http-get:*:video/mpeg:DLNA.ORG_PN=MPEG_PS_PAL_XAC3," + 
            "http-get:*:image/jpeg:DLNA.ORG_PN=JPEG_MED," +
            "http-get:*:audio/L16;rate=48000;channels=2:DLNA.ORG_PN=LPCM," + 
            "http-get:*:audio/mpeg:DLNA.ORG_PN=MP3," + 
            "http-get:*:video/x-ms-wmv:DLNA.ORG_PN=WMVHIGH_PRO," + 
            "http-get:*:video/mpeg:DLNA.ORG_PN=MPEG_PS_NTSC," + 
            "http-get:*:video/x-ms-wmv:DLNA.ORG_PN=WMVSPLL_BASE," + 
            "http-get:*:audio/mpeg:DLNA.ORG_PN=MP3X," + 
            "http-get:*:video/mpeg:DLNA.ORG_PN=MPEG1," + 
            "http-get:*:video/x-ms-wmv:DLNA.ORG_PN=WMVSPML_BASE," + 
            "http-get:*:video/x-ms-wmv:DLNA.ORG_PN=WMVMED_FULL," + 
            "http-get:*:image/jpeg:DLNA.ORG_PN=JPEG_LRG," + 
            "http-get:*:video/mpeg:DLNA.ORG_PN=MPEG_PS_NTSC_XAC3," +
            "http-get:*:audio/L16;rate=44100;channels=1:DLNA.ORG_PN=LPCM," + 
            "http-get:*:audio/x-ms-wma:DLNA.ORG_PN=WMAPRO," + 
            "http-get:*:video/x-ms-wmv:*," + 
            "http-get:*:audio/x-ms-wma:*," + 
            "http-get:*:video/avi:*";

        public ConnectionManagerService(UpnpServer server, UpnpDevice device)
            : base(server, device,
                "urn:schemas-upnp-org:service:ConnectionManager:1", "urn:upnp-org:serviceId:ConnectionManager", "/dlna/ConnectionManager.control",
                "/dlna/ConnectionManager.event", "/dlna/ConnectionManager.xml")
        {
        }

        protected override void WriteEventProp(XmlWriter writer)
        {
            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("SourceProtocolInfo", sourceProtocolInfo);
            writer.WriteEndElement();

            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("SinkProtocolInfo", string.Empty);
            writer.WriteEndElement();

            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("CurrentConnectionIDs", "0");
            writer.WriteEndElement();
        }

        [UpnpServiceArgument(0, "Source", "SourceProtocolInfo")]
        [UpnpServiceArgument(1, "Sink", "SinkProtocolInfo")]
        [ObfuscationAttribute]
        private void GetProtocolInfo(MyWebRequest request)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody(sourceProtocolInfo, string.Empty);
        }

        [UpnpServiceArgument(0, "ConnectionIDs", "CurrentConnectionIDs")]
        [ObfuscationAttribute]
        private void GetCurrentConnectionIDs(MyWebRequest request)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("0");
        }

        [UpnpServiceArgument(0, "RcsID", "A_ARG_TYPE_RcsID")]
        [UpnpServiceArgument(1, "AVTransportID", "A_ARG_TYPE_AVTransportID")]
        [UpnpServiceArgument(2, "ProtocolInfo", "A_ARG_TYPE_ProtocolInfo")]
        [UpnpServiceArgument(3, "PeerConnectionManager", "A_ARG_TYPE_ConnectionManager")]
        [UpnpServiceArgument(4, "PeerConnectionID", "A_ARG_TYPE_ConnectionID")]
        [UpnpServiceArgument(5, "Direction", "A_ARG_TYPE_Direction")]
        [UpnpServiceArgument(6, "Status", "A_ARG_TYPE_ConnectionStatus")]
        [ObfuscationAttribute]
        private void GetCurrentConnectionInfo(MyWebRequest request,
            [AliasAttribute("ConnectionID")][UpnpServiceArgument("A_ARG_TYPE_ConnectionID")] string ConnectionID)
        {
            if (ConnectionID != "0")
                throw new SoapException(402, "Invalid Args");

            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("-1", "-1", string.Empty, string.Empty, "-1", "Output", "OK");
        }
    }
}