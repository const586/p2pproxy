using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using System.Xml.Serialization;
using Ionic.Zip;
using PluginProxy;

namespace xbmc.pvr
{
    public class MainClass : IPluginProxy
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public event LoggerCallback Logger;
        private string _host;
        private static string _rootPath;
        public static bool inWork;

        public static string RootPath
        {
            get
            {
                if (string.IsNullOrEmpty(_rootPath))
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    _rootPath = Path.GetDirectoryName(assembly.Location);
                }
                return _rootPath;
            }
        }

        public void Init(string host)
        {
            RaiseLogger("Инициализация плагина");
            _host = host;
            Id = "xbmc.pvr";
            ThreadPool.QueueUserWorkItem((object obj) =>
            {
                try
                {
                    Wait();
                    int week =
                        CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Today,
                            CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                            CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
                    if (!File.Exists(RootPath + "/cache_" + week + ".xml"))
                    {
                        inWork = true;
                        if (!File.Exists(RootPath + "/epg_" + week + ".xml"))
                        {
                            if (!File.Exists(RootPath + "/epg_" + week + ".zip"))
                            {
                                RaiseLogger("Скачивание EPG на " + week + " неделю. В папку" + RootPath);
                                WebClient wc = new WebClient();
                                wc.DownloadFile(
                                    "http://82.119.145.58:12345/proxy/epg/epg_" + week + ".zip",
                                    RootPath + "/epg_" + week + ".zip");
                                
                            }
                            using (var file = File.Create(RootPath + "/epg_" + week + ".xml"))
                            {
                                RaiseLogger("Разархивация полученного файла");
                                ZipFile zip = new ZipFile(RootPath + "/epg_" + week + ".zip");
                                zip["epg_" + week + ".xml"].Extract(file);
                                file.Close();
                            }
                        }
                        XmlSerializer serial = new XmlSerializer(typeof(List<Channel>));
                        List<Channel> source;
                        using (var epgfile = File.OpenRead(RootPath + "/epg_" + week + ".xml"))
                        {
                            RaiseLogger("Подготовка программы");
                            source = (List<Channel>)serial.Deserialize(epgfile);
                        }
                        RaiseLogger("Получение плейлиста");
                        XDocument xd =
                            XDocument.Load(WebRequest.Create(_host + "/channels/").GetResponse().GetResponseStream());
                        if (xd.Root.Element("state").Element("success").Value == "0")
                        {
                            WebRequest.Create(_host + "/login").GetResponse().GetResponseStream();
                            xd =
                                XDocument.Load(
                                    WebRequest.Create(_host + "/channels/").GetResponse().GetResponseStream());
                            if (xd.Root.Element("state").Element("success").Value == "0")
                            {
                                RaiseLogger("Ошибка получения плейлиста: " + xd.Root.Element("state").Element("error").Value);
                                inWork = false;
                                return;
                            }
                        }
                        var xchannels = xd.Root.Element("channels").Elements("channel");
                        XElement xResultRoot = new XElement("programms");
                        xResultRoot.Add(new XAttribute("week", week));
                        var comparer = new StringComparer();
                        foreach (var xch in xchannels)
                        {

                            var xid = xch.Attribute("id");
                            if (xid == null)
                                continue;
                            var xname = xch.Attribute("name");
                            if (xname == null)
                                continue;
                            var epgch = source.FirstOrDefault(channel => channel.Names.Contains(xname.Value, comparer));
                            if (epgch == null)
                                continue;
                            var xchannel = new XElement("channel", new XAttribute("id", xid.Value), new XAttribute("name", xname.Value));
                            foreach (var epg in epgch.Epgs)
                            {
                                XElement xprogram = new XElement("program");
                                if (xname.Value.Contains("(+1)"))
                                {
                                    DateTime start = DateTime.ParseExact(epg.Start, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(1);
                                    DateTime stop = DateTime.ParseExact(epg.Stop, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(1);
                                    xprogram.Add(new XAttribute("start", start.ToString("ddMMyyyyHHmm")));
                                    xprogram.Add(new XAttribute("stop", stop.ToString("ddMMyyyyHHmm")));
                                }
                                else if (xname.Value.Contains("(+2)"))
                                {
                                    DateTime start = DateTime.ParseExact(epg.Start, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(2);
                                    DateTime stop = DateTime.ParseExact(epg.Stop, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(2);
                                    xprogram.Add(new XAttribute("start", start.ToString("ddMMyyyyHHmm")));
                                    xprogram.Add(new XAttribute("stop", stop.ToString("ddMMyyyyHHmm")));
                                }
                                else if (xname.Value.Contains("(+3)"))
                                {
                                    DateTime start = DateTime.ParseExact(epg.Start, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(3);
                                    DateTime stop = DateTime.ParseExact(epg.Stop, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(3);
                                    xprogram.Add(new XAttribute("start", start.ToString("ddMMyyyyHHmm")));
                                    xprogram.Add(new XAttribute("stop", stop.ToString("ddMMyyyyHHmm")));
                                }
                                else if (xname.Value.Contains("(+4)"))
                                {
                                    DateTime start = DateTime.ParseExact(epg.Start, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(4);
                                    DateTime stop = DateTime.ParseExact(epg.Stop, "ddMMyyyyHHmm", CultureInfo.InvariantCulture).AddHours(4);
                                    xprogram.Add(new XAttribute("start", start.ToString("ddMMyyyyHHmm")));
                                    xprogram.Add(new XAttribute("stop", stop.ToString("ddMMyyyyHHmm")));
                                }
                                else
                                {
                                    xprogram.Add(new XAttribute("start", epg.Start));
                                    xprogram.Add(new XAttribute("stop", epg.Stop));
                                }
                                xprogram.Add(new XAttribute("category", epg.Category ?? ""));
                                xprogram.Add(new XAttribute("title", epg.Title));
                                xprogram.Add(new XElement("description", epg.Desc));
                                xchannel.Add(xprogram);
                            }
                            xResultRoot.Add(xchannel);
                        }

                        var xResult = new XDocument();
                        xResult.Add(xResultRoot);
                        if (File.Exists(RootPath + "/cache_" + week + ".xml"))
                            File.Delete(RootPath + "/cache_" + week + ".xml");
                        RaiseLogger("Сохраняю кэшь программы для потомков");
                        xResult.Save(RootPath + "/cache_" + week + ".xml");
                    }
                    inWork = false;
                }
                catch (Exception ex)
                {
                    RaiseLogger(ex.Message);
                    inWork = false;
                }
            });
        }

        public static void Wait(int time = 4)
        {
            while (inWork) Thread.Sleep(4);
        }

        public IEnumerable<string> GetRouteUrls()
        {
            List<string> routes = new List<string>();
            routes.Add("playlist");
            routes.Add("epg");
            routes.Add("records");
            RaiseLogger("Регистрирую комманды плагина: " + string.Join(",", routes));
            return routes;
        }

        private IRequestData RecordsRequest()
        {
            RaiseLogger("Получение списка записей " + _host + "/records/all");
            var webresp = WebRequest.Create(_host + "/records/all").GetResponse();
            HttpResponse resp = new HttpResponse();
            XDocument xd = XDocument.Load(webresp.GetResponseStream());
            var xrecs = xd.Root.Elements("Record");
            XElement xResRoot = new XElement("records");
            rec_cache = null;
            foreach (var xrec in xrecs)
            {
                XElement xResRec = new XElement("record");
                xResRec.Add(new XAttribute("title", xrec.Attribute("Name").Value));
                xResRec.Add(new XAttribute("id", xrec.Attribute("Id").Value));
                DateTime Start = DateTime.ParseExact(xrec.Attribute("Start").Value, "ddMMyyyy_HHmmss", CultureInfo.InvariantCulture);
                DateTime End = DateTime.ParseExact(xrec.Attribute("End").Value, "ddMMyyyy_HHmmss", CultureInfo.InvariantCulture);
                xResRec.Add(new XAttribute("start", Start.ToString("ddMMyyyyHHmm")));
                xResRec.Add(new XAttribute("duration", (End - Start).TotalSeconds));
                xResRec.Add(new XAttribute("url", GetRecordUrl(xrec.Attribute("Name").Value)));
                xResRec.Add(new XAttribute("channel_id", xrec.Attribute("TorrentId").Value));
                xResRec.Add(new XAttribute("status", xrec.Attribute("Status").Value));
                xResRoot.Add(xResRec);
            }
            XDocument xResult = new XDocument();
            xResult.Add(xResRoot);
            resp.Stream = new MemoryStream();
            byte[] res = Encoding.UTF8.GetBytes(xResult.ToString());
            resp.Stream.Write(res, 0, res.Length);
            resp.Stream.Position = 0;
            resp.Headers.Add("Content-Length", resp.Stream.Length.ToString());
            resp.SetResultState(200);
            resp.Headers.Add("Content-Type", "text/xml");
            return resp;
        }

        private Stream rec_cache;

        private string GetRecordUrl(string name)
        {
            RaiseLogger("Получение URL для записи " + name);
            if (rec_cache == null)
            {
                rec_cache = new MemoryStream();
                WebRequest.Create(_host + "/records/?type=m3u").GetResponse().GetResponseStream().CopyTo(rec_cache);
            }
            rec_cache.Position = 0;
            var resp = new StreamReader(rec_cache);
            while (!resp.EndOfStream)
            {
                var line = resp.ReadLine();
                if (line.Contains(name))
                    return resp.ReadLine().Replace(_host + "/", "");
            }
            return "";
        }

        public IRequestData HttpRequest(string path, Dictionary<string, string> parameters)
        {
            RaiseLogger("Обработка команды " + path);
            HttpResponse result = new HttpResponse();
            if (path == "playlist")
            {
                RaiseLogger("Отправлка плейлиста " + _host + "/channels/");
                var resp = WebRequest.Create(_host + "/channels/").GetResponse();
                result.Stream = resp.GetResponseStream();
                result.Headers.Add("Content-Length", resp.ContentLength.ToString());
            } else if (path == "epg")
            {
                
                int week =
                        CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Today,
                            CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule,
                            CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);
                RaiseLogger("Отправка EPG на " + week + " неделю");
                Wait();
                if (!File.Exists(RootPath + "/cache_" + week + ".xml"))
                {
                    Init(_host);
                    Wait();
                }
                inWork = true;
                try
                {
                    result.Stream = File.OpenRead(RootPath + "/cache_" + week + ".xml");
                }
                finally
                {
                    inWork = false;
                }
                
            } else if (path == "records")
            {
                return RecordsRequest();
            }

            result.SetResultState(200);
            
            result.Headers.Add("Content-Type", "text/xml");

            return result;
        }

        public IPluginContent GetContent(Dictionary<string, string> parameters)
        {
            return null;
        }

        public IEnumerable<string> GetMenus()
        {
            return null;
        }

        public void ClickMenu(string menu)
        {
            
        }

        void RaiseLogger(string message)
        {
            if (Logger != null)
                Logger(this, message);
        }

        public void Dispose()
        {
            RaiseLogger("Плагин уничтожен");
        }
    }

    public class StringComparer : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(string obj)
        {
            return obj.GetHashCode();
        }
    }

    public class Programs
    {
        public int Week;
        public List<ChannelIner> Channels;

        public Programs()
        {
            Week = 0;
            Channels = new List<ChannelIner>();
        }
    }

    public class ChannelIner
    {
        public int Id;
        public List<EpgInner> Programms;
    }

    public class EpgInner
    {
        public DateTime Start;
        public DateTime Stop;
        public string Title;
        public string Desc;
        public string Category;
    }

    [Serializable]
    [ObfuscationAttribute]
    public class Channel
    {
        [XmlArrayItem(ElementName = "Name")]
        [ObfuscationAttribute]
        public List<string> Names;
        [ObfuscationAttribute]
        public List<Epg> Epgs;
    }

    [Serializable]
    [ObfuscationAttribute]
    public class Epg
    {
        [ObfuscationAttribute]
        public string Start;
        [ObfuscationAttribute]
        public string Stop;
        [ObfuscationAttribute]
        public string Title;
        [ObfuscationAttribute]
        public string Desc;
        [ObfuscationAttribute]
        public string Category;
    }
}
