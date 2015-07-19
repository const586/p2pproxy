using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;
using PluginProxy;
using XmlSettings;

namespace PluginFavourites
{
    class YoutubeContentProvider : IPluginContainer
    {
        private DateTime _lastUpdate;
        private List<IPluginContent> _contents;
        private TimeSpan _maxAge = new TimeSpan(0, 1, 0, 0);
        private string _playlist;
        private ushort _quality;
        private string _host;

        public YoutubeContentProvider(FavouriteContentProvider parent, string host)
        {
            Parent = parent;
            _lastUpdate = DateTime.Now.AddDays(-1);
            Settings sett = new Settings(Plugin.SelfPath + "/pluginfavourites.xml");
            _playlist = sett.GetValue("settings", "youtubeplaylist");
            _quality = ushort.Parse(sett.GetValue("settings", "youtubequality") ?? "0");
            _contents = new List<IPluginContent>();
            _host = host;
        }

        public string Id { get { return "youtube"; } }
        public string Title { get { return "Youtube"; } }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return PluginMediaType.Video; } }
        public IPluginContainer Parent { get; private set; }
        public string Url { get; private set; }
        public string GetUrl(string host)
        {
            if (!string.IsNullOrEmpty(Url))
                return Url.Replace(_host, host);
            return Url;
        }

        public TranslationType Translation { get; private set; }
        public SourceUrl GetSourceUrl()
        {
            return new SourceUrl();
        }

        public IEnumerable<IPluginContent> Children { get { return GetContent(); } }

        private IEnumerable<IPluginContent> GetContent()
        {
            UpdatePlaylist();
            return _contents;
        }

        private void UpdatePlaylist()
        {

            if (_lastUpdate + _maxAge < DateTime.Now)
                _contents.Clear();
            if (string.IsNullOrEmpty(_playlist))
                return;

            var req = WebRequest.Create("http://gdata.youtube.com/feeds/api/playlists/" + _playlist);
            var xd = XDocument.Load(req.GetResponse().GetResponseStream());
            
            var entrys = xd.Element("{http://www.w3.org/2005/Atom}feed").Elements("{http://www.w3.org/2005/Atom}entry");

            foreach (var entry in entrys)
            {
                try
                {
                    var link = entry.Elements("{http://www.w3.org/2005/Atom}link").FirstOrDefault(element => element.Attribute("rel").Value == "alternate");
                    var url = link.Attribute("href").Value;
                    var watch = url.Split("?".ToCharArray(), 2)[1];
                    var id = watch.Split("&".ToCharArray()).FirstOrDefault(s => s.Contains("v=")).Split("=".ToCharArray())[1];
                    if (_contents.Any(c => c.Id.Contains(id)))
                        continue;
                    var urls = YouTubeDownloader.GetYouTubeVideoUrls(url);
                    url = urls.Where(quality => quality.Dimension <= _quality).Max().DownloadUrl;
                    var title = entry.Element("{http://www.w3.org/2005/Atom}title").Value;
                    _contents.Add(new YoutubeItem(id, title, PluginMediaType.Video, this, TranslationType.Broadcast, url));
                }
                catch (Exception)
                {
                    continue;
                }
                
            }
            _lastUpdate = DateTime.Now;
        }

        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return Children;
        }

        public bool CanSorted { get { return true; } }
    }

    public class YoutubeItem : Item
    {
        public string ItemUrl;

        public YoutubeItem(string id, string title, PluginMediaType type, IPluginContainer parent, TranslationType translation, string url) : base(id, title, type, parent, translation, "")
        {
            ItemUrl = url;
        }

        public override SourceUrl GetSourceUrl()
        {
            return new SourceUrl
                       {
                           Type = SourceType.File,
                           Url = ItemUrl
                       };
        }
    }
}
