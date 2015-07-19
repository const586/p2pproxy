using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CryptoLibrary;
using P2pProxy.UPNP;
using P2pProxy;
using System.Linq;
using P2pProxy.Broadcasting.VLC;

namespace gui
{
    public partial class FormOption : Form
    {
        public static bool Opened { get; private set; }
        private bool relogin;
        private int webport;
        private int vlcport;
        private int vlccache;
        private int vlcmuxcache;
        private readonly P2pProxyApp _app;

        public FormOption(P2pProxyApp app)
        {
            InitializeComponent();
            _app = app;
            Icon = Icon.FromHandle(Properties.Resources.biglogo.GetHicon());
        }

        private void FormOption_Load(object sender, EventArgs e)
        {
            relogin = false;
            Text = "Настройки";
            tabPageAuth.Text = "Регистрационные данные";
            tabPageSystem.Text = "Системные";
            labelLogin.Text = "Пользователь";
            labelPassword.Text = "Пароль";
            labelWebPort.Text = "HTTP-порт";
            record_path.Text = "Папка для записей";
            lblVlcCache.Text = "Live-кэш (c.)";
            lblVlcMux.Text = "Стартовый кэшь";
            lblVlcPort.Text = "Порт";
            chkExtVlc.Text = "Использовать внешний VLC";
            as_timeout.Text = "Таймаут AceStream";
            lblVlcRtspPort.Text = "RTSP-порт";
            
            textBoxLogin.Text = P2pProxyApp.MySettings.GetSetting("torrent-tv.ru", "login", "anonymous");
            textBoxHttpPort.Text = P2pProxyApp.MySettings.GetSetting("web", "port", 8081).ToString();
            string pass = String.Empty;
            try
            {
                pass = CryptoHelper.Decrypt<System.Security.Cryptography.AesCryptoServiceProvider>(
                    P2pProxyApp.MySettings.GetSetting("torrent-tv.ru", "password", "anonymous"), Environment.MachineName, "_Cr[e?g1");
            }
            catch { }
            if (string.IsNullOrEmpty(pass))
            {
                pass = "anonymous";
                P2pProxyApp.MySettings.GetSetting("torrent-tv.ru", "login", "anonymous");
            }
            textBoxPassword.Text = pass;
            buttonCancel.Text = "Выход";
            buttonOK.Text = "Сохранить";
            Opened = true;
            string recpath = P2pProxyApp.MySettings.GetSetting("records", "path", new Uri(P2pProxyApp.ApplicationDataFolder + "/records").LocalPath);
            textRecordPath.Text = recpath;
            vlcport = P2pProxyApp.MySettings.GetSetting("vlc", "vlcport", 4212);
            txtVlcPort.Text = vlcport.ToString();
            vlccache = P2pProxyApp.MySettings.GetSetting("vlc", "vlccache", 5000);
            txtVlcCache.Text = vlccache.ToString();
            vlcmuxcache = P2pProxyApp.MySettings.GetSetting("vlc", "vlcmuxcache", 0);
            txtVlcMux.Text = vlcmuxcache.ToString();
            string vlcpath = P2pProxyApp.MySettings.GetSetting("vlc", "vlcpath");
            if (string.IsNullOrEmpty(vlcpath))
                chkExtVlc.Checked = false;
            else
            {
                txtExtVlcPath.Text = vlcpath;
                chkExtVlc.Checked = P2pProxyApp.MySettings.GetSetting("vlc", "vlcext", false);
            }

            txtVlcRtspPort.Text = P2pProxyApp.MySettings.GetSetting("vlc", "rtspport", 5554).ToString();

            if (_app != null)
            {
                var broadcaster = _app.Broadcaster as VlcBroadcaster;
                if (broadcaster != null)
                {
                    comboTranscode.Items.AddRange(broadcaster.GetTranscodes().Select(t => t.Name).ToArray());
                }
            }

            int astimeout = P2pProxyApp.MySettings.GetSetting("system", "as_timeout", 60);
            txtAsTimeOut.Text = astimeout.ToString();

            txtMaxAge.Text = P2pProxyApp.MySettings.GetSetting("dlna", "max-age", _app.Device.MaxAge.ToString());
            txtUpnpPort.Text = P2pProxyApp.MySettings.GetSetting("dlna", "port", "1900");
            txtUdn.Text = P2pProxyApp.MySettings.GetSetting("dlna", "udn", Guid.NewGuid().ToString());

            BindingSource bsource = new BindingSource();
            var profiles = UpnpSettingManager.GetProfiles(); ;
            bsource.DataSource = profiles;
            cbProfile.DataSource = bsource;
            cbProfile.DisplayMember = "Value";
            cbProfile.ValueMember = "Key";

            string profile = P2pProxyApp.MySettings.GetSetting("dlna", "profile", "default");
            cbProfile.SelectedItem = profiles.ContainsKey(profile) ? profiles.First(p => p.Key == profile) : profiles.First(p => p.Key == "default");
            txtLabelDlna.Text = P2pProxyApp.MySettings.GetSetting("dlna", "label", "P2pProxy");

            chkAutoUpdate.Checked = P2pProxyApp.MySettings.GetSetting("system", "update", true);
            chckDlnaEnabled.Checked = P2pProxyApp.MySettings.GetSetting("dlna", "enable", true);
            chkUseVLC.Checked = P2pProxyApp.MySettings.GetSetting("system", "usevlc", true);
        }

        
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            if (textBoxLogin.Text == String.Empty)
                textBoxLogin.Text = "anonymous";

