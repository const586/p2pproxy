using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml.Linq;
using P2pProxy;
using TTVApi;

namespace gui
{
    public partial class FormChannels : Form
    {
        public delegate void OnChannelChoosed(Telecast epg, Channel ch);

        public event OnChannelChoosed Choosed;
        private P2pProxyApp _proxy;

        public FormChannels(P2pProxyApp app)
        {
            _proxy = app;
            InitializeComponent();
        }

        protected virtual void RaisedChoosed(Telecast epg, Channel ch)
        {
            if (Choosed != null)
            {
                Choosed(epg, ch);
            }
        }

        private void FormChannels_Load(object sender, EventArgs e)
        {
            var playlist = _proxy.Device.ChannelsProvider.GetChannels().GroupBy(x => x.group).ToDictionary(x => x.Key, x => x.ToList());
            var categories = _proxy.Device.ChannelsProvider.GetCategories();
            foreach (var cat in categories)
            {
                if (treeView1 != null)
                {
                    TreeNode tr = new TreeNode("cat" + cat.id) {Tag = cat, Text = cat.name};
                    treeView1.Nodes.Add(tr);
                    if (!playlist.ContainsKey(cat.id))
                        continue;
                    foreach (var ch in playlist[cat.id])
                    {
                        TreeNode trch = new TreeNode("ch" + ch.id) {Tag = ch, Text = ch.name};
                        tr.Nodes.Add(trch);
                    }
                }
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.GetNodeCount(true) == 0 && e.Node.Level == 0 && treeView1.SelectedNode != null)
            {
                
            }
            else if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag is Channel)
            {
                FeelEpgForChannel((Channel)treeView1.SelectedNode.Tag, treeView1.SelectedNode);

            }
            else if (treeView1.SelectedNode != null && treeView1.SelectedNode.Tag is Telecast)
            {
                RaisedChoosed((Telecast)treeView1.SelectedNode.Tag, (Channel) treeView1.SelectedNode.Parent.Tag);
            }
        }

        private void FeelEpgForChannel(Channel channel, TreeNode parent)
        {
            var res = _proxy.Device.RecordsProvider.GetListOfEpg(channel.epg_id);
            if (res.Count == 0) 
            {

                var curepg = new Telecast
                                    {
                                        name = "Весь эфир",
                                        StartTime = DateTime.Now,
                                        EndTime = DateTime.Today.AddDays(1).AddTicks(-1),
                                        epg_id = 0
                                    };
                TreeNode node = new TreeNode(String.Format("{0} - {1} {2}", curepg.StartTime.ToShortTimeString(), curepg.EndTime.ToShortTimeString(), curepg.name))
                                    {Tag = curepg};

                treeView1.Invoke(new MethodInvoker(() => parent.Nodes.Add(node)));
            }
            else
            {
                foreach (var curepg in res.Where(e => e.EndTime >= DateTime.Now))
                {
                    TreeNode node = new TreeNode(String.Format("{0} - {1} {2}", curepg.StartTime.ToShortTimeString(), curepg.EndTime.ToShortTimeString(), curepg.name))
                                        {Tag = curepg};
                    parent.Nodes.Add(node);
                }
            }
            treeView1.Invoke(new MethodInvoker(parent.Toggle));
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }
    }
}
