using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using PluginProxy;
using SimpleLogger;
using P2pProxy.Http.Server;

namespace P2pProxy.Http.Content
{
    public class PluginContentProvider : ContentProvider
    {
        private readonly List<IPluginProxy> _plugins;
        private readonly P2pProxyDevice _device;

        public PluginContentProvider(P2pProxyDevice device)
        {
            _device = device;
            _plugins = new List<IPluginProxy>();
            LoadPlugins();
            InitPlugins();
            RoutePlugins();
        }

        public List<IPluginProxy> GetPlugins()
        {
            return _plugins.ToList();
        }

        private string GetPluginFromPath(string path)
        {
            return path.Split(" / ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0];
        }

        public override string GetPlaylist(MyWebRequest req)
        {
            var plugpath = GetPluginFromPath(req.Url);
            var plugin = _plugins.FirstOrDefault(p => plugpath == p.Id);
            if (plugin == null)
                return "Plugin not found";
            var container = plugin.GetContent(req.Parameters) as IPluginContainer;
            if (container == null)
                return "No data";
            if (req.Parameters.ContainsKey("type"))
            {
                Playlist pl = Playlist.CreatePlaylist(req.Parameters["type"], req.Headers["host"], Playlist.ContentType.Plugin);
                var content = container.Children.Where(c => !_device.Filter.Check(plugin.Id).Check(c.Id).Check());
                
                if (req.Parameters.ContainsKey("sort") && container.CanSorted)
                {
                    switch (req.Parameters["sort"])
                    {
                        case "title":
                            content = content.OrderBy(item => item.Title).ToList();
                            break;
                        case "-title":
                            content = content.OrderByDescending(item => item.Title).ToList();
                            break;
                        case "id":
                            content = content.OrderBy(item => item.Id).ToList();
                            break;
                        case "-id":
                            content = content.OrderByDescending(item => item.Id).ToList();
                            break;
                        default:
                            content = container.OrderBy(req.Parameters["sort"]).ToList();
                            break;
                    }
                }
                foreach (var item in content)
                {
                    if (item is IPluginContainer)
                    {

                        var it = new VirtualContainer
                        {
                            Title = item.Title,
                            Id = item.Id,
                            Parent = item.Parent,
                            Url =
                                !string.IsNullOrEmpty(item.GetUrl(req.Headers["host"]))
                                    ? item.GetUrl(req.Headers["host"])
                                    : string.Format("http://{0}/{1}/?id={2}&type=m3u", req.Headers["host"], plugin.Id,
                                        item.Id)
                        };

                        pl.AddLine(it, true, req.Parameters.ContainsKey("transcode") ? req.Parameters["transcode"] : "");
                    }
                    else
                    {

                        var it = VirtualItem.Copy(item);
                        it.Url = string.Format("http://{0}/{1}/play?id={2}", req.Headers["host"], plugin.Id, item.Id);
                        pl.AddLine(it, append: req.Parameters.ContainsKey("transcode") ? req.Parameters["transcode"] : "");

                    }
                }
                return pl.ToString();
            }

            return GetXDocument(container, plugin.Id, true).ToString();
        }

        public XElement GetXDocument(IPluginContent content, string plugin, bool root = false, string host="")
        {
            if (content is IPluginContainer)
            {
                XElement xroot = new XElement("items");
                xroot.Add(new XAttribute("Id", content.Id ?? ""));
                xroot.Add(new XAttribute("Parent", content.Parent != null ? content.Parent.Id ?? "" : ""));
                xroot.Add(new XAttribute("Type", content.PluginMediaType));
                xroot.Add(new XAttribute("Title", content.Title));

                if (root)
                    foreach (var child in (content as IPluginContainer).Children)
                    {
                        if (_device.Filter.Check(plugin).Check(child.Id).HasChild() || !_device.Filter.Check(plugin).Check(child.Id).Check())
                        xroot.Add(GetXDocument(child, plugin));
                    }

                return xroot;
            }
            XElement xe = new XElement("item");
            xe.Add(new XAttribute("Id", content.Id));
            xe.Add(new XAttribute("Parent", content.Parent != null ? content.Parent.Id ?? "" : ""));
            xe.Add(new XAttribute("Type", content.PluginMediaType));
            xe.Add(new XAttribute("Title", content.Title));
            xe.Add(new XAttribute("Url", content.GetUrl(host)));
            return xe;
        }

        private void PlayNew(MyWebRequest req, SourceUrl source)
        {
            var url = source.Url;
            if (source.Type == SourceType.Torrent)
            {
                TorrentStream ts1 = new TorrentStream(req.Client);
                ts1.Connect();
                var respData = ts1.ReadTorrent(url, TTVApi.SourceType.Torrent);
                url = ts1.GetContentId(respData);
            }

        }
        public override void Play(MyWebRequest req)
        {

            if (!req.Parameters.ContainsKey("id"))
                return;
            var plugpath = GetPluginFromPath(req.Url);
            var plugin = _plugins.FirstOrDefault(p => plugpath == p.Id);
            if (plugin != null)
            {
                
                var content = plugin.GetContent(req.Parameters);
                if (content == null || content is IPluginContainer)
                    return;
                var source = content.GetSourceUrl();

                if (source.Type == SourceType.ContentId || source.Type == SourceType.Torrent)
                {
                    var ts = GetContentUrl(source, req);
                    string url = ts.GetPlayTask().Result;
                    if (ts == null || string.IsNullOrEmpty(url))
                    {
                        ts.Disconnect();
                        req.GetResponse().SendText("File Not Found");
                        return;
                    }
                    TorrentStream ts1 = new TorrentStream(req.Client);
                    ts1.Connect();
                    var resp = ts1.ReadTorrent(source.Url, (TTVApi.SourceType) (byte) source.Type);
                    string file = resp.Files[req.Parameters.ContainsKey("index") ? int.Parse(req.Parameters["index"]) : 0];
                    string ext = Path.GetExtension(file);
                    ts1.Disconnect();
                    if (content.Translation == TranslationType.Broadcast)
                        SendBroadcast(url, req, ext);
                    else if (content.Translation == TranslationType.VoD)
                    {
                        for (int i = 0; i < ts.Owner.Count && ts.Owner.Count > 1; i++)
                            ts.Owner[i].Close();
                        SendFile(url, req, ext);
                    }
                    Thread.Sleep(5712);
                    if (ts.Owner.All(c => !c.Connected))
                    {
                        if (content.Translation == TranslationType.Broadcast && !_device.Proxy.Broadcaster.Contains(url) ||
                            content.Translation == TranslationType.VoD)
                        {
                            ts.Disconnect();
                            _device.Proxy.RemoveFromTsPoos(ts);
                        }
                    }
                }
                else if (source.Type == SourceType.File)
                {
                    string ext = Path.GetExtension(source.Url);
                    if (content.Translation == TranslationType.Broadcast)
                        SendBroadcast(source.Url, req, ext);
                    else if (content.Translation == TranslationType.VoD)
                        SendFile(source.Url, req, ext);
                }
            }
        }

        private void SendFile(string url, MyWebRequest req, string ext)
        {
            
            var info =
                _device.UpnpSettings.Profile.Live.Info.FirstOrDefault(i => i.FileExt.Equals(ext, StringComparison.OrdinalIgnoreCase)) ??
                _device.UpnpSettings.Profile.Live.Info[0];
            var resp = req.GetResponse();
            var mime = WebServer.GetMime(ext) ?? WebServer.GetMime(".ts");
            resp.AddHeader(HttpHeader.ContentType, mime.ToString());
            resp.AddHeader("Cache-control", "no-cache");
            resp.AddHeader("contentFeatures.dlna.org", info.DlnaType + info.Feature);
            resp.AddHeader("transferMode.dlna.org", "Streaming");
            resp.AddHeader("realTimeInfo.dlna.org", "DLNA.ORG_TLAG=*");
            resp.SendFile(url);
        }

        public TorrentStream GetContentUrl(SourceUrl url, MyWebRequest req)
        {
            //locker.isSet = true;
            if (url.Type == SourceType.Torrent)
                url.Url = new Uri(url.Url, UriKind.Absolute).ToString();
            var ts = _device.Proxy.GetTsClient(url.Url);
            Task<string> waiter;
            try
            {
                if (ts == null)
                {
                    if (!req.Client.Connected)
                        return null;
                    ts = new TorrentStream(req.Client);
                    ts.Connect();
                    waiter = ts.Play(url.Url, (TTVApi.SourceType)(byte)url.Type, req.Headers.ContainsKey("index") ? int.Parse(req.Headers["index"]) : 0);

                    if (waiter != null)
                        _device.Proxy.AddToTsPool(ts);
                }
                else
                {
                    waiter = ts.GetPlayTask();
                    ts.Owner[0].Close();
                    ts.Owner.Add(req.Client);
                    ts.Owner.RemoveAt(0);
                }

                if (waiter != null && !waiter.IsCompleted)
                    waiter.Wait();
                else if (waiter == null)
                    throw new FileNotFoundException();
                if (string.IsNullOrEmpty(waiter.Result))
                {
                    _device.Proxy.RemoveFromTsPoos(ts);
                }
                return ts;
            }
            catch (Exception ex)
            {
                P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
                ts.Disconnect();
                _device.Proxy.RemoveFromTsPoos(ts);
                plaing = false;
                locker.isSet = false;
                return null;
            }
        }

        private bool plaing;

        private void SendBroadcast(string url, MyWebRequest req, string ext)
        {
            var broadcast = _device.Proxy.Broadcaster.GetStream(url, req.Parameters, req.Client);
            try
            {
                SendHeaders(req, ext);
                broadcast.CopyTo(req.GetResponse().GetStream());
            } finally
            {
                broadcast.Close();
            }
            //string broadcast = _device.Proxy.FindBroadcastUrl(url);
            //if (string.IsNullOrEmpty(broadcast))
            //    broadcast = _device.Proxy.StartBroadcastStream(url, req.Parameters.ContainsKey("transcode") ? req.Parameters["transcode"] : "");
            //_device.Proxy.AddVlcBroadcastClient(broadcast, req.Client);
            //req.GetResponse().SendBroadcast(broadcast, request => SendHeaders(request, ext));
            //_device.Proxy.StopBroadcast(broadcast, url);
        }

        private void SendHeaders(MyWebRequest req, string ext)
        {
            var info =
                _device.UpnpSettings.Profile.Live.Info.FirstOrDefault(i => i.FileExt.Equals(ext, StringComparison.OrdinalIgnoreCase)) ??
                _device.UpnpSettings.Profile.Live.Info[0];
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".ts").ToString());
            if (!resp.HeaderContains(HttpHeader.ContentLength) && _device.UpnpSettings.Profile.Live.SendContentLength)
                resp.AddHeader(HttpHeader.ContentLength, "2500000000");
            if (!resp.HeaderContains(HttpHeader.AcceptRanges))
                resp.AddHeader(HttpHeader.AcceptRanges, "none");
            resp.AddHeader("Cache-control", "no-cache");
            resp.AddHeader("contentFeatures.dlna.org", info.DlnaType + info.Feature);
            resp.AddHeader("transferMode.dlna.org", "Streaming");
            resp.AddHeader("realTimeInfo.dlna.org", "DLNA.ORG_TLAG=*");
            resp.SendHeaders();
        }