            P2pProxyApp.MySettings.SetSetting("torrent-tv.ru", "password", CryptoHelper.Encrypt<System.Security.Cryptography.AesCryptoServiceProvider>(textBoxPassword.Text, Environment.MachineName, "_Cr[e?g1"));
            P2pProxyApp.MySettings.SetSetting("torrent-tv.ru", "login", textBoxLogin.Text);
            P2pProxyApp.MySettings.SetSetting("web", "port", textBoxHttpPort.Text);
            P2pProxyApp.MySettings.SetSetting("records", "path", textRecordPath.Text);
            P2pProxyApp.MySettings.SetSetting("vlc", "vlcport", txtVlcPort.Text);
            P2pProxyApp.MySettings.SetSetting("vlc", "vlcmuxcache", txtVlcMux.Text);
            P2pProxyApp.MySettings.SetSetting("vlc", "vlccache", txtVlcCache.Text);
            P2pProxyApp.MySettings.SetSetting("vlc", "vlcext", chkExtVlc.Checked.ToString());
            P2pProxyApp.MySettings.SetSetting("vlc", "vlcpath", txtExtVlcPath.Text);
            P2pProxyApp.MySettings.SetSetting("vlc", "rtspport", txtVlcRtspPort.Text);
            P2pProxyApp.MySettings.SetSetting("system", "as_timeout", txtAsTimeOut.Text);
            P2pProxyApp.MySettings.SetSetting("system", "update", chkAutoUpdate.Checked.ToString());
            P2pProxyApp.MySettings.SetSetting("system", "usevlc", chkUseVLC.Checked);
            P2pProxyApp.MySettings.SetSetting("dlna", "profile", ((KeyValuePair<string, string>)cbProfile.SelectedItem).Key);
            P2pProxyApp.MySettings.SetSetting("dlna", "label", txtLabelDlna.Text);
            P2pProxyApp.MySettings.SetSetting("dlna", "enable", chckDlnaEnabled.Checked.ToString());
            P2pProxyApp.MySettings.SetSetting("dlna", "udn", txtUdn.Text);
            P2pProxyApp.MySettings.SetSetting("dlna", "max-age", txtMaxAge.Text);
            if (relogin)
            {
                if (_app.Login())
                    Close();
            }
            else
                Close();

        }

