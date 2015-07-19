using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using P2pProxy.Broadcasting.Internal;
using P2pProxy.Http.Server;

namespace P2pProxy.Broadcasting
{
    public abstract class Broadcaster
	{
		private readonly Dictionary<string, List<TcpClient>> clients = new Dictionary<string, List<TcpClient>>();

        internal abstract void Start();
		internal abstract void Stop();
		protected abstract Stream GetStreamInternal(string source, Dictionary<string, string> p);
		public abstract bool Contains(string source);
        protected InternalBroadcaster broadcaser;

        protected Broadcaster(WebServer server)
        {
            broadcaser = new InternalBroadcaster(server);
        }

        public string GetSource(string id)
        {
            return broadcaser.Acestream.GenContentUrl(id);
        }

        public abstract Stream GetStreamFromCID(string id, Dictionary<string, string> p);

        public Stream GetStream(string source, Dictionary<string, string> p, TcpClient client)
		{
			var res = GetStreamInternal(source, p);
		    if (res == null || res == Stream.Null)
		        return Stream.Null;
			BroadcastStream bs = new BroadcastStream(source, res, client);
			bs.OnClosed += ClosedStream;
		    lock (clients)
		    {
		        if (!clients.ContainsKey(source))
		            clients.Add(source, new List<TcpClient>());
		        clients[source].Add(client);
		    }
		    return bs;
		}

	    public string[] GetBroadcasts()
	    {
	        return clients.Keys.ToArray();
	    }

	    public void StopBroadcast(string source)
	    {
            CloseStreamInternal(source);
	        if (clients.ContainsKey(source))
	        {
	            clients[source].ForEach(c => c.Close());
                clients[source].Clear();
	        }
	    }

	    public ushort GetClientConnected(string source)
	    {
            if (!clients.ContainsKey(source))
                return 0;
	        return (ushort)clients[source].Count;
	    }

	    public void StopAll()
	    {
	        foreach (var source in clients)
                StopBroadcast(source.Key);
	    }

	    private void ClosedStream(object sender, StreamClosedEventArgs e)
		{
			if (clients.ContainsKey(e.Source))
			{
				e.Client.Close();
				clients[e.Source].RemoveAll(c => !c.Connected);
				if (clients[e.Source].Count == 0)
				{
					clients.Remove(e.Source);
					CloseStreamInternal(e.Source);
				}
			} else
			{
				CloseStreamInternal(e.Source);
			}
		}
		protected abstract void CloseStreamInternal(string source);
	}
}
