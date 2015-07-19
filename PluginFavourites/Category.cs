using System;
using System.Xml.Serialization;

namespace PluginFavourites
{
    [XmlRoot("category", ElementName = "category")]
    public class Category
    {
        [XmlAttribute]
        public int id;
        [XmlAttribute]
        public string name;
        [XmlAttribute]
        public int position;
        [XmlAttribute]
        public int adult;

        public static string GetApiUrl(string session, string type)
        {
            return
                String.Format("http://api.torrent-tv.ru/v2_alltranslation.php?session={0}&type={1}&typeresult=xml",
                              session, type);
        }
    }
}