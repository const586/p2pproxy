using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using P2pProxy.UPNP;
using SimpleLogger;
using P2pProxy.Http;
using P2pProxy.Http.Server;

namespace P2pProxy.UPNP
{
    public abstract class UpnpService
    {
        protected readonly UpnpServer server;

        protected readonly string serviceType;
        protected readonly string serviceId;
        protected readonly string controlUrl;
        protected readonly string eventSubUrl;
        protected readonly string SCPDURL;
        protected UpnpDevice device;

        private readonly byte[] descArray;

        private delegate void SendEventDel(string sid, Uri uri);


        public string ServiceType
        {
            get { return serviceType; }
        }

        protected UpnpService(UpnpServer server, UpnpDevice device, string serviceType, string serviceId, string controlUrl, string eventSubUrl, string SCPDURL)
        {
            this.server = server;
            this.device = device;
            this.serviceType = serviceType;
            this.serviceId = serviceId;
            this.controlUrl = controlUrl;
            this.eventSubUrl = eventSubUrl;
            this.SCPDURL = SCPDURL;

            MemoryStream memStream = new MemoryStream();
            using (XmlTextWriter descWriter = new XmlTextWriter(memStream, new UTF8Encoding(false)))
            {
                descWriter.Formatting = Formatting.Indented;
                descWriter.WriteRaw("<?xml version=\"1.0\"?>");

                descWriter.WriteStartElement("scpd", "urn:schemas-upnp-org:service-1-0");

                descWriter.WriteStartElement("specVersion");
                descWriter.WriteElementString("major", "1");
                descWriter.WriteElementString("minor", "0");
                descWriter.WriteEndElement();

                descWriter.WriteStartElement("actionList");

                MethodInfo[] methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
                foreach (MethodInfo method in methods)
                {
                    IEnumerable<UpnpServiceArgument> methAttribs = method.GetCustomAttributes(typeof(UpnpServiceArgument), true).Cast<UpnpServiceArgument>();
                    ParameterInfo[] parameters = method.GetParameters();
                    
                    if (methAttribs.Any() || parameters.Any(a => a.GetCustomAttributes(typeof(UpnpServiceArgument), true).Length > 0))
                    {
                        descWriter.WriteStartElement("action");
                        descWriter.WriteElementString("name", method.Name);
                        descWriter.WriteStartElement("argumentList");

                        //Zapisanie vstupnych parametrov
                        foreach (ParameterInfo parameter in parameters)
                        {
                            UpnpServiceArgument paramAttrib = parameter.GetCustomAttributes(typeof(UpnpServiceArgument), true).FirstOrDefault() as UpnpServiceArgument;
                            if (paramAttrib != null)
                            {
                                string param_name =
                                ((AliasAttribute)parameter.GetCustomAttributes(typeof(AliasAttribute), true)[0]).Name;
                                descWriter.WriteStartElement("argument");
                                descWriter.WriteElementString("name", param_name);
                                descWriter.WriteElementString("direction", "in");
                                descWriter.WriteElementString("relatedStateVariable", paramAttrib.RelatedStateVariable);
                                descWriter.WriteEndElement();
                            }
                        }

                        //Zapisanie vystupnych parametrov
                        foreach (UpnpServiceArgument methAttrib in methAttribs.OrderBy(a => a.Index))
                        {
                            descWriter.WriteStartElement("argument");
                            descWriter.WriteElementString("name", methAttrib.Name);
                            descWriter.WriteElementString("direction", "out");
                            descWriter.WriteElementString("relatedStateVariable", methAttrib.RelatedStateVariable);
                            descWriter.WriteEndElement();
                        }

                        descWriter.WriteEndElement();
                        descWriter.WriteEndElement();
                    }
                }

                descWriter.WriteEndElement();
                descWriter.WriteStartElement("serviceStateTable");

                //Zapisanie premennych
                foreach (UpnpServiceVariable variable in GetType().GetCustomAttributes(typeof(UpnpServiceVariable), true).Cast<UpnpServiceVariable>())
                {
                    descWriter.WriteStartElement("stateVariable");
                    descWriter.WriteAttributeString("sendEvents", variable.SendEvents ? "yes" : "no");
                    descWriter.WriteElementString("name", variable.Name);
                    descWriter.WriteElementString("dataType", variable.DataType);
                    if (variable.AllowedValue.Length > 0)
                    {
                        descWriter.WriteStartElement("allowedValueList");
                        foreach (string value in variable.AllowedValue)
                            descWriter.WriteElementString("allowedValue", value);
                        descWriter.WriteEndElement();
                    }
                    descWriter.WriteEndElement();
                }

                descWriter.WriteEndElement();

                descWriter.WriteEndElement();

                descWriter.Flush();
                descArray = memStream.ToArray();
            }

            var p2pProxyDevice = device as P2pProxyDevice;
            if (p2pProxyDevice != null)
            {
                p2pProxyDevice.AddRoute(SCPDURL, GetDescription, HttpMethod.Get);
                p2pProxyDevice.AddRoute(controlUrl, ProceedControl, HttpMethod.Post);
                p2pProxyDevice.AddRoute(eventSubUrl, ProceedEventSub, HttpMethod.Subscribe);
                p2pProxyDevice.AddRoute(eventSubUrl, ProceedEventUnsub, HttpMethod.Unsubscribe);
            }
        }

