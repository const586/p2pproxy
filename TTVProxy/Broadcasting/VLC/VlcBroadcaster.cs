using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using P2pProxy.Broadcasting.Internal;
using P2pProxy;
using P2pProxy.Http.Server;

namespace P2pProxy.Broadcasting.VLC
{
    public class VlcBroadcaster : Broadcaster
	{
		static readonly string WRONGPASS = "Wrong password";
		static readonly string AUTHOK = "Welcome, Master";
		static readonly string BROADCASTEXISTS = "Name already in use";
		static readonly string SYNTAXERR = "Wrong command syntax";
		static readonly string STARTOK = "new";
		static readonly string STOPOK = "del";
		internal static readonly string STOPERR = "media unknown";
		static readonly string SHUTDOWN = "Bye-bye!";

		private Socket _sock;
		private Process _mainproc;
		private Dictionary<string, string> _broadcasts = new Dictionary<string, string>();
		private bool _inproccess;
		private int _broadcastport = 9082;
		private List<Transcode> _transcodes;
		private int _vlcport = 9081;
		private int _cache = 2000;
		private int _vlcmuxcache = 0;
		private string _passw = "passw";
		private bool _extvlc;
        private string AcePath;
		private string _extvlcpath;
        
        public VlcBroadcaster(WebServer server) : base(server)
        {
            LoadSettings();
        }

        public void LoadSettings()
        {
            _sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            _vlcport = P2pProxyApp.MySettings.GetSetting("vlc", "vlcport", 4212);
            _passw = P2pProxyApp.MySettings.GetSetting("vlc", "vlcpassw", "admin");
            _broadcastport = P2pProxyApp.MySettings.GetSetting("vlc", "vlcbroadcastport", 8082);
            _cache = P2pProxyApp.MySettings.GetSetting("vlc", "vlccache", 2000);
            _vlcmuxcache = P2pProxyApp.MySettings.GetSetting("vlc", "vlcmuxcache", 0);
            _extvlc = P2pProxyApp.MySettings.GetSetting("vlc", "vlcext", false);
            _extvlcpath = P2pProxyApp.MySettings.GetSetting("vlc", "vlcpath");
            if (string.IsNullOrEmpty(_extvlcpath) || !File.Exists(_extvlcpath))
                _extvlc = false;
            _broadcasts = new Dictionary<string, string>();
            _transcodes = Transcode.LoadTranscodes();

        }

        public override Stream GetStreamFromCID(string id, Dictionary<string, string> p)
        {
            if (_sock == null || !_sock.Connected)
                Start();
            var source = new Uri(GetUrl(broadcaser.RegisterBroadcast(id), p));
            TcpClient client = new TcpClient(source.Host, source.Port);
            var breq = Encoding.UTF8.GetBytes(string.Format("GET {0}{1}\r\nHost: {2}:{3}\r\nConnection: Keep-Alive\r\n\r\n", source.PathAndQuery, "", source.Host, source.Port));
            var stream = client.GetStream();
            stream.Write(breq, 0, breq.Length);
            
            return stream;
        }

        protected override Stream GetStreamInternal(string source, Dictionary<string, string> p)
		{
			if (_sock == null)
                Start();
			var req = WebRequest.Create(GetUrl(source, p));
			return req.GetResponse().GetResponseStream();
		}

		private string GetUrl(string source, Dictionary<string, string> p)
		{
			return _broadcasts.ContainsKey(source) ? _broadcasts[source] : CreateBroadcast(source, p);
		}
		private string CreateBroadcast(string source, Dictionary<string, string> p)
		{
			string transcode = p.ContainsKey("transcode") ? p["transcode"] : null;
			if (_mainproc != null && _mainproc.ProcessName != null)
			{
				_mainproc.Refresh();
			}
			while (_inproccess) Thread.Sleep(5);
			if (!_sock.Connected)
			{
				Connect();
				return CreateBroadcast(source, p);
			}
			string pid = Guid.NewGuid().ToString();
			if (_broadcasts.ContainsKey(source))
				return _broadcasts[source];

			string defaultOut = String.Format("http{{mux={1},dst=:{2}/{0}}}",
				pid, "ts{use-key-frames}", _broadcastport);
			var trans = GetTranscode(transcode);
			string command = string.Format(
				"new \"{0}\" broadcast input \"{1}\" output #",
				pid, source);
			if (string.IsNullOrEmpty(transcode) || trans == null)
				command = command + defaultOut;
			else
			{
				command = string.Format("{0}{1}http{{mux={2},dst=:{3}/{4}}}", command, trans, trans.Incapsulate,
					_broadcastport, pid);
			}
		    command = command + " enabled";
            Thread.Sleep(16);
		    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		    {
		        byte[] convert = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding(866),
		            Encoding.UTF8.GetBytes(command));

		        Send(convert);
		    }
		    else
		        Send(command);
		    var buffer = new byte[1024];

