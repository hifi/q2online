using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;

namespace Launcher
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            Application.EnableVisualStyles();

            List<Package> packages = new List<Package>();

            // install everything now for aq2 target for testing
            packages.Add(new Package("baseq2"));

            string game = "action";

            if (game != "baseq2")
                packages.Add(new Package(game));

            if (Configuration.Instance.IsLinux)
            {
                packages.Add(new Package("client-linux"));
                packages.Add(new Package(game + "-linux"));
            }
            else if (Configuration.Instance.IsMacOSX)
            {
                packages.Add(new Package("client-darwin"));
                packages.Add(new Package(game + "-darwin"));
            }
            else
            {
                packages.Add(new Package("client-windows"));
                packages.Add(new Package(game + "-windows"));
            }

            // load package ETags from config
            foreach (var package in packages)
            {
                package.ETag = Configuration.Instance.GetETag(package.Name);
            }


            var lw = new LoadingWindow();
            var archives = lw.Run(packages);

            if (archives != null && archives.Count > 0)
            {
                var uw = new UpdateWindow();
                if (!uw.Run(archives))
                    return;

            }

            foreach (var package in packages)
            {
                Configuration.Instance.SetETag(package.Name, package.ETag);
            }

            Configuration.Instance.Save();

            // launching
            var exePath = Configuration.Instance.FilePath("q2pro.exe");

            if (Configuration.Instance.IsLinux)
            {
                exePath = Configuration.Instance.FilePath("q2pro-linux-x86" + (Environment.Is64BitOperatingSystem ? "_64" : ""));
                File.SetAttributes(exePath, (FileAttributes)((long)File.GetAttributes(exePath) | 0x80000000));
            }
            else if (Configuration.Instance.IsMacOSX)
            {
                exePath = Configuration.Instance.FilePath("q2pro-darwin-x86_64");
                File.SetAttributes(exePath, (FileAttributes)((long)File.GetAttributes(exePath) | 0x80000000));
            }

            var psi = new ProcessStartInfo();
            psi.FileName = exePath;
            psi.Arguments = "+set game action";
            psi.WorkingDirectory = Configuration.Instance.InstallPath;

            var p = new Process();
            p.StartInfo = psi;
            p.Start();
            p.WaitForExit();
        }
    }
}
