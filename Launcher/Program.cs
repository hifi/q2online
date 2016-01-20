using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.ComponentModel;

namespace Launcher
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            Application.EnableVisualStyles();

            List<Package> packages = new List<Package>();

            // install everything now for aq2 target for testing
            packages.Add(new Package("baseq2", null));
            packages.Add(new Package("client-windows", null));
            packages.Add(new Package("client-linux", null));
            packages.Add(new Package("action", null));
            packages.Add(new Package("action-windows", null));
            packages.Add(new Package("action-linux", null));

            var lw = new LoadingWindow();
            var archives = lw.Run(packages);

            var uw = new UpdateWindow();
            uw.Run(archives);
        }
    }
}
