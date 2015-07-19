using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using P2pProxy.UPNP;
using PluginProxy;
using SimpleLogger;
using P2pProxy.Http.Server;
using Playlist = System.Collections.Generic.Dictionary<TTVApi.ChannelGroup, System.Collections.Generic.List<TTVApi.Channel>>; 

namespace P2pProxy.UPNP
{
    public abstract class Item
    {
        protected string _mediaClass;
        protected ItemContainer parent;
        public string Title;
        public string Id;
        public string ParentId;
        public string IconUrl;
        public MediaType Type;
        public string ContentUrl;
        protected P2pProxyDevice _device;
        public object Tag;

        protected Item(ItemContainer parent, P2pProxyDevice device)
        {
            Title = "Unknown";
            Id = "0";
            ParentId = "0";
            IconUrl = null;
            this.parent = parent;
            _device = device;
        }

        public virtual string GetIds()
        {
            return ParentId + "_" + Id;
        }

        public abstract void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet);

        public abstract string GetMime();
    
        public abstract string GetDlnaType();

        public abstract string GetFeature();
        
        public ItemContainer GetParent()
        {
            return parent;
        }
    }

    public enum MediaType
    {
        Audio, Video, Image
    }

    public class ItemContainer : Item
    {
        protected List<Item> _childItems;
        protected bool CanSorted = true;
        public ItemContainer(ItemContainer parent, P2pProxyDevice device) : base(parent, device)
        {
            _childItems = new List<Item>();
        }

        public Item GetChild(int index)
        {
            return _childItems[index];
        }

        public Item GetChild(string id)
        {
            return _childItems.First(item => item.Id == id);
        }

        public void AddChild(Item child)
        {
            _childItems.Add(child);
        }

        public void ClearChild()
        {
            _childItems.Clear();
        }

        public int Count()
        {
            return _childItems.Count;
        }

        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            if (!string.IsNullOrEmpty(idParams))
            {
                var aids = idParams.Split("_".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var child = _childItems.FirstOrDefault(item => item.Id == aids[0]);
                child.BrowseMetadata(writer, aids.Length < 2 ? string.Empty : aids[1], host, filterSet);
                return;
            }
            writer.WriteStartElement("container");

            writer.WriteAttributeString("id", parent != null ? GetIds() : Id);
            writer.WriteAttributeString("restricted", "1");
            writer.WriteAttributeString("parentID", ParentId);
            writer.WriteAttributeString("childCount", _childItems.Count > 0 ? _childItems.Count.ToString() : (3).ToString());
            writer.WriteAttributeString("searchable", "0");

            writer.WriteElementString("dc", "title", null, Title);
            writer.WriteElementString("upnp", "class", null, "object.container");

            if (IconUrl == "RandomFromChild" && _childItems.Count > 0)
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                int rnd = rand.Next(_childItems.Count);
                Item item = _childItems[rand.Next(rnd)];

                while (item.IconUrl != "RandomFromChild" && item is ItemContainer)
                {
                    item = (item as ItemContainer).GetChild(rand.Next(_childItems.Count));
                }
                IconUrl = item.IconUrl;
            }
            if (!string.IsNullOrEmpty(IconUrl) && IconUrl != "RandomFromChild")
            {   
                if (filterSet == null || filterSet.Contains("upnp:icon"))
                    writer.WriteElementString("upnp", "icon", null, IconUrl);

                if (filterSet == null || filterSet.Contains("upnp:albumArtURI"))
                {
                    writer.WriteStartElement("upnp", "albumArtURI", null);
                    writer.WriteAttributeString("dlna", "profileID", "urn:schemas-dlna-org:metadata-1-0/", "PNG_TN");
                    writer.WriteValue(IconUrl);
                    writer.WriteEndElement();
                }
            }
            writer.WriteEndElement();
        }

        public override string GetMime()
        {
            throw new NotImplementedException();
        }

        public override string GetDlnaType()
        {
            throw new NotImplementedException();
        }

        public override string GetFeature()
        {
            throw new NotImplementedException();
        }

        public virtual void BrowseDirectChildren(XmlWriter writer, string idParams, uint startingIndex, uint requestedCount, string sortCriteria, out string numberReturned, out string totalMatches, string host, HashSet<string> filterSet)
        {
            if (CanSorted && !string.IsNullOrEmpty(sortCriteria))
                foreach (
                    var sortCrit in
                        sortCriteria.ToLower().Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                {
                    switch (sortCrit)
                    {
                        case "+dc:title":
                        {
                            var containers = _childItems.Where(i => i is ItemContainer).OrderBy(i => i.Title).ToList();
                            var items = _childItems.Where(i => !(i is ItemContainer)).OrderBy(i => i.Title).ToList();
                            _childItems.Clear();
                            _childItems.AddRange(containers.ToList());
                            _childItems.AddRange(items.ToList());
                            break;
                        }
                        case "-dc:title":
                        {
                            var containers = _childItems.Where(i => i is ItemContainer).OrderByDescending(i => i.Title).ToList();
                            var items = _childItems.Where(i => !(i is ItemContainer)).OrderByDescending(i => i.Title).ToList();
                            _childItems.Clear();
                            _childItems.AddRange(containers);
                            _childItems.AddRange(items);
                            break;
                        }
                    }

                }
            requestedCount = (requestedCount == 0) ? int.MaxValue : requestedCount;
            ushort count = 0;
            foreach (var item in _childItems.Skip((int)startingIndex).Take((int)requestedCount))
            {
                item.BrowseMetadata(writer, string.Empty, host, filterSet);
                count++;
            }
            numberReturned = count.ToString();
            totalMatches = _childItems.Count().ToString();
            
        }
    }

    public class ChannelItemContainer : ItemContainer
    {
        private DateTime _lastUpdate = new DateTime(1, 1, 1);
        private readonly TimeSpan _maxAge = new TimeSpan(0, 0, 0, 0, P2pProxyApp.AGE_CACHE);

        public ChannelItemContainer(ItemContainer parent, P2pProxyDevice device) : base(parent, device)
        {
            Type = MediaType.Video;
            _childItems = new List<Item>();
            IconUrl = "";
            Id = "channels";
            Title = "Каналы";
            ParentId = parent.GetIds();
            this.parent = parent;
        }

        public Item GetItem(string id)
        {
            var aids = id.Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
            var itemContainer = _childItems.First(item => item.Id == aids[0]);
            if (aids.Length < 2)
                return itemContainer;
            if (itemContainer is ItemContainer)
                return (itemContainer as ItemContainer).GetChild(aids[1]);
            return null;
        }

        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            //UpdatePlaylist(host);
            if (string.IsNullOrEmpty(idParams))
            {
                base.BrowseMetadata(writer, idParams, host, filterSet);
            }
            else
            {
                GetItem(idParams).BrowseMetadata(writer, "", host, filterSet);
                    //_childItems.First(item => item.Id == aids[0]).BrowseMetadata(writer, aids.Length > 1 ? aids[1] : string.Empty, host, filterSet);
            }
        }

        public override void BrowseDirectChildren(XmlWriter writer, string idParams, uint startingIndex, uint requestedCount, string sortCriteria, out string numberReturned, out string totalMatches, string host, HashSet<string> filterSet)
        {
            UpdatePlaylist(host);
            if (string.IsNullOrEmpty(idParams))
                base.BrowseDirectChildren(writer, idParams, startingIndex, requestedCount, sortCriteria, out numberReturned, out totalMatches, host, filterSet);
            else
            {
                var aids = idParams.Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                var itemContainer = _childItems.First(item => item.Id == aids[0]) as ItemContainer;
                if (itemContainer != null)
                    itemContainer.BrowseDirectChildren(writer, aids.Length < 2 ? string.Empty : aids[1], startingIndex, requestedCount, sortCriteria, out numberReturned, out totalMatches, host, filterSet);
                else
                {
                    numberReturned = "0";
                    totalMatches = "0";
                }
            }
            
        }

        private void UpdatePlaylist(string host)
        {
            if (_lastUpdate + _maxAge < DateTime.Now)
            {
                _childItems.Clear();
                var res = new TTVApi.TranslationList(TTVApi.FilterType.all).Run(_device.Proxy.SessionState.session);
                if (!res.IsSuccess)
                {
                    while (!_device.Proxy.Login() || _device.Proxy.SessionState.Error == TTVApi.ApiError.noconnect)
                    {

                    }
                    if (!_device.Proxy.SessionState.IsSuccess)
                        throw new Exception("No authorized");
                }
                foreach (var cat in res.categories)
                {
                    ItemContainer container = new ItemContainer(this, _device)
                                                  {
                                                      Type = MediaType.Video,
                                                      IconUrl = "RandomFromChild",
                                                      Id = cat.id.ToString(),
                                                      Title = cat.name,
                                                      ParentId = ParentId + "_" + Id
                                                  };
                    TTVApi.ChannelGroup cat1 = cat;
                    foreach (var source in res.channels.Where(channel => channel.group == cat1.id))
                    {
                        container.AddChild(new ItemStream(container, MediaType.Video, _device)
                                               {
                                                   IconUrl = source.ObsaluteLogo,
                                                   Id = source.id.ToString(),
                                                   Title = source.name,
                                                   ParentId = container.GetIds(),
                                                   ContentUrl = String.Format("{0}/channels/play?id={1}", host, source.id) + (!string.IsNullOrEmpty(_device.UpnpSettings.Profile.Live.Info[0].TranscodingProfile) ? "&transcode=" + _device.UpnpSettings.Profile.Live.Info[0].TranscodingProfile : "")
                                               });
                    }
                    AddChild(container);
                }
                _lastUpdate = DateTime.Now;
            }
        }
    }

    public class BlockItemContainer : ItemContainer
    {
        public BlockItemContainer(ItemContainer parent, P2pProxyDevice device) : base(parent, device)
        {
            Title = "Доступно только в полной версии";
            Id = "block";
            ParentId = "0";
            _childItems.Add(this);
        }
    }

    public class ItemRootContainer : ItemContainer
    {
        public ItemRootContainer(ItemContainer parent, P2pProxyDevice device) : base(parent, device)
        {
            Title = "Root";
            Id = "0";
            ParentId = "0";
            _childItems.Add(new ChannelItemContainer(this, device));
            _childItems.Add(new ArchiveItemContainer(this, device));
            _childItems.Add(new RecordItemContainer(this, device));

            var plugs = _device.PluginProvider.GetPlugins();
            foreach (var plug in plugs)
            {
                try
                {
                    if (plug.GetContent(null) != null)
                        _childItems.Add(new PluginContainer(this, device, plug));
                }
                catch (Exception e)
                {
                    P2pProxyApp.Log.Write(e.Message, TypeMessage.Error);
                }
            }
        }

        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            if (string.IsNullOrEmpty(idParams))
            {
                base.BrowseMetadata(writer, idParams, host, filterSet);
            }
            else
            {
                var aids = idParams.Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                var firstOrDefault = _childItems.FirstOrDefault(item => item.Id == aids[0]);
                if (firstOrDefault != null)
                    firstOrDefault.BrowseMetadata(writer, aids.Length < 2 ? string.Empty : aids[1], host, filterSet);
            }
        }

        public override void BrowseDirectChildren(XmlWriter writer, string idParams, uint startingIndex, uint requestedCount, string sortCriteria, out string numberReturned, out string totalMatches, string host, HashSet<string> filterSet)
        {
            if (string.IsNullOrEmpty(idParams))
            {
                base.BrowseDirectChildren(writer, idParams, startingIndex, requestedCount, sortCriteria,
                    out numberReturned, out totalMatches, host, filterSet);
            }
            else
            {
                var aids = idParams.Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                var child = (ItemContainer) _childItems.FirstOrDefault(item => item.Id == aids[0]);
                child.BrowseDirectChildren(writer, aids.Length < 2 ? string.Empty : aids[1], startingIndex,
                    requestedCount, sortCriteria, out numberReturned, out totalMatches, host, filterSet);
            }

        }

        public override string GetIds()
        {
            return Id;
        }
    }

    public class ArchiveItemContainer: ItemContainer
    {
        private DateTime _lastUpdate = new DateTime(1, 1, 1);
        private readonly TimeSpan _maxAge = new TimeSpan(0, 0, 0, 0, P2pProxyApp.AGE_CACHE);

        public ArchiveItemContainer(ItemContainer parent, P2pProxyDevice device) : base(parent, device)
        {
            Type = MediaType.Video;
            _childItems = new List<Item>();
            IconUrl = "";
            Id = "archives";
            Title = "Архив";
            ParentId = parent.GetIds();
            this.parent = parent;
            
        }

        private ItemContainer GetContainterByDate(DateTime date, ItemContainer container)
        {  
            if (container.Count() == 0 || date == DateTime.Today)
                return container;
            ItemContainer ch = (ItemContainer)container.GetChild(0);
            TimeSpan ts = DateTime.Today - date;
            for (int i = 0; i < ts.Days - 1 && ch.Count() > 0; i++)
            {
                ch = (ItemContainer) ch.GetChild(0);
            }
            return ch;
        }

        public override void BrowseDirectChildren(XmlWriter writer, string idParams, uint startingIndex, uint requestedCount, string sortCriteria, out string numberReturned, out string totalMatches, string host, HashSet<string> filterSet)
        {
            if (string.IsNullOrEmpty(idParams))
            {
                UpdateChannels();
                base.BrowseDirectChildren(writer, idParams, startingIndex, requestedCount, sortCriteria,
                                          out numberReturned, out totalMatches, host, filterSet);
            }
            else
            {
                if (_childItems.Count == 0) UpdateChannels();
                var aids = idParams.Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                var chdt = aids[0].Split("|".ToCharArray());
                DateTime date = DateTime.Today;
                if (chdt.Length > 1)
                    date = DateTime.Parse(chdt[1] ?? DateTime.Today.ToShortDateString());
                //date = UpdateRecords(date, host, int.Parse(aids[0]));
                string root = chdt[0] + "|" + DateTime.Today.ToShortDateString();
                ItemContainer container = (_childItems.First(item => item.Id.Split("_".ToCharArray())[0] == root) as ItemContainer);
                container = GetContainterByDate(date, container);
                if (container.Count() == 0)
                    UpdateRecords(date, host, int.Parse(chdt[0]));
                container.BrowseDirectChildren(writer, aids.Length > 1 ? aids[1] : string.Empty, startingIndex, requestedCount, "+dc:date", out numberReturned, out totalMatches, host, filterSet);
            }
        }

        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            if (string.IsNullOrEmpty(idParams))
            {
                base.BrowseMetadata(writer, idParams, host, filterSet);
            }
            else
            {
                
                var aids = idParams.Split("_".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
                var chdt = aids[0].Split("|".ToCharArray());
                DateTime date = DateTime.Today;
                if (chdt.Length > 1)
                    date = DateTime.Parse(chdt[1] ?? DateTime.Today.ToShortDateString());
                if (_childItems.Count == 0)
                {
                    UpdateChannels();
                    TimeSpan ts = DateTime.Today - date;
                    for (int i = 0; i <= ts.Days; i++)
                        UpdateRecords(DateTime.Today.AddDays(-1*i), host, int.Parse(chdt[0]));
                }
                string root = chdt[0] + "|" + DateTime.Today.ToShortDateString();
                ItemContainer container = (_childItems.First(item => item.Id.Split("_".ToCharArray())[0] == root) as ItemContainer);
                
                container = GetContainterByDate(date, container);
                if (aids.Length < 2)
                    container.BrowseMetadata(writer, string.Empty, host, filterSet);
                else
                    container.GetChild(aids[1]).BrowseMetadata(writer, string.Empty, host, filterSet);
                //_childItems.First(item => item.Id == aids[0]).BrowseMetadata(writer, aids.Length > 1 ? aids[1] : string.Empty, host, filterSet);
            }
        }

        public void UpdateChannels()
        {
            if (_lastUpdate + _maxAge < DateTime.Now)
            {
                _childItems.Clear();
                var channels = new TTVApi.ArcList().Run(_device.Proxy.SessionState.session);
                if (!channels.IsSuccess)
                {
                    while (!_device.Proxy.Login() || _device.Proxy.SessionState.Error == TTVApi.ApiError.noconnect)
                    {

                    }
                    if (!_device.Proxy.SessionState.IsSuccess)
                        throw new Exception("No authorized");
                }
                foreach (var channel in channels.channels)
                {
                    ItemContainer container = new ItemContainer(this, _device)
                        {
                            Type = MediaType.Video,
                            IconUrl = channel.ObsaluteLogo,
                            Id = channel.epg_id + "|" + DateTime.Today.ToShortDateString(),
                            Title = channel.name,
                            ParentId = ParentId + "_" + Id,
                            Tag = DateTime.Today.AddTicks(2)
                        };
                    AddChild(container);
                }
            }
        }

        public DateTime UpdateRecords(DateTime date, string host, int rec_id = 0)
        {
            if (_lastUpdate + _maxAge < DateTime.Now)
            {
                if (_childItems.Count == 0)
                    UpdateChannels();
                var ch = (ItemContainer)_childItems.Find(item => item.Id.Split("|".ToCharArray())[0] == rec_id.ToString());
                if (date != DateTime.Today)
                {
                    ch = GetContainterByDate(date, ch);
                }
                ch.ClearChild();
                date = ((DateTime) ch.Tag).Date;
                var records = new TTVApi.ArcRecords(rec_id, date).Run(_device.Proxy.SessionState.session);
                if (!records.IsSuccess && records.Error == TTVApi.ApiError.incorrect)
                {
                    while (!_device.Proxy.Login() || _device.Proxy.SessionState.Error == TTVApi.ApiError.noconnect)
                    {

                    }
                    if (!_device.Proxy.SessionState.IsSuccess)
                        throw new Exception("No authorized");
                }
                if (!records.IsSuccess && records.Error == TTVApi.ApiError.norecord && date != DateTime.Today)
                    return date;
                var bufdate = date.AddDays(-1);
                ch.AddChild(new ItemContainer(ch, _device)
                                {
                                    Type = MediaType.Video,
                                    IconUrl = ch.IconUrl,
                                    Id = rec_id + "|" + bufdate.ToShortDateString(),
                                    Title = bufdate.ToShortDateString(),
                                    ParentId = ParentId + "_" + Id,
                                    Tag = bufdate.AddTicks(1)
                                });
               
                foreach (var rec in records.records)
                {
                    
                    ItemStream itemrec = new ItemStream(ch, MediaType.Video, _device)
                    {
                        Type = MediaType.Video,
                        IconUrl = ch.IconUrl,
                        Id = rec.record_id.ToString(),
                        Title = String.Format("[{0}]{1}", rec.Time.ToShortTimeString(), rec.name),
                        ParentId = ch.GetIds(),
                        Tag = rec.Time.AddSeconds(1),
                        ContentUrl = String.Format("{0}/archive/play?id={1}", host, rec.record_id)
                    };
                    ch.AddChild(itemrec);
                }
            }
            return date;
        }
    }

    public class RecordItemContainer: ItemContainer
    {
        public RecordItemContainer(ItemContainer parent, P2pProxyDevice device) : base(parent, device)
        {
            Id = "records";
            Title = "Локальные записи";
            Type = MediaType.Video;
        }

        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            if (string.IsNullOrEmpty(idParams))
                base.BrowseMetadata(writer, idParams, host, filterSet);
            else
                _childItems.First(it => it.Id == idParams).BrowseMetadata(writer, string.Empty, host, filterSet);
        }

        public override void BrowseDirectChildren(XmlWriter writer, string idParams, uint startingIndex, uint requestedCount, string sortCriteria, out string numberReturned, out string totalMatches, string host, HashSet<string> filterSet)
        {
            UpdateItems(host);
            base.BrowseDirectChildren(writer, string.Empty, startingIndex, requestedCount, sortCriteria, out numberReturned, out totalMatches, host, filterSet);
        }

        public void UpdateItems(string host)
        {
            _childItems.Clear();

            int i = 1;
            foreach (var rec in _device.Records.GetRecords(RecordStatus.Finished))
            {
                AddChild(new ItemStream(this, MediaType.Video, _device)
                             {
                                 ContentUrl = String.Format("{0}/records/play?id={1}", host, rec.Id),
                                 Id = i.ToString(),
                                 ParentId = GetIds(),
                                 Tag = rec.Start,
                                 Title = rec.Name,
                             });
                i++;
            }
        }
    }

    public class PluginContainer : ItemContainer
    {
        private readonly IPluginProxy _plugin;
        public PluginContainer(ItemContainer parent, P2pProxyDevice device, IPluginProxy plugin) : base(parent, device)
        {
            _plugin = plugin;
            Id = _plugin.Id;
            Title = _plugin.Name;
            if (parent != null)
                ParentId = parent.GetIds();
            Type = MediaType.Video;
        }

        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            if (string.IsNullOrEmpty(idParams))
            {
                _childItems.Clear();
                base.BrowseMetadata(writer, idParams, host, filterSet);
            }
            else
            {
                idParams = string.Format("{0}_{1}_{2}", ParentId, Id, idParams);
                var buf = _childItems.FirstOrDefault(i => i.Id == idParams);
                if (buf != null)
                {
                    buf.BrowseMetadata(writer, idParams, host, filterSet);
                }
                else
                {
                    idParams = idParams.Replace(ParentId + "_" + Id + "_", "");
                    var content = _plugin.GetContent(new Dictionary<string, string> {{"id", idParams}});

                    if (content is IPluginContainer)
                    {
                        var container = GetItem(content, host);

                        container.BrowseMetadata(writer, idParams, host, filterSet);
                    }
                    else
                    {
                        GetItem(content, host).BrowseMetadata(writer, idParams, host, filterSet);
                    }
                }
            }

        }

        private Item GetItem(IPluginContent content, string host)
        {
            Item item;
            if (content is IPluginContainer)
            {
                item = new ItemContainer(null, _device) {Title = "[" + content.Title + "]"};
            }
            else if (content.Translation == TranslationType.VoD)
            {
                item = new ItemVideo(null, _device) {Title = content.Title};
                item.ContentUrl = host + "/" + _plugin.Id + "/play?id=" + content.Id;
            }
            else
            {
                item = new ItemStream(null, MediaType.Video, _device)
                {
                    Title = content.Title,
                    ContentUrl =
                        string.IsNullOrEmpty(content.GetUrl(host))
                            ? string.Format("{0}/{1}/play?id={2}", host, Id, content.Id)
                            : content.GetUrl(host)
                };
            }


            item.ParentId = ParentId + "_" + Id;
            item.Id = item.ParentId + "_" + content.Id;
            if (!string.IsNullOrEmpty(content.Parent.Id))
                item.ParentId = item.ParentId + "_" + content.Parent.Id;
            item.IconUrl = content.Icon;
            
            
            item.Type = MediaType.Video;
            return item;

        }

        public override void BrowseDirectChildren(XmlWriter writer, string idParams, uint startingIndex, uint requestedCount,
            string sortCriteria, out string numberReturned, out string totalMatches, string host, HashSet<string> filterSet)
        {
            _childItems.Clear();
            IPluginContainer container = null;
            try
            {
                if (string.IsNullOrEmpty(idParams))
                    container = _plugin.GetContent(null) as IPluginContainer;
                else
                {
                    idParams = idParams.Replace(ParentId + "_", "");
                    container =
                        _plugin.GetContent(new Dictionary<string, string> {{"id", idParams}}) as IPluginContainer;
                }
            }
            catch (Exception)
            {

            }
            
            if (container != null)
            {
                _childItems.Clear();
                CanSorted = container.CanSorted;
                foreach (var item in container.Children)
                {
                    _childItems.Add(GetItem(item, host));
                }

                base.BrowseDirectChildren(writer, idParams, startingIndex, requestedCount, sortCriteria,
                    out numberReturned, out totalMatches, host, filterSet);
            }
            else
            {
                numberReturned = "0";
                totalMatches = "0";
            }
        }
    }

    public class ItemVideo : Item
    {
        public TimeSpan Duration;
        private MediaFileInfo _info;
        public ItemVideo(ItemContainer parent, P2pProxyDevice device, string file_ext = ".m2ts") : base(parent, device)
        {
            Type = MediaType.Video;
            _info = device.UpnpSettings.Profile.VoD.Info.FirstOrDefault(info => info.FileExt.Equals(file_ext, StringComparison.OrdinalIgnoreCase)) ??
                    device.UpnpSettings.Profile.VoD.Info[0];
        }


        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            writer.WriteStartElement("item");

            writer.WriteAttributeString("id", parent != null ? GetIds() : Id);
            writer.WriteAttributeString("restricted", "true");
            writer.WriteAttributeString("parentID", ParentId);

            writer.WriteElementString("dc", "title", null, Title);
            writer.WriteElementString("upnp", "class", null, "object.item.videoItem");

            if (filterSet == null || filterSet.Contains("dc:date"))
                writer.WriteElementString("dc", "date", null, DateTime.Now.ToString());


            if (filterSet == null || filterSet.Contains("upnp:icon"))
                writer.WriteElementString("upnp", "icon", null, IconUrl);

            if (filterSet == null || filterSet.Contains("upnp:albumArtURI"))
            {
                writer.WriteStartElement("upnp", "albumArtURI", null);
                writer.WriteAttributeString("dlna", "profileID", "urn:schemas-dlna-org:metadata-1-0/", "PNG_TN");
                writer.WriteValue(IconUrl);
                writer.WriteEndElement();
            }

            if (filterSet == null || filterSet.Any(a => a.StartsWith("res")))
            {
                writer.WriteStartElement("res");
                if (!string.IsNullOrEmpty(_device.UpnpSettings.Profile.Live.Resolution))
                    writer.WriteAttributeString("resolution", _device.UpnpSettings.Profile.VoD.Resolution);
                writer.WriteAttributeString("protocolInfo", string.Format("http-get:*:{0}:{1}{2}", GetMime(), GetDlnaType(), GetFeature()));
                writer.WriteValue(ContentUrl);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public override string GetMime()
        {
            return _info.FileExt;
        }

        public override string GetDlnaType()
        {
            return _info.DlnaType;
        }

        public override string GetFeature()
        {
            return _info.Feature;
        }
    }
    public class ItemStream : Item
    {
        private MediaFileInfo _info;
        public ItemStream(ItemContainer parent, MediaType type, P2pProxyDevice device, string file_ext = ".m2ts") : base(parent, device)
        {
            Type = type;
            _info = device.UpnpSettings.Profile.Live.Info.FirstOrDefault(info => info.FileExt.Equals(file_ext, StringComparison.OrdinalIgnoreCase)) ??
                    device.UpnpSettings.Profile.Live.Info[0];
        }

        public override void BrowseMetadata(XmlWriter writer, string idParams, string host, HashSet<string> filterSet)
        {
            writer.WriteStartElement("item");


            writer.WriteAttributeString("id", parent != null ? GetIds() : Id);
            writer.WriteAttributeString("restricted", "1");
            writer.WriteAttributeString("parentID", ParentId);

            writer.WriteElementString("dc", "title", null, Title);
            writer.WriteElementString("upnp", "class", null, "object.item.videoItem");

            if (filterSet == null || filterSet.Contains("dc:date"))
                writer.WriteElementString("dc", "date", null, DateTime.Now.ToString("s"));


            if ((filterSet == null || filterSet.Contains("upnp:icon")) && !string.IsNullOrEmpty(IconUrl))
                writer.WriteElementString("upnp", "icon", null, IconUrl);

            if ((filterSet == null || filterSet.Contains("upnp:albumArtURI")) && !string.IsNullOrEmpty(IconUrl))
            {
                writer.WriteStartElement("upnp", "albumArtURI", null);
                writer.WriteAttributeString("dlna", "profileID", "urn:schemas-dlna-org:metadata-1-0/", "PNG_TN");
                writer.WriteValue(IconUrl);
                writer.WriteEndElement();
            }

            if (filterSet == null || filterSet.Any(a => a.StartsWith("res")))
            {
                writer.WriteStartElement("res");
                if (!string.IsNullOrEmpty(_device.UpnpSettings.Profile.Live.Resolution))
                    writer.WriteAttributeString("resolution", _device.UpnpSettings.Profile.Live.Resolution);
                if (filterSet == null || filterSet.Contains("res@duration"))
                    writer.WriteAttributeString("duration", "0:00:00.000");
                writer.WriteAttributeString("protocolInfo", string.Format("http-get:*:{0}:{1}{2}", GetMime(), GetDlnaType(), GetFeature()));
                writer.WriteValue(ContentUrl);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public override string GetMime()
        {
            return WebServer.GetMime(".ts").ToString();
        }

        public override string GetDlnaType()
        {
            return _info.DlnaType;
        }

        public override string GetFeature()
        {
            return _info.Feature;
        }
    }

    
}
