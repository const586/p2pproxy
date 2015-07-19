using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using PluginProxy;

namespace PluginFavourites
{
    public class EpgContentProvider : IPluginContainer
    {
        private string _id = "epg";
        private string _title = "Программа";
        private PluginMediaType _type = PluginMediaType.Video;
        private FavouriteContentProvider _parent;
        private string _url = "";
        private Dictionary<Channel, List<Epg>> _epg;
        private string _host;

        public string Id { get { return _id; } }
        public string Title { get { return _title; } }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return _type; } }
        public IPluginContainer Parent { get { return _parent; } }
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

        public EpgContentProvider(FavouriteContentProvider parent, string host)
        {
            _parent = parent;
            _host = host;
            _epg = new Dictionary<Channel, List<Epg>>();
        }

        public IEnumerable<IPluginContent> Children { 
            get
            { 
                return GetContent();
            }
        }

        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return Children;
        }

        public bool CanSorted { get { return false; }}

        public override string ToString()
        {
            return Title;
        }

        private IEnumerable<IPluginContent> GetContent()
        {
            UpdateEpg();
            Uri uri = new Uri(_host);
            string host = "http://" + uri.Authority + "/channels/play?id=";
            List<IPluginContent> res = new List<IPluginContent>();
            foreach (var epg in _epg)
            {
                res.AddRange(epg.Value.Take(2).Select(item => 
                    new Item(item.epg_id.ToString(),
                        String.Format("[{0}-{1}]({3}){2}", item.StartTime.ToShortTimeString(), item.EndTime.ToShortTimeString(), item.name, epg.Key.name), PluginMediaType.Video, this, TranslationType.Stream, host.Replace(uri.Authority, "{HOST}") + epg.Key.id)));
            }
            return res;
        }

        private void UpdateEpg()
        {
            if (_parent.Channels.Count == 0)
                _parent.UpdateChannels();
            foreach (var channel in _parent.Channels)
            {
                if (_epg.Any(pair => pair.Key.id == channel.id))
                {
                    _epg[channel] = _epg.First(epg => epg.Key.id == channel.id).Value.Where(epg => epg.EndTime > DateTime.Now).ToList();
                    if (_epg[channel].Count(epg => epg.EndTime > DateTime.Now) > 2)
                        continue;
                }
                if (!string.IsNullOrEmpty(channel.epg_id) && channel.epg_id != "0")
                {
                    var res = GetEpg.Run(_host, channel.epg_id);
                    if (res.state.IsSuccess())
                    {
                        if (_epg.ContainsKey(channel))
                            _epg[channel] = res.data;
                        else
                            _epg.Add(channel, res.data);
                    }
                }
            }
        }
    }
}