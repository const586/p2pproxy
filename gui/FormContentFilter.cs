using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using P2pProxy;
using PluginProxy;

namespace gui
{
    public partial class FormContentFilter : Form
    {
        private P2pProxyApp proxy;
        public FormContentFilter(P2pProxyApp proxy)
        {
            this.proxy = proxy;
            InitializeComponent();
        }

        private void FormContentFilter_Load(object sender, EventArgs e)
        {
            var ttv = treeView1.Nodes.Add("ttv", "Каналы");
            ttv.Tag = "ttv";
            ttv.Nodes.Add("");
            ThreadPool.QueueUserWorkItem(state =>
            {
                if (proxy.Device == null)
                    return;
                var plugs = proxy.Device.PluginProvider.GetPlugins();
                foreach (var plugin in plugs)
                {
                    var container = plugin.GetContent(null) as IPluginContainer;
                    if (container == null)
                        continue;
                    treeView1.BeginInvoke(new Action(() =>
                    {
                        var node = treeView1.Nodes.Add(plugin.Id, plugin.Name);
                        node.Tag = plugin;
                        node.Nodes.Add("");
                    }));
                }
            });
            Deserialize();
        }

        private void treeView1_AfterExpand(object sender, TreeViewEventArgs e)
        {
            
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                IPluginContainer root = null;
                
                if (e.Node.Tag.Equals("ttv"))
                {
                    treeView1.BeginInvoke(new Action(() => e.Node.Nodes.Clear()));
                    var cats = proxy.Device.ChannelsProvider.GetCategories();
                    foreach (var cat in cats)
                    {
                        treeView1.BeginInvoke(new Action(()=> 
                            {
                                var node = e.Node.Nodes.Add("cat" + cat.id, cat.name);
                                node.Tag = cat;
                                foreach (var ch in cat.Channels)
                                {
                                    var chnode = node.Nodes.Add("ch" + ch.id, ch.name);
                                    chnode.Tag = ch;
                                }
                            }));
                        
                    }
                }
                if (e.Node.Tag is IPluginProxy)
                    root = (e.Node.Tag as IPluginProxy).GetContent(null) as IPluginContainer;
                else if (e.Node.Tag is IPluginContainer)
                    root = (e.Node.Tag as IPluginContainer);

                if (root == null)
                    return;

                var contents = root.Children;
                treeView1.BeginInvoke(new Action(() => e.Node.Nodes.Clear()));
                foreach (var content in contents)
                {
                    treeView1.BeginInvoke(new Action(() =>
                    {
                        var node = e.Node.Nodes.Add(content.Id, content.Title);
                        node.Tag = content;
                        if (content is IPluginContainer)
                            node.Nodes.Add("");
                    }));
                }
            });
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            Add(e.Node);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            XElement root = new XElement("filter");
            foreach (TreeNode node in treeView2.Nodes)
            {
                var f = proxy.Device.Filter.Add(node.Name);
                root.Add(Serialize(node, f));
                
            }
            XDocument xd = new XDocument();
            xd.Add(root);
            using (var f = File.Open(P2pProxyApp.ApplicationDataFolder + "/filter.xml", FileMode.Create, FileAccess.Write))
            {
                xd.Save(f);
                f.Close();
            }
            proxy.Device.UpdateFilter();
            Close();
        }

        private XElement Serialize(TreeNode node, ContentFilter filter)
        {
            XElement elem = new XElement(node.Nodes.Count > 0 ? "group" : "item", new XAttribute("id", node.Name), new XAttribute("name", node.Text));
            foreach (TreeNode child in node.Nodes)
            {
                var f = filter.Add(child.Name);
                elem.Add(Serialize(child, filter));
                
            }
            return elem;
        }

        private void Deserialize()
        {
            if (!File.Exists(P2pProxyApp.ApplicationDataFolder + "/filter.xml"))
                return;
            using (var f = File.OpenRead(P2pProxyApp.ApplicationDataFolder + "/filter.xml"))
            {
                XDocument xd = XDocument.Load(f);
                var root = xd.Element("filter");
                if (root == null)
                    return;
                var list = root.Elements("group");
                foreach (var xplug in list)
                    treeView2.Nodes.Add(DeserializeNode(xplug));
                f.Close();
            }
        }

        private TreeNode DeserializeNode(XElement root)
        {
            TreeNode node = new TreeNode();
            node.Name = root.Attribute("id").Value;
            node.Text = root.Attribute("name").Value;
            if (root.Name == "group")
            {
                foreach (var xchild in root.Elements())
                    node.Nodes.Add(DeserializeNode(xchild));
            }
            return node;
        }

        private void treeView2_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode != null)
                Add(treeView1.SelectedNode);
        }

        public void Add(TreeNode node)
        {
            if (node.Tag is IPluginProxy)
                return;
            List<TreeNode> tree = new List<TreeNode>();
            tree.Add(node);
            TreeNode parent = node.Parent;
            while (parent != null)
            {
                tree.Insert(0, parent);
                parent = parent.Parent;
            }
            TreeNode root = null;
            foreach (var nod in tree)
            {
                if (root == null)
                {
                    if (treeView2.Nodes.ContainsKey(nod.Name))
                        root = treeView2.Nodes[nod.Name];
                    else
                        root = treeView2.Nodes.Add(nod.Name, nod.Text);
                    continue;
                }
                if (!root.Nodes.ContainsKey(nod.Name))
                {
                    root = root.Nodes.Add(nod.Name, nod.Text);
                    continue;
                }
                else
                    root = root.Nodes[nod.Name];
            }
        }

        public void Del(TreeNode node)
        {
            if (node.Parent == null)
                treeView1.Nodes.Remove(node);
            else node.Parent.Nodes.Remove(node);
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (treeView2.SelectedNode != null)
                Del(treeView2.SelectedNode);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
