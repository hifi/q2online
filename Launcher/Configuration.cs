using System;
using System.IO;
using System.Drawing;
using System.Configuration;

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

        private System.Configuration.Configuration _config;
        private bool _dirty;
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

        public string InstallPath {
            get { return _installPath; }
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

        // this is our best guess
        public bool IsLinux {
            get { return (int)Environment.OSVersion.Platform == 4; }
        }

        public bool IsMacOSX {
            get { return (int)Environment.OSVersion.Platform == 6; }
        }

        private Configuration()
        {
            _name = "Q2Online";
            _installPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + Path.DirectorySeparatorChar + "q2online";
            _baseUrl = "http://q2online.net/dist";
            _newsUrl = "http://q2online.net/changelog/";

            ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = _installPath + Path.DirectorySeparatorChar + "version.xml";
            _config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
        }

        public string UrlTo(string file)
        {
            return _baseUrl + "/" + file;
        }

        public string FilePath(string path)
        {
            return _installPath + Path.DirectorySeparatorChar + String.Join(Char.ToString(Path.DirectorySeparatorChar), path.Split(new char[]{'/'}));
        }

        public string GetETag(string package)
        {
            var kv = _config.AppSettings.Settings[package];
            return kv == null ? null : kv.Value;
        }

        public void SetETag(string package, string etag)
        {
            if (GetETag(package) != etag)
                _dirty = true;
            _config.AppSettings.Settings.Add(package, etag);
        }

        public void Save()
        {
            if (_dirty)
                _config.Save();
        }
    }
}

