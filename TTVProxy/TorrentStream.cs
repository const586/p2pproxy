using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLogger;
using TTVApi;

namespace P2pProxy
{
    public class TorrentStream
    {
        public static readonly string MSG_HELLOBG = "HELLOBG";
        public static readonly string MSG_HELLOTS = "HELLOTS";
        public static readonly string MSG_READY = "READY";
        public static readonly string MSG_LOADASYNC = "LOADASYNC";
        public static readonly string MSG_LOADRESP = "LOADRESP";
        public static readonly string MSG_START = "START";
        public static readonly string MSG_STOP = "STOP";
        public static readonly string MSG_GETPID = "GETPID";
        public static readonly string MSG_GETCID = "GETCID";
        public static readonly string MSG_GETADURL = "GETADURL";
        public static readonly string MSG_USERDATA = "USERDATA";
        public static readonly string MSG_SAVE = "SAVE";
        public static readonly string MSG_LIVESEEK = "LIVESEEK";
        public static readonly string MSG_SHUTDOWN = "SHUTDOWN";
        public static readonly string MSG_PLAY = "PLAY";
        public static readonly string MSG_PLAYAD = "PLAYAD";
        public static readonly string MSG_PLAYADI = "PLAYADI";
        public static readonly string MSG_PAUSE = "PAUSE";
        public static readonly string MSG_RESUME = "RESUME";
        public static readonly string MSG_DUR = "DUR";
        public static readonly string MSG_PLAYBACK = "PLAYBACK";
        public static readonly string MSG_EVENT = "EVENT";
        public static readonly string MSG_STATE = "STATE";
        public static readonly string MSG_INFO = "INFO";
        public static readonly string MSG_STATUS = "STATUS";
        public static readonly string MSG_AUTH = "AUTH";

        private readonly Socket _tssock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
        private readonly PlatformID _curPlantform = Environment.OSVersion.Platform;
        private readonly string _defaultAddr = "127.0.0.1";
        private static string _tsPath;
        private readonly int _port = 62062;
        private AsyncOperation _operation;
        private Task<string> _playdTask;
        private readonly List<TSMessage> _messagePool = new List<TSMessage>();
        private int _asTimeoute = 60;

        public delegate void OnReceive(TorrentStream sender, TSMessage msg);

        public delegate void OnReceiveError(TorrentStream sender, Exception ex);

        public event OnReceive OnReceived;
        public event OnReceiveError OnError;
        public string ContentType;

        public bool Connected { get { return _tssock.Connected; } }
        public string AceBroadcast { get; private set; }
        public string PlayedFile { get; private set; }
        public List<TcpClient> Owner { get; private set; }
        public bool Started { get; private set; }

        

        public static string AcePath
        {
            get
            {
                if (string.IsNullOrEmpty(_tsPath))
                {
                    RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\ACEStream");
                    if (key != null) _tsPath = (string)key.GetValue("EnginePath");
                }
                return Path.GetDirectoryName(_tsPath);
            }
        }

        public Task<string> GetPlayTask()
        {
            return _playdTask;
        }

        protected TorrentStream()
        {
            LoadSettings();
        }

        public TorrentStream(TcpClient owner = null) : this()
        {
            Owner = new List<TcpClient>();
            if (owner != null)
                Owner.Add(owner);
            if (_curPlantform == PlatformID.Win32NT)
            {
                RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\ACEStream");
                if (key != null) _tsPath = (string) key.GetValue("EnginePath");
                else RaiseError(new Exception("AceStreamNotInstalled"));
                string acePath = Path.GetDirectoryName(_tsPath) + "/acestream.port";
                if (!File.Exists(acePath))
                {
                    if (!File.Exists(_tsPath))
                    {
                        RaiseError(new Exception("AceStreamNotFound"));
                        return;
                    }
                    RunTS();
                }
                while (File.Exists(acePath) == false)
                {
                    Thread.Sleep(16);
                }
                using (StreamReader sr = File.OpenText(acePath))
                {
                    _port = Convert.ToInt32(sr.ReadLine());
                    sr.Close();
                }

            }
        }
     
