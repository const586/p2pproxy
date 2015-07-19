using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PluginFavourites
{
    public class YouTubeVideoQuality : IComparable
    {
        /// <summary>
        /// Gets or Sets the file name
        /// </summary>
        public string VideoTitle { get; set; }
        /// <summary>
        /// Gets or Sets the file extention
        /// </summary>
        public string Extention { get; set; }
        /// <summary>
        /// Gets or Sets the file url
        /// </summary>
        public string DownloadUrl { get; set; }
        /// <summary>
        /// Gets or Sets the youtube video url
        /// </summary>
        public string VideoUrl { get; set; }
        /// <summary>
        /// Gets or Sets the youtube video size
        /// </summary>
        public long VideoSize { get; set; }

        /// <summary>
        /// Gets the youtube video length
        /// </summary>
        public long Length { get; set; }

        public ushort Dimension { get; set; }

        public void SetSize(long size)
        {
            this.VideoSize = size;
        }

        public void SetQuality(string Extention, ushort Dimension)
        {
            this.Extention = Extention;
            this.Dimension = Dimension;
        }

        public int CompareTo(object obj)
        {
            return Dimension.CompareTo((obj as YouTubeVideoQuality).Dimension);
        }
    }
    /// <summary>
    /// Use this class to get youtube video urls
    /// </summary>
    public class YouTubeDownloader
    {
        public static List<YouTubeVideoQuality> GetYouTubeVideoUrls(params string[] VideoUrls)
        {
            List<YouTubeVideoQuality> urls = new List<YouTubeVideoQuality>();
            foreach (var VideoUrl in VideoUrls)
            {
                string html = Helper.DownloadWebPage(VideoUrl);
                string title = GetTitle(html);
                foreach (var videoLink in ExtractUrls(html))
                {
                    YouTubeVideoQuality q = new YouTubeVideoQuality();
                    q.VideoUrl = VideoUrl;
                    q.VideoTitle = title;
                    q.DownloadUrl = videoLink + "&title=" + title;
                    q.Length = long.Parse(Regex.Match(html, "\"length_seconds\":(.+?),", RegexOptions.Singleline).Groups[1].ToString().Replace("\"", ""));
                    bool IsWide = IsWideScreen(html);
                    if (getQuality(q, IsWide))
                        urls.Add(q);
                }
            }
            return urls;
        }
        private static string GetTitle(string RssDoc)
        {
            string str14 = Helper.GetTxtBtwn(RssDoc, "'VIDEO_TITLE': '", "'", 0);
            if (str14 == "") str14 = Helper.GetTxtBtwn(RssDoc, "\"title\" content=\"", "\"", 0);
            if (str14 == "") str14 = Helper.GetTxtBtwn(RssDoc, "&title=", "&", 0);
            str14 = str14.Replace(@"\", "").Replace("'", "&#39;").Replace("\"", "&quot;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("+", " ");
            return str14;
        }

        private static List<string> ExtractUrls(string html)
        {
            List<string> urls = new List<string>();
            string DataBlockStart = "\"url_encoded_fmt_stream_map\":\\s+\"(.+?)&";  // Marks start of Javascript Data Block

            html = Uri.UnescapeDataString(Regex.Match(html, DataBlockStart, RegexOptions.Singleline).Groups[1].ToString());

            string firstPatren = html.Substring(0, html.IndexOf('=') + 1);
            var matchs = Regex.Split(html, firstPatren);
            for (int i = 0; i < matchs.Length; i++)
                matchs[i] = firstPatren + matchs[i];
            foreach (var match in matchs)
            {
                if (!match.Contains("url=")) continue;

                string url = Helper.GetTxtBtwn(match, "url=", "\\u0026", 0);
                if (url == "") url = Helper.GetTxtBtwn(match, "url=", ",url", 0);
                if (url == "") url = Helper.GetTxtBtwn(match, "url=", "\",", 0);

                string sig = Helper.GetTxtBtwn(match, "sig=", "\\u0026", 0);
                if (sig == "") sig = Helper.GetTxtBtwn(match, "sig=", ",sig", 0);
                if (sig == "") sig = Helper.GetTxtBtwn(match, "sig=", "\",", 0);

                while ((url.EndsWith(",")) || (url.EndsWith(".")) || (url.EndsWith("\"")))
                    url = url.Remove(url.Length - 1, 1);

                while ((sig.EndsWith(",")) || (sig.EndsWith(".")) || (sig.EndsWith("\"")))
                    sig = sig.Remove(sig.Length - 1, 1);

                if (string.IsNullOrEmpty(url)) continue;
                if (!string.IsNullOrEmpty(sig))
                    url += "&signature=" + sig;
                urls.Add(url);
            }
            return urls;
        }

        /// <summary>
        /// check whether the video is in widescreen format
        /// </summary>
        public static Boolean IsWideScreen(string html)
        {
            bool res = false;

            string match = Regex.Match(html, @"'IS_WIDESCREEN':\s+(.+?)\s+", RegexOptions.Singleline).Groups[1].ToString().ToLower().Trim();
            res = ((match == "true") || (match == "true,"));
            return res;
        }

        private static bool getQuality(YouTubeVideoQuality q, Boolean _Wide)
        {
            int iTagValue;
            string itag = Regex.Match(q.DownloadUrl, @"itag=([1-9]?[0-9]?[0-9])", RegexOptions.Singleline).Groups[1].ToString();
            if (itag != "")
            {
                if (int.TryParse(itag, out iTagValue) == false)
                    iTagValue = 0;

                switch (iTagValue)
                {
                    case 5: q.SetQuality("flv", 320); break;
                    case 6: q.SetQuality("flv", 480); break;
                    case 17: q.SetQuality("3gp", 176); break;
                    case 18: q.SetQuality("mp4", 640); break;
                    case 22: q.SetQuality("mp4", 1280); break;
                    case 34: q.SetQuality("flv", 640); break;
                    case 35: q.SetQuality("flv", 854); break;
                    case 37: q.SetQuality("mp4", 1920); break;
                    case 38: q.SetQuality("mp4", 2048); break;
                    case 43: q.SetQuality("webm", 640); break;
                    case 44: q.SetQuality("webm", 854); break;
                    case 45: q.SetQuality("webm", 1280); break;
                    case 46: q.SetQuality("webm", 1920); break;
                    case 83: q.SetQuality("3D.mp4", 640); break;        // 3D
                    case 84: q.SetQuality("3D.mp4", 1280); break;       // 3D
                    case 85: q.SetQuality("3D.mp4", 1920); break;     // 3D
                    case 100: q.SetQuality("3D.webm", 640); break;      // 3D
                    case 101: q.SetQuality("3D.webm", 640); break;      // 3D
                    case 102: q.SetQuality("3D.webm", 1280); break;     // 3D
                    case 120: q.SetQuality("live.flv", 1280); break;    // Live-streaming - should be ignored?
                }
                return true;
            } return false;
        }
    }
}
