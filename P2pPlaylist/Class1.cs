using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginProxy;

namespace P2pPlaylist
{
    public class Plugin : IPluginProxy
    {
        private string host;
        private ContentProvider _provider;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string Id
        {
            get { return "p2pplaylist"; }
        }

        public string Name
        {
            get { return "Свой плейлист"; }
        }

        public event LoggerCallback Logger;
        
        public void Init(string host)
        {
            this.host = host;
            _provider = new ContentProvider(this);
        }

        public IEnumerable<string> GetRouteUrls()
        {
            return new string[0];
        }

        public IRequestData HttpRequest(string path, Dictionary<string, string> parameters)
        {
            return null;
        }

        public IPluginContent GetContent(Dictionary<string, string> parameters)
        {
            if (parameters != null && parameters.ContainsKey("id"))
                return _provider.Children.First(content => content.Id == parameters["id"]);
            return _provider;
        }

        public IEnumerable<string> GetMenus()
        {
            return new string[0];
        }

        public void ClickMenu(string menu)
        {
            
        }

        public void RaiseLog(string msg)
        {
            if (Logger != null)
                Logger(this, msg);
        }
    }
}
