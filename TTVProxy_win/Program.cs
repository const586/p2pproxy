using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using P2pProxy.Broadcasting.VLC;
using PluginProxy;
using SimpleLogger;
using P2pProxy;
using gui;
using System.Linq;
using Timer = System.Windows.Forms.Timer;

namespace P2pProxy_win
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var context = new AppContext();
            Application.EnableVisualStyles();
            Application.Run(context);
        }
    }

    public class AppContext : ApplicationContext
    {
        private readonly NotifyIcon _ni;
        public static readonly P2pProxyApp App = new P2pProxyApp();

        public AppContext()
        {
            _ni = new NotifyIcon
                        {
                            Text = "P2pProxy",
                            Visible = true,
                            Icon = Icon.FromHandle(gui.Properties.Resources.biglogo.GetHicon()),
                            //Icon = new Icon("stream.ico")
                            //ContextMenu = new ContextMenu()
                        };

            App.Notifyed += AppOnNotifyed;
            _ni.DoubleClick += MenuOptionOnClick;
            _ni.ContextMenuStrip = new ContextMenuStrip();
            _ni.ContextMenuStrip.Items.Add("Настройки", gui.Properties.Resources.settings.ToBitmap(),
                                            MenuOptionOnClick);
            _ni.ContextMenuStrip.Items.Add("Фильтр", null, FilterOnClick);
            _ni.ContextMenuStrip.Items.Add("Записи",
                                            gui.Properties.Resources.recsystem.ToBitmap(), OnShowRecords);

            var plugsmnu = new ToolStripMenuItem("Плагины");
            _ni.ContextMenuStrip.Items.Add(plugsmnu);

            var helpmnu = new ToolStripMenuItem("Справка");
            //helpmnu.DropDownItems.Add("Помощь", null, ShowHelp);
            helpmnu.DropDownItems.Add("Список комманд", null, ShowCommands);
            helpmnu.DropDownItems.Add("Статистика", null, ShowStatistic);
            helpmnu.DropDownItems.Add("О программе", null, AboutClick);
            _ni.ContextMenuStrip.Items.Add(helpmnu);
                
            _ni.ContextMenuStrip.Items.Add("-");
            _ni.ContextMenuStrip.Items.Add("Выход", gui.Properties.Resources.exit.ToBitmap(),
                                            MenuExitOnClick);
            App.Start();
            
            Application.ThreadExit += Application_ThreadExit;
            if (!App.IsWorking)
                return;
            var plugs = App.Device.PluginProvider.GetPlugins();
            foreach (var plug in plugs)
            {
                var menus = plug.GetMenus();
                if (menus == null)
                    continue;
                var enumerable = menus as string[] ?? menus.ToArray();
                if (menus != null && enumerable.Any())
                {
                    var plugmnu = new ToolStripMenuItem(plug.Name) {Tag = plug};
                    plugsmnu.DropDownItems.Add(plugmnu);
                    foreach (var menu in enumerable)
                    {
                        var menuitem = new ToolStripMenuItem(menu) {Tag = menu};
                        menuitem.Click += PluginMenuClick;
                        plugmnu.DropDownItems.Add(menuitem);
                    }
                }
            }
        }

        private void FilterOnClick(object sender, EventArgs eventArgs)
        {
            new FormContentFilter(App).Show();
        }

        void Application_ThreadExit(object sender, EventArgs e)
        {
            if (App.IsWorking)
                MenuExitOnClick(sender, e);
        }

        

        private void ShowCommands(object sender, EventArgs eventArgs)
        {
            Process.Start("http://127.0.0.1:" + App.Device.Web.Port + "/help");
        }

        private void ShowStatistic(object sender, EventArgs eventArgs)
        {
            Process.Start("http://127.0.0.1:" + App.Device.Web.Port + "/stat");
        }

        private void PluginMenuClick(object sender, EventArgs eventArgs)
        {
            var menu = (ToolStripMenuItem) sender;
            var plug = (IPluginProxy) menu.OwnerItem.Tag;
            if (plug == null || string.IsNullOrEmpty((string)menu.Tag))
                return;
            try
            {
                plug.ClickMenu((string) menu.Tag);
            }
            catch (Exception e)
            {
                MessageBox.Show("В плагине " + plug.Id + " произошла ошибка: " + e.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AboutClick(object sender, EventArgs eventArgs)
        {
            new FormAbout(App, App.SessionState).Show();
        }

        private void OnShowRecords(object sender, EventArgs eventArgs)
        {
            new FormRecords(App).Show();
        }


        private void AppOnNotifyed(string text, TypeMessage typemsg)
        {
            ToolTipIcon icon;
            switch (typemsg)
            {
                case TypeMessage.Critical: 
                case TypeMessage.Error:
                    icon = ToolTipIcon.Error;
                    break;
                case TypeMessage.Info:
                    icon = ToolTipIcon.Info;
                    break;
                case TypeMessage.Warning:
                    icon = ToolTipIcon.Warning;
                    break;
                default:
                    icon = ToolTipIcon.None;
                    break;
            }
            _ni.ShowBalloonTip(3000, "P2pProxy Proxy", text, icon);
        }

        private void MenuOptionOnClick(object sender, EventArgs e)
        {
            if (!FormOption.Opened) new FormOption(App).Show();
        }

        private void MenuExitOnClick(object sender, EventArgs e)
        {
            _ni.Visible = false;
            App.Close();
            ExitThread();

        }

        public void GlobalException(object sender, ThreadExceptionEventArgs args)
        {
            if (P2pProxyApp.Log != null)
                P2pProxyApp.Log.Write(args.Exception.Message, TypeMessage.Error);
            if (args.Exception is VlcConnectError)
                MessageBox.Show("Ошибка подключения к VLC", "Ошибка", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
        }
    }
}
