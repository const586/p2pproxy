using System;
using System.Reflection;
using P2pProxy.UPNP;
using P2pProxy.Http;
using P2pProxy.Http.Server;

namespace P2pProxy.UPNP
{
    [UpnpServiceVariable("AuthorizationDeniedUpdateID", "ui4", true)]
    [UpnpServiceVariable("A_ARG_TYPE_DeviceID", "string", false)]
    [UpnpServiceVariable("A_ARG_TYPE_RegistrationRespMsg", "bin.base64", false)]
    [UpnpServiceVariable("ValidationRevokedUpdateID", "ui4", true)]
    [UpnpServiceVariable("ValidationSucceededUpdateID", "ui4", true)]
    [UpnpServiceVariable("A_ARG_TYPE_Result", "int", false)]
    [UpnpServiceVariable("AuthorizationGrantedUpdateID", "ui4", true)]
    [UpnpServiceVariable("A_ARG_TYPE_RegistrationReqMsg", "bin.base64", false)]
    public class MediaReceiverRegistrarService : UpnpService
    {

        public MediaReceiverRegistrarService(UpnpServer server, UpnpDevice device)
            : base(server, device,
                "urn:microsoft.com:service:X_MS_MediaReceiverRegistrar:1", "urn:microsoft.com:serviceId:X_MS_MediaReceiverRegistrar",
                "/dlna/X_MS_MediaReceiverRegistrar.control", "/dlna/X_MS_MediaReceiverRegistrar.event", "/dlna/X_MS_MediaReceiverRegistrar.xml")
        {
        }

        protected override void WriteEventProp(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("AuthorizationDeniedUpdateID", string.Empty);
            writer.WriteEndElement();

            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("ValidationRevokedUpdateID", string.Empty);
            writer.WriteEndElement();

            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("ValidationSucceededUpdateID", string.Empty);
            writer.WriteEndElement();

            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("AuthorizationGrantedUpdateID", string.Empty);
            writer.WriteEndElement();
        }

        [UpnpServiceArgument(0, "RegistrationRespMsg", "A_ARG_TYPE_RegistrationRespMsg")]
        [ObfuscationAttribute]
        private void RegisterDevice(MyWebRequest request, [UpnpServiceArgument("A_ARG_TYPE_RegistrationReqMsg")][AliasAttribute("RegistrationReqMsg")] string RegistrationReqMsg)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("OK");
        }

        [UpnpServiceArgument(0, "Result", "A_ARG_TYPE_Result")]
        [ObfuscationAttribute]
        private void IsAuthorized(MyWebRequest request, [UpnpServiceArgument("A_ARG_TYPE_DeviceID")][AliasAttribute("DeviceID")] string DeviceID)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("1");
        }

        [UpnpServiceArgument(0, "Result", "A_ARG_TYPE_Result")]
        [ObfuscationAttribute]
        private void IsValidated(MyWebRequest request, [UpnpServiceArgument("A_ARG_TYPE_DeviceID")][AliasAttribute("DeviceID")] string DeviceID)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("1");
        }
    }
}