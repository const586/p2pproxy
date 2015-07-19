using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PluginProxy;
using XmlSettings;

namespace PluginFavourites
{
    public class TorrentContentProvider : IPluginContainer
    {
        public string Id { get { return "torrents"; } }
        public string Title { get { return "Торренты"; } }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return PluginMediaType.Video; } }
        public IPluginContainer Parent { get; private set; }
        public string Url { get; private set; }
        public string GetUrl(string host)
        {
            return Url;
        }

        public TranslationType Translation { get; private set; }

        private string _path;
        public TorrentContentProvider(IPluginContainer parent)
        {
            Parent = parent;
            var settings = new Settings(Plugin.SelfPath + "/pluginfavourites.xml");
            _path = settings.GetValue("settings", "torrentpath") ?? "";
        }
        public SourceUrl GetSourceUrl()
        {
            return new SourceUrl();
        }

        public IEnumerable<IPluginContent> GetContent()
        {
            Console.WriteLine("Получение списка торрентов");
            if (string.IsNullOrEmpty(_path) || !Directory.Exists(_path))
                return new List<IPluginContent>();
            return Directory.GetFiles(_path, "*.torrent").Select(s => new TorrentItem(Path.GetFileNameWithoutExtension(s).Replace("_", "").Replace(" ", ""), Path.GetFileNameWithoutExtension(s), PluginMediaType.Video, this, TranslationType.Broadcast, s));
        }

        public IEnumerable<IPluginContent> Children { get { return GetContent(); }
        }
        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return Children;
        }

        public bool CanSorted { get { return true; } }
    }

    public class TorrentItem : Item
    {
        private string _itemurl;
        public TorrentItem(string id, string title, PluginMediaType type, IPluginContainer parent, TranslationType translation, string url) : base(id, title, type, parent, translation, "")
        {
            _itemurl = url;
            Translation = TranslationType.VoD;
        }

        public override SourceUrl GetSourceUrl()
        {
            return new SourceUrl {Type = SourceType.Torrent, Url = _itemurl};
        }
    }
}
