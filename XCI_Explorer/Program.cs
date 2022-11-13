using System;
using System.IO;
using System.Windows.Forms;

namespace XCI_Explorer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (object sender, ResolveEventArgs args) =>
        {
            System.Reflection.AssemblyName embeddedAssembly = new System.Reflection.AssemblyName(args.Name);
            string resourceName = "XCI_Explorer" + "." + embeddedAssembly.Name + ".dll";

            using Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            byte[] assemblyData = new byte[stream.Length];
            stream.Read(assemblyData, 0, assemblyData.Length);
            return System.Reflection.Assembly.Load(assemblyData);
        };
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
    }
}
