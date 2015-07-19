using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P2pProxy.Broadcasting.Simple
{
    class HlsStream : Stream
    {
        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Length
        {
            get { throw new NotImplementedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
        private Stream Current;
        private HlsClient server;
        private HlsSegment lastSegment;
        public HlsStream(string url)
        {
            server = HlsClient.Request(url);
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Current == null || !Current.CanRead)
            {
                if (!server.MoveNext()) throw new IOException("AceStream not Connected");
                lastSegment = server.Current;
                Current = WebRequest.Create(lastSegment.Url).GetResponse().GetResponseStream();
                
            }
            try
            {
                int readed = Current.Read(buffer, offset, count);
                if (readed == 0)
                {
                    Current = null;
                    var end = server.LastRequested + lastSegment.Duration;
                    if (end > DateTime.Now)
                    {
                        //Console.WriteLine("Ожидаем кусок");
                        //Thread.Sleep(end - DateTime.Now);
                    }
                    return Read(buffer, offset, count);
                }
                return readed;
            } catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
