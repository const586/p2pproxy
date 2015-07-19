using System;
using System.Reflection;
using System.Text;
using System.Xml;
using P2pProxy.UPNP;
using SimpleLogger;
using P2pProxy.Http;
using P2pProxy.Http.Server;

namespace P2pProxy.UPNP
{
    public enum BrowseFlag { BrowseMetadata, BrowseDirectChildren };

    [UpnpServiceVariable("A_ARG_TYPE_BrowseFlag", "string", false, "BrowseMetadata", "BrowseDirectChildren")]
    [UpnpServiceVariable("A_ARG_TYPE_Filter", "string", false)]
    [UpnpServiceVariable("A_ARG_TYPE_SortCriteria", "string", false)]
    [UpnpServiceVariable("A_ARG_TYPE_Index", "ui4", false)]
    [UpnpServiceVariable("A_ARG_TYPE_Count", "ui4", false)]
    [UpnpServiceVariable("A_ARG_TYPE_UpdateID", "ui4", false)]
    [UpnpServiceVariable("SearchCapabilities", "string", false)]
    [UpnpServiceVariable("SortCapabilities", "string", false)]
    [UpnpServiceVariable("SystemUpdateID", "ui4", true)]
    [UpnpServiceVariable("A_ARG_TYPE_Result", "string", false)]
    [UpnpServiceVariable("A_ARG_TYPE_ObjectID", "string", false)]
    [UpnpServiceVariable("A_ARG_TYPE_Featurelist", "string", false)]
    [ObfuscationAttribute]
    public class ContentDirectoryService : UpnpService
    {
        public ContentDirectoryService(UpnpServer server, UpnpDevice device)
            : base(server, device,
                "urn:schemas-upnp-org:service:ContentDirectory:1", "urn:upnp-org:serviceId:ContentDirectory", "/dlna/ContentDirectory.control",
                "/dlna/ContentDirectory.event", "/dlna/ContentDirectory.xml")
        {

        }

        protected override void WriteEventProp(XmlWriter writer)
        {
            writer.WriteStartElement("e", "property", null);
            writer.WriteElementString("SystemUpdateID", "0");
            writer.WriteEndElement();
        }

        [UpnpServiceArgument(0, "SearchCaps", "SearchCapabilities")]
        [ObfuscationAttribute]
        private void GetSearchCapabilities(MyWebRequest request)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("");
        }

        [UpnpServiceArgument(0, "SortCaps", "SortCapabilities")]
        [ObfuscationAttribute]
        private void GetSortCapabilities(MyWebRequest request)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("dc:title,dc:date");
        }

        [UpnpServiceArgument(0, "Id", "SystemUpdateID")]
        [ObfuscationAttribute]
        private void GetSystemUpdateID(MyWebRequest request)
        {
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody("0");
        }

        [UpnpServiceArgument(0, "Result", "A_ARG_TYPE_Result")]
        [UpnpServiceArgument(1, "NumberReturned", "A_ARG_TYPE_Count")]
        [UpnpServiceArgument(2, "TotalMatches", "A_ARG_TYPE_Count")]
        [UpnpServiceArgument(3, "UpdateID", "A_ARG_TYPE_UpdateID")]
        [ObfuscationAttribute]
        private void Browse(MyWebRequest request,
            [ObfuscationAttribute][UpnpServiceArgument("A_ARG_TYPE_ObjectID")][AliasAttribute("ObjectID")] string ObjectID,
            [ObfuscationAttribute][UpnpServiceArgument("A_ARG_TYPE_BrowseFlag")][AliasAttribute("BrowseFlag")] string BrowseFlag,
            [ObfuscationAttribute][UpnpServiceArgument("A_ARG_TYPE_Filter")][AliasAttribute("Filter")] string Filter,
            [ObfuscationAttribute][UpnpServiceArgument("A_ARG_TYPE_Index")][AliasAttribute("StartingIndex")] string StartingIndex,
            [ObfuscationAttribute][UpnpServiceArgument("A_ARG_TYPE_Count")][AliasAttribute("RequestedCount")] string RequestedCount,
            [ObfuscationAttribute][UpnpServiceArgument("A_ARG_TYPE_SortCriteria")] [AliasAttribute("SortCriteria")] string SortCriteria)
        {

            string Result = "", NumberReturned = "", TotalMatches = "";
            uint startingIndex, requestedCount;
            BrowseFlag browseFlag;
            
            if (!uint.TryParse(StartingIndex, out startingIndex) || !uint.TryParse(RequestedCount, out requestedCount) ||
                !Enum.TryParse(BrowseFlag, true, out browseFlag))
            {
                if (P2pProxyApp.Debug)
                {
                    Console.WriteLine("Invalid Args");
                }
                throw new SoapException(402, "Invalid Args");
            }
                

            SortCriteria = (SortCriteria == string.Empty) ? "+dc:title" : SortCriteria;
            try
            {
                this.device.ItemManager.Browse(request.Headers, ObjectID, browseFlag, Filter, startingIndex, requestedCount,
                                           SortCriteria, out Result, out NumberReturned, out TotalMatches);
            }
            catch (Exception ex)
            {
                P2pProxyApp.Log.Write(string.Format("Content::Browse({0}) - {1}", ObjectID, ex.Message), TypeMessage.Error);
                return;
            }
            

            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody(Result, NumberReturned, TotalMatches, "0");
        }

        [UpnpServiceArgument(0, "FeatureList", "A_ARG_TYPE_Featurelist")]
        [ObfuscationAttribute]
        private void X_GetFeatureList(MyWebRequest request)
        {
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("Features", "urn:schemas-upnp-org:av:avs");
                writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi", "schemaLocation", null, "urn:schemas-upnp-org:av:avs http://www.upnp.org/schemas/av/avs.xsd");

                writer.WriteStartElement("Feature");
                writer.WriteAttributeString("name", "samsung.com_BASICVIEW");
                writer.WriteAttributeString("version", "1");

                writer.WriteStartElement("container");
                writer.WriteAttributeString("id", "0");
                writer.WriteAttributeString("type", "object.item.videoItem");
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteEndElement();
            }
            MyWebResponse response = request.GetResponse();
            response.SendSoapHeadersBody(sb.ToString());

        }
    }

    internal class SoapException : Exception
    {
        private int _errorCode;
        public SoapException(int i, string invalidArgs) : base(invalidArgs)
        {
            _errorCode = i;
        }

        public int Code
        {
            get { return _errorCode; }
        }
    }
}