        public Task<string> Play(string file, SourceType source, int index = 0)
        {
            if (source == SourceType.Torrent)
            {
                if (string.IsNullOrEmpty(file))
                {
                    P2pProxyApp.Log.Write("Не верная ссылка на торрент", TypeMessage.Error);
                    throw new Exception("Не верная ссылка на торрент");
                }
                file = new Uri(file).ToString();
            }
            string req = String.Format("START {0} {1} ", source.ToString().ToUpper(), file);
            if (source == SourceType.ContentId)
                req = req + index;
            else
                req = req + index + " 0 0 0";
            _playdTask = new Task<string>(WaytingPlay);
            _playdTask.Start();
            SendMessage(req.Replace("CONTENTID", "PID"));
            PlayedFile = file;
            return _playdTask;
        }

        public LoadRespData ReadTorrent(string file, SourceType source)
        {
            if (source == SourceType.Torrent)
            {
                if (string.IsNullOrEmpty(file))
                {
                    P2pProxyApp.Log.Write("Не верная ссылка на торрент", TypeMessage.Error);
                    throw new Exception("Не верная ссылка на торрент");
                }
                file = new Uri(file).ToString();
            }
            Random rand = new Random(DateTime.Now.Millisecond);
            string req = String.Format("LOADASYNC {2} {0} {1}", source.ToString().ToUpper(), file, rand.Next(100000, 200000));
            if (source != SourceType.ContentId)
                req = req + " 0 0 0";
            req = req.Replace("CONTENTID", "PID");
            //SendMessage("LOADASYNC 126500 PID 1ccf192064ee2d95e91a79f91c6097273d582827");
            SendMessage(req.Replace("CONTENTID", "PID"));
            int cwait = 0;
            bool exit = false;
            LoadRespData files = null;
            try
            {
                while (!exit)
                {
                    Thread.Sleep(24);
                    cwait += 24;
                    if (cwait >= 1000)
                        break;
                    lock (_messagePool)
                    {
                        if (!_messagePool.Any())
                            continue;
                        foreach (var msg in _messagePool)
                        {
                            if (msg.Type == MSG_LOADRESP)
                            {
                                exit = true;
                                var data = (msg.InnerData as LoadRespData);
                                if (data == null)
                                    break;
                                files = msg.InnerData as LoadRespData;
                                break;
                                //resp.files.AddRange(new String[(msg.InnerData as LoadRespData).Files.Count]);
                                //foreach (var f in (msg.InnerData as LoadRespData).Files)
                                //    resp.files[(int)f[1]] = f[0].ToString();

                            }
                        }
                        _messagePool.Clear();
                    }
                    
                }
            }
            catch (Exception)
            {
            }
            
            return files;
        }

        public string GetContentId(LoadRespData resp)
        {
            var cmd = string.Format("GETCID checksum={0} infohash={1} developer={2} affiliant={3} zoneid={4}", resp.CheckSum, resp.InfoHash, 0, 0, 0);
            SendMessage(cmd);
            bool exit = false;
            int cwait = 0;
            string res = string.Empty;
            try
            {
                while (!exit)
                {
                    Thread.Sleep(24);
                    cwait += 24;
                    if (cwait >= 1000)
                        break;
                    lock (_messagePool)
                    {
                        if (!_messagePool.Any())
                            continue;
                        foreach (var msg in _messagePool)
                        {
                            if (msg.Text.Substring(0, 2) == "##")
                            {
                                exit = true;
                                res = msg.Text.Substring(2);
                            }
                        }
                        _messagePool.Clear();
                    }

                }
            }
            catch (Exception)
            {
            }
            return res;
        }

        private void LoadSettings()
        {
            try
            {
                _asTimeoute = P2pProxyApp.MySettings.GetSetting("system", "as_timeout", 60);
            }
            catch
            {
                _asTimeoute = 60;
            }
        }

