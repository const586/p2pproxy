using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;
using System.Xml;
using P2pProxy;
using P2pProxy.UPNP;
using P2pProxy.Http.Content;
using P2pProxy.Http.Server;
using P2pProxy.UPNP;
using Timer = System.Timers.Timer;

namespace P2pProxy
{
    public class P2pProxyDevice : UpnpDevice
    {
        
        private ItemManager _itemManager;
        public readonly P2pProxyApp Proxy;
        public ChannelContentProvider ChannelsProvider;
        public RecordContentProvider RecordsProvider;
        public PluginContentProvider PluginProvider;
        public RecordManager Records;
        private Timer _rectm;
        public UpnpSettingManager UpnpSettings;
        private int _maxAge;
        private int _ssdport;
        public ArchiveContentProvider ArchiveProvider;
        public int MaxAge { get { return _maxAge; }}
        public int SsdpPort { get { return _ssdport; }}
        public ContentFilter Filter { get; set; }

        public P2pProxyDevice(WebServer webServer, P2pProxyApp proxy) : base(webServer)
        {

            udn = Guid.NewGuid();
            friendlyName = "P2pProxy DLNA (" + Environment.MachineName + ")";
            deviceType = "urn:schemas-upnp-org:device:MediaServer:1";
            manufacturer = "BeGoodSoft";
            modelName = "P2pProxy DLNA Server";
            modelNumber = P2pProxyApp.Version;
            serialNumber = udn.ToString();
            modelUrl = "const86@yandex.ru";

            
            Web.AddRouteUrl("/login", LoginRequest, HttpMethod.Get);
            Web.AddRouteUrl("/stat", StatRequest, HttpMethod.Get);
            Web.AddRouteUrl("/clear_broadcast", ClearBroadcast, HttpMethod.Get);

            if (P2pProxyApp.MySettings.GetSetting("dlna", "enable", true))
            {
                Web.AddRouteUrl("/dlna/description.xml", SendDescription, HttpMethod.Get);
                Web.AddRouteUrl("/dlna/logo48.png", SendFile, HttpMethod.Get);
                services.Add(new ConnectionManagerService(server, this));
                services.Add(new ContentDirectoryService(server, this));
                services.Add(new MediaReceiverRegistrarService(server, this));
            }
            
            Proxy = proxy;
            Filter = new ContentFilter("root");
        }

        private void ClearBroadcast(MyWebRequest obj)
        {
            Proxy.Broadcaster.StopAll();
            obj.GetResponse().SendText("OK");
        }

