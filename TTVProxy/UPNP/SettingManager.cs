using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using P2pProxy;

namespace P2pProxy.UPNP
{
    public class UpnpSettingManager
    {
        public UpnpConfig Profile { get; private set; }

        public UpnpSettingManager(string file = "default")
        {
            string path = P2pProxyApp.ApplicationDataFolder + "/dlna";
            if (File.Exists(P2pProxyApp.ExeDir + "/dlna/" + file + ".ups"))
                LoadConfigs(P2pProxyApp.ExeDir + "/dlna/" + file + ".ups");
            else if (File.Exists(P2pProxyApp.ApplicationDataFolder + "/dlna/" + file + ".ups"))
                LoadConfigs(P2pProxyApp.ApplicationDataFolder + "/dlna/" + file + ".ups");
            else
                LoadDefaultConfig();            
        }

        private void LoadDefaultConfig()
        {
            UpnpConfig config = new UpnpConfig
                {
                    Name = "Default",
                    Version = "1",
                    VoD = new VoDItem
                    {
                        Resolution = "640x480",
                        Info = new List<MediaFileInfo>
                            {
                                new MediaFileInfo
                                    {
                                        DlnaType =
                                            "DLNA.ORG_PN=MPEG_TS_SD_EU_ISO;",
                                        Feature =
                                            "DLNA.ORG_OP=10;DLNA.ORG_FLAGS=8D100000000000000000000000000000",
                                        FileExt = ".ts",
                                        TranscodingProfile = ""
                                    }
                            }
                    },
                    Live = new LiveItem
                    {

                        Resolution = "640x480",
                        SendContentLength = false,
                        SendHead = false,
                        Info = new List<MediaFileInfo>
                            {
                                new MediaFileInfo
                                    {
                                        DlnaType =
                                            "DLNA.ORG_PN=MPEG_PS_PAL;",
                                        Feature =
                                            "DLNA.ORG_OP=00;DLNA.ORG_CI=1;DLNA.ORG_FLAGS=01500000000000000000000000000000",
                                        FileExt = "m2ts",
                                        TranscodingProfile = ""
                                    }
                            }
                    }
                };
            Profile = config;
        }

        void LoadConfigs(string path)
        {
            try
            {
                XmlSerializer serial = new XmlSerializer(typeof(UpnpConfig));
                Profile = (UpnpConfig)serial.Deserialize(File.OpenRead(path));
            }
            catch (Exception)
            {
                LoadDefaultConfig();
            }
        }

        public void SaveConfig()
        {
            XmlSerializer serial = new XmlSerializer(typeof(UpnpConfig));
            StreamWriter sw = new StreamWriter(P2pProxyApp.ExeDir + "/dlna.ups", false);
            serial.Serialize(sw, Profile);
        }

        public static Dictionary<string, string> GetProfiles()
        {
            string path = P2pProxyApp.ExeDir + "/dlna/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            Dictionary<string, string> _profiles = new Dictionary<string, string>();
            _profiles.Add("default", "По умолчанию");
            
            LoadProfile(path, _profiles);

            path = P2pProxyApp.ApplicationDataFolder + "/dlna";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            LoadProfile(path, _profiles);

            return _profiles;
        }

        private static void LoadProfile(string path, Dictionary<string, string> container)
        {

            foreach (var file in Directory.GetFiles(path, "*.ups"))
            {
                try
                {
                    XDocument xd = XDocument.Load(file);
                    if (xd.Root.Element("Version").Value != "1")
                        continue;
                    container.Add(Path.GetFileNameWithoutExtension(file), xd.Root.Element("Name").Value);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
