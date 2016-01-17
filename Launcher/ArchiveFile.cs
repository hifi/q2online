using System;
using System.Xml;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Launcher
{
    public class ArchiveFile
    {
        private string _src;
        private string _dst;
        private string _sha1;
        private string _localSha1;
        private bool _deleted;

        public string Src {
            get { return _src; }
        }

        public string Dst {
            get { return _dst; }
        }

        public string SHA1 {
            get { return _sha1; }
        }

        public bool Deleted {
            get { return _deleted; }
        }

        public bool Valid {
            get {
                if (_localSha1 == null)
                    return Validate();

                return _localSha1.Equals(_sha1);
            }
        }

        public ArchiveFile(XmlNode parent)
        {
            foreach (XmlNode node in parent.ChildNodes) {
                if (node.Name.Equals("src"))
                {
                    _src = node.FirstChild.Value;
                }
                else if (node.Name.Equals("dst"))
                {
                    _dst = node.FirstChild.Value;
                }
                else if (node.Name.Equals("sha1"))
                {
                    _sha1 = node.FirstChild.Value;
                }
                else if (node.Name.Equals("deleted"))
                {
                    _deleted = true;
                }
            }

            if (_deleted)
            {
                _src = null;
                _sha1 = null;
            }
        }

        public bool Validate()
        {
            string filePath = Configuration.Instance.FilePath(Dst);
            if (!File.Exists(filePath))
                return false;

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            using (BufferedStream bs = new BufferedStream(fs))
            {
                using (SHA1Managed sha1 = new SHA1Managed())
                {
                    byte[] hash = sha1.ComputeHash(bs);
                    StringBuilder formatted = new StringBuilder(2 * hash.Length);
                    foreach (byte b in hash)
                    {
                        formatted.AppendFormat("{0:x2}", b);
                    }
                    _localSha1 = formatted.ToString();
                }
            }

            return _sha1.Equals(_localSha1);
        }
    }
}

