using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SimpleLogger;
using P2pProxy.UPNP;

namespace P2pProxy.Http.Server
{
    public class WebServer
    {
        public const int BUFFER_SIZE = 1496;
        //private TcpListener websock;
        private TcpListener websock;
        private bool _closed;
        private readonly Dictionary<string, Action<MyWebRequest>> _routers;
        private static readonly HttpMimeDictionary _mimes;
        private List<TcpClient> clients = new List<TcpClient>();

        static WebServer()
        {
            _mimes = HttpMimeDictionary.GetDefaults();
        }

        public WebServer(int port = 8080)
        {
            Port = port;
            _routers = new Dictionary<string, Action<MyWebRequest>>(StringComparer.OrdinalIgnoreCase);
            
        }

        public void Start()
        {
            websock = new TcpListener(IPAddress.Any, Port);
            websock.Start();
            var thr = new Thread(ClientAccept) { IsBackground = true };
            thr.Start();

        }

        public int Port
        {
            get;
            private set;
        }

        public static HttpMime GetMime(string type)
        {
            return _mimes[type];
        }

        public string[] GetRoutes()
        {
            string[] res = new string[_routers.Keys.Count];
            _routers.Keys.CopyTo(res, 0);
            return res;
        }

        private void ClientAccept()
        {
            while (!_closed)
            {
                try
                {
                    //var client = websock.AcceptTcpClient();
                    var client = websock.AcceptTcpClient();
                    clients.RemoveAll(tcpClient => !tcpClient.Connected);
                    clients.Add(client);
                    ThreadPool.QueueUserWorkItem(ClientReceived, client);
                }
                catch (Exception ex)
                {
                    P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
                }
            }
        }

        private void ClientReceived(object o)
        {
            var client = (TcpClient)o;
            if (client.Connected)
            {
                MyWebRequest msg = null;
                try
                {
                    msg = MyWebRequest.Create(client);
                    if (msg.Url != null)
                    {
                        if (P2pProxyApp.Debug)
                            P2pProxyApp.Log.Write(string.Format("WebServer::ClientRequest({0}): {1}", msg.Client.Client.RemoteEndPoint, msg.Url), TypeMessage.Info);
                        if (_routers.ContainsKey(msg.Method + "_" + msg.Url))
                            _routers[msg.Method + "_" + msg.Url].Invoke(msg);
                        else
                            Send404(msg);
                    }
                }
                catch (SoapException ex)
                {
                    MyWebResponse response = msg.GetResponse();
                    try { response.SendSoapErrorHeadersBody(ex.Code, ex.Message); }
                    catch { }
                }
                catch (Exception ex)
                {
                    P2pProxyApp.Log.Write(string.Format("WebServer::ClientRequest({0}):{1}", msg != null ? msg.Url + "?" + msg.QueryString : "", ex.Message), TypeMessage.Error);
                }
                finally
                { client.Close(); }
                
            }

        }

        public void Send404(MyWebRequest req)
        {
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, "text/html; charset=UTF-8");
            byte[] msg = Encoding.UTF8.GetBytes("<h1>404. Файл не найден</h1>");
            resp.AddHeader(HttpHeader.ContentLength, msg.Length.ToString());
            resp.SendHeaders();
            try
            {
                resp.GetStream().Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
            }
            
        }

        public void SendFile(MyWebRequest req, string fpath)
        {
            if (!File.Exists(fpath))
                Send404(req);
            FileStream stream = File.OpenRead(fpath);
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, _mimes[Path.GetExtension(fpath)].ToString());
            resp.AddHeader(HttpHeader.ContentLength, stream.Length.ToString());
            stream.CopyTo(resp.GetStream());
        }

        public void Close()
        {
            _closed = true;
            clients.ForEach(client => client.Close());
            clients.Clear();
            GC.Collect();
            websock.Stop();
        }

        public void AddRouteUrl(string url, Action<MyWebRequest> route, HttpMethod method)
        {
            string key = method + "_" + url;
            if (!_routers.ContainsKey(key))
                _routers.Add(key, route);
            else
                P2pProxyApp.Log.Write(String.Format("URL {0} already routed", url), TypeMessage.Error);
        }

        public void RemoveRouteUrl(string url, HttpMethod method)
        {
            string key = method + "_" + url;
            if (_routers.ContainsKey(key))
                _routers.Remove(key);
        }
    }

    public enum HttpMethod
    {
        Get,
        Post,
        Subscribe,
        Unsubscribe,
        Head
    }

    public enum HttpHeader { 
        ContentLength, 
        ContentType, 
        Server, 
        Date, 
        Connection, 
        AcceptRanges, 
        TransferEncoding,
        CacheControl
    }
}
