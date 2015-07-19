using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using P2pProxy.Http.Server;
using SimpleLogger;

namespace P2pProxy.Http.Server
{
    public class MyWebResponse
    {
        
        private readonly MyWebRequest _request;
        private readonly NetworkStream _stream;
        private const int _stateCode = 200;
        private readonly Dictionary<string, string> _headers;
        private static Dictionary<int, string> _codes = new Dictionary<int, string>
            {
                {100, "Continue"}, {101, "Switching Protocols"},
                {200, "OK"}, {201, "Created"}, {202, "Accepted"}, {203, "Non-Authoritative Information"},
                {204, "No Content"}, {205, "Reset Content"}, {206, "Partial Content"},
                {300, "Multiple Choices"}, {301, "Moved Permanently"}, {302, "Found"}, {303, "See Other"},
                {304, "Not Modified"}, {305, "Use Proxy"}, {306, "(Unused)" }, {307, "Temporary Redirect"},
                {400, "Bad Request"}, {401, "Unauthorized"}, {402, "Payment Required"}, {403, "Forbidden"},
                {404, "Not Found"}, {405, "Method Not Allowed"}, {406, "Not Acceptable"}, 
                {407, "Proxy Authentication Required"}, {408, "Request Timeout"}, {409, "Conflict"}, {410, "Gone"},
                {411, "Length Required"}, {412, "Precondition Failed"}, {413, "Request Entity Too Large"},
                {414, "Request-URI Too Long"}, {415, "Unsupported Media Type"}, 
                {416, "Requested Range Not Satisfiable"}, {417, "Expectation Failed"}, 
                {500, "Internal Server Error"}, {501, "Not Implemented"}, {502, "Bad Gateway"},
                {503, "Service Unavailable"}, {504, "Gateway Timeout"}, {505, "HTTP Version Not Supported"}
            };
        private bool responseSended;

