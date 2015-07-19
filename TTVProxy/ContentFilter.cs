using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using P2pProxy;

namespace P2pProxy
{
    public class ContentFilter
    {
        private string id;
        private List<ContentFilter> _filters;
        private bool _check = false;

        public ContentFilter(string id) : this()
        {
            this.id = id;
            _check = true;
        }

        public ContentFilter()
        {
            _check = false;
            _filters = new List<ContentFilter>();
        }

        public ContentFilter Add(string id)
        {
            ContentFilter filter = new ContentFilter(id);
            _filters.Add(filter);
            return filter;
        }

        public ContentFilter Find(string id)
        {
            ContentFilter res = new ContentFilter();
            foreach (var f in _filters)
            {
                if (res.Check())
                    break;
                if (f.id == id)
                {
                    res = f;
                    break;
                }
                else 
                {
                    res = f.Find(id);
                }
            }
            return res;
        }

        private static ContentFilter empty = new ContentFilter();

        public ContentFilter Check(string id)
        {
            if (_check == false) return empty;
            return _filters.FirstOrDefault(c => c.id == id) ?? empty;
        }

        public bool Check()
        {
            return _check;
        }

        public static ContentFilter Load()
        {
            string path = Path.Combine(P2pProxyApp.ApplicationDataFolder, "filter.xml");
            if (!File.Exists(path))
                return new ContentFilter("root");
            XDocument xd = XDocument.Load(path);
            var xprovs = xd.Root.Elements();
            ContentFilter root = new ContentFilter("root");
            foreach (var xp in xprovs)
            {
                root._filters.Add(LoadFilter(xp));
            }
            return root;
        }

        private static ContentFilter LoadFilter(XElement xe)
        {
            ContentFilter root = new ContentFilter(xe.Attribute("id").Value);
            foreach (var x in xe.Elements())
            {
                root._filters.Add(LoadFilter(x));
            }
            return root;
        }

        internal bool HasChild()
        {
            return _filters.Any() || _check == false;
        }
    }
}
