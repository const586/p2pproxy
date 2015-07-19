using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDLNA
{
    public class UpnpServer
    {
        private IPAddress[] hostAddresses = new IPAddress[0];
        private UpnpDevice rootDevice;

        public IPAddress[] HostAddresses
        {
            get { return this.hostAddresses; }
        }

        public object HttpPort
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public UpnpDevice RootDevice
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
