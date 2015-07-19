using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using PluginProxy;

namespace P2pPlaylist
{
    public class ContentProvider : IPluginContainer
    {
        private List<IPluginContent> _contents;
        private Plugin _owner;

        public ContentProvider(Plugin owner)
        {
            _owner = owner;
            _contents = new List<IPluginContent>();
            string path = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) + "/p2ppl.csv";
            if (!File.Exists(path))
                using (var f = File.Create(path))
                {
                    f.Close();
                }
            var sr = new StreamReader(path);
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (string.IsNullOrEmpty(line))
                    continue;
                var aitem = line.Split(";".ToCharArray(), 3, StringSplitOptions.None);
                if (string.IsNullOrEmpty(aitem[0]))
                    continue;

                try
                {
                    var item = new Content();
                    Uri url = new Uri(aitem[0], UriKind.Absolute);
                    SourceUrl source = new SourceUrl();
                    source.Url = url.Host;
                    source.Type = url.Scheme == "acestream" ? SourceType.ContentId : SourceType.Torrent;
                    item.Source = source;
                    item.Id = string.IsNullOrEmpty(aitem[2]) ? Guid.NewGuid().ToString("N") : aitem[2];
                    item.Title = string.IsNullOrEmpty(aitem[1]) ? item.Id : aitem[1];
                    item.Parent = this;
                    _contents.Add(item);
                }
                catch (Exception e)
                {
                    _owner.RaiseLog(e.Message);
                }
            }
        }

        public string Id
        {
            get { return "root"; }
        }

        public string Title
        {
            get { return "root"; }
        }

        public string Icon
        {
            get { return string.Empty; }
        }

        public PluginMediaType PluginMediaType
        {
            get { return PluginMediaType.Video;}
        }

        public IPluginContainer Parent
        {
            get { return null; }
        }

        public string GetUrl(string host)
        {
            return string.Empty;
        }

        public TranslationType Translation
        {
            get { return TranslationType.Broadcast; }
        }

        public SourceUrl GetSourceUrl()
        {
            return new SourceUrl();
        }

        public IEnumerable<IPluginContent> Children
        {
            get { return _contents; }
        }

        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return _contents;
        }

        public bool CanSorted
        {
            get { return false; }
        }
    }

    public class Content : IPluginContent
    {
        public string Id
        {
            get; internal set;
        }

        public string Title
        {
            get; internal set;
        }

        public string Icon
        {
            get; internal set;
        }

        public PluginMediaType PluginMediaType
        {
            get { return PluginMediaType.Video;}
        }

        public IPluginContainer Parent
        {
            get; internal set;
        }

        public string GetUrl(string host)
        {
            return string.Empty;
        }

        public TranslationType Translation
        {
            get { return TranslationType.Broadcast;}
        }

        public SourceUrl Source;

        public SourceUrl GetSourceUrl()
        {
            return Source;
        }
    }
}