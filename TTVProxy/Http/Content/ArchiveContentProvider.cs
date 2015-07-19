using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using SimpleLogger;
using TTVApi;
using P2pProxy.Http.Server;

namespace P2pProxy.Http.Content
{
    public class ArchiveContentProvider : ContentProvider
    {
        private readonly P2pProxyDevice _device;
        private readonly DateTime _lastUpdate = new DateTime(1, 1, 1);
        private readonly TimeSpan _maxAge = new TimeSpan(0, 0, 0, 0, P2pProxyApp.AGE_CACHE);
        private ArcList _channels;
        private ArcRecords _records;
        public ArchiveContentProvider(WebServer server, P2pProxyDevice device)
        {
            server.AddRouteUrl("/archive/", SendResponse, HttpMethod.Get);
            server.AddRouteUrl("/archive/play", Play, HttpMethod.Get);
            server.AddRouteUrl("/archive/play", SendHead, HttpMethod.Head);
            server.AddRouteUrl("/archive/channels", GetChannelsRequest, HttpMethod.Get);
            _device = device;
            
        }

        private void GetChannelsRequest(MyWebRequest req)
        {
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".xml").ToString());
            var res = new ArcList().Execute(_device.Proxy.SessionState.session, TypeResult.Xml);
            XDocument xd = XDocument.Load(res);
            var xchs = xd.Root.Element("channels").Elements().ToArray();
            foreach (var xch in xchs)
            {
                if (_device.Filter.Check("ttv").Find("ch" + xch.Attribute("id").Value).Check())
                    xch.Remove();
            }
            MemoryStream ms = new MemoryStream();
            xd.Save(ms);
            ms.Position = 0;
            resp.AddHeader(HttpHeader.ContentLength, ms.Length.ToString());
            resp.SendHeaders();
            ms.CopyTo(resp.GetStream());
        }

        private void SendHead(MyWebRequest obj)
        {
            var info =
                _device.UpnpSettings.Profile.VoD.Info.FirstOrDefault(i => i.FileExt.Equals(".ts", StringComparison.OrdinalIgnoreCase)) ??
                _device.UpnpSettings.Profile.VoD.Info[0];
            var resp = obj.GetResponse();
            resp.AddHeader(HttpHeader.ContentType, WebServer.GetMime(".ts").ToString());
            if (_device.UpnpSettings.Profile.Live.SendContentLength)
                resp.AddHeader(HttpHeader.ContentLength, "50000000000");
            resp.AddHeader(HttpHeader.AcceptRanges, "none");
            resp.AddHeader(HttpHeader.Date, DateTime.Now.ToString("r"));
            resp.AddHeader(HttpHeader.Server, String.Format("{0}/{1}.{2} UPnP/1.1 TestDlna/{3}", Environment.OSVersion.Platform, Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, _device.ModelNumber));
            resp.AddHeader("Cache-control", "no-cache");
            resp.AddHeader("contentFeatures.dlna.org", info.DlnaType + info.Feature);
            resp.AddHeader("transferMode.dlna.org", "Streaming");
            resp.AddHeader("realTimeInfo.dlna.org", "DLNA.ORG_TLAG=*");
            resp.SendHeaders();

        }

        private DateTime ParseDateTime(string sdate)
        {
            return DateTime.ParseExact(sdate, "d-M-yyyy", CultureInfo.InvariantCulture);
        }

        private void UpdatePlaylist(MyWebRequest req)
        {

            if (_lastUpdate + _maxAge < DateTime.Now)
            {
                
                DateTime date = req.Parameters.ContainsKey("date") ? ParseDateTime(req.Parameters["date"]) : DateTime.Today;
                _channels = new ArcList().Run(_device.Proxy.SessionState.session);
                _channels.channels.RemoveAll(c => _device.Filter.Check("ttv").Find("ch" + c.id).Check());
                if (req.Parameters.ContainsKey("channel_id"))
                {
                    int channel_id = int.Parse(req.Parameters["channel_id"]);
                    var ch = _channels.channels.FirstOrDefault(c => c.epg_id == channel_id);
                    if (ch != null)
                        _records = new ArcRecords(ch == null ? 0 : ch.epg_id, date).Run(_device.Proxy.SessionState.session);
                    else
                        throw new FileNotFoundException();
                }
                else
                {
                    _records = new ArcRecords(0, date).Run(_device.Proxy.SessionState.session);
                    var groups = _records.records.GroupBy(r => r.epg_id).ToArray();
                    foreach (var kp in groups)
                    {
                        var ch = _channels.channels.Where(c => c.epg_id == kp.Key);
                        if (!ch.Any())
                            _records.records.RemoveAll(r => r.epg_id == kp.Key);
                    }
                }
                    
                    
            }

            if (!_channels.IsSuccess && !_records.IsSuccess)
            {
                if (_channels.Error == ApiError.incorrect || _channels.Error == ApiError.noconnect ||
                    _records.Error == ApiError.incorrect || _records.Error == ApiError.noconnect)
                {
                    while (!_device.Proxy.Login() || _device.Proxy.SessionState.Error == ApiError.noconnect)
                    {
                    }
                    if (!_device.Proxy.SessionState.IsSuccess)
                    {
                        throw new Exception("Ошибка подключения");
                    }
                    UpdatePlaylist(req);
                }
                else
                    switch (_records.Error)
                    {
                        case ApiError.norecord:
                            throw new Exception("Нет записей");
                        case ApiError.noconnect:
                            throw new Exception("ошибка соединения с БД");
                        case ApiError.noparam:
                            throw new Exception("ошибка входных параметров");
                    }
            }
        }

        public override string GetPlaylist(MyWebRequest req)
        {
            try
            {
                UpdatePlaylist(req);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            if (req.Parameters.ContainsKey("type"))
            {
                Playlist pl = Playlist.CreatePlaylist(req.Parameters["type"], req.Headers["host"], Playlist.ContentType.Archive);
                List<Record> arcs = new List<Record>();
                arcs.AddRange(_records.records);
                if (req.Parameters.ContainsKey("sort"))
                {
                    switch (req.Parameters["sort"])
                    {
                        case "channel":
                            arcs = arcs.OrderBy(arc => arc.epg_id).ToList();
                            break;
                        case "-channel":
                            arcs = arcs.OrderByDescending(arc => arc.epg_id).ToList();
                            break;
                        case "title":
                            arcs = arcs.OrderBy(arc => arc.name).ToList();
                            break;
                        case "-title":
                            arcs = arcs.OrderByDescending(arc => arc.name).ToList();
                            break;
                        case "id":
                            arcs = arcs.OrderBy(arc => arc.record_id).ToList();
                            break;
                        case "-id":
                            arcs = arcs.OrderByDescending(arc => arc.record_id).ToList();
                            break;
                        case "datetime":
                            arcs = arcs.OrderBy(arc => arc.Time).ToList();
                            break;
                        case "-datetime":
                            arcs = arcs.OrderByDescending(arc => arc.Time).ToList();
                            break;
                    }
                }
                _records.CastChannels(_channels.channels);
                foreach (var ch in arcs)
                {
                    if (ch.Channel != null)
                        pl.AddLine(ch, append: req.Parameters.ContainsKey("transcode") ? "&transcode=" + req.Parameters["transcode"] : "");
                }
                return pl.ToString();
            }
            DateTime date = req.Parameters.ContainsKey("date") ? ParseDateTime(req.Parameters["date"]) : DateTime.Today;
            var res = new ArcRecords(req.Parameters.ContainsKey("channel_id") ? int.Parse(req.Parameters["channel_id"]) : 0, date).Execute(_device.Proxy.SessionState.session, TypeResult.Xml);
            XDocument xd = XDocument.Load(res);
            var xchs = xd.Root.Element("records").Elements().GroupBy(e => int.Parse(e.Attribute("epg_id").Value)).ToArray();
            foreach (var xch in xchs)
            {
                if (!_channels.channels.Any(c => c.epg_id == xch.Key))
                    xch.Remove();
            }
            return xd.ToString();
        }

        //private void PlayNew(MyWebRequest req, ArcStream url)
        //{
        //    string cid = url.source;
        //    if (url.Type != SourceType.ContentId)
        //    {
        //        TorrentStream ts1 = new TorrentStream(req.Client);
        //        ts1.Connect();
        //        var respData = ts1.ReadTorrent(url.source, url.Type);
        //        cid = ts1.GetContentId(respData);
        //    }
        //    var stream = _device.Proxy.Broadcaster.GetSource(cid);
        //    req.GetResponse().SendFile(stream);
        //}

        public override void Play(MyWebRequest req)
        {
            locker.isSet = true;
            int id = int.Parse(req.Parameters["id"].Split("#".ToCharArray(), 2)[0]);
            var url = new ArcStream(id).Run(_device.Proxy.SessionState.session);
            if (!url.IsSuccess)
            {
                while (!_device.Proxy.Login() && _device.Proxy.SessionState.Error == ApiError.noconnect)
                {

                }
                if (!_device.Proxy.SessionState.IsSuccess)
                    throw new Exception("No authorized");
                Play(req);
                return;
            }
            var ts = _device.Proxy.GetTsClient(url.source);
            Task<string> waiter;
            
            try
            {
                if (ts == null)
                {
                    if (!req.Client.Connected)
                        return;
                    ts = new TorrentStream(req.Client);
                    ts.Connect();
                    waiter = ts.Play(url.source, url.Type);

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
                locker.isSet = false;
                if (waiter != null && !waiter.IsCompleted)
                    waiter.Wait();
                else if (waiter == null)
                    throw new FileNotFoundException();

                if (string.IsNullOrEmpty(waiter.Result))
                {
                    _device.Proxy.RemoveFromTsPoos(ts);
                }
                while (plaing) Thread.Sleep(256);
                plaing = true;
                req.GetResponse().SendFile(waiter.Result);
                plaing = false;
                do Thread.Sleep(512);
                while (locker.isSet); 
                if (ts.Owner.All(c => !c.Connected))
                {
                        
                    ts.Disconnect();
                    _device.Proxy.RemoveFromTsPoos(ts);
                    //GC.Collect();
                }
                
            }
            catch (Exception ex)
            {
                P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
                ts.Disconnect();
                _device.Proxy.RemoveFromTsPoos(ts);
                plaing = false;
            }
            
        }

        private EventSet locker = new EventSet();
        private bool plaing;
    }

    public class EventSet
    {
        public bool isSet = false;
    }

}