        private void StatRequest(MyWebRequest req)
        {
            StringBuilder sb = new StringBuilder("<html><head><meta http-equiv=\"Refresh\" content=\"30\" /><title>Статистика</title></head><body>");
            sb.Append("<h1>Версия приложения:</h1>");
            sb.Append(Assembly.GetExecutingAssembly().ManifestModule.Assembly);
            sb.Append("<h1>Статистика бродкаста</h1>");
            sb.Append("<table border=1><tr><td><b>Трансляция</b></td><td><b>Количество подключений</b></td></tr>");

            var broads = Proxy.Broadcaster.GetBroadcasts();
            foreach (var broad in broads)
            {
                sb.AppendFormat("<tr><td>{0}</td><td>{1}</td></tr>", broad, Proxy.Broadcaster.GetClientConnected(broad));
            }
            
            sb.Append("</table><h1>Статистика VL VoD</h1><table border=1>");
            sb.Append("<tr><td><b>URL-источника</b></td><td><b>VLC VoD URL</b></td></tr>");

            sb.Append("</table></body></html>");
            var res = Encoding.UTF8.GetBytes(sb.ToString());
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".html").ToString());
            resp.AddHeader(HttpHeader.ContentLength, res.Length.ToString());
            resp.SendHeaders();
            resp.GetStream().Write(res, 0, res.Length);
        }

        private void LoginRequest(MyWebRequest obj)
        {
            Proxy.Login();
            var resp = obj.GetResponse();
            var state = Encoding.UTF8.GetBytes(Proxy.SessionState.ToString("xml"));
            resp.AddHeader(HttpHeader.ContentType, "text/xml");
            resp.AddHeader(HttpHeader.ContentLength, state.Length.ToString());
            resp.SendHeaders();
            resp.GetStream().Write(state, 0, state.Length);
        }

        private void RectmOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            var recs = Records.GetRecords(RecordStatus.Wait);
            foreach (var rec in recs)
            {
                if (rec.Start < DateTime.Now && rec.End > DateTime.Now)
                {
                    rec.Record();
                }
            }
        }

        public void LoadSettings()
        {
            string s = P2pProxyApp.MySettings.GetSetting("dlna", "udn", Guid.NewGuid().ToString());
            serialNumber = s;
            udn = Guid.Parse(s);

            var label = P2pProxyApp.MySettings.GetSetting("dlna", "label", "P2pProxy DLNA");
            friendlyName = label + " (" + Environment.MachineName + ")";

            _maxAge = P2pProxyApp.MySettings.GetSetting("dlna", "max-age", 148);
            
            _ssdport = P2pProxyApp.MySettings.GetSetting("dlna", "port", 1900);

            UpnpSettings = new UpnpSettingManager(P2pProxyApp.MySettings.GetSetting("dlna", "profile", "default"));
            Filter = ContentFilter.Load();
        }

        public void UpdateFilter()
        {
            Filter = ContentFilter.Load();
        }

        public override ItemManager ItemManager { 
            get { return _itemManager; }
        }

        public new void Start()
        {
            LoadSettings();
            ChannelsProvider = new ChannelContentProvider(Web, this);
            ArchiveProvider = new ArchiveContentProvider(Web, this);
            var epg = new EpgContentProvider(this);
            RecordsProvider = new RecordContentProvider(Web, this);
            Records = new RecordManager(this);
            _rectm = new Timer(1001);
            _rectm.Elapsed += RectmOnElapsed;
            PluginProvider = new PluginContentProvider(this);

            _rectm.Start();
            if (P2pProxyApp.MySettings.GetSetting("dlna", "enable", true))
            {
                base.Start();
                _itemManager = new ItemManager(this);
            }
        }

        public override void Stop()
        {
            PluginProvider.Clear();
            Records.SaveRecords();
            _rectm.Stop();
            base.Stop();
        }

        public void SendFile(MyWebRequest req)
        {
            switch (req.Url)
            {
                case "/dlna/logo48.png":
                    {
                        if (!File.Exists(P2pProxyApp.ExeDir + "/logo48.png"))
                            return;
                        FileStream stream = File.OpenRead(P2pProxyApp.ExeDir + "/logo48.png");
                        var resp = req.GetResponse();
                        resp.AddHeader(HttpHeader.ContentType, "image/png");
                        resp.AddHeader(HttpHeader.ContentLength, stream.Length.ToString());
                        stream.CopyTo(resp.GetStream());
                        break;
                    }
            }
        }

        public void AddRoute(string url, Action<MyWebRequest> route, HttpMethod method)
        {
            Web.AddRouteUrl(url, route, method);
        }

        public void SendDescription(MyWebRequest msg)
        {
            try
            {
                MemoryStream stream = new MemoryStream(server.GetDescription(msg.Headers["host"]));
                var resp = msg.GetResponse();
                resp.AddHeader(HttpHeader.ContentLength, stream.Length.ToString());
                resp.AddHeader(HttpHeader.ContentType, "text/xml; charset=\"utf-8\"");
                resp.SendHeaders();
                stream.CopyTo(resp.GetStream());
            }
            catch (Exception)
            {

            }
        }

        protected override void WriteSpecificDescription(XmlTextWriter descWriter, string host = null)
        {

            descWriter.WriteElementString("presentationURL", (string.IsNullOrEmpty(host) ? "http://127.0.0.1:" + Web.Port : "http://" + host) + "/help");
            descWriter.WriteElementString("dlna", "X_DLNADOC", "urn:schemas-dlna-org:device-1-0", "DMS-1.50");

            descWriter.WriteStartElement("iconList");

            foreach (string size in new[] { "48" })
            {
                descWriter.WriteStartElement("icon");
                descWriter.WriteElementString("mimetype", "image/png");
                descWriter.WriteElementString("width", size);
                descWriter.WriteElementString("height", size);
                descWriter.WriteElementString("depth", "24");
                descWriter.WriteElementString("url", string.Format("logo48.png"));
                descWriter.WriteEndElement();
            }

            descWriter.WriteEndElement();
        }
    }
}
