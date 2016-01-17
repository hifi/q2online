using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Launcher
{
    public class UpdateChecker : IDisposable
    {
        protected LoadingWindow _lw;
        private List<Package> _packages;

        public UpdateChecker (List<Package> packages)
        {
            _packages = packages;
            _lw = LoadingWindow.Create();
            _lw.Visible = true;
        }

        public List<Archive> Run()
        {
            _lw.Status =  "Loading...";

            List<Archive> allUpdates = new List<Archive>();

            foreach (Package package in _packages) {
                try {
                    List<Archive> updates = CheckPackage(package);

                    foreach (Archive update in updates) {
                        long size = update.Size;
                        Console.WriteLine(update.Path + " (" + size + " bytes) is scheduled to be downloaded.");
                    }

                    allUpdates.AddRange(updates);
                } catch (Exception e) {
                    if (Configuration.Instance.FirstLaunch) {
                        throw e;
                    } else {
                        Console.WriteLine("Ignored update exception: " + e + ": " + e.Message);
                    }
                }
            }

            _lw.Status = "Done!";

            return allUpdates;
        }

        public List<Archive> CheckPackage(Package package)
        {
            List<Archive> reinstall = new List<Archive>();

            _lw.Status = "Checking updates for " + package.Name + "...";

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(package.Url);
            req.Timeout = Configuration.Instance.FirstLaunch ? 60000 : 2000;

            try {
                if (package.ETag != null)
                    req.Headers["If-None-Match"] = package.ETag;

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (MemoryStream ms = new MemoryStream())
                using (Stream responseStream = resp.GetResponseStream())
                {
                    _lw.Status = "Calculating " + package.Name + " update size...";

                    package.ETag = resp.Headers["ETag"];

                    Utils.CopyStream(responseStream, ms);
                    responseStream.Close();
                    byte[] data = ms.ToArray();

                    package.LoadXML(data);

                    foreach (Archive a in package.Archives)
                    {
                        List<ArchiveFile> files = a.Files;

                        if (!a.Validate())
                        {
                            Console.WriteLine("Archive failed validation: " + a.Path);
                            reinstall.Add(a);
                        }
                    }
                }

                return reinstall;

            } catch (WebException e) {
                if (e.Status == WebExceptionStatus.ProtocolError) {
                    HttpStatusCode status = ((HttpWebResponse)e.Response).StatusCode;
                    if (status == HttpStatusCode.NotModified) {
                        Console.WriteLine("Up-to-date.");
                        return reinstall;
                    }
                }

                throw;
            }
        }

        #region IDisposable implementation

        public void Dispose()
        {
            _lw.Visible = false;
            _lw.Close();
            _lw.Dispose();
        }

        #endregion
    }
}

