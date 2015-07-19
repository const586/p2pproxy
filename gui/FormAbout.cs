using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Windows.Forms;
using CryptoLibrary;
using TTVApi;
using P2pProxy;

namespace gui
{
    public partial class FormAbout : Form
    {
        private P2pProxyApp _app;
        public FormAbout(P2pProxyApp app, Auth state)
        {
            InitializeComponent();
            _app = app;
            labelVersion.Text = labelVersion.Text + Application.ProductVersion;
            labelUser.Text = labelUser.Text + state.Login;
            labelBalance.Text = labelBalance.Text + state.balance + "р.";
            textUdn.Text = _app.Device.Udn.ToString();
        }


        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void linkPremium_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(String.Format("http://localhost:{0}/pay", _app.Device.Web.Port));
        }
    }
}
