using System;
using System.Globalization;
using System.IO;

namespace P2pProxy.Broadcasting
{
    class ChunkedStream : Stream
    {
        private readonly Stream innerStream;

        internal ChunkedStream(Stream innerStream)
        {
            if (!innerStream.CanRead)
                throw new ArgumentException();
            this.innerStream = innerStream;
        }


        int currentChunk = -1;

        int currentChunkReaded;

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (currentChunk == 0)
                return 0;

            if (currentChunk == -1 || currentChunk == currentChunkReaded)
                ReadNextChunkDeclaration();

            if (currentChunk == 0)
                return 0;

            int result = innerStream.Read(buffer, offset, Math.Min(count, currentChunk - currentChunkReaded));

            currentChunkReaded += result;
            return result;
        }

        private void ReadNextChunkDeclaration()
        {
            int result;
            while (true)
            {
                string temp = ReadLine();

                if (int.TryParse(temp.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
                {
                    currentChunk = result;
                    currentChunkReaded = 0;
                    return;
                }
            }
        }

        private string ReadLine()
        {
            string result = "";
            while (!result.EndsWith("\r\n"))
            {
                int b = innerStream.ReadByte();
                if (b == -1)
                    return result;
                result += (char)b;
            }
            return result.Substring(0, result.Length - 2);
        }

        public override bool CanRead
        {
            get { return innerStream.CanRead; }
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
            throw new NotSupportedException();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            innerStream.Close();
        }
    }
}