		    int b = _sock.Receive(buffer);

		    string recv = Encoding.UTF8.GetString(buffer, 0, b).Replace("> ", "");

		    if (recv.Contains(STARTOK))
		    {
		        _broadcasts.Add(source,
		            String.Format("http://127.0.0.1:{0}/{1}", _broadcastport, pid));
		        Send(String.Format("control {0} play", pid));
		        _sock.Receive(buffer);
		    }
		    else
		        throw new VlcError(recv);
		    return _broadcasts[source];
		}


		private void Send(string msg)
		{
            lock (_sock)
			if (_sock.Connected)
			{
				_sock.Send(Encoding.ASCII.GetBytes(msg + "\r\n"));
				Thread.Sleep(4);
			}
		}

		private void Send(byte[] msg)
		{
            lock(_sock)
			if (_sock.Connected)
			{
				_sock.Send(msg);
				_sock.Send(Encoding.ASCII.GetBytes("\r\n"));
				Thread.Sleep(4);
			}
		}


		private Transcode GetTranscode(string name)
		{
		    lock (_transcodes)
		    {
		        if (string.IsNullOrEmpty(name))
		            return null;
		        return _transcodes.FirstOrDefault(t => string.Equals(t.Name, name, StringComparison.OrdinalIgnoreCase));
		    }
		}

		private void Connect()
		{
		    lock (_sock)
		    {
		        try
		        {
		            _inproccess = true;
		            if (_mainproc != null)
		                Close();
		            _sock.Connect("127.0.0.1", _vlcport);
		        }
		        catch (Exception)
		        {
		            string vlcparam = string.Format(
		                "-I telnet --clock-jitter=0 --clock-synchro 0 --no-network-synchronisation --network-caching {0} --sout-mux-caching {3} --telnet-password {1} --telnet-port {2}",
		                _cache, _passw, _vlcport, _vlcmuxcache);
#if DEBUG
		            vlcparam = vlcparam + " --file-logging --logfile=vlc-log.txt";
#endif
		            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
		            {

		                if (!_extvlc)
		                {
                            var dir = new DirectoryInfo(TorrentStream.AcePath);
                            if (dir.Parent != null)
                                dir = new DirectoryInfo(dir.Parent.FullName + "/player");
                            _mainproc = Process.Start(dir.FullName + "/ace_player.exe",
                                          vlcparam);
		                }
		                else
		                    _mainproc = Process.Start(_extvlcpath, vlcparam);
		            }
		            else
		            {
		                _mainproc = Process.Start("vlc",
		                    vlcparam);
		            }
		            _mainproc.WaitForInputIdle();
		            _mainproc.EnableRaisingEvents = true;
		            _mainproc.Exited += RestartVlc;
		            int ci = 0;
		            while (!_sock.Connected && ci < 300)
		            {
		                ci++;
		                try
		                {
		                    _sock.Connect("127.0.0.1", _vlcport);
		                }
		                catch (Exception)
		                {
		                    Thread.Sleep(100);
		                }
		            }

		        }
		        if (!_sock.Connected)
		        {
		            _inproccess = false;
		        }

		        Thread.Sleep(4);
		        var buffer = new byte[1024];

		        _sock.ReceiveTimeout = 29893;
		        _sock.Receive(buffer);
		        _sock.Send(Encoding.ASCII.GetBytes(_passw+"\r\n"));
		        int b = _sock.Receive(buffer);
		        string recv = Encoding.UTF8.GetString(buffer, 0, b).Replace(">", "");
		        if (!recv.Contains(AUTHOK))
		        {
		            _sock.Send(Encoding.ASCII.GetBytes("shutdown\r\n"));
		            _sock.Close();
		        }
		    }
		    _inproccess = false;
		}

		private void RestartVlc(object sender, EventArgs e)
		{
			Connect();
			_inproccess = false;

		}

		private void Close()
		{
			if (_sock.Connected)
			{
				Send("del all");
				Send("shutdown");
				_sock.Close();
				_sock.Dispose();
				_sock = null;
				_broadcasts.Clear();
			}
			if (_mainproc != null)
			{
				_mainproc.Exited -= RestartVlc;
				try
				{

					_mainproc.Kill();
				} catch (Exception)
				{
				}

				_mainproc.Close();
				_mainproc.Dispose();
			}
			GC.Collect();

		}

