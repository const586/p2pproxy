using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using XmlSettings;

namespace PluginFavourites
{
    public partial class FormSettings : Form
    {
        private Settings _set;
        public FormSettings()
        {
            InitializeComponent();
            _set = new Settings(Plugin.SelfPath + "/pluginfavourites.xml");
            textBox1.Text = _set.GetValue("settings", "filepath") ?? "";
            string ytq = _set.GetValue("settings", "youtubequality") ?? "1280";
            int index = comboBox1.Items.IndexOf(ytq);
            comboBox1.SelectedIndex =  index < 0 ? comboBox1.Items.IndexOf("1280") : index;
            textBox2.Text = _set.GetValue("settings", "youtubeplaylist") ?? "";
            textBox3.Text = _set.GetValue("settings", "torrentpath") ?? "";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = textBox1.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dialog.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _set.SetValue("settings", "filepath", textBox1.Text);
            _set.SetValue("settings", "youtubequality", (string)comboBox1.SelectedItem);
            _set.SetValue("settings", "youtubeplaylist", textBox2.Text);
            _set.SetValue("settings", "torrentpath", textBox3.Text);
            Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = textBox3.Text;
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = dialog.SelectedPath;
            }
        }
    }
}
