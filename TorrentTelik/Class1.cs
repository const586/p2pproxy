using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginProxy;

namespace TorrentTelik
{
    public class Plugin : IPluginProxy
    {
        private string host;
        public const string NAME = "torrent-telik";
        public void Dispose()
        {
            
        }

        public string Id { get { return NAME; } }
        public string Name { get { return "Torrent-telik - Смотри ТВ по новому"; } }
        public event LoggerCallback Logger;
        public void Init(string host)
        {
            this.host = host;
        }

        public IEnumerable<string> GetRouteUrls()
        {
            return new string [0];
        }

        public IRequestData HttpRequest(string path, Dictionary<string, string> parameters)
        {
            return null;
        }

        public IPluginContent GetContent(Dictionary<string, string> parameters)
        {
            if (parameters == null || !parameters.ContainsKey("id"))
            {
                return root;
            }
            var ids = parameters["id"].Split("_".ToCharArray());
            var path = (IPluginContainer)root.Children.FirstOrDefault(content => content.Id == ids[0]);
            if (ids.Length == 1)
                return path;
            return path.Children.FirstOrDefault(content => content.Id == parameters["id"]);
        }

        public IEnumerable<string> GetMenus()
        {
            return new String[0];
        }

        public void ClickMenu(string menu)
        {
            
        }

        private RootContainer root = new RootContainer();
    }

    public class RootContainer : IPluginContainer
    {
        public string Id { get { return "root"; } }
        public string Title { get { return "Torrent-Telik"; } }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get {return PluginMediaType.Video;} }
        public IPluginContainer Parent { get; private set; }
        public List<PluginContainer> _children = new List<PluginContainer>();

        public RootContainer()
        {
            _children.Add(new PluginContainer("torrent-tv", this));
            _children.Add(new PluginContainer("mob-torrent-tv", this));
            _children.Add(new PluginContainer("allfon", this));
        }

        public string GetUrl(string host)
        {
            return string.Empty;
        }

        public TranslationType Translation { get; private set; }
        public SourceUrl GetSourceUrl()
        {
            return default(SourceUrl);
        }

        public IEnumerable<IPluginContent> Children { get { return _children; } }
        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return null;
        }

        public bool CanSorted { get { return false; } }
    }
}
