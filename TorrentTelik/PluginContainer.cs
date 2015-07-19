using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using PluginProxy;

namespace TorrentTelik
{
    public class PluginContainer : IPluginContainer
    {
        public string Id { get { return id; } }
        public string Title { get { return Id; } }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return PluginMediaType.Video;} }
        public IPluginContainer Parent { get; private set; }
        private string id;

        public PluginContainer(string id, IPluginContainer parent)
        {
            this.id = id;
            this.Parent = parent;
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

        public IEnumerable<IPluginContent> Children
        {
            get
            {
                try
                {
                    if (DateTime.Now - lastupdate > new TimeSpan(0, 1, 0, 0))
                    {
                        DataContractJsonSerializer serial = new DataContractJsonSerializer(typeof (Playlist));
                        var stream =
                            WebRequest.Create(string.Format("http://torrent-telik.com/channels/{0}.json", id))
                                .GetResponse()
                                .GetResponseStream();
                        var str = Encoding.UTF8.GetBytes(new StreamReader(stream).ReadToEnd());
                        MemoryStream ms = new MemoryStream(str.Length);
                        ms.Write(str, 0, str.Length);
                        ms.Position = 0;
                        playlist = (Playlist) serial.ReadObject(ms);
                        playlist.channels.ForEach(channel =>
                        {
                            channel.Parent = this;
                        });
                        lastupdate = DateTime.Now;
                    }
                    return playlist.channels;
                }
                catch (Exception e)
                {
                    lastupdate = new DateTime(0);
                    return new[] {new Channel() {name = "Ошибка " + e.Message, url = "0", Parent = this}};
                }
                

            }
        }

        private Playlist playlist;
        private DateTime lastupdate = new DateTime(0);
        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return null;
        }

        public bool CanSorted { get { return false; } }
    }
    [DataContract]
    public class Playlist
    {
        [DataMember]
        public List<Channel> channels;
    }
    [DataContract]
    public class Channel : IPluginContent
    {
        [DataMember]
        public string name;
        [DataMember]
        public string url;

        public string Id { get { return Parent.Id + "_" + url; } }
        public string Title { get { return name; } }
        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return PluginMediaType.Video;} }
        public IPluginContainer Parent { get; set; }
        public string GetUrl(string host)
        {
            return string.Empty;
        }

        public TranslationType Translation { get; private set; }
        public SourceUrl GetSourceUrl()
        {
            return new SourceUrl { Type = SourceType.ContentId, Url = url};
        }
    }
}