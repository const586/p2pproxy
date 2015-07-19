using System;
using System.Collections.Generic;
using System.Xml;
using P2pProxy.Http;
using P2pProxy.Http.Server;

namespace P2pProxy.UPNP
{
    public abstract class UpnpDevice
    {
        protected Guid udn;
        protected string friendlyName;
        protected string deviceType;
        protected string manufacturer;
        protected string manufacturerUrl;
        protected string modelName;
        protected string modelNumber;
        protected string modelUrl;
        protected string serialNumber;
        protected readonly List<UpnpService> services = new List<UpnpService>();
        protected bool settingsChanged;
        protected UpnpServer server;
        public readonly WebServer Web;

        private bool started;


        public IEnumerable<UpnpService> Services
        {
            get { return services; }
        }

        public Guid Udn
        {
            get { return udn; }
        }

        public string DeviceType
        {
            get { return deviceType; }
        }

        public string ModelNumber
        {
            get { return modelNumber; }
        }

        public string FriendlyName
        {
            get { return friendlyName; }
            set
            {
                CheckStopped();

                friendlyName = value;
                SettingsChanged();
            }
        }

        public UpnpServer Server
        {
            get { return server; }
        }

        public bool Started
        {
            get { return started; }
        }

        public abstract ItemManager ItemManager{ get; }

        protected void Start()
        {
            started = true;
            server.Start();
        }

        protected UpnpDevice(WebServer web)
        {
            server = new UpnpServer(this);
            Web = web;
        }

        public virtual void Stop()
        {
            Console.WriteLine("UpnpDevice Stop");
            server.Stop();
            started = false;
        }

        public void WriteDescription(XmlTextWriter descWriter, string host = null)
        {
            descWriter.WriteElementString("UDN", "uuid:" + udn);

            descWriter.WriteElementString("friendlyName", friendlyName);

            descWriter.WriteElementString("deviceType", deviceType);

            descWriter.WriteElementString("manufacturer", manufacturer);

            if (!string.IsNullOrEmpty(manufacturerUrl))
                descWriter.WriteElementString("manufacturerURL", manufacturerUrl);

            descWriter.WriteElementString("modelName", modelName);

            if (!string.IsNullOrEmpty(modelNumber))
                descWriter.WriteElementString("modelNumber", modelNumber);

            if (!string.IsNullOrEmpty(modelUrl))
                descWriter.WriteElementString("modelURL", modelUrl);

            if (!string.IsNullOrEmpty(serialNumber))
                descWriter.WriteElementString("serialNumber", serialNumber);

            WriteSpecificDescription(descWriter, host);

            descWriter.WriteStartElement("serviceList");
            foreach (UpnpService service in services)
            {
                descWriter.WriteStartElement("service");
                service.WriteDescription(descWriter);
                descWriter.WriteEndElement();
            }
            descWriter.WriteEndElement();
        }


        protected virtual void WriteSpecificDescription(XmlTextWriter descWriter, string host = null) { }

        internal void CheckStopped()
        {
            if (started)
            {
                throw new Exception("Server must be stopped to perform this operation");
            }

        }

        internal void SettingsChanged()
        {
            settingsChanged = true;
        }
    }
}