		internal override void Start()
		{
			_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
			Connect();
		}

		internal override void Stop()
		{
			Close();
		}

		protected override void CloseStreamInternal(string source)
		{
			if (_sock.Connected)
			{
				if (_broadcasts.ContainsKey(source))
				{
					Send(String.Format("del \"{0}\"", _broadcasts[source]));

					var buffer = new byte[1024];
					int b = _sock.Receive(buffer);
					string recv = Encoding.UTF8.GetString(buffer, 0, b).Replace("> ", "");
					if (recv.Contains(STOPOK))
						_broadcasts.Remove(source);
					else throw new VlcStopError();
				} else
				{

				}
			}

		}

		public override bool Contains(string source)
		{
			return _broadcasts.ContainsKey(source);
		}

        public IEnumerable<Transcode> GetTranscodes()
        {
            return _transcodes.AsEnumerable();
        }

        public void SaveTranscodes()
        {
            Transcode.SaveTranscodes(_transcodes);
        }

        public void AddTranscode(Transcode trans)
        {
            _transcodes.Add(trans);
        }

        public void RemoveTranscode(Transcode trans)
        {
            _transcodes.Remove(trans);
        }

	}

	public class Transcode
	{
		private static readonly string VIDEO_CODEC = "vcodec";
		private static readonly string VIDEO_BITRATE = "vb";
		private static readonly string VIDEO_FPS = "fps";
		private static readonly string VIDEO_WIDTH = "width";
		private static readonly string VIDEO_HEIGHT = "height";
		private static readonly string AUDIO_CODEC = "acodec";
		private static readonly string AUDIO_BITRATE = "ab";
		private static readonly string AUDIO_CHANNELS = "channels";
		private static readonly string AUDIO_RATE = "samplerate";
		private static readonly string INCAPSULATE = "mux";

		public string Name;
		public string VideoCodec;
		public ushort VideoBitrate;
		public float VideoFps;
		public ushort VideoWidth;
		public ushort VideoHeight;
		public string AudioCodec;
		public ushort AudioBitrate;
		public ushort AudioChannels;
		public uint AudioRate;
		public string Incapsulate;

		public Transcode(string name)
		{
			Name = name;
			Incapsulate = "ts";

			VideoCodec = "";
			VideoBitrate = 0;
			VideoFps = 0;
			VideoWidth = 0;
			VideoHeight = 0;
			AudioCodec = "";
			AudioBitrate = 0;
			AudioChannels = 0;
			AudioRate = 0;
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			List<string> transes = new List<string>();
			if (!string.IsNullOrEmpty(VideoCodec) || string.IsNullOrEmpty(AudioCodec))
			{
				sb.Append("transcode{");
				if (!string.IsNullOrEmpty(VideoCodec))
					transes.Add(VIDEO_CODEC + "=" + VideoCodec);
				//sb.Append(VIDEO_CODEC + "=" + VideoCodec);
				if (VideoBitrate > 0)
					transes.Add(VIDEO_BITRATE + "=" + VideoBitrate);
				//sb.Append("," + VIDEO_BITRATE + "=" + VideoBitrate);
				if (VideoFps > 0)
					transes.Add(VIDEO_FPS + "=" + VideoFps);
				//sb.Append("," + VIDEO_FPS + "=" + VideoFps);
				if (VideoWidth > 0)
					transes.Add(VIDEO_WIDTH + "=" + VideoWidth);
				//sb.Append("," + VIDEO_WIDTH + "=" + VideoWidth);
				if (VideoHeight > 0)
					transes.Add(VIDEO_HEIGHT + "=" + VideoHeight);
				//sb.Append("," + VIDEO_HEIGHT + "=" + VideoHeight);
				if (!string.IsNullOrEmpty(AudioCodec))
					transes.Add(AUDIO_CODEC + "=" + AudioCodec);
				//sb.Append("," + AUDIO_CODEC + "=" + AudioCodec);
				if (AudioBitrate > 0)
					transes.Add(AUDIO_BITRATE + "=" + AudioBitrate);
				//sb.Append("," + AUDIO_BITRATE + "=" + AudioBitrate);
				if (AudioChannels > 0)
					transes.Add(AUDIO_CHANNELS + "=" + AudioChannels);
				//sb.Append("," + AUDIO_CHANNELS + "=" + AudioChannels);
				if (AudioRate > 0)
					transes.Add(AUDIO_RATE + "=" + AudioRate);
				//sb.Append("," + AUDIO_RATE + "=" + AudioRate);
				sb.Append(string.Join(",", transes));
				sb.Append("}:");
			}
			return sb.ToString();
		}

		public static List<Transcode> LoadTranscodes()
		{
            string file = P2pProxyApp.ApplicationDataFolder + "/transcodes.xml";
			List<Transcode> listOfTranses = new List<Transcode>();
			if (!File.Exists(file))
				return listOfTranses;
			XDocument xd = XDocument.Load(file);
			var xroot = xd.Element("transcodes");
			if (xroot == null)
				return listOfTranses;

			foreach (var xe in xroot.Elements("transcode"))
			{
				Transcode trans = new Transcode(xe.Attribute("name").Value);
				string value = xe.Attribute("value").Value;
				if (string.IsNullOrEmpty(value))
					continue;
				var values = value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				foreach (var v in values)
				{
					var vs = v.Split("=".ToCharArray(), StringSplitOptions.None);
					if (vs[0] == VIDEO_CODEC)
						trans.VideoCodec = vs[1];
					else if (vs[0] == VIDEO_BITRATE)
						ushort.TryParse(vs[1], out trans.VideoBitrate);
					else if (vs[0] == VIDEO_FPS)
					{
						NumberFormatInfo format = new NumberFormatInfo { CurrencyDecimalSeparator = "." };
						float.TryParse(vs[1], NumberStyles.Float, format, out trans.VideoFps);
					} else if (vs[0] == VIDEO_WIDTH)
						ushort.TryParse(vs[1], out trans.VideoWidth);
					else if (vs[0] == VIDEO_HEIGHT)
						ushort.TryParse(vs[1], out trans.VideoHeight);
					else if (vs[0] == AUDIO_CODEC)
						trans.AudioCodec = vs[1];
					else if (vs[0] == AUDIO_BITRATE)
						ushort.TryParse(vs[1], out trans.AudioBitrate);
					else if (vs[0] == AUDIO_CHANNELS)
						ushort.TryParse(vs[1], out trans.AudioChannels);
					else if (vs[0] == AUDIO_RATE)
						uint.TryParse(vs[1], out trans.AudioRate);
					else if (vs[0] == INCAPSULATE)
						trans.Incapsulate = vs[1];
				}
				listOfTranses.Add(trans);
			}
			return listOfTranses;
		}

		public static void SaveTranscodes(List<Transcode> transes)
		{
			try
			{
				XDocument xd = new XDocument();
				XElement xroot = new XElement("transcodes");
				xd.Add(xroot);
				foreach (var transcode in transes)
				{
					List<string> values = new List<string>();
					if (!string.IsNullOrEmpty(transcode.Incapsulate))
						values.Add(INCAPSULATE + "=" + transcode.Incapsulate);
					if (!string.IsNullOrEmpty(transcode.VideoCodec))
						values.Add(VIDEO_CODEC + "=" + transcode.VideoCodec);
					if (!string.IsNullOrEmpty(transcode.AudioCodec))
						values.Add(AUDIO_CODEC + "=" + transcode.AudioCodec);
					if (transcode.AudioBitrate > 0)
						values.Add(AUDIO_BITRATE + "=" + transcode.AudioBitrate);
					if (transcode.AudioChannels > 0)
						values.Add(AUDIO_CHANNELS + "=" + transcode.AudioChannels);
					if (transcode.AudioRate > 0)
						values.Add(AUDIO_RATE + "=" + transcode.AudioRate);
					if (transcode.VideoBitrate > 0)
						values.Add(VIDEO_BITRATE + "=" + transcode.VideoBitrate);
					if (transcode.VideoFps > 0)
						values.Add(VIDEO_FPS + "=" + transcode.VideoFps);
					if (transcode.VideoHeight > 0)
						values.Add(VIDEO_HEIGHT + "=" + transcode.VideoHeight);
					if (transcode.VideoWidth > 0)
						values.Add(VIDEO_WIDTH + "=" + transcode.VideoWidth);
					string value = string.Join(",", values);
					xroot.Add(new XElement("transcode", new XAttribute("name", transcode.Name), new XAttribute("value", value)));
				}
				string file = "/transcodes.xml";
				if (File.Exists(file))
					File.Delete(file);
				xd.Save(file);
			} catch
			{

			}
		}
	}


}
