using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using SimpleLogger;
using TTVApi;
using P2pProxy.Http.Server;

namespace P2pProxy.Http.Content
{
    public class ChannelContentProvider : ContentProvider
    {
        public static readonly string PLAYLIST_PATH = "/channels/";
        public static readonly string PLAY_PATH = "/channels/play";
        public static readonly string FAVOURITE_ADD = "/channels/favourite/add";
        public static readonly string FAVOURITE_DEL = "/channels/favourite/del";

        private readonly P2pProxyDevice _device;
        private TranslationList _apires;
        private readonly DateTime _lastUpdate = new DateTime(1, 1, 1);
        private readonly TimeSpan _maxAge = new TimeSpan(0, 0, 0, 0, P2pProxyApp.AGE_CACHE);
        private string _lastreq;

        public ChannelContentProvider(WebServer server, P2pProxyDevice device)
        {
            server.AddRouteUrl(PLAYLIST_PATH, SendResponse, HttpMethod.Get);
            server.AddRouteUrl(PLAY_PATH, Play, HttpMethod.Get);
            server.AddRouteUrl(PLAY_PATH, SendHead, HttpMethod.Head);
            server.AddRouteUrl(FAVOURITE_ADD, AddFavouriteRequest, HttpMethod.Get);
            server.AddRouteUrl(FAVOURITE_DEL, DelFavouriteRequest, HttpMethod.Get);
            _device = device;
            
        }

