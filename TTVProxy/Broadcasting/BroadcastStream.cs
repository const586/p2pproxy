using System;
using System.IO;
using System.Net.Sockets;

namespace P2pProxy.Broadcasting
{
	class StreamClosedEventArgs : EventArgs
	{
		public string Source;
		public TcpClient Client;
	}
	class BroadcastStream : Stream
	{
		private Stream _source;
		private TcpClient _client;
		private string _key;
		public EventHandler<StreamClosedEventArgs> OnClosed;
		public BroadcastStream(string key, Stream source, TcpClient client)
		{
			_key = key;
			_source = source;
			_client = client;
		}
		public override bool CanRead
		{
			get
			{
				return _source.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return _source.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _source.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return _source.Length;
			}
		}

		public override long Position
		{
			get
			{
				return _source.Position;
			}

			set
			{
				_source.Position = value;
			}
		}

		public override void Flush()
		{
			_source.Flush();
		}
        //private FileStream f = File.Create("d:/temp/test.ts");
		public override int Read(byte[] buffer, int offset, int count)
		{
            int r =_source.Read(buffer, offset, count);
            //f.Write(buffer, 0, r);
            //Console.WriteLine(r);
            return r;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return _source.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			_source.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_source.Write(buffer, offset, count);
		}

		public override void Close()
		{
			base.Close();
			RaiseOnClosed();
		}

		private void RaiseOnClosed()
		{
			var h = OnClosed;
			if (h != null)
				h(this, new StreamClosedEventArgs { Source = _key, Client = _client });
		}
	}
}