        private void FormOption_FormClosed(object sender, FormClosedEventArgs e)
        {
            Opened = false;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsNumber(e.KeyChar) || e.KeyChar == '\b')
                return;
            e.KeyChar = (char)0;
            
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box == null) return;
            switch (box.Name)
            {
                case "textBoxHttpPort":
                    if (int.Parse(box.Text) > ushort.MaxValue - 1 || box.Text == "")
                        box.Text = webport.ToString();
                    break;
                case "txtVlcPort":
                    if (int.Parse(box.Text) > ushort.MaxValue - 1 || box.Text == "")
                        box.Text = vlcport.ToString();
                    break;
                case "txtVlcCache":
                    if (int.Parse(box.Text) > 60000 || box.Text == "")
                        box.Text = "60000";
                    break;
                case "txtVlcMux":
                    if (string.IsNullOrEmpty(box.Text))
                        box.Text = vlcmuxcache.ToString();
                    break;
                case "txtAsTimeoute":
                    if (int.Parse(txtAsTimeOut.Text) > ushort.MaxValue - 1 || txtAsTimeOut.Text == "")
                        txtAsTimeOut.Text = "60";
                    break;
                case "txtVlcRtspPort":
                    if (int.Parse(box.Text) > ushort.MaxValue - 1 || box.Text == "")
                        box.Text = "5554";
                    break;
                case "txtExtVlcPath":
                    if (box.Text == "" || !File.Exists(box.Text))
                        chkExtVlc.Checked = false;
                    break;
            }
        }

        private void btnSetRecordPath_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textRecordPath.Text = dialog.SelectedPath;
            }
        }

        private void textBoxPassword_TextChanged(object sender, EventArgs e)
        {
            relogin = true;
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void chkExtVlc_CheckedChanged(object sender, EventArgs e)
        {
            if (chkExtVlc.Checked && !File.Exists(txtExtVlcPath.Text))
            {
                MessageBox.Show("VLC не найден");
                chkExtVlc.Checked = false;
            }
        }

        private void btnChooseVlcPath_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog {Filter = "EXE|*.exe"};
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                txtExtVlcPath.Text = dialog.FileName;
                chkExtVlc.Checked = true;
            }
        }

        private void btnTransEdit_Click(object sender, EventArgs e)
        {
            var broadcaster = _app.Broadcaster as VlcBroadcaster;
            if (broadcaster == null)
                return;
            var trans = broadcaster.GetTranscodes().FirstOrDefault(t => t.Name == comboTranscode.Text);
            if (trans == null)
            {
                MessageBox.Show("Профиль транскодирования с таким именем не найден", "Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            var form = new FormTranscode(trans);
            form.Closed += (o, args) =>
            {
                if (form.DialogResult == DialogResult.OK)
                    broadcaster.SaveTranscodes();
            };
            form.ShowDialog();
        }

        private void btnTransAdd_Click(object sender, EventArgs e)
        {
            var broadcaster = _app.Broadcaster as VlcBroadcaster;
            if (broadcaster == null)
                return;
            Transcode trans = new Transcode("Новый профиль");
            var form = new FormTranscode(trans);
            form.Closed += (o, args) =>
            {
                if (form.DialogResult == DialogResult.OK)
                {
                    broadcaster.AddTranscode(trans);
                    broadcaster.SaveTranscodes();
                    comboTranscode.Items.Add(trans.Name);
                    comboTranscode.SelectedItem = trans.Name;
                }
            };
            form.ShowDialog();
        }

        private void btnDelTranscode_Click(object sender, EventArgs e)
        {
            var broadcaster = _app.Broadcaster as VlcBroadcaster;
            if (broadcaster == null)
                return;
            var trans = broadcaster.GetTranscodes().FirstOrDefault(t => t.Name == (string)comboTranscode.SelectedItem);
            if (trans == null)
            {
                MessageBox.Show("Профиль транскодирования с таким именем не найден", "Ошибка", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            broadcaster.RemoveTranscode(trans);
            broadcaster.SaveTranscodes();
            comboTranscode.Items.Remove(comboTranscode.SelectedItem);

        }

        private void chkUseVLC_CheckedChanged(object sender, EventArgs e)
        {
            txtVlcCache.Enabled = txtVlcMux.Enabled = txtVlcPort.Enabled =
                txtVlcRtspPort.Enabled = comboTranscode.Enabled = btnTransAdd.Enabled = 
                btnTransEdit.Enabled = btnDelTranscode.Enabled = chkExtVlc.Enabled = txtExtVlcPath.Enabled = 
                btnChooseVlcPath.Enabled = chkUseVLC.Checked;
        }


    }
}
