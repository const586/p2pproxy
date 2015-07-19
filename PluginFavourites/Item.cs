using System;
using System.Xml.Linq;
using PluginProxy;

namespace PluginFavourites
{
    public class Item : IPluginContent
    {
        private string _id;
        private string _title;
        private PluginMediaType _type;
        private IPluginContainer _parent;
        private string _url;
        public Item(string id, string title, PluginMediaType type, IPluginContainer parent, TranslationType translation, string url)
        {
            _id = id;
            if (!string.IsNullOrEmpty(parent.Id))
                _id = parent.Id + "_" + id;
            _title = title;
            _type = type;
            _parent = parent;
            _url = url;
            Translation = translation;
        }

        public string Id { get { return _id; } }
        public string Title { get { return _title; } }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return _type; } }
        public IPluginContainer Parent { get { return _parent; } }
        public string Url { get { return _url; } }
        public string GetUrl(string host)
        {
            if (host.Contains("http://"))
                host = new Uri(host).Authority;
            return Url.Replace("{HOST}", host);
        }

        public TranslationType Translation { get; protected set; }
        public virtual SourceUrl GetSourceUrl()
        {
            return new SourceUrl { Type = SourceType.File, Url = Url};
        }
    }
}