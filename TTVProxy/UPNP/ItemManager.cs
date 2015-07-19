using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Linq;
using SimpleLogger;

namespace P2pProxy.UPNP
{
    public class ItemManager
    {
        private UpnpDevice P2pProxyDevice;
        private ItemRootContainer _rootItems;

        public ItemManager(P2pProxyDevice TVProxyDevice)
        {
            this.P2pProxyDevice = P2pProxyDevice;
            _rootItems = new ItemRootContainer(null, TVProxyDevice);
        }

        public void Add(Item item)
        {
            _rootItems.AddChild(item);
        }

        public ItemRootContainer GetItems()
        {
            return _rootItems;
        }

        internal void Browse(Dictionary<string, string> headers, string objectId, BrowseFlag browseFlag, string filter, uint startingIndex,
            uint requestedCount, string sortCriteria, out string result, out string numberReturned, out string totalMatches)
        {
            string host = headers.ContainsKey("host") ? "http://" + headers["host"] : string.Empty;
            StringBuilder sb = new StringBuilder();
            HashSet<string> filterSet = null;
            if (filter != "*")
                filterSet = new HashSet<string>(filter.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);

            Item mainObject = null;
            string[] idParams = objectId.Split(new[] { '_' }, 2, StringSplitOptions.RemoveEmptyEntries);

            mainObject = _rootItems;

            using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings {OmitXmlDeclaration = true}))
            {
                writer.WriteStartElement("DIDL-Lite", "urn:schemas-upnp-org:metadata-1-0/DIDL-Lite/");
                writer.WriteAttributeString("xmlns", "dc", null, "http://purl.org/dc/elements/1.1/");
                writer.WriteAttributeString("xmlns", "upnp", null, "urn:schemas-upnp-org:metadata-1-0/upnp/");
                writer.WriteAttributeString("xmlns", "av", null, "urn:schemas-sony-com:av");

                try
                {
                    if (browseFlag == BrowseFlag.BrowseMetadata)
                    {
                        mainObject.BrowseMetadata(writer, idParams.Length > 1 ? idParams[1] : string.Empty, host,
                            filterSet);
                        numberReturned = "1";
                        totalMatches = "1";
                    }
                    else
                    {
                        (mainObject as ItemContainer).BrowseDirectChildren(writer,
                            idParams.Length > 1 ? idParams[1] : string.Empty, startingIndex, requestedCount,
                            sortCriteria, out numberReturned, out totalMatches, host, filterSet);
                    }
                    writer.WriteEndElement();
                }
                catch (Exception ex)
                {
                    P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
                    throw ex;
                }
                
            }
            result = sb.ToString();
        }
    }
}