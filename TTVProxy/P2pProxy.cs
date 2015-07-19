using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using CryptoLibrary;
using Microsoft.Win32;
using P2pProxy;
using P2pProxy.Broadcasting;
using P2pProxy.Broadcasting.Internal;
using P2pProxy.Broadcasting.Simple;
using P2pProxy.Broadcasting.VLC;
using SimpleLogger;
using TTVApi;
using P2pProxy.Http.Server;

namespace P2pProxy
{
    public class P2pProxyApp
    {
        public const int AGE_CACHE = 403;
        public static string ExeDir = "";
        public static Logger Log;
        public static SettingManager MySettings;
        public P2pProxyDevice Device;
        public Auth SessionState { get; protected set; }

        public Broadcaster Broadcaster;

        public static bool Debug;

        private WebServer _web;

        private static string _appdata;


        public static string ApplicationDataFolder
        {
            get
            {
                if (string.IsNullOrEmpty(_appdata))
                {
                    _appdata = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"P2pProxy");
                    if (!Directory.Exists(_appdata))
                        Directory.CreateDirectory(_appdata);
                }
                return _appdata;
            }
        }

        public static string Version
        {
            get
            {
                Version ver = Assembly.GetExecutingAssembly().GetName().Version;
                return ver.ToString(4);
            }
        }

        public bool IsWorking { get; set; }


        public delegate void Notify(string text, TypeMessage typemsg);
        public event Notify Notifyed;

        protected virtual void OnNotifyed(string text, TypeMessage typemsg)
        {
            Notify handler = Notifyed;
            
            Log.Write(text, typemsg);
            if (handler != null) handler(text, typemsg);
        }

        private readonly List<TorrentStream> _tsPool = new List<TorrentStream>();

        public P2pProxyApp(bool console = false, bool debug = false)
        {
            ExeDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log = new Logger(ApplicationDataFolder + String.Format("/P2pProxy_{0:ddMMyy}.log", DateTime.Now), console);
            MySettings = new SettingManager(ApplicationDataFolder + "/settings.xml");
            IsWorking = true;
            Debug = debug;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;


        }

        private bool AlwaysRunning()
        {
            Assembly asm = Assembly.GetEntryAssembly();
            var proc = Process.GetProcessesByName(asm.GetName().Name);
            if (proc.Count() > 1)
                return true;
            return false;
        }

        public void Start()
        {
            if (AlwaysRunning())
            {
                OnNotifyed("Программа уже запущена", TypeMessage.Critical);
                Thread.Sleep(4);
                Process.GetCurrentProcess().Close();
                Process.GetCurrentProcess().Kill();
                IsWorking = false;
                return;
            }
            Log.Write("Start P2pProxy", TypeMessage.Info);
            var port = MySettings.GetSetting("web", "port", 8081);

            Log.Write("[P2pProxy] Запуск Веб-сервера", TypeMessage.Info);
            try
            {
                _web = new WebServer(Convert.ToUInt16(port));
                _web.AddRouteUrl("/help", HelpRequest, HttpMethod.Get);
                _web.Start();
                IsWorking = true;
            }
            catch
            {
                IsWorking = false;
                OnNotifyed("Порт закрыт. Дальнейшая работа программы не возможна", TypeMessage.Critical);
            }

            Device = new P2pProxyDevice(_web, this);
            Device.Start();
            Login();
                
            try
            {
                Broadcaster = new VlcBroadcaster(_web);
            }
            catch (Exception)
            {

                OnNotifyed("Не удалось запустить VLC-сервер. Дальнейшая работа программы не возможна", TypeMessage.Error);
                IsWorking = false;
                return;
            }

        }

