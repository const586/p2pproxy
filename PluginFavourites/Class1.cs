using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using PluginProxy;

namespace PluginFavourites
{
    public class Plugin : IPluginProxy
    {
        private const string ABOUT_PATH = "about";
        private const string ABOUT_MENU = "О плагине";
        private const string SETTINGS_MENU = "Настройки";

        private string _host;
        private string _id = "favourites";
        private string _name = "Избранное";

        public string Id { get { return _id; } }
        public string Name { get { return _name; } }
        public event LoggerCallback Logger
        {
            add { LoggerCallback += value; }
            remove { LoggerCallback -= value; }
        }

        internal static event LoggerCallback LoggerCallback;
        private static IPluginProxy ThisClass;

        private FavouriteContentProvider _content;
        private List<string> _menus;

        public static string SelfPath
        {
            get { return Path.GetDirectoryName(Assembly.GetCallingAssembly().Location); }
        }

        public void Init(string host)
        {
            ThisClass = this;
            _host = host;
            _content = new FavouriteContentProvider(_host);
            _menus = new List<string> {SETTINGS_MENU, ABOUT_MENU};
            RaiseLogger("Плагин инициализирован");
        }

        private static void RaiseLogger(string message)
        {
            if (LoggerCallback != null)
                LoggerCallback(ThisClass, message);
        }

        private void ShowSettings()
        {
            RaiseLogger("Открытие настроек");
            var frm = new FormSettings();
            frm.Show();
            frm.Closed += (sender, args) =>
                              {
                                  Init(_host);
                              };
        }

        private void ShowAbout()
        {
            RaiseLogger("Открытие справочной страницы");
            Process.Start(_host + "/about");
        }

        public IEnumerable<string> GetRouteUrls()
        {
            return new List<string>
                       {
                           ABOUT_PATH
                       };
        }

        public IRequestData HttpRequest(string path, Dictionary<string, string> Parameters)
        {
            var paths = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            switch (paths[0])
            {
                case ABOUT_PATH:
                    return new AboutRequestData(_host, Parameters.ContainsKey("id") ? Parameters["id"] : "");
                default:
                    RaiseLogger("Страница не найдена: " + path);
                    break;
            }
            return null;
        }

        public IPluginContent GetContent(Dictionary<string,string> parameters)
        {
            if (parameters != null && parameters.ContainsKey("id"))
            {
                RaiseLogger("Возвращаю контент с ID " + parameters["id"]);
                return _content.GetChildContent(parameters["id"]);
            }
            RaiseLogger("Возвращаю корневой узел");
            return _content;
        }

        public IEnumerable<string> GetMenus()
        {
            RaiseLogger("Возвращаю меню: " + string.Join(",", _menus));
            return _menus;
        }

        public void ClickMenu(string menu)
        {
            RaiseLogger("Вызов обработчика для меню " + menu);
            switch (menu)
            {
                case ABOUT_MENU:
                    ShowAbout();
                    break;
                case SETTINGS_MENU:
                    ShowSettings();
                    break;
                default:
                    RaiseLogger("Меню с имененем " + menu + " не найдено");
                    break;
            }
        }

        public void Dispose()
        {
            RaiseLogger("Уничтожение плагина");
            ThisClass = null;
            
        }
    }
}
