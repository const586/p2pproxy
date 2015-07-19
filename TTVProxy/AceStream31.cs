using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using SimpleLogger;
using P2pProxy;

namespace P2pProxy
{
    public class AceStream31
    {
        private PlatformID _curPlantform;
        private string _tsPath;
        private int _port;
        public static int WebASPort = 6878;
        private Dictionary<string, int> _contents = new Dictionary<string, int>();

        public string GenContentUrl(string id)
        {
            return string.Format("http://127.0.0.1:{0}/ace/getstream?id={1}&sid={2}", WebASPort, id, _contents[id]);
        }

        public string GenContentUrlHls(string id)
        {
            return string.Format("http://127.0.0.1:{0}/ace/manifest.m3u8?id={1}&sid={2}", WebASPort, id, _contents[id]);
        }

        public string Play(string id, StreamType type = StreamType.Live)
        {
            if (_contents.ContainsKey(id))
                return type == StreamType.Live ? GenContentUrlHls(id) : GenContentUrl(id);
            int max = 0;
            if (_contents.Count > 0)
                max = _contents.Max(c => c.Value);
            max++;
            _contents.Add(id, max);
            return type == StreamType.Live ? GenContentUrlHls(id) : GenContentUrl(id);
        }
    }

    public enum StreamType
    {
        Vod, Live
    }
}