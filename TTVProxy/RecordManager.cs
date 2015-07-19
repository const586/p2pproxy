using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TTVApi;

namespace P2pProxy
{
    public class RecordManager
    {
        private readonly List<Records> _records;

        private readonly object _xmloc = new object();
        private string _outpath;
        private readonly P2pProxyDevice _device;
        public string RecordsPath { get { return _outpath; } }

        public RecordManager(P2pProxyDevice device)
        {
            _device = device;
            LoadSettings();
            _records = new List<Records>();
            LoadRecords();
        }

        public Records this[string id]
        {
            get { return _records.FirstOrDefault(rec => rec.Id.Equals(id, StringComparison.OrdinalIgnoreCase)); }
        }

        internal List<Records> GetRecords(RecordStatus status)
        {
            lock (_records)
            {
                return _records.Where(records => records.Status == status).ToList();
            }
        }

        public List<Records> GetRecords()
        {
            Records[] recs = new Records[_records.Count];
            lock (_records)
            {
                _records.CopyTo(recs);
            }
            return recs.ToList();
        }

        internal XElement GetXmlElement()
        {
            return new XElement("Records");
        }

        internal void SaveRecords()
        {
            lock (_xmloc)
            {
                if (File.Exists(P2pProxyApp.ApplicationDataFolder + "/records.xml"))
                    File.Delete(P2pProxyApp.ApplicationDataFolder + "/records.xml");
                var xd = new XDocument();
                var root = GetXmlElement();
                foreach (var r in _records)
                {
                    root.Add(r.GetXml());
                }
                xd.Add(root);
                xd.Save(P2pProxyApp.ApplicationDataFolder + "/records.xml");
            }
        }

        private void LoadSettings()
        {
            _outpath = P2pProxyApp.MySettings.GetSetting("records", "path", P2pProxyApp.ApplicationDataFolder + "/records");
        }

        private void LoadRecords()
        {
            _records.Clear();

            XDocument xdoc;
            lock (_xmloc)
            {
                if (!File.Exists(P2pProxyApp.ApplicationDataFolder + "/records.xml"))
                {
                    var xd = new XDocument();
                    xd.Add(new XElement("Records"));
                    xd.Save(P2pProxyApp.ApplicationDataFolder + "/records.xml");
                }
                try
                {
                    xdoc = XDocument.Load(P2pProxyApp.ApplicationDataFolder + "/records.xml");
                }
                catch (Exception)
                {
                    return;
                }
            }
            var xRecords = xdoc.Element("Records");
            if (xRecords != null)
            {
                var xrecs = xRecords.Elements("Record");
                var xElements = xrecs as XElement[] ?? xrecs.ToArray();
                if (xElements.Any())
                {
                    foreach (var xrec in xElements)
                    {
                        var rec = new Records("http://127.0.0.1:" + _device.Web.Port, _outpath);
                        var buf = xrec.Attribute("Name");
                        if (buf != null)
                            rec.Name = buf.Value;
                        else continue;
                        buf = xrec.Attribute("Start");
                        if (buf != null)
                            rec.Start = ParseDateTime(buf.Value);
                        else continue;
                        buf = xrec.Attribute("End");
                        if (buf != null)
                            rec.End = ParseDateTime(buf.Value);
                        else continue;
                        buf = xrec.Attribute("Status");
                        if (buf != null)
                            rec.Status = (RecordStatus)Enum.Parse(typeof(RecordStatus), buf.Value, true);
                        else continue;
                        buf = xrec.Attribute("TorrentId");
                        if (buf != null)
                            rec.TorrentId = Convert.ToInt32(buf.Value);
                        else continue;

                        buf = xrec.Attribute("Id");
                        if (buf != null)
                            rec.Id = buf.Value;
                        else continue;

                        if (rec.Status == RecordStatus.Starting || rec.Status == RecordStatus.Pause || rec.Status == RecordStatus.Start)
                            rec.Status = RecordStatus.Finished;

                        if ((rec.Status == RecordStatus.Wait || rec.Status == RecordStatus.Init) && rec.End < DateTime.Now)
                            continue;

                        if (rec.Status == RecordStatus.Finished && !File.Exists(_outpath + "/" + rec.Id + ".ts"))
                            continue;

                        Add(rec);
                    }
                }
            }
        }

        internal void Add(Channel ch, DateTime start, DateTime end, RecordStatus status, string name="")
        {
            Records rec = new Records("http://127.0.0.1:" + _device.Web.Port, _outpath)
            {
                Id = Guid.NewGuid().ToString(),
                
            };
            if (string.IsNullOrEmpty(name))
            {
                if (ch.epg_id > 0 && end < DateTime.Today.AddDays(1).AddTicks(-1))
                {
                    var epgs = _device.RecordsProvider.GetListOfEpg(ch.epg_id);
                    if (epgs != null && epgs.Count > 0)
                    {
                        var epg =
                            epgs.FirstOrDefault(
                                epg1 => epg1.StartTime < start.Add(start - epg1.StartTime) && epg1.EndTime > end);
                        if (epg != null && epg.EndTime.AddMinutes(30) < end)
                        {
                            rec.Name = string.Format("[{0}-{1}] {2} - {3}", start.ToString("dd/MM/yy HH:mm"),
                                end.ToString("dd/MM/yy HH:mm"), ch.name, epg.name);
                        }
                    }
                }
                if (string.IsNullOrEmpty(rec.Name))
                    rec.Name = string.Format("[{1}-{2}] {0}", ch.name, start.ToString("dd/MM/yy HH:mm"),
                        end.ToString("dd/MM/yy HH:mm"));
            }
            else
                rec.Name = name;
            rec.Start = start;
            rec.End = end;
            rec.Status = status;
            rec.TorrentId = ch.id;
            Add(rec);
        }

        public void Add(Records record)
        {
            lock (_records)
            {
                _records.Add(record);
            }
            record.Finished += record_Finished;
            if (record.Status == RecordStatus.Finished)
                record_Finished(record);
            SaveRecords();
        }

        void record_Finished(Records sender)
        {
            SaveRecords();
            //string file = new Uri(Path.Combine(_outpath, sender.Id + ".ts")).LocalPath;
            
            //_device.Proxy.AddVodToVlc(sender.Id, file);
        }

        public void Del(string id)
        {
            lock (_records)
                _records.RemoveAll(records => records.Id == id);
            
            SaveRecords();
        }


        private DateTime ParseDateTime(string sdate)
        {
            try
            {
                return DateTime.ParseExact(sdate, "ddMMyyyy_HHmmss", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return DateTime.Now;
            }
            
        }
    }
}