        internal MyWebResponse(MyWebRequest req, NetworkStream stream)
        {
            _request = req;
            _stream = stream;
            _headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public void AddHeader(string key, string value)
        {
            if (!_headers.ContainsKey(key))
                _headers.Add(key, value);
            else _headers[key] = value;
        }

        public bool HeaderContains(HttpHeader key)
        {
            return _headers.ContainsKey(key.ToString());
        }

        public void AddHeader(HttpHeader header, string value, string charset="utf-8")
        {
            switch (header)
            {
                case HttpHeader.AcceptRanges: AddHeader("Accept-Ranges", value); return;
                case HttpHeader.ContentLength: AddHeader("Content-Length", value); return;
                case HttpHeader.ContentType:
                    if (value.Contains("charset="))
                        AddHeader("Content-Type", value);
                    else
                        AddHeader("Content-Type", string.Format("{0};charset={1}",value, charset)); 
                    return;
                case HttpHeader.Date: AddHeader("Date", value); return;
                case HttpHeader.Server: AddHeader("Server", value); return;
                case HttpHeader.Connection: AddHeader("Connection", value); return;
                case HttpHeader.TransferEncoding: AddHeader("Transfer-Encoding", value); return;
            }
        }

        public void SendHeaders(int code = 200)
        {
            if (!_headers.ContainsKey("Server"))
                AddHeader(HttpHeader.Server, string.Format("{2}/{0}.{1} P2pProxy/{3}",
                                                       Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor, Environment.OSVersion.Platform, P2pProxyApp.Version));
            if (!_headers.ContainsKey("Date"))
                AddHeader(HttpHeader.Date, DateTime.Now.ToString("r"));
            if (!_headers.ContainsKey("Connection"))
                AddHeader(HttpHeader.Connection, "close");
            if (!_headers.ContainsKey("Access-Control-Allow-Origin"))
                AddHeader("Access-Control-Allow-Origin", "*");
            if (!_headers.ContainsKey("Cache-control"))
                AddHeader(HttpHeader.CacheControl, "no-cache");
            //if (!_headers.ContainsKey("realTimeInfo.dlna.org"))
            //    AddHeader("realTimeInfo.dlna.org", "DLNA.ORG_TLAG=*");

            string status = _codes.ContainsKey(code) ? _codes[code] : "Unknown";
            byte[] data = Encoding.ASCII.GetBytes(String.Format("HTTP/1.1 {0} {1} \r\n", code, status));
            _stream.Write(data, 0, data.Length);

            foreach (KeyValuePair<string, string> kvp in _headers)
            {
                data = Encoding.ASCII.GetBytes(string.Format("{0}: {1}\r\n", kvp.Key, kvp.Value));
                try
                {
                    _stream.Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    
                }
                
            }
            data = Encoding.ASCII.GetBytes("\r\n");
            try
            {
                _stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
            }
            
        }

        public string GetState()
        {
            switch (_stateCode)
            {
                case 200: return "OK";
            }
        }

        public Stream GetStream()
        {
            return _stream;
        }



        public void SendBroadcast(string url, Action<MyWebRequest> SendHeaders = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                P2pProxyApp.Log.Write("Нечего проигрывать", TypeMessage.Error);
                return;
            }
            Uri uri = new Uri(url, UriKind.Absolute);


            var webts = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
            try
            {
                webts.Connect(uri.Host, uri.Port);
                webts.Send(Encoding.UTF8.GetBytes(String.Format("GET {0}{1}\r\nHost: {2}:{3}\r\nConnection: Keep-Alive\r\n\r\n",
                                                                uri.PathAndQuery, " HTTP/1.1", uri.Host, uri.Port)));
            }
            catch (Exception)
            {
                SendText("File not found");
                responseSended = true;
                return;
            }


            var buffer = new byte[WebServer.BUFFER_SIZE];
            int trying = 0;
            
            if (SendHeaders != null)
            {
                var b = webts.Receive(buffer, 0, buffer.Length, SocketFlags.None);
                SendHeaders(_request);
                string oldheader = Encoding.ASCII.GetString(buffer, 0, b);
                string header = oldheader.Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0];
                var newbuffer = buffer.Take(b).Skip(header.Length + 4).ToArray();
                _stream.Write(newbuffer, 0, newbuffer.Length);
            }
            while (webts.Connected && _stream.CanWrite)
            {
                try
                {
                    var b = webts.Receive(buffer, 0, buffer.Length, SocketFlags.None);

                    if (b == 0)
                    {

                        Thread.Sleep(1234);

                        if (trying >= 3)
                        {
                            break;
                        }
                        trying++;
                        continue;
                    }
                    trying = 0;
                    _stream.Write(buffer, 0, b);
                }
                catch
                {
                    break;
                }
            }
            if (webts.Connected) webts.Close();
            responseSended = true;

        }