        private void GetDescription(MyWebRequest request)
        {
            MyWebResponse response = request.GetResponse();
            response.AddHeader(HttpHeader.ContentLength, this.descArray.Length.ToString());
            response.AddHeader(HttpHeader.ContentType, "text/xml; charset=\"utf-8\"");

            using (MemoryStream stream = new MemoryStream(this.descArray))
            {
                response.SendHeaders();

                stream.CopyTo(response.GetStream());
            }
        }

        private void SendEvent(string sid, Uri uri)
        {
            StringBuilder content = new StringBuilder();
            using (XmlWriter writer = XmlWriter.Create(content, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                writer.WriteRaw(@"<?xml version=""1.0""?>");
                writer.WriteStartElement("e", "propertyset", "urn:schemas-upnp-org:event-1-0");
                WriteEventProp(writer);
                writer.WriteEndElement();
            }
            byte[] contentBytes = Encoding.UTF8.GetBytes(content.ToString());

            WebRequest request = WebRequest.Create(uri);
            request.Method = "NOTIFY";
            request.ContentType = "text/xml; charset=\"utf-8\"";
            request.ContentLength = contentBytes.Length;
            request.Headers.Add("NT", "upnp:event");
            request.Headers.Add("NTS", "upnp:propchange");
            request.Headers.Add("SID", sid);
            request.Headers.Add("SEQ", "0");

            request.Proxy = null;

            System.Threading.Thread.Sleep(1000);

            try
            {
                Stream dataStream = request.GetRequestStream();
                dataStream.WriteTimeout = 20000;
                dataStream.Write(contentBytes, 0, contentBytes.Length);
                dataStream.Close();
                var responseStream = request.GetResponse().GetResponseStream();
                if (responseStream != null)
                    responseStream.Close();
            }
            catch { }
        }

        public void WriteDescription(XmlTextWriter descWriter)
        {

            descWriter.WriteElementString("serviceType", serviceType);

            descWriter.WriteElementString("serviceId", serviceId);

            descWriter.WriteElementString("controlURL", controlUrl);

            descWriter.WriteElementString("eventSubURL", eventSubUrl);

            descWriter.WriteElementString("SCPDURL", SCPDURL);
        }

        protected abstract void WriteEventProp(XmlWriter writer);

        public void RouteRequest(MyWebRequest req)
        {
            switch (req.Method)
            {
                case HttpMethod.Get:
                    SendDescription(req);
                    break;
                case HttpMethod.Post:
                    ProceedControl(req);
                    break;
                case HttpMethod.Subscribe:
                    ProceedEventSub(req);
                    break;
                case HttpMethod.Unsubscribe:
                    ProceedEventUnsub(req);
                    break;
            }
        }

        private void ProceedEventUnsub(MyWebRequest req)
        {
            req.GetResponse().SendHeaders();
        }

        private void ProceedEventSub(MyWebRequest req)
        {
            uint timeout = uint.Parse(req.Headers["TIMEOUT"].Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries).Last());

            string sid;
            if (req.Headers.ContainsKey("SID"))
            {
                sid = req.Headers["SID"];
            }
            else
            {
                sid = "uuid:" + Guid.NewGuid();
                string callback = req.Headers["CALLBACK"];
                int startIdx = callback.IndexOf('<') + 1;
                int endIdx = callback.IndexOf('>', startIdx);
                Uri uri = new Uri(callback.Substring(startIdx, endIdx - startIdx));

                new SendEventDel(SendEvent).BeginInvoke(sid, uri, null, null);
            }

            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentLength, "0");
            resp.AddHeader("SID", sid);
            resp.AddHeader("TIMEOUT", "Second-" + timeout);