        private EventSet locker = new EventSet();

        private void InitPlugins()
        {
            foreach (var plugin in _plugins)
            {
                plugin.Logger += plugin_Logger;
                plugin.Init("http://127.0.0.1:" + _device.Web.Port);
            }
        }

        void plugin_Logger(IPluginProxy sender, string message)
        {
            if (sender != null)
                P2pProxyApp.Log.Write(string.Format("[{0}({2})]: {1}", sender.Id, message, sender.GetType().Module), TypeMessage.Info);
        }

        private void RoutePlugins()
        {
            foreach (var plugin in _plugins)
            {
                if (!string.IsNullOrEmpty(plugin.Id))
                {
                    _device.Web.AddRouteUrl(String.Format("/{0}/", plugin.Id), SendResponse, HttpMethod.Get);
                    _device.Web.AddRouteUrl(String.Format("/{0}/play", plugin.Id), Play, HttpMethod.Get);
                    _device.Web.AddRouteUrl(String.Format("/{0}/play", plugin.Id), Play, HttpMethod.Head);

                    var routes = plugin.GetRouteUrls();
                    if (routes == null)
                        continue;
                    foreach (var routeUrl in routes)
                    {
                        _device.Web.AddRouteUrl(String.Format("/{0}/{1}", plugin.Id, routeUrl), HttpRequest, HttpMethod.Get);
                    }
                }
            }
        }

