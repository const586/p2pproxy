using System;
using System.IO;
using System.Threading;

namespace P2pProxy.Broadcasting.Simple
{
    public class BroadcastStreamReader : Stream
    {
        private bool _closed = false;
        private static byte[] empty = new byte[16];
        private CircularBuffer<byte> _buffer = new CircularBuffer<byte>(ushort.MaxValue);
        private FileStream f;
        static Random rand = new Random(DateTime.Now.Millisecond);
        public BroadcastStreamReader()
        {
            f = File.Create(string.Format("d:/{0}.ts", rand.Next(100)));
        }
        
        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            _buffer.Capacity = (int)value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_closed)
                throw new IOException("Попытка чтения из закрытого потока");
            while (_buffer.Size == 0)
                Thread.Sleep(1);
            lock (_buffer)
            {
                if (count > _buffer.Capacity)
                    _buffer.Capacity = 2*count;
                int res = _buffer.Get(buffer, 0, count);
                f.Write(buffer, 0, res);
                return res;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (_closed)
                throw new IOException("Попытка записи в закрытый поток");
            lock (_buffer)
                _buffer.Put(buffer, 0, count);
        }

        public override bool CanRead
        {
            get { return !_closed; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return !_closed; }
        }

        public override long Length
        {
            get { return _buffer.Size; }
        }

        public override long Position { get { return _buffer.Size; } set { throw new NotImplementedException();} }
        public int FreeSize { get { return (int)(_buffer.Capacity - Position); }}
        public override void Close()
        {
            _closed = true;
            f.Close();
            f.Dispose();
        }
    }
}