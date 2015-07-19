using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using PluginProxy;
using XmlSettings;

namespace PluginFavourites
{
    class LocalFileContentProvider : IPluginContainer
    {
        private IPluginContainer _parent;
        private string files_path = "";
        private string _id = "files";
        private string _title = "Локальные файлы";
        private string _url = "";
        public string Id { get { return _id; }}
        public string Title {
            get { return _title; }
        }

        public string Icon { get; private set; }
        public PluginMediaType PluginMediaType { get { return PluginMediaType.Video; }}
        public IPluginContainer Parent { get { return _parent; }}
        public string Url { get { return _url; }}

        public string GetUrl(string host)
        {
            return Url;
        }

        public TranslationType Translation { get; private set; }
        public SourceUrl GetSourceUrl()
        {
            return new SourceUrl();
        }

        public IEnumerable<IPluginContent> Children { get { return GetContent(); }}
        public IEnumerable<IPluginContent> OrderBy(string field)
        {
            return Children;
        }

        public bool CanSorted { get { return true; } }

        public LocalFileContentProvider(IPluginContainer parent)
        {
            var settings = new Settings(Plugin.SelfPath + "/pluginfavourites.xml");
            files_path = settings.GetValue("settings", "filepath") ?? "";
            _parent = parent;
        }

        public LocalFileContentProvider(string path, IPluginContainer parent)
        {
            files_path = path;
            _parent = parent;
            var dir = new DirectoryInfo(path);
            _id = dir.Name.Replace("_", "").Replace(" ", "").ToLower();
            if (parent is LocalFileContentProvider)
                _id = parent.Id + "_" + _id;
            _title = dir.Name;
        }

        public IPluginContent GetChildContent(string id)
        {
            var content = Children;
            var ids = id.Split("_".ToCharArray());
            for (int i = 2; i < ids.Length; i++)
            {
                var cont = content.First(c => c.Id.ToLower() == string.Join("_", ids.Take(i)));
                if (cont is Item)
                    return cont;
                else
                    content = (cont as LocalFileContentProvider).Children;
            }
            return content.First(c => c.Id == id);
        }

        private IEnumerable<IPluginContent> GetContent()
        {
            if (!Directory.Exists(files_path))
                return null;
            var topdir = new DirectoryInfo(files_path);
            List<IPluginContent> res = new List<IPluginContent>();
            res.AddRange(topdir.GetDirectories().Select(dir => new LocalFileContentProvider(dir.FullName, this)));
            string[] exts = {".mpg", ".avi", ".mp4", ".mkv", ".flv"};
            res.AddRange(topdir.EnumerateFiles().Where(f => exts.Contains(f.Extension)).Select(file => new Item(file.Name.Replace("_", "").Replace(" ", ""), Path.GetFileNameWithoutExtension(file.Name), PluginMediaType.Video, this, TranslationType.VoD, file.FullName)));
            return res;
        }
    }
}
