using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using PluginProxy;

namespace PluginFavourites
{
    public class AboutRequestData : IRequestData
    {
        private readonly Stream _stream;
        public Dictionary<string, string> Headers { get; private set; }
        private ushort _result = 200;

        public AboutRequestData(string host, string id = "")
        {
            if (!string.IsNullOrEmpty(id))
            {
                _stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PluginFavourites.html." + id + ".html");
            }
            if (_stream == null)
            {
                var about = Assembly.GetExecutingAssembly().GetManifestResourceStream("PluginFavourites.html.about.html");
                if (about != null)
                {
                    StreamReader sr = new StreamReader(about);
                    string text = sr.ReadToEnd();
                    text = text.Replace("{CONTENT}", string.Format("{0}/about?id=content", host));
                    text = text.Replace("{CONTAINER}", string.Format("{0}/about?id=container", host));
                    text = text.Replace("{PLUGIN}", string.Format("{0}/about?id=plugin", host));
                    text = text.Replace("{DATA}", string.Format("{0}/about?id=data", host));
                    text = text.Replace("{MEDIA_TYPE}", string.Format("{0}/about?id=media", host));
                    text = text.Replace("{SOURCE_TYPE}", string.Format("{0}/about?id=source", host));
                    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(text));
                    _stream = ms;

                }
                else
                {
                    var text = Encoding.UTF8.GetBytes("<html><body><h1>404 Not Found</h1></body></html>");
                    _stream = new MemoryStream(text);
                    _result = 404;
                }
            }
            Headers = new Dictionary<string, string>
                          {{"Content-Type", "text/html; charset=UTF-8"}, 
                          {"Content-Length", _stream.Length.ToString()}};
        }

        public Stream GetStream()
        {
            return _stream;
        }

        public ushort ResultState { get { return _result; } }
    }
}