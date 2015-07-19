using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PluginProxy;

namespace PluginFavourites
{
    public class FavouriteContentProvider : IPluginContainer
    {
        private string _host;
        private string _url;
        private List<IPluginContent> _contents;
        private TimeSpan _maxage = new TimeSpan(0, 0, 10, 0);
        private DateTime _lastupd;
        private EpgContentProvider _epg;
        private LocalFileContentProvider _local;
        private YoutubeContentProvider _youtube;
        private TorrentContentProvider _torrent;

        public string Id { get { return null; } }
        public string Title { get { return "Избранное"; }}
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return PluginMediaType.Video; } }
        public IPluginContainer Parent { get { return null; } }
        public string Url { get { return _url; } }

        public string GetUrl(string host)
        {
            return Url.Replace(_host, host);
        }

        public TranslationType Translation { get; private set; }
        public SourceUrl GetSourceUrl()
        {
            return new SourceUrl();
        }

        public List<Channel> Channels; 

        public IEnumerable<IPluginContent> Children { 
            get
            {
                _contents = (List<IPluginContent>)GetContent();
                return _contents;
            }
        }

        public IPluginContent GetChildContent(string id)
        {
            if (_contents.Count == 0)
                _contents = (List<IPluginContent>) GetContent();
            string[] ids = id.Split("_".ToCharArray(), 2);
            
            IPluginContent content = _contents.FirstOrDefault(c => c.Id.ToLower() == ids[0]);
            
            if (ids.Length == 1)
                return content;
            if (content is EpgContentProvider)
                return (content as EpgContentProvider).Children.FirstOrDefault(c => c.Id == id);
            if (content is LocalFileContentProvider)
                return (content as LocalFileContentProvider).GetChildContent(id);
            if (content is YoutubeContentProvider)
                return _youtube.Children.FirstOrDefault(c => c.Id == id);
            if (content is TorrentContentProvider)
                return _torrent.Children.FirstOrDefault(c => c.Id == id);
            return null;
        }

        public FavouriteContentProvider(string host)
        {
            _host = host;
            _contents = new List<IPluginContent>();
            _lastupd = DateTime.Today.AddDays(-1);
            _epg = new EpgContentProvider(this, host);
            _local = new LocalFileContentProvider(this);
            _youtube = new YoutubeContentProvider(this, host);
            _torrent = new TorrentContentProvider(this);
        }

        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return Children;
        }

        public bool CanSorted { get { return true; } }

        public void UpdateChannels()
        {
            var playlist = new Playlist();
            playlist.Run(_host);                      
            Channels = playlist.channels; 
        }

        private IEnumerable<IPluginContent> GetContent()
        {
            UpdateChannels();
            Uri uri = new Uri(_host);
            string host = "http://" + uri.Authority + "/channels/play?id=";
            List<IPluginContent> res = new List<IPluginContent> { _epg, _local, _youtube, _torrent };
            res.AddRange(Channels.Select(
                    channel => new Item(channel.id, channel.name, PluginMediaType.Video, this, TranslationType.Stream, host.Replace(uri.Authority, "{HOST}") + channel.id)).
                    ToList());
            return res;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}