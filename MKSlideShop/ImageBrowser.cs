using NLog;
using System.Diagnostics;
using System.IO;

namespace MKSlideShop
{
    internal class ImageBrowser
    {
        static readonly Logger log = LogManager.GetCurrentClassLogger();

        internal static void OpenExplorer(string forFile, string altBrowser)
        {
            if (File.Exists(forFile))
            {
                log.Debug($"Explore to: {forFile}");
                FileInfo fi = new FileInfo(forFile);

                string program = "explorer.exe";
                if (!string.IsNullOrEmpty(altBrowser) && File.Exists(altBrowser))
                    program = altBrowser;
                Process.Start(program, fi.DirectoryName!);

                //?? Process.Start("explorer.exe", string.Format($"/select {CurrentFile}"));
            }
        }


    }
}
