using MetadataExtractor;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using File = System.IO.File;

namespace MKSlideShop
{
    /// <summary>
    /// ViewModel handles data and activities of the SlideInfoWindow
    /// </summary>
    public class SlideInfoWindowViewModel : INotifyPropertyChanged
    {
        #region PropertyChangeHandler

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion //PropertyChangeHandler

        #region Properties

        static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Name of active image file
        /// </summary>
        public string CurrentFile
        {
            get { return currentFile; }
            set
            {
                currentFile = value;
                OnPropertyChanged();
                // and now update fields for currentFile
                UpdateInfo(currentFile);
            }
        }
        private string currentFile = string.Empty;

        /// <summary>
        /// List of the image (exif) meta data as text
        /// </summary>
        public List<string> MetaDataList 
        {
            get { return metaDataList; }
            set 
            { 
                metaDataList = value; 
                OnPropertyChanged(); 
            }
        }
        private List<string> metaDataList = new List<string>();

        /// <summary>
        /// Path where the active image resides
        /// </summary>
        public string FilePath
        {
            get { return filePath; }
            set
            {
                filePath = value;
                OnPropertyChanged();
            }
        }
        private string filePath = string.Empty;

        /// <summary>
        /// Date of last modification of the image
        /// </summary>
        public DateTime LastWrite 
        { 
            get { return lastWrite; }
            set 
            { 
                lastWrite = value; 
                OnPropertyChanged(); 
            } 
        }
        private DateTime lastWrite = DateTime.MinValue;

        /// <summary>
        /// lenght of current file in bytes 
        /// </summary>
        public long FileSize 
        { 
            get { return fileSize; } 
            set 
            { 
                fileSize = value; 
                OnPropertyChanged(); 
            } 
        }
        private long fileSize = 0;


        /// <summary>
        /// Composes the dialog title
        /// </summary>
        public string CurrentTitle
        {
            get { return currentTitle; }
            set
            {
                currentTitle = value;
                OnPropertyChanged();
            }
        }

        public string BrowserPath { get; internal set; } = string.Empty;

        private string currentTitle = IniTitle;

        const string IniTitle = "Info";

        #endregion // Properties

        /// <summary>
        /// Update the information on image change
        /// </summary>
        /// <param name="curFile"></param>
        private void UpdateInfo(string curFile)
        {
            log.Debug($"Update info to: {curFile}");
            string fname = "not set";
            if (File.Exists(curFile))
            {
                fname = Path.GetFileName(curFile);

                FileInfo fi = new FileInfo(curFile);
                FilePath = fi.DirectoryName!;
                LastWrite = fi.LastWriteTime;
                FileSize = fi.Length;


                var directories = ImageMetadataReader.ReadMetadata(curFile);

                List<string> strings = new List<string>();

                if (directories != null)
                {
                    foreach (var directory in directories)
                    {
                        foreach (var tag in directory.Tags)
                            strings.Add(string.Format($"[{directory.Name}] {tag.Name} = {tag.Description}"));

                        if (directory.HasError)
                        {
                            foreach (var error in directory.Errors)
                                strings.Add($"ERROR: {error}");
                        }
                    }
                }
                MetaDataList = strings;
            }
            CurrentTitle = string.Format($"{IniTitle} : {fname}");
        }

        /// <summary>
        /// Handles the 'Explore' button to open the image folder a windows explorer 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void OpenExplorer(object sender, RoutedEventArgs e)
        {
            ImageBrowser.OpenExplorer(CurrentFile, BrowserPath);

            //if(File.Exists(CurrentFile))
            //{
            //    log.Debug($"Explore to: {CurrentFile}");
            //    FileInfo fi = new FileInfo(CurrentFile);
            //    Process.Start("explorer.exe", fi.DirectoryName!);
            //    //?? Process.Start("explorer.exe", string.Format($"/select {CurrentFile}"));
            //}
        }
    }
}
