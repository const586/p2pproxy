using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SimpleLogger;
using P2pProxy.Http;

namespace P2pProxy.UPNP
{
    public class SsdpServer
    {
        private class MySocket
        {
            public Socket ListenerSocket { get; set; }
            public Socket NotifySocket { get; set; }
            public IPAddress Address { get; set; }
        }

        private readonly UpnpServer upnpServer;
        private Thread[] listenerThreads;
        private Timer[] notifyTimers;
        private MySocket[] sockets;

        private readonly Random rn = new Random();

        public SsdpServer(UpnpServer upnpServer)
        {
            this.upnpServer = upnpServer;

        }

        public void Start()
        {
            if (listenerThreads == null)
            {

                sockets = GetSockets().ToArray();

                listenerThreads = new Thread[sockets.Length];
                notifyTimers = new Timer[sockets.Length];

                for (int i = 0; i < sockets.Length; i++)
                {
                    listenerThreads[i] = new Thread(ListenNotify);
                    listenerThreads[i].Start(sockets[i]);

                    notifyTimers[i] = new Timer(OnNotifyTimeout, sockets[i], 1000, upnpServer.RootDevice.MaxAge * 900);
                }
            }
        }

        public void Stop()
        {
            if (listenerThreads != null)
            {
                foreach (Timer timer in notifyTimers)
                    timer.Dispose();

                //Uzavre ListenerSocket a tym aj funkcia ListenNotify odosle spravu "byebye" a uzavre NotifySocket
                foreach (MySocket socket in sockets)
                    socket.ListenerSocket.Close();

                foreach (Thread thread in listenerThreads)
                    thread.Join();

                sockets = null;
                listenerThreads = null;
                notifyTimers = null;
            }
        }

        private void OnNotifyTimeout(object socketObj)
        {
            MySocket socket = (MySocket)socketObj;
            SendNotify(socket.NotifySocket, socket.Address.ToString(), true);
        }

        private IEnumerable<MySocket> GetSockets()
        {
            IEnumerable<IPAddress> addresses = upnpServer.HostAddresses.Length <= 0 ? Dns.GetHostAddresses(Dns.GetHostName()).Where(a => a.AddressFamily == AddressFamily.InterNetwork) : upnpServer.HostAddresses;

            return addresses.Select(delegate(IPAddress address)
            {

                Socket listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listenerSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                listenerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
                listenerSocket.Bind(new IPEndPoint(address, upnpServer.RootDevice.SsdpPort));

                listenerSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership,
                    new MulticastOption(IPAddress.Parse("239.255.255.250"), address));

                Socket notifySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                notifySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                notifySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

                notifySocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
                notifySocket.Bind(new IPEndPoint(address, upnpServer.RootDevice.SsdpPort));

                return new MySocket { ListenerSocket = listenerSocket, NotifySocket = notifySocket, Address = address };

            }).ToArray();
        }

        private void ListenNotify(object socketObj)
        {
            MySocket socket = (MySocket)socketObj;
            byte[] buffer = new byte[1024];

            string localEndPoint = socket.Address.ToString();
            int length;
            while (true)
            {
                EndPoint receivePoint = new IPEndPoint(IPAddress.Any, 0);
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    try
                    {
                        length = socket.ListenerSocket.ReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None,
                            ref receivePoint);
                    }
                    catch (Exception e)
                    {
                        break;
                    }


                }
                else
                {
                    try
                    {
                        UdpClient udp = new UdpClient(upnpServer.RootDevice.SsdpPort);
                        IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

                        buffer = udp.Receive(ref endPoint);
                        receivePoint = endPoint;
                        length = buffer.Length;
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                }
                string[] lines = Encoding.ASCII.GetString(buffer, 0, length).Split(new[] { "\r\n" },
                    StringSplitOptions.RemoveEmptyEntries);

                if (lines.Length > 0 && lines[0] == "M-SEARCH * HTTP/1.1")
                {
                    Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    for (int i = 1; i < lines.Length; i++)
                    {
                        string[] keyValue = lines[i].Split(new[] { ':' }, 2);
                        if (keyValue.Length == 2)
                        {
                            dict[keyValue[0].Trim()] = keyValue[1].Trim();
                        }
                    }

                    if (!dict.ContainsKey("MAN") ||
                        StringComparer.OrdinalIgnoreCase.Compare(dict["MAN"], "\"ssdp:discover\"") != 0)
                    {
                        if (P2pProxyApp.Debug)
                            P2pProxyApp.Log.Write("SsdpServer::Listen - MAN ignore", TypeMessage.Info);
                        continue;
                    }

                    int mx;
                    if (!dict.ContainsKey("MX") || !int.TryParse(dict["MX"], out mx) || mx < 0)
                    {
                        if (P2pProxyApp.Debug)
                            P2pProxyApp.Log.Write("SsdpServer::Listen - MX ignore", TypeMessage.Info);
                        continue;
                    }


                    if (!dict.ContainsKey("ST"))
                    {
                        if(P2pProxyApp.Debug)
                            P2pProxyApp.Log.Write("SsdpServer::Listen - ST ignore", TypeMessage.Info);
                        continue;
                    }

                    string st = dict["ST"];
                    string usn = "uuid:" + upnpServer.RootDevice.Udn;
                    if (StringComparer.OrdinalIgnoreCase.Compare(st, "upnp:rootdevice") == 0 ||
                        StringComparer.OrdinalIgnoreCase.Compare(st, upnpServer.RootDevice.DeviceType) == 0)
                    {

                    }
                    else if (StringComparer.OrdinalIgnoreCase.Compare(st, usn) == 0)
                    {
                        st = usn;
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Compare(st, "ssdp:all") == 0)
                    {
                        Thread.Sleep(rn.Next(mx * 850));

                        SendResponseMessage(socket.ListenerSocket, receivePoint, localEndPoint, "upnp:rootdevice", usn);
                        SendResponseMessage(socket.ListenerSocket, receivePoint, localEndPoint, usn, usn);
                        SendResponseMessage(socket.ListenerSocket, receivePoint, localEndPoint, upnpServer.RootDevice.DeviceType, usn);

                        foreach (UpnpService service in upnpServer.RootDevice.Services)
                            SendResponseMessage(socket.ListenerSocket, receivePoint, localEndPoint, service.ServiceType, usn);

                        continue;
                    }
                    else if (upnpServer.RootDevice.Services.FirstOrDefault(a =>
                        StringComparer.OrdinalIgnoreCase.Compare(st, a.ServiceType) == 0) == null)
                    {
                        if (P2pProxyApp.Debug)
                            P2pProxyApp.Log.Write("SsdpServer::Listen - ignoring service " + st, TypeMessage.Info);
                        continue;
                    }

                    Thread.Sleep(rn.Next(mx * 850));
                    SendResponseMessage(socket.ListenerSocket, receivePoint, localEndPoint, st, usn);
                }
            }

