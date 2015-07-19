using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Web;

namespace P2pProxy.Http.Server
{
    public class MyWebRequest
    {
        internal struct Range
        {
            public enum RangeType
            {
                Bytes
            }
            public long Start;
            public long End;
            public RangeType Type;
            private string _value;

            public static Range CompileRange(string value)
            {
                
                Range range = new Range();
                range._value = value;
                if (string.IsNullOrEmpty(value))
                {
                    range.Type = RangeType.Bytes;
                    range.Start = -1;
                    range.End = -1;
                }
                else
                {
                    var p = value.Split("=".ToCharArray());
                    var bytes = p[1].Split("-".ToCharArray());
                    range.Type = (RangeType)Enum.Parse(typeof(RangeType), p[0], true);
                    range.Start = long.Parse(bytes[0]);
                    if (string.IsNullOrEmpty(bytes[1]))
                        range.End = -1;
                    else
                        range.End = long.Parse(bytes[1]);
                }
                
                return range;
            }

            public override string ToString()
            {
                return string.Format("{0} {1}-{2}", Type.ToString().ToLower(), Start, End);
            }

            public string CreateResponseString(long position, long length)
            {
                if (End < 0)
                    return string.Format("{2} {0}-{1}/{1}", position, length, Type.ToString().ToLower());
                int rangeLength = (int)(End - position);
                return string.Format("{3} {0}-{1}/{2}", position, rangeLength, length, Type.ToString().ToLower());
            }

            public bool IsEmpty { get { return Start < 0; }}
        }
        public TcpClient Client { get; private set; }
        private Dictionary<string, string> headers;
        private Dictionary<string, string> urlParams;
        private HttpMethod method;
        private string version;
        private string url;
        private NetworkStream stream;
        private string[] _soapOutParams;
        private string _soapService;
        private string _soapAction;

        public string Url { get { return url; } }
        public HttpMethod Method { get { return method; } }
        public Dictionary<string, string> Headers { get { return headers; } }
        public string Version { get { return version; } }
        public Dictionary<string, string> Parameters { get { return urlParams; } }
        public string QueryString { get; private set; }

        public string[] SoapOutParams
        {
            get { return _soapOutParams; }
        }

        public object SoapAction
        {
            get { return _soapAction; }
        }

        public string SoapService
        {
            get { return _soapService; }
        }

        public string[] SoapOutParam
        {
            get { return _soapOutParams; }
        }

        public static MyWebRequest Create(TcpClient client)
        {
            var ret = new MyWebRequest
                          {
                              headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                              urlParams = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                              Client = client,
                              stream = client.GetStream(),
                              QueryString = string.Empty
                          };

            StringBuilder sb = new StringBuilder(128);
            bool isLastR = false, isFirstLine = true;
            int iByte;

            while ((iByte = ret.stream.ReadByte()) >= 0)
            {
                if (iByte == '\n' && isLastR)
                {
                    string line = sb.ToString(0, sb.Length - 1);
                    if (line == string.Empty)
                        break;
                    sb.Clear();

                    if (isFirstLine)
                    {
                        string[] values = line.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        ret.method = (HttpMethod)Enum.Parse(typeof(HttpMethod), values[0], true);

                        int index = values[1].LastIndexOf(' ');
                        ret.version = values[1].Substring(index + 1);

                        values = HttpUtility.UrlDecode(values[1].Substring(0, index)).Split(new[] { '?' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        ret.url = values[0];
                        if (values.Length > 1)
                            ret.QueryString = values[1];
                        if (values.Length == 2)
                        {
                            foreach (string parameter in values[1].Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                string[] keyValue = parameter.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                                ret.urlParams[keyValue[0]] = (keyValue.Length == 2) ? keyValue[1] : string.Empty;
                            }
                        }

                        isFirstLine = false;
                    }
                    else
                    {
                        string[] keyValue = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        string header = keyValue[0].Trim();
                        string values = keyValue[1].Trim();
                        ret.headers[header] = keyValue.Length > 1 ? values : string.Empty;
                    }

                    isLastR = false;
                }
                else if (iByte == '\r')
                {
                    sb.Append((char)iByte);
                    isLastR = true;
                }
                else
                {
                    sb.Append((char)iByte);
                    isLastR = false;
                }
            }

            return ret;
        }

        public int GetLength()
        {
            return int.Parse(headers["Content-Length"]);
        }

        public MemoryStream GetContent()
        {
            byte[] buffer = new byte[GetLength()];
            int readed, offset = 0;

            while ((readed = stream.Read(buffer, offset, buffer.Length - offset)) > 0)
            {
                offset += readed;
                if (offset >= buffer.Length)
                    break;
            }

            return new MemoryStream(buffer, 0, buffer.Length);
        }

        public void SetSoap(string soapAction, string soapService, string[] soapOutParam)
        {
            this._soapAction = soapAction;
            this._soapService = soapService;
            this._soapOutParams = soapOutParam;
        }

        public MyWebResponse GetResponse()
        {
            return new MyWebResponse(this, stream);
        }

    }
}