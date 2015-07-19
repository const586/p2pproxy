using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginProxy;

namespace ConsolidatedPlaylist
{
    public class Plugin : IPluginProxy
    {
        public void Dispose()
        {
            
        }

        public string Id { get { return "consolidated"; } }
        public string Name {
            get { return "Сводный плейлист"; }
        }
        public event LoggerCallback Logger;
        public void Init(string host)
        {
            
        }

        public IEnumerable<string> GetRouteUrls()
        {
            throw new NotImplementedException();
        }

        public IRequestData HttpRequest(string path, Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        public IPluginContent GetContent(Dictionary<string, string> parameters)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetMenus()
        {
            throw new NotImplementedException();
        }

        public void ClickMenu(string menu)
        {
            throw new NotImplementedException();
        }
    }
}
