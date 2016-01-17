using System;
using System.Xml;
using System.Collections.Generic;
using System.Net;

namespace Launcher
{
    public class Archive
    {
        private List<ArchiveFile> _files;
        private string _path;
        private int _size;

        public string Path {
            get { return _path; }
        }

        public int Size {
            get {
                if (_size == 0)
                {
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(URL);
                    req.Method = "HEAD";

                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    {
                        _size = Int32.Parse(resp.Headers["Content-Length"]);
                    }
                }

                return _size;
            }
        }

        public List<ArchiveFile> Files {
            get { return _files; }
        }

        public string URL {
            get { return Configuration.Instance.UrlTo(_path); }
        }

        public Archive(XmlNode parent)
        {
            _files = new List<ArchiveFile>();

            foreach (XmlNode node in parent.ChildNodes)
            {
                if (node.Name.Equals("path"))
                {
                    _path = node.FirstChild.Value;
                }
                else if (node.Name.Equals("file"))
                {
                    _files.Add(new ArchiveFile(node));
                }
            }
        }

        public bool Validate()
        {
            bool valid = true;

            foreach (ArchiveFile af in _files)
            {
                if (!af.Valid)
                {
                    valid = false;
                }
            }

            return valid;
        }
    }
}

