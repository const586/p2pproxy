using System;
using System.Xml.Serialization;

namespace PluginFavourites
{
    [XmlRoot("channel")]
    [Serializable]
    public class Channel
    {
        private string _icon;
        [XmlAttribute]
        public string id { get; set; }
        [XmlAttribute]
        public string name { get; set; }
        [XmlAttribute]
        public string group { get; set; }
        [XmlIgnore]
        public string group_name
        {
            get
            {
                if (_category == null)
                    return group;
                return _category.name;
            }
        }
        [XmlAttribute]
        public string logo
        {
            get { return _icon; }
            set { _icon = "http://torrent-tv.ru/uploads/" + value; }
        }
        [XmlAttribute]
        public string epg_id;
        [XmlAttribute]
        public string channel_id { get { return epg_id; } set { epg_id = value; } }
        [XmlAttribute]
        public ChannelsType type;
        [XmlAttribute]
        public string source;
        [XmlAttribute]
        public string access_translation;
        [XmlAttribute]
        public string access_user;

        private Category _category;

        public int GetId()
        {
            return int.Parse(id);
        }

        public int GetGroup()
        {
            return int.Parse(group);
        }

        public void SetCategory(Category cat)
        {
            _category = cat;
        }

        public int GetEpgId()
        {
            return string.IsNullOrEmpty(epg_id) ? 0 : int.Parse(epg_id);
        }
    }

    public enum AccessTranslation
    {
        all, registred, vip
    }

    public enum ChannelsType
    {
        channel,
        moderation,
        translation
    }
}