            SendNotify(socket.NotifySocket, socket.Address.ToString(), false);
            socket.NotifySocket.Close();

        }

        private void SendNotify(Socket socket, string host, bool isAlive)
        {
            SendNotifyMessage(socket, host, "upnp:rootdevice", upnpServer.RootDevice.Udn.ToString(), isAlive);
            SendNotifyMessage(socket, host, upnpServer.RootDevice.Udn.ToString(), upnpServer.RootDevice.Udn.ToString(), isAlive);
            SendNotifyMessage(socket, host, upnpServer.RootDevice.DeviceType, upnpServer.RootDevice.Udn.ToString(), isAlive);

            foreach (UpnpService service in upnpServer.RootDevice.Services)
                SendNotifyMessage(socket, host, service.ServiceType, upnpServer.RootDevice.Udn.ToString(), isAlive);
        }

        private void SendNotifyMessage(Socket socket, string host, string nt, string usn, bool isAlive)
        {
            int port = upnpServer.RootDevice.Web.Port;
            string message = string.Format(@"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:{11}
CACHE-CONTROL: max-age = {0}
LOCATION: http://{1}:{2}/dlna/description.xml
NT: {3}
NTS: ssdp:{4}
SERVER: {10}/{5}.{6} UPnP/1.1 P2pProxy/{7}
USN: uuid:{8}{9}

", upnpServer.RootDevice.MaxAge, host, port, nt == usn ? "uuid:" + nt : nt, isAlive ? "alive" : "byebye",
            Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, upnpServer.RootDevice.ModelNumber,
            usn, nt == usn ? string.Empty : "::" + nt, Environment.OSVersion.Platform, upnpServer.RootDevice.SsdpPort);

            byte[] data = Encoding.ASCII.GetBytes(message);

            try
            {
                socket.SendTo(data, new IPEndPoint(IPAddress.Broadcast, upnpServer.RootDevice.SsdpPort));
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendNotifyMessageError " + ex.Message);
            }

            Thread.Sleep(100);
        }

        private void SendResponseMessage(Socket socket, EndPoint receivePoint, string host, string st, string usn)
        {
            int port = upnpServer.RootDevice.Web.Port;
            string message = string.Format(@"HTTP/1.1 200 OK
CACHE-CONTROL: max-age = {0}
DATE: {1}
EXT:
LOCATION: http://{2}:{3}/dlna/description.xml
SERVER: {10}/{4}.{5} UPnP/1.1 P2pProxy/{6}
ST: {7}
USN: {8}{9}

", upnpServer.RootDevice.MaxAge, DateTime.Now.ToString("r"), host, port, Environment.OSVersion.Version.Major,
            Environment.OSVersion.Version.Minor, upnpServer.RootDevice.ModelNumber, st, usn, st == usn ? string.Empty : "::" + st, Environment.OSVersion.Platform);

            byte[] data = Encoding.ASCII.GetBytes(message);

            try { socket.SendTo(data, receivePoint); }
            catch { }
        }
    }
}