using System.IO;
using System.Net;
using System.Text;
using System.Xml;

namespace P2pProxy.UPNP
{
    public class UpnpServer
    {
        private readonly UpnpDevice rootDevice;
        private readonly SsdpServer ssdpServer;
        private byte[] descArray;
        private readonly IPAddress[] hostAddresses = new IPAddress[0];

        public IPAddress[] HostAddresses
        {
            get { return hostAddresses; }
        }

        public P2pProxyDevice RootDevice
        {
            get { return (P2pProxyDevice)rootDevice; }
        }

        public UpnpServer(UpnpDevice rootDevice)
        {
            this.rootDevice = rootDevice;
            ssdpServer = new SsdpServer(this);

        }

        public byte[] GetDescription(string host = null)
        {
            GenerateDescription(host);
            return descArray;
        }

        private void GenerateDescription(string host = null)
        {
            MemoryStream memStream = new MemoryStream();
            using (XmlTextWriter descWriter = new XmlTextWriter(memStream, new UTF8Encoding(false)))
            {
                descWriter.Formatting = Formatting.Indented;
                descWriter.WriteRaw("<?xml version=\"1.0\"?>");

                descWriter.WriteStartElement("root", "urn:schemas-upnp-org:device-1-0");

                descWriter.WriteStartElement("specVersion");
                descWriter.WriteElementString("major", "1");
                descWriter.WriteElementString("minor", "0");
                descWriter.WriteEndElement();

                descWriter.WriteStartElement("device");
                rootDevice.WriteDescription(descWriter, host);
                descWriter.WriteEndElement();

                descWriter.WriteEndElement();

                descWriter.Flush();
                descArray = memStream.ToArray();
            }
        }

        public void Start()
        {
            ssdpServer.Start();
        }

        public void Stop()
        {
            ssdpServer.Stop();
            //this.httpServer.Stop();
        }
    }
}