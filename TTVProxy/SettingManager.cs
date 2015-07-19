using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using P2pProxy;

namespace P2pProxy
{
    public class SettingManager
    {
        private XmlSettings.Settings _settings;

        public SettingManager(string path)
        {
            _settings = new XmlSettings.Settings(path);
        }

        public int GetSetting(string section, string key, int defvalue)
        {
            int res;
            if (!int.TryParse(_settings.GetValue(section, key), out res))
            {
                res = defvalue;
                _settings.SetValue(section, key, defvalue.ToString());
            }
            return res;
        }

        public string GetSetting(string section, string key, string defvalue = null)
        {
            string res = _settings.GetValue(section, key);
            if (!string.IsNullOrEmpty(defvalue) && string.IsNullOrEmpty(res) && defvalue != null)
            {
                res = defvalue;
                _settings.SetValue(section, key, defvalue);
            }
            return res;
        }

        public bool GetSetting(string section, string key, bool defvalue)
        {
            bool res;
            if (!bool.TryParse(_settings.GetValue(section, key), out res))
            {
                res = defvalue;
                _settings.SetValue(section, key, defvalue.ToString());
            }
            return res;
        }

        public void SetSetting(string section, string key, object value)
        {
            _settings.SetValue(section, key, value.ToString());
        }
    }
}
