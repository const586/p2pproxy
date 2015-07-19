using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PluginFavourites
{
    [XmlRoot("result")]
    public class Playlist
    {
        public List<Channel> channels;

        public void Run(string host)
        {
            Uri uri = new Uri(host);
            channels = new List<Channel>();
            XDocument xdoc = XDocument.Load(WebRequest.Create("http://" + uri.Authority + "/channels/?filter=favourite").GetResponse().GetResponseStream());
            var xchannels = xdoc.Element("result").Element("channels").Elements("channel");
            foreach (var xch in xchannels)
            {
                Channel ch = new Channel();
                if (xch.Attribute("access_user").Value != "1")
                    continue;
                ch.id = xch.Attribute("id").Value;
                if (string.IsNullOrEmpty(ch.id))
                    continue;
                ch.name = xch.Attribute("name").Value;
                ch.logo = "http://torrent-tv.ru/uploads/" + xch.Attribute("logo").Value;
                ch.epg_id = xch.Attribute("epg_id").Value;
                channels.Add(ch);
            }
        }
    }
}