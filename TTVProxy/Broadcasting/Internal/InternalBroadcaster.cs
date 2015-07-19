using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using P2pProxy.Http.Server;

namespace P2pProxy.Broadcasting.Internal
{
    public class InternalBroadcaster
    {
        private WebServer _server;
        public AceStream31 Acestream;
        public InternalBroadcaster(WebServer server)
        {
            _server = server;
            Acestream = new AceStream31();
        }

        public string RegisterBroadcast(string cid)
        {
            _server.AddRouteUrl("/broadcast", Request, HttpMethod.Get);
            return string.Format("http://127.0.0.1:{0}/broadcast?id={1}", _server.Port, cid);
        }

        private void Request(MyWebRequest req)
        {
            if (!req.Headers.ContainsKey("id"))
            {
                _server.Send404(req);
                return;
            }
            TcpClient client = new TcpClient("127.0.0.1", AceStream31.WebASPort);
            var stream = client.GetStream();
            Uri uri = new Uri(Acestream.Play(req.Headers["id"]));
            var sendreq = Encoding.UTF8.GetBytes(string.Format("GET {0}{1}\r\nHost: {2}:{3}\r\nConnection: Keep-Alive\r\n\r\n", uri.PathAndQuery, uri.Host, uri.Port));
            stream.Write(sendreq, 0, sendreq.Length);
            var resp = req.GetResponse();
            resp.AddHeader("Content-Type", "application/octet-stream");
            resp.AddHeader("Cache-control", "no-cache");
            resp.SendHeaders();
            stream.CopyTo(resp.GetStream());
        }
    }
}
