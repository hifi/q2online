using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using ZipStream;
using System.Text;

namespace Launcher
{
    public class UpdaterEventArgs : EventArgs
    {
        public int progress;
        public string status;

        public UpdaterEventArgs(int progress, string status)
        {
            this.progress = progress;
            this.status = status;
        }
    }

    public class Updater
    {
        private List<Archive> _archives;
        private long _totalPosition;
        private long _totalSize;
        private long _lastProgressPosition;
        private long _nextProgressTick;
        private long _nextReadyTick;
        private string _readyToPlay;

        public delegate void StatusChangedEventHandler(object sender, UpdaterEventArgs e);

        public event StatusChangedEventHandler StatusChanged;

        protected virtual void OnStatusChanged(UpdaterEventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }

        public Updater(List<Archive> archives)
        {
            _archives = archives;
            _readyToPlay = "Calculating speed...";
        }

        private void UpdateStatus()
        {
            UpdateStatus(false);
        }

        private void UpdateStatus(bool forceUpdate)
        {
            if (DateTime.Now.Ticks < _nextProgressTick && !forceUpdate)
                return;

            _nextProgressTick = DateTime.Now.Ticks + TimeSpan.TicksPerSecond;

            if (DateTime.Now.Ticks >= _nextReadyTick || _totalPosition == _totalSize)
            {
                int bytesPerSecond = (int)Math.Max(0, (_totalPosition - _lastProgressPosition) / 5);
                _readyToPlay = ReadyToPlay(bytesPerSecond);
                _nextReadyTick = DateTime.Now.Ticks + (TimeSpan.TicksPerSecond * 5);
                _lastProgressPosition = _totalPosition;
            }

            OnStatusChanged(new UpdaterEventArgs((_totalPosition == _totalSize ? 100 : (int)(((double)_totalPosition / _totalSize) * 100)), _readyToPlay));
        }

        private string ReadyToPlay(int bytesPerSecond)
        {
            long bytesLeft = Math.Max(0, _totalSize - _totalPosition);
            long secondsLeft = (long)(bytesLeft / (double)bytesPerSecond);

            if (bytesLeft == 0)
                return "Ready!";

            if (secondsLeft < 4)
                return "Ready to play soon...";

            long seconds = secondsLeft % 60;
            long minutes = secondsLeft / 60;
            long hours = 0;
            long days = 0;

            if (minutes > 60) {
                minutes = minutes % 60;
                hours = minutes / 60;
            }

            if (hours > 24) {
                hours = hours % 24;
                days = hours / 24;
            }

            StringBuilder timeString = new StringBuilder();

            if (days > 0) {
                timeString.Append(days);
                timeString.Append(" ");
                timeString.Append(days == 1 ? "day" : "days");
                timeString.Append(" ");
            }

            if (hours > 0) {
                timeString.Append(hours);
                timeString.Append(" ");
                timeString.Append(hours == 1 ? "hour" : "hours");
                timeString.Append(" ");
            }

            if (minutes > 0) {
                timeString.Append(minutes);
                timeString.Append(" ");
                timeString.Append(minutes == 1 ? "minute" : "minutes");
                timeString.Append(" ");
            }

            timeString.Append(seconds);
            timeString.Append(" ");
            timeString.Append((seconds == 1 ? "second" : "seconds"));

            return "Ready to play in " + timeString.ToString() + "...";
        }

        private bool InstallArchive(Archive archive)
        {
            long pos;
            int retry = 0;

            while (retry < 3)
            {
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

                                    UpdateStatus();
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

                        UpdateStatus(true);
                    }

                    break;

                } catch (Exception e) {
                    _totalPosition -= pos;
                    UpdateStatus(true);

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

            _totalPosition = _totalSize;
            UpdateStatus(true);
        }
    }
}

