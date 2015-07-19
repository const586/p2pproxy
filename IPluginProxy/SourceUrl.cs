using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PluginProxy
{
    public struct SourceUrl
    {
        public string Url;
        public SourceType Type;
    }

    public enum SourceType : byte
    {
        Torrent = 0, ContentId = 1, File = 2
    }
}
