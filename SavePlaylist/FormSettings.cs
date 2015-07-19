using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SavePlaylist
{
    public partial class FormSettings : Form
    {
        public FormSettings(List<Menu> menus)
        {
            InitializeComponent();
            BindingSource source = new BindingSource();
            source.DataSource = menus;
            dataGridView1.DataSource = source;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
