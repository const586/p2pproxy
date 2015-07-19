using System.Net;
using System.Xml;
using System.Xml.Serialization;

namespace PluginFavourites
{
    [XmlRoot("result")]
    public class Auth : StateApi
    {
        public static Auth Run(string host)
        {
            XmlSerializer serial = new XmlSerializer(typeof(Auth));
            return (Auth)serial.Deserialize(XRun(host));
        }

        public static XmlReader XRun(string host)
        {
            return new XmlTextReader(WebRequest.Create(host + "/login").GetResponse().GetResponseStream());

        }
    }
}