        private string WaytingPlay()
        {
            DateTime time = DateTime.Now;
            time = time.AddSeconds(_asTimeoute);
            while (_tssock.Connected && (Owner.Any(client => client.Connected) && DateTime.Now < time || Owner.Count == 0))
            {
                lock (_messagePool)
                {
                    foreach (var tsMessage in _messagePool)
                    {
                        if (tsMessage.Type == MSG_START)
                        {
                            if (tsMessage.Parameters.ContainsKey("ad"))
                            {

                                var req = WebRequest.Create(AceBroadcast).GetResponse().GetResponseStream();
                                var buf = new byte[384];
                                for (int i = 0; i < 4 && req != null && req.Read(buf, 0, buf.Length) > 0; i++)
                                {
                                    Thread.Sleep(1000);
                                    switch (i)
                                    {
                                        case 0: SendMessage(String.Format("PLAYBACK {0} 0", AceBroadcast)); break;
                                        case 1: SendMessage(String.Format("PLAYBACK {0} 25", AceBroadcast)); break;
                                        case 2: SendMessage(String.Format("PLAYBACK {0} 50", AceBroadcast)); break;
                                        case 3: SendMessage(String.Format("PLAYBACK {0} 75", AceBroadcast)); break;
                                    }
                                }
                                SendMessage(String.Format("PLAYBACK {0} 100", AceBroadcast));
                                if (req != null) req.Close();
                            }
                            else
                            {
                                _messagePool.Clear();
                                return AceBroadcast;
                            }
                        }
                    }
                    _messagePool.Clear();
                }
                Thread.Sleep(4);
            }
            return null;
        }

        public void RunTS()
        {
            P2pProxyApp.Log.Write("[AceStream] Запуск AceStream", TypeMessage.Info);
            if (_curPlantform == PlatformID.Win32NT)
                Process.Start(_tsPath);
            else
            {
                Process.Start("acestreamengine", "--client-console");
            }
        }

        public TorrentStream(int port, TcpClient owner = null) : this()
        {
            _port = port;
            Owner = new List<TcpClient>();
            if (owner != null)
                Owner.Add(owner);

            if (IsRunning()) RunTS();
        }

        public TorrentStream(string addr, int port, TcpClient owner = null) : this()
        {
            _defaultAddr = addr;
            _port = port;
            Owner = new List<TcpClient>();
            if (owner != null)
                Owner.Add(owner);

            if (IsRunning()) RunTS();
        }

        public bool IsRunning()
        {
            if (_curPlantform == PlatformID.Win32NT)
                return Process.GetProcessesByName("tsengine.exe").Any();
            return Process.GetProcesses().Any(i => i.ProcessName.Contains("acestreamengine"));
        }

        public bool Connect()
        {
            try
            {
                _tssock.Connect(_defaultAddr, _port);
            }
            catch
            {
                RunTS();
                int ci = 0;
                while (!_tssock.Connected && ci < 100)
                {
                    try
                    {
                        _tssock.Connect(_defaultAddr, _port);
                        break;
                    }
                    catch (Exception)
                    {
                        ci++;
                        Thread.Sleep(100);
                    }
                }
                
            }
            if (_tssock.Connected)
            {
                
                SendMessage(MSG_HELLOBG + " version=4");
                var buf = new byte[192];
                int bcount = _tssock.Receive(buf);
                const string productKey = "c1rvequTEgoyC06zTVz1-Yl12lvAzWyv-7WYVhxe7A4zR2fUNPZw8l1y-riKqspd5";
                var sbuf = Encoding.Default.GetString(buf, 0, bcount - 2);
                var prms = ParseParametersToDictionary(sbuf);
                if (!prms.ContainsKey("key"))
                {
                    Disconnect();
                    return false;
                }
                var sha = new SHA1CryptoServiceProvider();
                var result = sha.ComputeHash(Encoding.Default.GetBytes(prms["key"] + productKey));
                var signature = BitConverter.ToString(result).Replace("-", "").ToLower();
                var x = productKey.Split("-".ToCharArray())[0];
                SendMessage(String.Format("{2} key={0}-{1}", x, signature, MSG_READY));
                _operation = AsyncOperationManager.CreateOperation(null);
                var thr = new Thread(Receive) { IsBackground = true };
                thr.Start();
            }
            else throw new Exception(SocketError.ConnectionRefused.ToString());
            
            return _tssock.Connected;
        }