        private void DelFavouriteRequest(MyWebRequest req)
        {
            int id = int.Parse(req.Parameters["id"].Split("#".ToCharArray(), 2)[0]);
            var res = new FavouriteDel(id).Execute(_device.Proxy.SessionState.session, TypeResult.Xml);
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".json").ToString());
            resp.AddHeader(HttpHeader.ContentLength, res.Length.ToString());
            resp.SendHeaders();
            res.CopyTo(resp.GetStream());
        }

        private void AddFavouriteRequest(MyWebRequest req)
        {
            int id = int.Parse(req.Parameters["id"].Split("#".ToCharArray(), 2)[0]);
            var res = new FavouriteAdd(id).Execute(_device.Proxy.SessionState.session, TypeResult.Xml);
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".json").ToString());
            resp.AddHeader(HttpHeader.ContentLength, res.Length.ToString());
            resp.SendHeaders();
            res.CopyTo(resp.GetStream());
        }



        private void SendHead(MyWebRequest obj)
        {
            var info =
                _device.UpnpSettings.Profile.Live.Info.FirstOrDefault(i => i.FileExt.Equals(".m2ts", StringComparison.OrdinalIgnoreCase)) ??
                _device.UpnpSettings.Profile.Live.Info[0];
            var resp = obj.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".ts").ToString());
            if (_device.UpnpSettings.Profile.Live.SendContentLength)
                resp.AddHeader(HttpHeader.ContentLength, "2500000000");
            //else
            //    resp.AddHeader("Transfer-Encoding", "chunked");
            resp.AddHeader(HttpHeader.AcceptRanges, "none");
            //resp.AddHeader(HttpHeader.Date, DateTime.Now.ToString("r"));
            //resp.AddHeader(HttpHeader.Server, String.Format("{0}/{1}.{2} UPnP/1.1 TestDlna/{3}", Environment.OSVersion.Platform, Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, _device.ModelNumber));
            resp.AddHeader("Cache-control", "no-cache");
            resp.AddHeader("contentFeatures.dlna.org", info.DlnaType + info.Feature);
            resp.AddHeader("transferMode.dlna.org", "Streaming");
            resp.AddHeader("realTimeInfo.dlna.org", "DLNA.ORG_TLAG=*");
            resp.SendHeaders();

        }

        public void UpdateTranslations(Dictionary<string, string> req, string query)
        {
            if (_lastUpdate + _maxAge < DateTime.Now || _lastreq != query)
            {
                FilterType type = req.ContainsKey("filter") ? (FilterType)Enum.Parse(typeof(FilterType), req["filter"]) : FilterType.all;
                _apires = new TranslationList(type).Run(_device.Proxy.SessionState.session);
            }
            _lastreq = query;
            if (!_apires.IsSuccess)
            {
                if (_apires.Error == ApiError.incorrect || _apires.Error == ApiError.noconnect)
                {
                    while (!_device.Proxy.Login() && _device.Proxy.SessionState.Error == ApiError.noconnect)
                    {
                    }
                    if (!_device.Proxy.SessionState.IsSuccess)
                        throw new Exception(Encoding.UTF8.GetString(_device.Proxy.SessionState.Deserialize()));
                    UpdateTranslations(req, query);
                    return;
                }
                throw new Exception(Encoding.UTF8.GetString(_device.Proxy.SessionState.Deserialize()));
            }
        }

        public List<Channel> GetChannels()
        {
            Dictionary<string, string> req = new Dictionary<string, string> {{"filter", "all"}};
            UpdateTranslations(req, "filter=all");
            return _apires.channels;
        }

        public List<ChannelGroup> GetCategories()
        {
            Dictionary<string, string> req = new Dictionary<string, string> { { "filter", "all" } };
            UpdateTranslations(req, "filter=all");
            return _apires.categories;
        }

        public override string GetPlaylist(MyWebRequest req)
        {
            if (req.Parameters.ContainsKey("type"))
            {
                try
                {
                    UpdateTranslations(req.Parameters, req.QueryString);
                }
                catch (Exception e)
                {
                    return e.Message;
                }

                Playlist pl = Playlist.CreatePlaylist(req.Parameters["type"], req.Headers["host"], Playlist.ContentType.Channel);
                
                List<Channel> channels;
                if (req.Parameters.ContainsKey("group"))
                {
                    var groups = req.Parameters["group"].Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(byte.Parse).OrderBy(b => b);
                    channels = _apires.channels.Where(channel => groups.Contains(channel.group)).ToList();
                } else { channels = _apires.channels; }
                if (req.Parameters.ContainsKey("sort"))
                {
                    switch (req.Parameters["sort"])
                    {
                        case "group":
                            channels = channels.OrderBy(channel => channel.group).ToList();
                            break;
                        case "-group":
                            channels = channels.OrderByDescending(channel => channel.group).ToList();
                            break;
                        case "title":
                            channels = channels.OrderBy(channel => channel.name).ToList();
                            break;
                        case "-title":
                            channels = channels.OrderByDescending(channel => channel.name).ToList();
                            break;
                        case "id":
                            channels = channels.OrderBy(channel => channel.id).ToList();
                            break;
                        case "-id":
                            channels = channels.OrderByDescending(channel => channel.id).ToList();
                            break;
                    }
                }
                foreach (var ch in channels)
                {
                    if (!_device.Filter.Check("ttv").Check("cat" + ch.group).HasChild())
                        continue;
                    if (!_device.Filter.Check("ttv").Check("cat"+ch.group).Check("ch"+ch.id).Check())
                        pl.AddLine(ch, false, req.Parameters.ContainsKey("transcode") ? "&transcode=" + req.Parameters["transcode"] : "");
                }
                return pl.ToString();
            }
            FilterType type = req.Parameters.ContainsKey("filter") ? (FilterType)Enum.Parse(typeof(FilterType), req.Parameters["filter"], true) : FilterType.all;
            var stream = new TranslationList(type).Execute(_device.Proxy.SessionState.session, TypeResult.Xml);
            XDocument xd = XDocument.Load(stream);
            var xchs = xd.Root.Element("channels").Elements().ToArray();
            foreach (var xch in xchs)
            {
                if (_device.Filter.Check("ttv").Check("cat" + xch.Attribute("group").Value).Check() && !_device.Filter.Check("ttv").Check("cat" + xch.Attribute("group").Value).HasChild())
                    xch.Remove();
                else if (_device.Filter.Check("ttv").Check("cat" + xch.Attribute("group").Value).Check("ch" + xch.Attribute("id").Value).Check())
                    xch.Remove();

            }
            return xd.ToString();
        }

        //private void NewPlay(MyWebRequest req, TranslationStream url)
        //{
        //    string cid = url.Source;
        //    if (url.Type != SourceType.ContentId)
        //    {
        //        TorrentStream ts1 = new TorrentStream(req.Client);
        //        ts1.Connect();
        //        var respData = ts1.ReadTorrent(url.Source, url.Type);
        //        cid = ts1.GetContentId(respData);
        //    }
        //    var stream = _device.Proxy.GetStreamFromCID(cid, req.Parameters);
        //    var resp = req.GetResponse();
        //    //resp.AddHeader("Transfer-Encoding", "chunked");
        //    //resp.AddHeader("Content-Type", "video/mp2t");
        //    //resp.SendHeaders();
        //    //req.GetResponse().AddHeader("Transfer-Encoding", "chunked");
        //    SendHead(req);
        //    try
        //    {
        //        stream.CopyTo(req.GetResponse().GetStream());
        //    }
        //    catch
        //    {
        //        stream.Close();
        //    }
        //}

        public override void Play(MyWebRequest req)
        {
            int id = int.Parse(req.Parameters["id"].Split("#".ToCharArray(), 2)[0]);
            if (_device.Proxy.SessionState == null)
                throw new Exception("Необходима авторизация");
            var url = new TranslationStream(id).Run(_device.Proxy.SessionState.session);
            if (!url.IsSuccess)
            {
                if (url.Error == ApiError.incorrect)
                {
                    while (!_device.Proxy.Login() && _device.Proxy.SessionState.Error == ApiError.noconnect)
                    {
                        P2pProxyApp.Log.Write("Попыдка авторизации", TypeMessage.Info);
                    }
                    if (!_device.Proxy.SessionState.IsSuccess)
                        throw new Exception("No authorized");
                    Play(req);
                    return;
                }
                throw new Exception(url.error.ToString());
            }

            var ts = _device.Proxy.GetTsClient(url.Source);
            Task<string> waiter;
            if (ts == null)
            {
                if (!req.Client.Connected)
                    return;
                ts = new TorrentStream(req.Client);
                ts.Connect();
                waiter = ts.Play(url.Source, url.Type);

                if (waiter != null)
                    _device.Proxy.AddToTsPool(ts);
            }
            else
            {
                waiter = ts.GetPlayTask();
                ts.Owner.Add(req.Client);
            }

            if (waiter != null && !waiter.IsCompleted)
                waiter.Wait();
            else if (waiter == null)
                throw new FileNotFoundException();

            if (string.IsNullOrEmpty(waiter.Result))
            {
                _device.Proxy.RemoveFromTsPoos(ts);
                req.GetResponse().SendText("AceStream TimeOut");
                return;
            }
            string aceurl = waiter.Result;
            string file = string.Empty;
            try
            {
                Uri aceuri = new Uri(aceurl);

                var broadcast = _device.Proxy.Broadcaster.GetStream(aceurl, req.Parameters, req.Client);
                
                //file = _device.Proxy.FindBroadcastUrl(aceurl);
                //if (string.IsNullOrEmpty(file))
                //    file = _device.Proxy.StartBroadcastStream(aceurl,
                //        req.Parameters.ContainsKey("transcode") ? req.Parameters["transcode"] : "");

                //_device.Proxy.AddVlcBroadcastClient(file, req.Client);
                SendHead(req);
                try
                {
                    broadcast.CopyTo(req.GetResponse().GetStream());
                    //req.GetResponse().SendBroadcast(file);
                }
                catch
                {
                    
                }
                finally
                {
                    broadcast.Close();
                    req.Client.Close();
                    //if (req.Client.Connected)
                    //    req.Client.Close();
                    if (!_device.Proxy.Broadcaster.Contains(aceurl))
                    {
                        ts.Disconnect();
                        _device.Proxy.RemoveFromTsPoos(ts);
                    }
                }
                //if (_device.UpnpSettings.Profile.Live.SendHead)
                //    req.GetResponse().SendBroadcast(file, SendHead);
                //else
                //    req.GetResponse().SendBroadcast(file);
                
                //if (_device.Proxy.StopBroadcast(file, aceurl))
                //{
                //    ts.Disconnect();
                //    _device.Proxy.RemoveFromTsPoos(ts);
                //}
            }
            catch (Exception ex)
            {
                
                P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
                ts.Disconnect();
                _device.Proxy.RemoveFromTsPoos(ts);
                _device.Proxy.Broadcaster.StopBroadcast(aceurl);
            }
        }
    }
}
