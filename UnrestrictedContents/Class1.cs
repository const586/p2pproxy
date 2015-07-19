using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using PluginProxy;

namespace UnrestrictedContents
{
    public class Plugin : IPluginProxy
    {
        public void Dispose()
        {
            
        }

        public string Id { get { return "unrestricted"; } }
        public string Name { get { return "Unrestricted Content"; } }
        public event LoggerCallback Logger;

        public void Init(string host)
        {
            
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
            if (parameters == null)
                return null;
            if (!parameters.ContainsKey("id"))
                return null;
            string path = parameters["id"];
            try
            {
                Uri uri = new Uri(path);
                string ext = Path.GetExtension(path);
                return new Content()
                       {
                           Id = path,
                           PluginMediaType = PluginMediaType.Video,
                           Title = Path.GetFileName(path),
                           Translation = TranslationType.Broadcast,
                           Type =
                               ext.Equals(".torrent", StringComparison.OrdinalIgnoreCase) ||
                               ext.Equals(".acestream", StringComparison.OrdinalIgnoreCase)
                                   ? SourceType.Torrent
                                   : SourceType.File,
                           Path = path
                       };
            }
            catch (Exception)
            {
                if (path.Length == 40)
                    return new Content()
                    {
                        Id = path,
                        PluginMediaType = PluginMediaType.Video,
                        Title = Path.GetFileName(path),
                        Translation = TranslationType.Broadcast,
                        Type = SourceType.ContentId,
                        Path = path
                    };
                return null;
            }
        }

        public IEnumerable<string> GetMenus()
        {
            return new String[0];
        }

        public void ClickMenu(string menu)
        {
            throw new NotImplementedException();
        }
    }

    class Content : IPluginContent
    {
        public string Id { get; internal set; }
        public string Title { get; internal set; }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get; internal set; }
        public IPluginContainer Parent { get; private set; }
        public string GetUrl(string host)
        {
            return string.Empty;
        }

        public TranslationType Translation { get; internal set; }
        public SourceType Type;
        public string Path;
        public SourceUrl GetSourceUrl()
        {
            return new SourceUrl() {Type = Type, Url = Path};
        }
    }
}
