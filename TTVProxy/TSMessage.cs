using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace P2pProxy
{
    public struct TSMessage
    {
        public string Type;
        public string Text;
        public Dictionary<string, string> Parameters;
        public object InnerData;
        public static TSMessage Construct(string msg)
        {
            var tsmsg = new TSMessage {Text = msg};
            var amsg = msg.Split(" ".ToArray(), StringSplitOptions.RemoveEmptyEntries);
            tsmsg.Type = amsg[0];
            if (tsmsg.Type == TorrentStream.MSG_HELLOTS)
            {
                if (amsg.Count() == 2)
                {
                    tsmsg.Parameters = new Dictionary<string, string>();
                    var aprms = amsg[1].Split("=".ToCharArray(), StringSplitOptions.None);
                    tsmsg.Parameters.Add(aprms[0], aprms[1]);
                }
            }
            else if (tsmsg.Type == TorrentStream.MSG_START)
            {
                tsmsg.Parameters = new Dictionary<string, string> { { "url", amsg[1].Split("=".ToCharArray())[1].Replace("%3A", ":") } };
                for (int i = 2; i <amsg.Length; i++)
                {
                    var prm = amsg[i].Split("=".ToCharArray());
                    tsmsg.Parameters.Add(prm[0], prm[1]);
                }
                
            } else if (tsmsg.Type == TorrentStream.MSG_LOADRESP)
            {
                tsmsg.Parameters = new Dictionary<string, string>();
                DataContractJsonSerializer serial = new DataContractJsonSerializer(typeof(LoadRespData));
                var data = msg.Split(" ".ToArray(), 3);
                MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(data[2]));
                LoadRespData resp = (LoadRespData)serial.ReadObject(ms);
                tsmsg.Parameters.Add("key", data[1]);
                tsmsg.InnerData = resp;
            }
            return tsmsg;
        }
    }
    [DataContract]
    public class LoadRespData
    {
        [DataMember(Name = "status")] public byte Status;
        [DataMember(Name = "files")] private List<List<object>> files;

        public List<string> Files
        {
            get { return files.Select(f => f[0].ToString()).ToList(); }
        }
        [DataMember(Name = "infohash")] public string InfoHash;
        [DataMember(Name = "checksum")] public string CheckSum;
    }
}