        public void SendFile(string url)
        {
            Uri uri = new Uri(url, UriKind.Absolute);
            TcpClient client = null;
            if (!uri.Scheme.Equals("file", StringComparison.OrdinalIgnoreCase))
            {
                client = GetStreamFromWeb(url);
                Stream stream = client.GetStream();
                try
                {
                    StreamReader sr = new StreamReader(stream, Encoding.ASCII);
                    var first = sr.ReadLine();
                    if (!first.Contains("200") && !first.Contains("206"))
                    {
                        sr.Close();
                        throw new Exception(first);
                    }
                    while (true)
                    {
                        var linehead = sr.ReadLine();
                        if (string.IsNullOrEmpty(linehead))
                            break;

                        var head = linehead.Split(":".ToCharArray(), 2, StringSplitOptions.None);
                        var key = head[0].Trim();
                        var value = (string.IsNullOrEmpty(head[1]) ? "" : head[1].Trim());
                        if (key.Equals("Server", StringComparison.OrdinalIgnoreCase))
                            continue;
                        AddHeader(key, value);
                    }
                    if (_headers.ContainsKey("Content-Length") && _request.Headers.ContainsKey("Range"))
                    {
                        Console.WriteLine(_headers["Content-Range"]);
                    }
                    this.SendHeaders();
                    long count = 0;
                    while (true)
                    {
                        byte[] buffer = new byte[WebServer.BUFFER_SIZE];
                        int b = stream.Read(buffer, 0, buffer.Length);
                        string test = Encoding.ASCII.GetString(buffer, 0, b);
                        _stream.Write(buffer, 0, b);
                        count += b;
                        if (_headers.ContainsKey("Content-Length") && count >= long.Parse(_headers["Content-Length"]))
                            break;
                    }
                }
                catch (IOException e)
                {
                    client.GetStream().Close();
                    if (e.InnerException is SocketException)
                    {
                        if ((e.InnerException as SocketException).ErrorCode == 10060)
                        {
                            client.GetStream().Dispose();
                        }
                    }
                    client.Close();
                    
                    Console.WriteLine(e.Message);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error. " + e.Message);
                }   
                finally {
                    Console.WriteLine("End stream.");
                    if (client != null)
                    {
                        client.Close();
                    }
                }
            }
            else
            {
                AddHeader(HttpHeader.AcceptRanges, "bytes");
                AddHeader(HttpHeader.ContentType, WebServer.GetMime(Path.GetExtension(url)).ToString());
                var stream = File.OpenRead(url);
                if (_request.Headers.ContainsKey("Range") && !string.IsNullOrEmpty(_request.Headers["Range"]) &&
                    stream.CanSeek && stream.Length > 0)
                {
                    var range = Range.Parse(_request.Headers["Range"])[0];
                    long length = 0;
                    if (range.IsFirst || range.IsCenter)
                    {
                        stream.Position = range.From.Value;
                        if (range.IsCenter)
                            length = range.To.Value - range.From.Value + 1;
                        else
                            length = stream.Length - stream.Position;
                    }
                    else if (range.IsLast)
                    {
                        stream.Position = stream.Length - range.To.Value;
                        length = range.To.Value;
                    }
                    AddHeader("transferMode.dlna.org", "Streaming");
                    AddHeader("File-Size", stream.Length.ToString());
                    AddHeader("Content-Range",
                        string.Format("{0} {1}-{2}/{3}", range.Unit, stream.Position, stream.Position + length - 1,
                            stream.Length));
                    AddHeader(HttpHeader.ContentLength, length.ToString());
                    SendHeaders();
                    stream.CopyTo(_stream);
                }
                else
                {
                    AddHeader(HttpHeader.ContentLength, stream.Length.ToString());
                    SendHeaders();
                    stream.CopyTo(_stream);
                }
            }
        }