        private void HttpRequest(MyWebRequest req)
        {
            string[] url = req.Url.Split("/".ToCharArray(), 2, StringSplitOptions.RemoveEmptyEntries);
            var plugin = _plugins.FirstOrDefault(proxy => proxy.Id == url[0]);
            if (plugin == null)
            {
                _device.Web.Send404(req);
                return;
            }
            var res = plugin.HttpRequest(url[1], req.Parameters);
            var resp = req.GetResponse();
            foreach (var header in res.Headers)
                resp.AddHeader(header.Key, header.Value);
            resp.SendHeaders(res.ResultState);
            res.GetStream().CopyTo(resp.GetStream());
        }

        private void LoadPlugins()
        {
            string path = P2pProxyApp.ExeDir + "/plugins";
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path, "*.dll"))
                {
                    IPluginProxy plugin = LoadPlugin(file);
                    if (plugin != null)
                        _plugins.Add(plugin);
                }
            }
            path = P2pProxyApp.ApplicationDataFolder + "/plugins";
            if (Directory.Exists(path))
            {
                foreach (var file in Directory.GetFiles(path))
                {
                    IPluginProxy plugin = LoadPlugin(file);
                    if (plugin != null)
                        _plugins.Add(plugin);
                }
            }
        }

        private IPluginProxy LoadPlugin(string path)
        {
            try
            {
                Assembly asm = Assembly.LoadFile(path);
                var type = asm.GetTypes().FirstOrDefault(type1 => type1.GetInterface("IPluginProxy") != null);
                if (type != null)
                {
                    IPluginProxy plugin = (IPluginProxy)asm.CreateInstance(type.FullName);
                    {
                        if (plugin != null)
                            return plugin;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                P2pProxyApp.Log.Write(string.Format("[{0}] {1}", path, ex.Message), TypeMessage.Error);
                return null;
            }
            
        }

        private class VirtualContainer : IPluginContainer
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Icon { get; private set; }
            public PluginMediaType PluginMediaType { get; set; }
            public IPluginContainer Parent { get; set; }
            public string Url { get; set; }
            public string GetUrl(string host)
            {
                return Url;
            }

            public TranslationType Translation { get; set; }
            public SourceUrl GetSourceUrl()
            {
                throw new NotImplementedException();
            }

            public IEnumerable<IPluginContent> Children { get; set; }
            public IEnumerable<IPluginContent> OrderBy(string field)
            {
                return Children;
            }

            public bool CanSorted { get; private set; }
        }

        public class VirtualItem : IPluginContent
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Icon { get; private set; }
            public PluginMediaType PluginMediaType { get; set; }
            public IPluginContainer Parent { get; set; }
            public string Url { get; set; }
            public string GetUrl(string host)
            {
                return Url;
            }

            public TranslationType Translation { get; set; }
            public SourceUrl GetSourceUrl()
            {
                throw new NotImplementedException();
            }

            public static VirtualItem Copy(IPluginContent content)
            {
                return new VirtualItem
                {
                    Id = content.Id,
                    Title = content.Title,
                    PluginMediaType = content.PluginMediaType,
                    Parent = content.Parent,
                    Translation = content.Translation
                };
            }
        }

        public void Clear()
        {
            foreach (var plugin in _plugins)
                plugin.Dispose();
            _plugins.Clear();
            GC.Collect();
        }
    }

    

}
