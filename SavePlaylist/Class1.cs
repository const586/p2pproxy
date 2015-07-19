using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using PluginProxy;

namespace SavePlaylist
{
    public class Class1 : IPluginProxy
    {
        public string Id { get { return "playlists"; }}
        public string Name { get { return "Плейлисты"; } }
        public event LoggerCallback Logger;
        private List<Menu> Menus;

        private string SelfPath;
        private string host;

        public void Init(string host)
        {
            this.host = host;
            SelfPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Menus = new List<Menu>();
            if (File.Exists(SelfPath + "/menus.dat"))
            {
                FileStream f = File.OpenRead(SelfPath + "/menus.dat");
                XmlSerializer serial = new XmlSerializer(typeof(List<Menu>));
                Menus = (List<Menu>) serial.Deserialize(f);
                f.Close();
            }
            RaiseLogger("Плагин инициализирован");
        }

        public IEnumerable<string> GetRouteUrls()
        {
            return new List<string> {"about"};
        }

        public IRequestData HttpRequest(string path, Dictionary<string, string> parameters)
        {
            RaiseLogger("Обработка запроса " + path);
            switch (path)
            {
                case "about":
                    return SendAbout();
            }
            return new ResponseData();
        }

        private IRequestData SendAbout()
        {
            RaiseLogger("Показываю справку");
            ResponseData result =new ResponseData();
            result.ResultState = 200;
            result.Stream = new MemoryStream();
            var bres = Encoding.UTF8.GetBytes("Плагин предоставляет интерфейс для сохранения плейлистов в файл");
            result.Stream.Write(bres, 0, bres.Length);
            result.Stream.Position = 0;
            result.Headers.Add("Content-Length", bres.ToString());
            result.Headers.Add("Content-Type", "text/plain;charset=utf-8");
            return result;
        }

        public IPluginContent GetContent(Dictionary<string, string> parameters)
        {
            return null;
        }

        public IEnumerable<string> GetMenus()
        {
            List<string> menus = new List<string>();
            menus.Add("Настройки");
            menus.Add("О плагине");
            menus.AddRange(Menus.Select(menu => menu.Name));
            RaiseLogger("Формирование меню плагина:" + string.Join(",", menus));
            return menus;
        }

        public void ClickMenu(string menu)
        {
            if (string.IsNullOrEmpty(menu))
                return;
            RaiseLogger("Обработка нажатия на меню " + menu);
            switch (menu)
            {
                case "Настройки":
                    ShowOptions();
                    break;
                case "О плагине":
                    ShowAbout();
                    break;
                default:
                    SaveUrlResponse(menu);
                    break;
            }
        }

        private void SaveUrlResponse(string menu)
        {
            if (Menus.Exists(menu1 => menu1.Name == menu))
            {
                RaiseLogger("Открытие плейлиста " + menu);
                Menu _menu = Menus.First(menu1 => menu1.Name == menu);
                if (_menu.BeSave)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        FileStream f = File.Create(dialog.FileName);
                        WebRequest.Create(_menu.Url).GetResponse().GetResponseStream().CopyTo(f);
                        f.Close();
                    }
                }
                else
                    Process.Start(_menu.Url);
                
            }
            else
                RaiseLogger("Сохраненный плейлист " + menu + " не найден");
        }

        private void ShowAbout()
        {
            Process.Start(host + "/about");
        }

        private void ShowOptions()
        {
            RaiseLogger("Открытие настроек");
            using (FormSettings form = new FormSettings(Menus))
            {
                form.ShowDialog();
                XmlSerializer serial = new XmlSerializer(typeof (List<Menu>));
                if (Menus.Count == 0)
                    File.Delete(SelfPath + "/menus.dat");
                else
                {
                    using (FileStream f = File.Create(SelfPath + "/menus.dat"))
                    {
                        serial.Serialize(f, Menus);
                        f.Close();
                    }
                }
            }
            RaiseLogger("Настройки сохранены");
        }

        void RaiseLogger(string message)
        {
            if (Logger != null)
                Logger(this, message);
        }

        public void Dispose()
        {
            RaiseLogger("Плагин уничтожен");
        }
    }
    [Serializable]
    public class Menu
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool BeSave { get; set; }
    }

    public class ResponseData : IRequestData
    {
        public Dictionary<string, string> Headers { get; private set; }
        public Stream Stream;
        public Stream GetStream()
        {
            return Stream;
        }

        public ResponseData()
        {
            Stream = Stream.Null;
            Headers = new Dictionary<string, string>();
            ResultState = 404;
        }

        public ushort ResultState { get; set; }
    }
}
