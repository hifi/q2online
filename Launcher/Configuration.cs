using System;
using System.IO;
using System.Drawing;

namespace Launcher
{
    public class Configuration
    {
        private static Configuration _instance = null;
        private static readonly object _lock = new object();

        public static Configuration Instance {
            get {
                lock (_lock) {
                    if (_instance == null)
                    {
                        _instance = new Configuration();
                    }

                    return _instance;
                }
            }
        }

        private string _name;
        private string _installPath;
        private string _baseUrl;
        private string _newsUrl;

        public string Name {
            get { return _name; }
        }

        public string BaseURL {
            get { return _baseUrl; }
        }

        public bool FirstLaunch {
            get { return true; }
        }

        public Image Background {
            get { return Image.FromStream(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Launcher.Resources.header.png"), false, true); }
        }

        public Icon Icon {
            get { return new Icon(System.Reflection.Assembly.GetEntryAssembly().GetManifestResourceStream("Launcher.Resources.icon.ico")); }
        }

        public string NewsURL {
            get { return _newsUrl; }
        }

        private Configuration()
        {
            _name = "Q2Online";
            _installPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "q2online";
            _baseUrl = "http://q2online.net/dist";
            _newsUrl = "http://q2online.net/changelog/";
        }

        public string UrlTo(string file)
        {
            return _baseUrl + "/" + file;
        }

        public string FilePath(string path)
        {
            return _installPath + Path.DirectorySeparatorChar + String.Join(Char.ToString(Path.DirectorySeparatorChar), path.Split(new char[]{'/'}));
        }
    }
}

