using System.Xml.Serialization;

namespace PluginFavourites
{
    public enum ErrorType
    {
        incorrect,
        noconnect,
        noparam,
        nochannel,
        noepg,
        [XmlEnum(Name = "")]
        none
    }
}