using System;
using System.IO;
using System.Reflection;
using System.Text;
using P2pProxy.Http.Server;

namespace P2pProxy.Http.Content
{
    public abstract class ContentProvider
    {
        public void SendResponse(MyWebRequest req)
        {
            string res;
            try
            {
                res = GetPlaylist(req);
            }
            catch (Exception ex)
            {
                res = ex.Message;
            }

            MyWebResponse resp = req.GetResponse();
            resp.SendText(res);
        }
        public abstract string GetPlaylist(MyWebRequest req);
        public abstract void Play(MyWebRequest req);
    }

    public class Playlist
    {
        public enum ContentType
        {
            Channel, Record, Archive, Plugin
        }
        private readonly StringBuilder _result = new StringBuilder();
        private ExtPlayList _templ;
        private ContentType _type;
        private string _host;

        private Playlist(ExtPlayList templ, string host)
        {
            _templ = templ;
            _host = host;
            string line = string.Empty;
            switch (_type)
            {
                case ContentType.Archive:
                    line = _templ.Archive.Header;
                    break;
                case ContentType.Channel:
                    line = _templ.Channel.Header;
                    break;
                case ContentType.Record:
                    line = _templ.Record.Header;
                    break;
                case ContentType.Plugin:
                    line = _templ.Plugin.Header;
                    break;
            }
            _result.AppendLine(line.Replace("{HOST}", host));
        }

        public static Playlist CreatePlaylist(string ext, string host, ContentType type)
        {
            var tmpldir = P2pProxyApp.ExeDir + "/pltempl" + "/" + ext + ".xml";
            if (!File.Exists(tmpldir))
                tmpldir = P2pProxyApp.ApplicationDataFolder + "/pltempl" + "/" + ext + ".xml";
            var templ = ExtPlayList.LoadPlaylist(tmpldir);
            if (templ != null)
                return new Playlist(templ, host)
                           {
                               _templ = templ,
                               _type = type
                           };
            throw new Exception("Playlist not found");
        }

        public void AddLine(object obj, bool sublist = false, string append = "")
        {
            Type t = obj.GetType();
            string line = "";
            switch (_type)
            {
                case ContentType.Archive:
                    line = _templ.Archive.Line;
                    break;
                case ContentType.Channel:
                    line = _templ.Channel.Line;
                    break;
                case ContentType.Record:
                    line = _templ.Record.Line;
                    break;
                case ContentType.Plugin:
                    line = sublist ? _templ.Plugin.Sublist : _templ.Plugin.Line;
                    break;
            }
            foreach (PropertyInfo property in t.GetProperties())
            {
                var value = property.GetValue(obj, null);
                if (value != null)
                    line = line.Replace("{" + property.Name.ToUpper() + "}", value.ToString());
            }
            line = line.Replace("{HOST}", _host).
            Replace(ExtPlayList.SPECIAL_CHAR_GT, "<").
            Replace(ExtPlayList.SPECIAL_CHAR_LT, "<");
            line += append;
            _result.AppendLine(line);
        }

        public override string ToString()
        {
            switch (_type)
            {
                case ContentType.Archive:
                    return _result + _templ.Archive.Basemenet;
                case ContentType.Channel:
                    return _result + _templ.Channel.Basemenet;
                case ContentType.Plugin:
                    return _result + _templ.Plugin.Basemenet;
                case ContentType.Record:
                    return _result + _templ.Record.Basemenet;
                default:
                    return _result.ToString();
            }
            
        }
    }
}