        private Dictionary<string, string> ParseParametersToDictionary(string value)
        {
            var cmd = value.Split(" ".ToCharArray());
            return cmd.Skip(1).Select(s => s.Split("=".ToCharArray())).ToDictionary(prm => prm[0], prm => prm[1]);
        }

        private void RaisReceived(object msg)
        {
            _operation.Post(state =>
            {
                if (OnReceived != null && msg != null)
                    OnReceived(this, (TSMessage)msg);
            }, null);
            
        }

        private void RaiseError(object e)
        {
            _operation.Post(state =>
            {
                if (OnError != null)
                    OnError(this, (Exception)e);
            }, null);
            
        }

        private void Receive()
        {
            var buffer = new byte[Int16.MaxValue];
            string msg = String.Empty;
            while (_tssock.Connected)
            {
                try
                {
                    if (Owner == null)
                    {
                        _tssock.Close();
                        return;
                    }

                    int bytes = 0;
                    try
                    {
                        bytes = _tssock.Receive(buffer);
                    }
                    catch (ObjectDisposedException e)
                    {
                        break;
                    }
                    if (bytes > 0)
                    {
                        msg = msg + Encoding.UTF8.GetString(buffer, 0, bytes);
                        if (!msg.Contains("\r\n") || msg.LastIndexOf("\r\n", StringComparison.Ordinal) < msg.Length - 2)
                            continue;
                        var msglist = msg.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (var s in msglist)
                        {
                            var tmsg = TSMessage.Construct(s);
                            if (tmsg.Type == MSG_START)
                            {
                                Started = true;
                                AceBroadcast = tmsg.Parameters["url"];
                            }
                            else if (s == "STATUS 0")
                                Started = false;
                            else if (tmsg.Type == MSG_EVENT)
                            {
                                if (tmsg.Text.Contains("getuserdata"))
                                {
                                    SendMessage("USERDATA [{\"gender\": 1}, {\"age\": 1}]");
                                    Disconnect();
                                }
                            }
                            lock (_messagePool)
                            {
                                _messagePool.Add(tmsg);
                            }
                            RaisReceived(tmsg);
                        }
                        msg = String.Empty;
                    }
                    else
                    {
                        break;
                    }
                    Thread.Sleep(20);
                }
                catch (SocketException)
                {
                    lock (_tssock)
                    {
                        _tssock.Close();
                    }
                    break;
                }
                
            }
            if (_tssock.Connected)
                Disconnect();
        }

        public void SendMessage(string msg)
        {
            try
            {
#if DEBUG
                Console.WriteLine(msg);
#endif
                lock (_tssock)
                {
                    if (_tssock.Connected)
                        _tssock.Send(Encoding.UTF8.GetBytes(msg + "\r\n"));
                    else
                        RaiseError(new Exception("AceStream is disconnected"));
                }
                
            }
            catch (Exception)
            {
                
            }
            
        }

        public void Disconnect(bool shutdown = true)
        {
            lock (_tssock)
            {
                if (_tssock.Connected)
                {
                    SendMessage(MSG_STOP);
                    SendMessage(MSG_SHUTDOWN);
                    //tssock.Disconnect(true);
                    _tssock.Close(1000);
                }
            }
            
        }

        public string GetStreamUrl()
        {
            return String.Empty;
        }

        public string GetTorrentUrl()
        {
            return String.Empty;
        }
    }
}
