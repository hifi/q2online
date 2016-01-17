using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Launcher
{
    class MainClass
    {
        public static void Main (string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, bargs) =>
            {
                String dllName = new AssemblyName(bargs.Name).Name + ".dll";
                var assem = Assembly.GetExecutingAssembly();
                String resourceName = null;
                foreach (string res in assem.GetManifestResourceNames())
                {
                    if (res.EndsWith(dllName))
                        resourceName = res;
                }
                if (resourceName == null) return null; // Not found, maybe another handler will find it
                using (var stream = assem.GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };

            new Thread(delegate() {
                Application.EnableVisualStyles();
                Application.Run();
            }).Start();

            List<Package> packages = new List<Package>();

            // install everything now for aq2 target for testing
            packages.Add(new Package("baseq2", null));
            packages.Add(new Package("client-windows", null));
            packages.Add(new Package("client-linux", null));
            packages.Add(new Package("action", null));
            packages.Add(new Package("action-windows", null));
            packages.Add(new Package("action-linux", null));

            List<Archive> archives;

            using (UpdateChecker uc = new UpdateChecker(packages))
            {
                archives = uc.Run();
            }

            Updater u = new Updater(archives);
            u.Run();

            Application.Exit();
        }
    }
}
