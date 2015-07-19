using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PluginFavourites
{
    [XmlRoot("telecast")]
    public class Epg
    {
        [XmlAttribute]
        public string name;
        public int epg_id;
        [XmlAttribute]
        public int btime;
        [XmlAttribute]
        public int etime;

        public DateTime StartTime
        {
            get { return UTSToDateTime(btime); }
        }

        public DateTime EndTime
        {
            get { return UTSToDateTime(etime); }
        }

        private static DateTime UTSToDateTime(double unixTimeStamp)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }

    [XmlRoot("result")]
    public class GetEpg
    {
        public StateApi state;
        [XmlArray("data"), XmlArrayItem(typeof(Epg), ElementName = "telecast")]
        public List<Epg> data;

        public static GetEpg Run(string host, string epg_id)
        {
            XmlSerializer serial = new XmlSerializer(typeof(GetEpg));
            Uri uri = new Uri(host);
            List<Epg> epgs = new List<Epg>();
            XDocument xdoc = XDocument.Load(WebRequest.Create("http://" + uri.Authority + "/epg/?id=" + epg_id).GetResponse().GetResponseStream());
            try
            {
                var res = (GetEpg)serial.Deserialize(xdoc.CreateReader());
                res.data.ForEach(e => e.epg_id = int.Parse(epg_id));
                return res;
            }
            catch (Exception )
            {
                return null;
            }
            
        }
    }

}
