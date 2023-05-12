using NLog;
using SlideStore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SlideWalker
{

    /// <summary>
    /// PathWalker walks (asynchronously?) accross selected paths to collect desired files
    /// </summary>
    public class PathWalker
    {
        #region Properties

        static readonly Logger log = LogManager.GetCurrentClassLogger();

        public delegate void FileHandler(object sender, FileCheckEventArgs args);
        public event FileHandler FileCheckEvent;
        public event FileHandler SlideCheckEvent;

        public bool Aborted { get; set; } = false;

        /// <summary>
        /// supported image extensions
        /// </summary>
        //static List<string> FileExtensions = new List<string>() { ".jpg", ".bmp", ".gif", ".png", ".tif" };
        public List<string> FileExtensions 
        {
            get { return fileExtensions; }
            set { fileExtensions = value; }
        }
        private List<string> fileExtensions = new List<string>() { ".jpg", ".bmp", ".gif", ".png", ".tif" };

        #endregion // Properties

        public async Task WalkDirectoriesAsync(DirectoryInfo dir)
        {
            log.Trace($"WDasync: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo di in dirs)
            {
                await WalkDirectoriesAsync(di);
            }
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo fi in files)
            {
                //FileCheckEvent?.Invoke(MainWin, new FileCheckEventArgs(fi));
                FileCheckEvent?.Invoke(this, new FileCheckEventArgs(fi));
            }
        }

        public void WalkDirectories(DirectoryInfo dir)
        {
            log.Trace($"WDs: {dir.FullName}");
            if (Aborted)
                return;
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo di in dirs)
            {
                if (Aborted)
                    return;
                WalkDirectories(di);
            }
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo fi in files)
            {
                if (Aborted)
                    return;
                //FileCheckEvent?.Invoke(dir, new FileCheckEventArgs(fi));
                if (IsSlideFile(fi))
                {
                    log.Trace($"Invoke SlideCheck: {fi.Name}");
                    SlideCheckEvent?.Invoke(dir, new FileCheckEventArgs(fi));
                }
            }
        }

        /// <summary>
        /// More: Seems to be ...
        /// </summary>
        /// <param name="fi"></param>
        /// <returns></returns>
        private bool IsSlideFile(FileInfo fi)
        {
            if(fi.Exists)
            {
                if(fi.Length > 0)
                {
                    if(FileExtensions.Contains( fi.Extension, StringComparer.InvariantCultureIgnoreCase))
                    {
                        log.Trace($"\tIsSlide: {fi.Name}");
                        return true;
                    }
                }
            }
            return false;
        }

    }
}