        void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Resume: Start(); break;
                case PowerModes.Suspend: Stop(); break;
            }
            Log.Write("P2pProxy is " + e.Mode, TypeMessage.Info);
        }

        private void Pays(MyWebRequest myWebRequest)
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("P2pProxy.pay.html");
            if (stream != null)
            {
                StreamReader sr = new StreamReader(stream);
                var text = sr.ReadToEnd().Replace("@udn", Device.Udn.ToString());
            
                var resp = myWebRequest.GetResponse();
                resp.AddHeader("Content-Type", "text/html; charset=UTF-8");
                var res = Encoding.UTF8.GetBytes(text);
                resp.AddHeader("Content-Length", res.Length.ToString());
                resp.SendHeaders();
                resp.GetStream().Write(res, 0, res.Count());
            }
        }

        public Stream GetStreamFromCID(string id, Dictionary<string, string> p)
        {
            //if (!p.ContainsKey("transcode"))
            //    return defaultBroadcater.GetStreamFromCID(id, p);
            return Broadcaster.GetStreamFromCID(id, p);
        }

        public void Stop()
        {
            _web.Close();
            Broadcaster.Stop();
            _tsPool.ForEach(stream => stream.Disconnect());
            _tsPool.Clear();
            Device.Stop();
        }

        private void HelpRequest(MyWebRequest req)
        {
            var resp = req.GetResponse();
            StringBuilder sb = new StringBuilder();
            sb.Append("<html><head><title>P2pProxy</title></head><body><h1>Список доступных команд</h1>");
            foreach (var route in _web.GetRoutes().Where(s => s.Contains("Get_")))
            {
                sb.Append(String.Format("<a href=http://{0}{1}>http://{0}{1}</a><br/>", req.Headers["host"], route.Split("_".ToCharArray(), 2)[1]));
            }
            sb.Append("</body></html>");
            var res = Encoding.UTF8.GetBytes(sb.ToString());
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".html").ToString());
            resp.AddHeader(HttpHeader.ContentLength, res.Length.ToString());
            resp.SendHeaders();
            resp.GetStream().Write(res, 0, res.Length);
        }

        public void Close()
        {
            try
            {
                SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
                Broadcaster.Stop();
                lock (_tsPool)
                {
                    foreach (var torrentStream in _tsPool)
                    {
                        torrentStream.Disconnect();
                    }
                    _tsPool.Clear();
                }
                Device.Stop();
                _web.Close();
                IsWorking = false;
                Log.Close();
            }
            catch (Exception)
            {

            }
            
        }

        public bool Login()
        {
            try
            {
                string login = MySettings.GetSetting("torrent-tv.ru", "login", "anonymous");
                string pass = MySettings.GetSetting("torrent-tv.ru", "password", "anonymous");
                if (pass != "anonymous")
                {
                    try
                    {
                        pass = CryptoHelper.Decrypt<System.Security.Cryptography.AesCryptoServiceProvider>(
                            pass ?? "", Environment.MachineName, "_Cr[e?g1");
                    }
                    catch
                    {
                        OnNotifyed("Ошибка авторизации: incorrect", TypeMessage.Error);
                        SessionState = new Auth { error = ApiError.incorrect.ToString()};
                        return false;
                    }
                }
                var func = new Auth(login, pass, Device.Udn.ToString());
                SessionState = func.Run("");
                if (!SessionState.IsSuccess)
                {
                    OnNotifyed("Ошибка авторизации: " + SessionState.error, TypeMessage.Error);
                    return false;
                }
                OnNotifyed("Авторизация успешна", 0);
                return true;
            }
            catch (Exception ex)
            {
                Log.Write(ex.Message, TypeMessage.Error);
                return false;
            }
            
        }

        public TorrentStream GetTsClient(string key)
        {
            lock (_tsPool) return _tsPool.FirstOrDefault(c => c.PlayedFile == key);
        }

        public void AddToTsPool(TorrentStream ts)
        {
            lock (_tsPool) _tsPool.Add(ts);
        }

        public void RemoveFromTsPoos(TorrentStream ts)
        {
            lock (_tsPool)
            {
                if (_tsPool.Contains(ts))
                {
                    ts.Disconnect();
                    _tsPool.Remove(ts);
                }
            }
        }

        private readonly object locker = new object();

        //public string FindBroadcastUrl(string file)
        //{
        //    lock (locker)
        //        return Vlc.StartBroadcast(new Uri(file), "", null).ToString();
        //}

        //public string StartBroadcastStream(string file, string transcode = null)
        //{
        //    lock (locker)
        //    {
        //        return Vlc.StartBroadcast(new Uri(file), transcode, null).ToString();
        //    }
        //}

        //internal List<VlcTranscode> GetTranscodes()
        //{
        //    if (Vlc is VlcBroadcast)
        //    {
        //        lock (locker)
        //            return (Vlc as VlcBroadcast).GetTranscodes();
        //    }
        //    return new List<VlcTranscode>();
        //}

        //public void SaveTranscodes()
        //{
        //    if (Broadcaster is VlcBroadcast)
        //    lock (locker)
        //        (Broadcaster as VlcBroadcast).SaveTranscodes();
        //}

        //public void AddTranscode(VlcTranscode trans)
        //{
        //    if (Broadcaster is VlcBroadcast)
        //    lock (locker)
        //        (Broadcaster as VlcBroadcast).AddTranscode(trans);
        //}

        //public void RemoveTranscode(VlcTranscode trans)
        //{
        //    if (Broadcaster is VlcBroadcast)
        //    lock (locker)
        //        (Broadcaster as VlcBroadcast).RemoveTranscode(trans);
        //}

        //public void AddVlcBroadcastClient(string file, TcpClient client)
        //{
        //    lock (locker)
        //    {
        //        var bc = new BroadcastClient(new Uri(file), client))
        //        Vlc.StartBroadcast(new Uri(file), "", client);
        //    }
        //}

        //public void AddVodToVlc(string key, string path)
        //{
        //    lock (locker)
        //        Vlc.AddFileToVod(path, key);
        //}

        //public int GetVlcRtspPort()
        //{
        //    return Vlc.RtspPort;
        //}

        //public Dictionary<string, string> GetBroadcasts()
        //{
        //    lock (locker)
        //        return Vlc.GetBroadcasts();
        //}

        //public IEnumerable<KeyValuePair<string, string>> GetVods()
        //{
        //    lock (locker)
        //    {
        //        return Vlc.GetVods();
        //    }
        //}

        //public int RoutedClientsCount(string url)
        //{
        //    lock (locker)
        //        return Vlc.RoutedClientsCount(url);
        //}

        //public string GetVodUrl(string key)
        //{
        //    lock (locker)
        //        return Vlc.GetVodUrl(key);
        //}

        //public bool StopBroadcast(string broadcast, string file)
        //{
        //    lock (locker)
        //    {
        //        if (Vlc.RoutedClientsCount(broadcast) == 0)
        //        {
        //            Vlc.Stop(file);
        //            return true;
        //        }
        //        return false;
        //    }
        //}
    }
}
