using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ZipStream;

namespace Launcher
{
    public class Updater
    {
        private List<Archive> _archives;
        private long _totalPosition;
        private long _totalSize;
        private int _lastUpdate;

        public Updater(List<Archive> archives)
        {
            _archives = archives;
        }

        private bool InstallArchive(Archive archive)
        {
            long pos;
            int retry = 0;

            while (retry < 3)
            {
                _lastUpdate = 0;
                pos = 0;

                try {
                    Console.WriteLine("Installing " + archive.Path + "...");

                    Dictionary<string, ArchiveFile> targets = new Dictionary<string, ArchiveFile>();

                    foreach (ArchiveFile af in archive.Files)
                    {
                        if (!af.Deleted) {
                            targets.Add(af.Src, af);
                        }
                    }

                    Console.WriteLine("Downloading from " + archive.URL);
                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(archive.URL);
                    req.Timeout = 60000;

                    using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                    using (Stream responseStream = resp.GetResponseStream())
                    using (PositionedStream ps = new PositionedStream(responseStream))
                    using (ZipStreamFilter zsf = new ZipStreamFilter(ps))
                    using (ZipInputStream zis = new ZipInputStream(zsf))
                    {
                        foreach (ZipStreamEntry entry in zis.Entries)
                        {
                            if (!targets.ContainsKey(entry.FileName))
                                continue;

                            ArchiveFile af = targets[entry.FileName];
                            targets.Remove(entry.FileName);

                            string dstFile = Configuration.Instance.FilePath(af.Dst);
                            string dstPath = dstFile.Substring(0, dstFile.LastIndexOf(Path.DirectorySeparatorChar));
                            Directory.CreateDirectory(dstPath);

                            if (af.SHA1 == null && File.Exists(dstFile))
                            {
                                Console.WriteLine("Skipping {0} because no hash and file exists...", af.Dst);
                                continue;
                            }

                            Console.WriteLine("Extracting {0}...", af.Dst);

                            using (FileStream fs = new FileStream(dstFile + ".part", FileMode.Create))
                            using (BufferedStream bs = new BufferedStream(fs))
                            {
                                byte[] buf = new byte[4096];
                                int i;
                                while ((i = zis.Read(buf,  0, buf.Length)) > 0) {
                                    bs.Write(buf, 0, i);
                                    long inc = ps.Position - pos;
                                    pos += inc;
                                    _totalPosition += inc;

                                    // publish position
                                }
                            }

                            if (File.Exists(dstFile))
                            {
                                File.Delete(dstFile);
                            }

                            File.Move(dstFile + ".part", dstFile);

                            // skip rest of this archive if we can
                            if (targets.Count == 0)
                                break;
                        }

                        responseStream.Close();
                        resp.Close();
                    }

                    break;

                } catch (Exception e) {
                    _totalPosition -= pos;

                    if (++retry == 3)
                    {
                        Console.WriteLine(e.ToString(), e.Message);
                        return false;
                    }
                    else
                    {
                        Console.WriteLine("Archive install failed, retrying...");
                    }
                }
            }

            return true;
        }

        public void Run()
        {
            foreach (Archive archive in _archives)
            {
                _totalSize += archive.Size;
            }

            foreach (Archive archive in _archives)
            {
                if (!InstallArchive(archive))
                    throw new Exception("Archive install failed, aborting.");
            }

            // launch!
        }
    }
}