        private TcpClient GetStreamFromWeb(string url)
        {
            try
            {
                Uri uri = new Uri(url, UriKind.Absolute);
                TcpClient client = new TcpClient();
                client.ReceiveTimeout = 30000;
                client.SendTimeout = 30000;
                client.Connect(uri.Host, uri.Port);
                StreamWriter sw = new StreamWriter(client.GetStream(), Encoding.ASCII);
                //sw.NewLine = "\r\n";
                //sw.Write(String.Format("GET {0}{1}\r\nHost: {2}:{3}\r\n\r\n",
                //                                                uri.PathAndQuery, " HTTP/1.1", uri.Host, uri.Port));
                sw.Write(string.Format("GET {0} HTTP/1.1\r\n", uri.PathAndQuery));
                sw.Write(string.Format("Host: {0}:{1}\r\n", uri.Host, uri.Port));
                //sw.Write("Connection: Keep-Alive\r\n");
                foreach (var kp in _request.Headers)
                {
                    if (kp.Key.Equals("Host", StringComparison.OrdinalIgnoreCase))
                        continue;
                    sw.Write(string.Format("{0}: {1}\r\n", kp.Key, kp.Value));
                }
                sw.Write("\r\n");
                //sw.Write("Connection: Close\r\n");
                //sw.Write("Icy-MetaData: 1\r\n");
                //if (_request.Headers.ContainsKey("User-Agent"))
                //    sw.Write("User-Agent: " + _request.Headers["User-Agent"] + "\r\n");
                
                //if (_request.Headers.ContainsKey("Range"))
                //    sw.Write("Range: " + _request.Headers["Range"] + "\r\n\r\n");
                //sw.WriteLine();
                sw.Flush();

                //req = (HttpWebRequest)WebRequest.CreateHttp(url);
                //if (_request.Headers.ContainsKey("Range"))
                //{
                //    try
                //    {
                //        var ranges = Range.Parse(_request.Headers["Range"]);
                //        foreach (var r in ranges)
                //        {
                //            if (r.IsCenter)
                //                req.AddRange(r.Unit, r.From.Value, r.To.Value);
                //            else if (r.IsFirst)
                //                req.AddRange(r.Unit, r.From.Value);
                //            else if (r.IsLast)
                //                req.AddRange(r.Unit, -r.To.Value);
                //        }
                //    }
                //    catch
                //    {
                //    }
                //}
                //req.KeepAlive = true;
                //if (_request.Headers.ContainsKey("User-Agent"))
                //    req.UserAgent = _request.Headers["User-Agent"];
                //req.Headers.Add("Icy-MetaData", "1");
                //req.Timeout = 4321;
                return client;
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    GC.Collect();
                    return GetStreamFromWeb(url);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public void SendText(string text)
        {
            byte[] res = Encoding.UTF8.GetBytes(text);
            AddHeader(HttpHeader.ContentType, "text/plain;charset=utf-8");
            AddHeader(HttpHeader.ContentLength, res.Length.ToString());
            SendHeaders();
            try
            {
                _stream.Write(res, 0, res.Length);
            }
            catch (Exception ex)
            {
                
                P2pProxyApp.Log.Write(ex.Message, TypeMessage.Error);
            }
            responseSended = true;

        }

        public void SendSoapHeadersBody(params string[] arguments)
        {
            if (responseSended)
                return;
            if (arguments.Length != this._request.SoapOutParams.Length)
                throw new HttpException(400, "Bad number of SOAP parameters");
            MemoryStream memoryStream = new MemoryStream();
            using (XmlTextWriter soapWriter = new XmlTextWriter(memoryStream, new UTF8Encoding(false)))
            {
                soapWriter.Formatting = Formatting.Indented;
                soapWriter.WriteRaw("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");

                soapWriter.WriteStartElement("s", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                soapWriter.WriteAttributeString("s", "encodingStyle", null, "http://schemas.xmlsoap.org/soap/encoding/");

                soapWriter.WriteStartElement("s", "Body", null);

                soapWriter.WriteStartElement("u", _request.SoapAction + "Response", this._request.SoapService);

                int i = 0;
                foreach (string argument in arguments)
                {
                    soapWriter.WriteElementString(this._request.SoapOutParam[i++], argument);
                }

                soapWriter.WriteEndElement();

                soapWriter.WriteEndElement();

                soapWriter.WriteEndElement();

                soapWriter.Flush();
                memoryStream.Position = 0;

                AddHeader(HttpHeader.ContentLength, memoryStream.Length.ToString());
                AddHeader(HttpHeader.ContentType, "text/xml; charset=\"utf-8\"");

                SendHeaders();
                memoryStream.CopyTo(GetStream());
            }
        }

        public void SendSoapErrorHeadersBody(int code, string message)
        {
            if (this.responseSended)
                return;

            MemoryStream memStream = new MemoryStream();
            using (XmlTextWriter soapWriter = new XmlTextWriter(memStream, new UTF8Encoding(false)))
            {
                soapWriter.Formatting = Formatting.Indented;
                soapWriter.WriteRaw("<?xml version=\"1.0\"?>");

                soapWriter.WriteStartElement("s", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
                soapWriter.WriteAttributeString("s", "encodingStyle", null, "http://schemas.xmlsoap.org/soap/encoding/");

                soapWriter.WriteStartElement("s", "Body", null);

                soapWriter.WriteStartElement("s", "Fault", null);

                soapWriter.WriteElementString("faultcode", "s:Client");
                soapWriter.WriteElementString("faultstring", "UPnPError");

                soapWriter.WriteStartElement("detail");

                soapWriter.WriteStartElement("UPnPError", "urn:schemas-upnp-org:control-1-0");
                soapWriter.WriteElementString("errorCode", code.ToString());
                soapWriter.WriteElementString("errorDescription", message);

                soapWriter.WriteEndElement();

                soapWriter.WriteEndElement();

                soapWriter.WriteEndElement();

                soapWriter.WriteEndElement();

                soapWriter.WriteEndElement();

                soapWriter.Flush();
                memStream.Position = 0;
                AddHeader(HttpHeader.ContentLength, memStream.Length.ToString());
                AddHeader(HttpHeader.ContentType, "text/xml; charset=\"utf-8\"");
                SendHeaders(500);
                memStream.CopyTo(GetStream());
            }

        }

        public MyWebRequest Request { get { return _request; } }
    }
}