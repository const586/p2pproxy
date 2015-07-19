using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using P2pProxy;

namespace P2pProxy.UPNP
{
    [ObfuscationAttribute]
    public class MediaFileInfo
    {
        [ObfuscationAttribute]
        public string FileExt;
        [ObfuscationAttribute]
        public string DlnaType;
        [ObfuscationAttribute]
        public string Feature;
        [ObfuscationAttribute]
        public string TranscodingProfile;
    }

    [ObfuscationAttribute]
    public class VoDItem
    {
        [ObfuscationAttribute]
        public List<MediaFileInfo> Info;
        [ObfuscationAttribute]
        public string Resolution;
    }

    [ObfuscationAttribute]
    public class LiveItem
    {
        [ObfuscationAttribute]
        public bool SendHead;
        [ObfuscationAttribute]
        public bool SendContentLength;
        [ObfuscationAttribute]
        public string Resolution;
        [ObfuscationAttribute]
        public List<MediaFileInfo> Info;
    }

    [ObfuscationAttribute]
    public class UpnpConfig
    {
        [ObfuscationAttribute]
        public string Version;
        [ObfuscationAttribute]
        public string Name;
        [ObfuscationAttribute]
        public VoDItem VoD;
        [ObfuscationAttribute]
        public LiveItem Live;
    }
}