            resp.SendHeaders();
        }

        private void ProceedControl(MyWebRequest req)
        {
            if (!req.Headers["SOAPACTION"].Trim('"').StartsWith(serviceType, StringComparison.OrdinalIgnoreCase))
                throw new HttpException(500, "Service type mismatch");
            XmlDocument xDoc = new XmlDocument();
            using (MemoryStream stream = req.GetContent())
            {
                xDoc.Load(stream);
                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xDoc.NameTable);
                namespaceManager.AddNamespace("soapNam", "http://schemas.xmlsoap.org/soap/envelope/");
                XmlNode bodyNode = xDoc.SelectSingleNode("/soapNam:Envelope/soapNam:Body/*[1]", namespaceManager);

                if (bodyNode == null)
                    throw new HttpException(500, "Body node of SOAP message not found");
                if (P2pProxyApp.Debug)
                    P2pProxyApp.Log.Write("Run method " + GetType().Name + "." + bodyNode.LocalName, TypeMessage.Info);
                MethodInfo method = GetType().GetMethod(bodyNode.LocalName, BindingFlags.Instance | BindingFlags.NonPublic);
                if (method == null)
                    throw new HttpException(401, "Invalid Action " + GetType().Name + "." + bodyNode.LocalName);
                string[] outParam = method.GetCustomAttributes(typeof(UpnpServiceArgument), true).Cast<UpnpServiceArgument>().OrderBy(
                    a => a.Index).Select(a => a.Name).ToArray();
                ParameterInfo[] paramDef = method.GetParameters();

                req.SetSoap(bodyNode.LocalName, serviceType, outParam);
                
                object[] paramVal = new object[paramDef.Length];
                paramVal[0] = req;
                for (int i = 1; i < paramDef.Length; i++)
                {
                    string param_name =
                        ((AliasAttribute) paramDef[i].GetCustomAttributes(typeof (AliasAttribute), true)[0]).Name;
                    XmlNode paramNode = bodyNode.SelectSingleNode(param_name);
                    if (paramNode == null)
                        throw new HttpException(402, "Invalid Args " + bodyNode.LocalName + " " + param_name);

                    paramVal[i] = paramNode.InnerXml;
                }
                try
                {
                    method.Invoke(this, paramVal);
                }
                catch (ArgumentException)
                {
                    throw new HttpException(402, "Invalid Args " + bodyNode.LocalName + " " + string.Join("|", paramVal));
                }
                catch (TargetParameterCountException)
                {
                    throw new HttpException(402, "Invalid Args " + bodyNode.LocalName + " " + string.Join("|", paramVal));
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is SoapException)
                        throw ex.InnerException;
                    P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
                    throw new SoapException(501, "Action Failed");
                }
            }
        }

        private void SendDescription(MyWebRequest req)
        {
            var resp = req.GetResponse();
            resp.AddHeader(HttpHeader.ContentLength, descArray.Length.ToString());
            resp.AddHeader(HttpHeader.ContentType, "text/xml; charset=\"utf-8\"");
            resp.SendHeaders();
            var stream = new MemoryStream(descArray);
            stream.CopyTo(resp.GetStream());
        }
    }
}