using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using P2pProxy.Extensions;
using P2pProxy.Http.Server;

namespace P2pProxy.Broadcasting.Simple
{
    class BroadcastServer
    {
        private Stream _stream;
        private CircularStream buffer;
        private List<BroadcastStreamReader> _clients = new List<BroadcastStreamReader>();
        private bool _stopped = false;
        private string _source;

        public BroadcastServer(string source)
        {
            _source = source;
            buffer = new CircularStream(ushort.MaxValue);
        }

        private Stream GetChunkedStream(string source)
        {
            TcpClient client = new TcpClient();
            Uri uri = new Uri(source, UriKind.Absolute);
            client.Connect(uri.Host, uri.Port);
            var buf = string.Format("GET {0} HTTP/1.1\r\nHost: {1}:{2}\r\nConnection: Keep-Alive\r\n\r\n", uri.LocalPath, uri.Host, uri.Port).GetBytes();
            var instream = client.GetStream();
            instream.Write(buf, 0, buf.Length);

            StringBuilder sb = new StringBuilder();
            int c = 0;
            bool isChunked = false;
            while (true)
            {
                byte iByte = (byte)instream.ReadByte();
                if (iByte == '\n')
                {
                    string line = sb.ToString(0, sb.Length - 1);
                    if (line.Equals("Transfer-Encoding: chunked", StringComparison.OrdinalIgnoreCase))
                        isChunked = true;
                    if (line == string.Empty)
                        break;
                    sb.Clear();
                }
                else
                {
                    sb.Append((char)iByte);
                }
                if (c == 2)
                    break;
            }
            return isChunked ? (Stream)new ChunkedStream(instream) : instream;
        }
        private static TimeSpan TIMEOUT = new TimeSpan(0, 0, 30);
        private static byte[] empty = new byte[16];
        public void Start()
        {
            ThreadPool.QueueUserWorkItem(e =>
            {
                _stream = GetChunkedStream(_source);
                var buf = new byte[WebServer.BUFFER_SIZE];
                List<BroadcastStreamReader> clients = new List<BroadcastStreamReader>(2);
                var f = File.Create("d:/test.ts");
                //BroadcastStreamReader bs = (BroadcastStreamReader)GetUserStream();
                //_clients.Add(bs);
                //var f1 = File.Create("c:/temp/test2.ts");
                //ThreadPool.QueueUserWorkItem(obj =>
                //{
                //    bs.CopyTo(f1);
                //});
                int needread = 0;
                int read = 0;
                while (!_stopped)
                {
                    try
                    {

                        clients.Clear();
                        lock (_clients)
                        {
                            _clients.RemoveAll(c => !c.CanRead);
                            clients.AddRange(_clients.Where(c => c.FreeSize > 0 && c.CanRead));
                        }
                        if (clients.Count > 0)
                            needread = Math.Min(buf.Length, clients.Min(c => c.FreeSize));
                        else
                            needread = 0;
                        if (needread == 0)
                        {
                            Thread.Sleep(8);
                            continue;
                        }
                        if (clients.Count == 1)
                        {

                            Console.WriteLine("Только один клиент");
                        }
                        var t = _stream.ReadAsync(buf, 0, needread);
                        while (!t.Wait(TIMEOUT))
                            SendToClient(empty, Math.Min(empty.Length, needread), clients);
                        if (t.Result == 0)
                            break;
                        f.Write(buf, 0, t.Result);
                        SendToClient(buf, t.Result, clients);

                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                f.Close();
                f.Dispose();
            });
            
        }

        private void SendToClient(byte[] buffer, int count, IEnumerable<BroadcastStreamReader> clients)
        {
            foreach (var client in clients)
            {
                try
                {
                    client.Write(buffer, 0, count);
                }
                catch (Exception)
                {
                    lock (_clients)
                        _clients.Remove(client);
                }

            }
        }

        public void Stop()
        {
            _stopped = true;
            lock (_clients)
            {
                _clients.ForEach(c => c.Close());
                _clients.Clear();
                _stream.Close();
            }
            
        }

        public Stream GetUserStream()
        {
            var client = new BroadcastStreamReader();
            lock (_clients)
                _clients.Add(client);
            return client;
        }
    }
}
