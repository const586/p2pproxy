using System.Reflection;
using System.Xml;
using TTVProxy.UPNP;

namespace P2pProxy.UPNP
{
    [UpnpServiceVariable("Status", "string", false, "OK")]
    [Obfuscation]
    public class BasicService : UpnpService
    {
        public BasicService(UpnpServer server, UpnpDevice device) :
            base(server, device, "urn:axis-com:service:BasicService:1", "urn:axis-com:serviceId:BasicServiceId",
            "/dlna/BasicServiceId.control", "/upnp/BasicServiceId.event", "/dlna/scpd_basic.xml")
        {
        }

        protected override void WriteEventProp(XmlWriter writer)
        {
            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("SystemUpdateID", "0");
            writer.WriteEndElement();
        }
    }
}