using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2pProxy.Broadcasting.Simple
{
    class HlsClient : IEnumerator<HlsSegment>
    {
        public HlsSegment[] Playlist = new HlsSegment[0];
        public string _url;
        public byte _cache;
        public short _index = -1;
        public DateTime LastRequested;
        private IFormatProvider doubleProvider;
        public HlsSegment Current
        {
            get
            {
                if (Playlist.Length == 0 || Playlist.All(s => s.Index < _index || _index == -1))
                {
                    Console.WriteLine("Запрос из Current");
                    UpdatePlaylist();
                }
                Console.WriteLine("Запрошен {0} максимальный {1} сегментов в плейлисте {2}", _index, Playlist.Last().Index, Playlist.Length);
                return this[_index];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }
        
        private HlsClient(string url, byte cache = 1)
        {
            _url = url;
            _cache = cache;
        }
        public void UpdatePlaylist()
        {
            List<HlsSegment> segments = new List<HlsSegment>();
            using (StreamReader sr = new StreamReader(WebRequest.CreateHttp(_url).GetResponse().GetResponseStream()))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine().ToUpper();
                    if (string.IsNullOrEmpty(line))
                        continue;
                    if (line.IndexOf("#EXTINF:") == 0)
                    {
                        double duration = double.Parse(line.Substring(8, 8), CultureInfo.InvariantCulture);
                        line = sr.ReadLine();
                        HlsSegment segment = new HlsSegment
                        {
                            Duration = TimeSpan.FromSeconds(duration),
                            Url = line,
                            Index = short.Parse(Path.GetFileNameWithoutExtension(line))
                        };
                        segments.Add(segment);
                    }
                }
            }
            Playlist = segments.ToArray();
        }
        public void Dispose()
        {
            
        }
        private ushort sleep = 0;
        public bool MoveNext()
        {
            if (Playlist.Length == 0 || Playlist.All(s => s.Index <= _index))
            {
                if (sleep > 0)
                    Thread.Sleep(sleep);
                try
                {
                    Console.WriteLine("Запрос из MoveNext()");
                    UpdatePlaylist();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return false;
                }
                return MoveNext();
            }
            if (_index == -1)
            {
                _index = Playlist.Last().Index;
                //if (Playlist.Length > 1)
                //    _index--;
                Console.WriteLine("Взят последний кусок " + _index);
            } else
            {
                _index = Playlist.First(s => s.Index > _index).Index;
                Console.WriteLine("Взят первый кусок" + _index);
            }
            sleep++;
            LastRequested = DateTime.Now;
            return true;
        }
        public HlsSegment this[short index]
        {
            get
            {
                return Playlist.First(s => s.Index == index);
            }
        }
        public void Reset()
        {
            Playlist = new HlsSegment[0];
            _index = 0;
        }

        public static HlsClient Request(string url, byte cache = 1)
        {
            return new HlsClient(url, cache);
        }
        public IEnumerator<HlsSegment> GetEnumerator() {
            return this;
        }
    }

    struct HlsSegment
    {
        public string Url;
        public short Index;
        public TimeSpan Duration;
    }
}
