using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TTVApi;
using P2pProxy.Http.Server;

namespace P2pProxy.Http.Content
{
    public class RecordContentProvider : ContentProvider
    {
        public static readonly string PLAYLIST_PATH = "/records/";
        public static readonly string ADD_PATH = "/records/add";
        public static readonly string DEL_PATH = "/records/del";
        public static readonly string STOP_PATH = "/records/stop";
        public static readonly string API_PATH = "/records/all";
        public static readonly string PLAY_PATH = "/records/play";
        private readonly P2pProxyDevice _device;

        public RecordContentProvider(WebServer server, P2pProxyDevice device)
        {
            server.AddRouteUrl(PLAYLIST_PATH, SendResponse, HttpMethod.Get);
            server.AddRouteUrl(ADD_PATH, AddRequest, HttpMethod.Get);
            server.AddRouteUrl(DEL_PATH, DelRequest, HttpMethod.Get);
            server.AddRouteUrl(STOP_PATH, StopRequest, HttpMethod.Get);
            server.AddRouteUrl(API_PATH, AllRequest, HttpMethod.Get);
            server.AddRouteUrl(PLAY_PATH, Play, HttpMethod.Get);
            _device = device;
        }

        private DateTime ParseDateTime(string sdate)
        {
            return DateTime.ParseExact(sdate, "ddMMyyyy_HHmmss", CultureInfo.InvariantCulture);
        }

        private XDocument GetXml(IEnumerable<Records> recs)
        {
            XDocument xdoc = new XDocument();
            XElement root = _device.Records.GetXmlElement();
            foreach (var rec in recs)
            {
                root.Add(rec.GetXml());
            }
            xdoc.Add(root);
            return xdoc;
        }

        private void AllRequest(MyWebRequest obj)
        {
            obj.GetResponse().SendText(GetXml(_device.Records.GetRecords()).ToString());
        }

        private void StopRequest(MyWebRequest req)
        {
            if (!req.Parameters.ContainsKey("id"))
            {
                _device.Web.Send404(req);
                return;
            }
            Stop(req.Parameters["id"]);
            req.GetResponse().SendText("OK");
        }

        private void AddRequest(MyWebRequest req)
        {
            if (!req.Parameters.ContainsKey("channel_id"))
            {
                _device.Web.Send404(req);
                return;
            }
            Channel ch = _device.ChannelsProvider.GetChannels().FirstOrDefault(channel => channel.id == int.Parse(req.Parameters["channel_id"]));
            if (ch == null)
            {
                _device.Web.Send404(req);
                return;
            }
            DateTime start = !req.Parameters.ContainsKey("start") ? DateTime.Now : ParseDateTime(req.Parameters["start"]);
            DateTime end = !req.Parameters.ContainsKey("end") ? DateTime.Today.AddDays(1).AddTicks(-1) : ParseDateTime(req.Parameters["end"]);
            Add(ch, start, end, req.Parameters.ContainsKey("name") ? req.Parameters["name"] : string.Empty);
            req.GetResponse().SendText("OK");
            
        }

        private void DelRequest(MyWebRequest req)
        {
            if (!req.Parameters.ContainsKey("id"))
            {
                _device.Web.Send404(req);
                return;
            }

            Del(req.Parameters["id"]);
            req.GetResponse().SendText("OK");
        }

        public List<Telecast> GetListOfEpg(int ch)
        {
            if (ch == 0)
                return null;
            var func = new TranslationEpg(ch);
            var res = func.Run(_device.Proxy.SessionState.session);
            if (!res.IsSuccess && (res.Error == ApiError.incorrect || res.Error == ApiError.noconnect))
            {
                _device.Proxy.Login();
                return GetListOfEpg(ch);
            }
            return res.data;
        }

        private void Add(Channel ch, DateTime start, DateTime end, string name = "")
        {
            _device.Records.Add(ch, start, end, RecordStatus.Wait, name);
        }

        private void Del(string id)
        {
            _device.Records.Del(id);
        }

        private void Stop(string id)
        {
            var rec = _device.Records.GetRecords().Find(records => records.Id == id);
            rec.Stop();
        }

        public override string GetPlaylist(MyWebRequest req)
        {
            if (req.Parameters.ContainsKey("type"))
            {
                Playlist pl = Playlist.CreatePlaylist(req.Parameters["type"], req.Headers["host"], Playlist.ContentType.Record);
                List<Records> recs = _device.Records.GetRecords(RecordStatus.Finished);
                if (req.Parameters.ContainsKey("sort"))
                {
                    switch (req.Parameters["sort"])
                    {
                        case "channel":
                            recs = recs.OrderBy(rec => rec.TorrentId).ToList();
                            break;
                        case "-channel":
                            recs = recs.OrderByDescending(rec => rec.TorrentId).ToList();
                            break;
                        case "title":
                            recs = recs.OrderBy(rec => rec.Name).ToList();
                            break;
                        case "-title":
                            recs = recs.OrderByDescending(rec => rec.Name).ToList();
                            break;
                        case "datetime":
                            recs = recs.OrderBy(rec => rec.Start).ToList();
                            break;
                        case "-datetime":
                            recs = recs.OrderByDescending(rec => rec.Start).ToList();
                            break;
                    }
                }
                foreach (var rec in recs)
                {
                    pl.AddLine(rec);
                }
                return pl.ToString();
            }
            return GetXml(_device.Records.GetRecords(RecordStatus.Finished)).ToString();
        }

        public override void Play(MyWebRequest req)
        {
            string id = req.Parameters["id"].Split("#".ToCharArray(), 2)[0];
            Records rec = _device.Records[id];
            req.GetResponse().SendFile(rec.Path.LocalPath);
        }
    }
}
