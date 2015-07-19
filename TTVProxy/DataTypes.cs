using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;

namespace P2pProxy
{

    public class ExtPlayList
    {
        public enum OutTypes { Web, File, Auto}

        public static readonly string PARAM_NAME = "{NAME}";
        public static readonly string PARAM_GROUP_NAME = "{GROUP_NAME}";
        public static readonly string PARAM_HOST = "{HOST}";
        public static readonly string PARAM_GROUP_ID = "{GROUP_ID}";
        public static readonly string PARAM_CHANNEL_ID = "{CHANNEL_ID}";
        public static readonly string PARAM_CONTENT_TYPE = "{CONTENT_TYPE}";
        public static readonly string PARAM_CURRENT_EPG = "{CURRENT_EPG}";
        public static readonly string PARAM_ID = "{ID}";
        public static readonly string PARAM_TIME = "{TIME}";
        public static readonly string PARAM_MINUTE = "{MINUTE}";
        public static readonly string PARAM_FILENAME = "{FILE_NAME}";
        public static readonly string PARAM_NUM = "{NUM}";
        public static readonly string PARAM_SCREEN = "{SCREEN}";

        public static readonly string SPECIAL_CHAR_AMP = "&amp;";
        public static readonly string SPECIAL_CHAR_LT = "&lt;";
        public static readonly string SPECIAL_CHAR_GT = "&gt;";
        public static readonly string CONST_NONE = "None";
        public static readonly string PARAM_EPGID = "{EPG_ID}";
        public static readonly string PARAM_RECORD_ID = "{RECORD_ID}";
        public static readonly string PARAM_CHANNEL_LOGO = "{CHANNEL_LOGO}";
        

        public string Format;
        public OutTypes[] Out;
        public string Ext;
        public string [] Icon;

        public PlaylistTempate Channel { get; private set; }
        public PlaylistTempate Archive { get; private set; }
        public PlaylistTempate Record { get; private set; }
        public PlaylistTempate Plugin { get; private set; }

        public static ExtPlayList LoadPlaylist(string fname)
        {
            if (!File.Exists(fname))
                return null;
            var pl = new ExtPlayList();
            var xd = XDocument.Load(fname);
            var xroot = xd.Element("pltempl");
            if (xroot == null)
                return null;
            var xmanifest = xroot.Element("manifest");
            if (xmanifest == null)
                return null;
            var xformat = xmanifest.Element("format");
            if (xformat == null)
                return null;
            pl.Format = xformat.Value.Replace(SPECIAL_CHAR_AMP, "&").Replace(SPECIAL_CHAR_GT, ">").Replace(SPECIAL_CHAR_LT, "<");

            var xchannel = xroot.Element("channels");
            if (xchannel != null)
            {

                pl.Channel = GetTemplate(xchannel);
            }
            var xarchive = xroot.Element("archive");
            if (xarchive != null)
            {
                pl.Archive = GetTemplate(xarchive);
            }
            var xrecrod = xroot.Element("records");
            if (xrecrod != null)
            {
                pl.Record = GetTemplate(xrecrod);
            }
            var xplugin = xroot.Element("plugin");
            if (xplugin != null)
            {
                pl.Plugin = GetTemplate(xplugin);
            }
            
            return pl;
        }

        private static PlaylistTempate GetTemplate(XElement element)
        {
            XElement xHeader = element.Element("header");
            XElement xLine = element.Element("lines");
            XElement xSublist = element.Element("sublist");
            XElement xBasement = element.Element("basement");
            try
            {
                var playlist = new PlaylistTempate();
                if (xHeader != null)
                {
                    playlist.Header =
                        xHeader.Value.Replace(SPECIAL_CHAR_AMP, "&")
                            .Replace(SPECIAL_CHAR_GT, ">")
                            .Replace(SPECIAL_CHAR_LT, "<");
                    playlist.Header = ConvertNewLineString(playlist.Header);
                }
                if (xLine != null)
                {
                    playlist.Line =
                        xLine.Value.Replace(SPECIAL_CHAR_AMP, "&")
                            .Replace(SPECIAL_CHAR_GT, ">")
                            .Replace(SPECIAL_CHAR_LT, "<");
                    playlist.Line = ConvertNewLineString(playlist.Line);
                }
                if (xSublist != null)
                {
                    playlist.Sublist =
                        xSublist.Value.Replace(SPECIAL_CHAR_AMP, "&")
                            .Replace(SPECIAL_CHAR_GT, ">")
                            .Replace(SPECIAL_CHAR_LT, "<");
                    playlist.Sublist = ConvertNewLineString(playlist.Sublist);
                }
                if (xBasement != null)
                {
                    playlist.Basemenet = xBasement.Value.Replace(SPECIAL_CHAR_AMP, "&")
                            .Replace(SPECIAL_CHAR_GT, ">")
                            .Replace(SPECIAL_CHAR_LT, "<");
                    playlist.Basemenet = ConvertNewLineString(playlist.Basemenet);
                }
                return playlist;
            }
            catch (Exception)
            {
                return new PlaylistTempate();
            }
        }

        private ExtPlayList()
        {
            
        }

        public static string ConvertNewLineString(string value)
        {
            if (!value.Contains("\r\n") && value.Contains("\n"))
            {
                return value.Replace("\n", Environment.NewLine);
            }
            return value;
        }
    }

    public struct PlaylistTempate
    {
        public string Header;
        public string Line;
        public string Sublist;
        public string Basemenet;
    }
}
