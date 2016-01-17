using System;
using System.Collections.Generic;
using System.Xml;

namespace Launcher
{
    public class Package {

        private string _name;
        private string _etag;
        private List<Archive> _archives;

        public string Url {
            get { return Configuration.Instance.UrlTo(_name + ".xml"); }
        }

        public string Name {
            get { return _name; }
        }

        public string ETag {
            get { return _etag; }
            set { _etag = value; }
        }

        public List<Archive> Archives {
            get { return _archives; }
        }

        public Package (string name, string etag)
        {
            _name = name;
            _etag = etag;
            _archives = new List<Archive>();
        }

        public void LoadXML(byte[] data)
        {
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(System.Text.Encoding.UTF8.GetString(data));
            doc.DocumentElement.Normalize();

            foreach (XmlNode node in doc.DocumentElement.ChildNodes) {
                String tag = node.Name;

                if (tag.Equals("archive"))
                {
                    _archives.Add(new Archive(node));
                }
            }
        }
    }
}

