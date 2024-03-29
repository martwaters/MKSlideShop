﻿using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Xml;

namespace MKSlideShop
{
    /// <summary>
    /// ViewModel handles MainWindow data and activities
    /// </summary>
    internal class MainWindowModel : INotifyPropertyChanged
    {
        #region PropertyChangeHandler

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion // PropertyChangeHandler

        #region Properties

        public ObservableCollection<CheckedItem> FileExtTypes
        {
            get { return fileExtTypes; }
            set
            {
                fileExtTypes = value;
                OnPropertyChanged();
            }

        }
        private ObservableCollection<CheckedItem> fileExtTypes = new()
            {
                new CheckedItem() { Use=true, Name=".bmp"},
                new CheckedItem() { Use=true, Name=".gif"},
                new CheckedItem() { Use=true, Name=".jpg"},
                new CheckedItem() { Use=true, Name=".png"},
                new CheckedItem() { Use=true, Name=".tif"},
            };

        bool HasExtSelected { get; set; } = false;

        public enum StartChanges : ushort { keep = 0, hide = 1, close = 2 };
        public StartChanges StartChange 
        { 
            get { return startChange; } 
            set
            {
                startChange = value;
                switch (startChange)
                {
                    case StartChanges.keep:
                        KeepChecked = true; CloseChecked = false; HideChecked = false;
                        break;
                    case StartChanges.close:
                        CloseChecked = true; HideChecked = false; KeepChecked = false;
                        break;
                    case StartChanges.hide:
                        HideChecked = true; CloseChecked = false; KeepChecked = false;
                        break;
                }
            }
        }
        private StartChanges startChange = StartChanges.keep;

        static readonly Logger log = LogManager.GetCurrentClassLogger();
        private static ShowSettings settings = ShowSettings.Default;

        /// <summary>
        /// Paths to be scanned for images
        /// </summary>
        private ObservableCollection<string> paths = new();
        public ObservableCollection<string> Paths
        {
            get { return paths; }
            set
            {
                paths = value;
                OnPropertyChanged();
                CanStart = paths.Count > 0 && HasExtSelected;
            }
        }

        public string ExplorerPath { get { return explorerPath; } set { explorerPath = value; } }
        private string explorerPath = string.Empty;

        /// <summary>
        /// Duration [s] to display a single slide
        /// </summary>
        public int Duration { get; set; }   

        /// <summary>
        /// Start button state
        /// </summary>
        private bool canStart = true;
        public bool CanStart
        {
            get { return canStart; }
            set
            {
                canStart = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Keep radio button state
        /// </summary>
        public bool KeepChecked
        {
            get { return keepChecked; }
            set
            {
                keepChecked = value;
                OnPropertyChanged();
            }
        }
        private bool keepChecked = true;

        /// <summary>
        /// Hide radio button state
        /// </summary>
        public bool HideChecked
        {
            get { return hideChecked; }
            set
            {
                hideChecked = value;
                OnPropertyChanged();
            }
        }
        private bool hideChecked = true;

        /// <summary>
        /// Clode Radio button state
        /// </summary>
        public bool CloseChecked
        {
            get { return closeChecked; }
            set
            {
                closeChecked = value;
                OnPropertyChanged();
            }
        }
        private bool closeChecked = true;

        #endregion // Properties

        #region Settings operations

        internal void WindowInitialized(object? sender, EventArgs e)
        {
            if (sender is MainWindow mw && settings != null)
            {
                log.Debug($"Restore Position: {(WindowState)settings.MainState} L={settings.MainLeft} W={settings.MainWidth} T={settings.MainTop} H={settings.MainHeight}");
                if (settings.MainWidth > 0 && settings.MainHeight > 0)
                {
                    mw.Left = settings.MainLeft;
                    mw.Width = settings.MainWidth;
                    mw.Top = settings.MainTop;
                    mw.Height = settings.MainHeight;

                    mw.WindowState = (WindowState)settings.MainState;
                }
            }
        }

        private void SaveSettings()
        {
            settings.LastPaths = new System.Collections.Specialized.StringCollection();
            settings.LastPaths.AddRange(Paths.ToArray());
            settings.ShowTime = Duration;
            settings.BrowserPath = ExplorerPath;

            startChange = KeepChecked ? StartChanges.keep : HideChecked ? StartChanges.hide : StartChanges.close;
            settings.MainOnStart = (ushort) StartChange;

            settings.ImageTypes = 0;
            for(int i = 0; i < FileExtTypes.Count; i++)
            {
                if (FileExtTypes[i].Use)
                    settings.ImageTypes |= (ushort) (1 << i);
            }
            settings.Save();
        }

        internal void MainClosing(object? sender, CancelEventArgs? e)
        {
            if (sender is MainWindow mw)
            {
                log.Debug($"Store MainWin: {mw.WindowState} L={mw.Left} W={mw.Width} T={mw.Top} H={mw.Height}");

                if (settings == null)
                    throw new System.FieldAccessException(nameof(settings));

                settings.MainState = (int)mw.WindowState;
                if (!mw.WindowState.Equals(WindowState.Minimized))
                {
                    settings.MainLeft = mw.Left;
                    settings.MainWidth = mw.Width;
                    settings.MainTop = mw.Top;
                    settings.MainHeight = mw.Height;
                }
            }

            SaveSettings();
        }

        internal void LoadSetting()
        {
            ObservableCollection<string> collect = new();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                collect.Add(args[1]);
            }
            else if (settings.LastPaths != null && settings.LastPaths.Count > 0)
            {
                foreach (string? path in settings.LastPaths)
                {
                    if (!string.IsNullOrEmpty(path))
                        collect.Add(path);
                }
            }
            else
            {
                // add def image path
                collect.Add(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            }
            Paths = collect;
            Duration = settings.ShowTime;
            StartChange = (StartChanges) settings.MainOnStart;
            ExplorerPath = settings.BrowserPath;

            //BitArray bat = new BitArray()
            for (int i = 0; i < FileExtTypes.Count; i++)
            {
                ushort check = (ushort) (1 << i);
                FileExtTypes[i].Use = (settings.ImageTypes & check) == check;
            }

        }

        #endregion // Settings operations

        #region Search Paths

        /// <summary>
        /// Remove the selected Path from List
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                if (sender is ListBox listBox)
                {
                    if (e.KeyStates.HasFlag(KeyStates.Toggled))
                        Paths = new();
                    else if (listBox.SelectedItem is string path)
                    {
                        ObservableCollection<string> collect = new(Paths);
                        collect.Remove(path);
                        Paths = collect;
                    }
                }
            }
        }

        /// <summary>
        /// Select a folder to add to the Paths collection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ButAddFolder(object sender, RoutedEventArgs e)
        {
            string currentDirectory = Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures);
            var dlg = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select an image (parent) folder",
                InitialDirectory = currentDirectory,

                AddToMostRecentlyUsedList = false,
                AllowNonFileSystemItems = false,
                DefaultDirectory = currentDirectory,
                EnsureFileExists = true,
                EnsurePathExists = true,
                EnsureReadOnly = false,
                EnsureValidNames = true,
                Multiselect = false,
                ShowPlacesList = true
            };

            CommonFileDialogResult result = dlg.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                if (Paths.Contains(dlg.FileName))
                {
                    MessageBox.Show(String.Format("Path '{0}' already in list!", dlg.FileName));
                }
                else
                {
                    ObservableCollection<string> collect = new(Paths)
                    {
                        dlg.FileName
                    };
                    Paths = collect;
                }
            }

        }

        #endregion // Search Paths

        /// <summary>
        /// Start the slide show in a new window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void ButStartShow(object sender, RoutedEventArgs e)
        {
            log.Debug($"Start Show ({Paths.Count} path(s)):");

            foreach (var s in Paths)
                    log.Debug($"\t{s}");

            SaveSettings();

            if (settings.LastPaths == null || settings.LastPaths.Count == 0)
            {
                log.Error("Slides path not set!");
                MessageBox.Show("No path(s) set to be searched!");
                return;
            }

            if (settings.ShowTime < 1)
            {
                log.Error("No reasonable display duration set!");
                MessageBox.Show("No reasonable display time set!");
                return;
            }

            List<string> extensions = new List<string>();
            foreach(CheckedItem cit in FileExtTypes)
            {
                if(cit.Use)
                {
                    extensions.Add(cit.Name);
                }
            }
            if(extensions.Count == 0)
            {
                log.Error("No image type selected!");
                MessageBox.Show("No image type selected!");
                return;
            }

            SlideWindow slides = new(settings, extensions);
            slides.StartShow();

            ShowAsDesired();
        }

        private void ShowAsDesired()
        {
            switch(StartChange)
            {
                case StartChanges.keep:
                    return;

                case StartChanges.hide:
                    App.Current.MainWindow.Visibility= Visibility.Hidden;
                    return;

                case StartChanges.close:
                    App.Current.MainWindow.Close();
                    return;                  
            }
        }

        internal void StoreShow(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            SaveFileDialog sFD = new()
            {
                DefaultExt=".slides",
                Filter= "MKSlides settings (.slides)|*.slides"
            };
            if (!string.IsNullOrEmpty(settings.SettingsPath) && Directory.Exists(settings.SettingsPath))
            {
                sFD.InitialDirectory = settings.SettingsPath;
            }

            if (sFD.ShowDialog() == true)
            {
                settings.SettingsPath = new FileInfo(sFD.FileName).DirectoryName;
                SettingsXml setx = new(settings);
                // iterate settings.Properties ...
                string setString = XmlClassSerializer.Object2Xml(setx);
                File.WriteAllText(sFD.FileName, setString);                
               

            }
        }

        internal void RestoreShow(object sender, RoutedEventArgs e)
        {
            OpenFileDialog oFD = new()
            {
                DefaultExt = ".slides",
                Filter = "MKSlides settings (.slides)|*.slides"
            };
            if (!string.IsNullOrEmpty(settings.SettingsPath) && Directory.Exists(settings.SettingsPath))
            {
                oFD.InitialDirectory = settings.SettingsPath;
            }

            if (oFD.ShowDialog() == true)
            {
                //XmlReader rd = new XmlReader<ShowSettings>();
                string xml = File.ReadAllText(oFD.FileName); 
                if(XmlClassSerializer.Xml2Object(xml, typeof(SettingsXml)) is SettingsXml setx)
                {
                    // Apply setx
                    Paths = new ObservableCollection<string>(setx.Paths);
                    Duration = setx.ShowTime;
                    ExplorerPath = setx.Browser;
                    StartChange = (StartChanges) setx.MKeep;

                    //settings.SettingsPath = setx.SettingsPath;
                    settings.SettingsPath = new FileInfo(oFD.FileName).DirectoryName;

                    settings.SlideState = setx.SState;
                    settings.SlideLeft = setx.SLeft;
                    settings.SlideTop = setx.STop;
                    settings.SlideWidth = setx.SWidth;
                    settings.SlideHeight = setx.SHeight;

                    settings.MainState = setx.MState;
                    settings.MainLeft = setx.MLeft;
                    settings.MainTop = setx.MTop;
                    settings.MainWidth = setx.MWidth;
                    settings.MainHeight = setx.MHeight;

                    settings.ImageTypes= setx.ImageTypes;

                    if (sender is FrameworkElement parent)
                    {
                        while (parent.Parent is FrameworkElement pw)
                        {
                            if (pw is MainWindow mw)
                            {
                                // show position
                                mw.Left = setx.MLeft;
                                mw.Top = setx.MTop;
                                mw.Width = setx.MWidth;
                                mw.Height = setx.MHeight;
                                mw.WindowState = (WindowState) setx.MState;
                                mw.Show();
                                break;
                            }
                            else
                            {
                                parent = pw;
                            }
                        }
                    }
                }

            }
        }

        internal void AboutDialog(object sender, RoutedEventArgs e)
        {
            AboutDialog ad = new();
            ad.ShowDialog();
        }

        internal void AutoGenFTypeColumn(object? sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if("Name".Equals(e.Column.Header))
            {
                e.Column.IsReadOnly = true;
            }
            else if("Use".Equals(e.Column.Header)) 
            {
                foreach (CheckedItem cit in FileExtTypes)
                {
                    cit.PropertyChanged += FileExtChanged;
                }
                CheckFileExtSelection();
            }
        }

        private void FileExtChanged(object? sender, PropertyChangedEventArgs e)
        {
            if("Use".Equals(e.PropertyName))
            {
                CheckFileExtSelection();
            }
        }

        private void CheckFileExtSelection()
        {
            HasExtSelected = false;
            foreach (CheckedItem cit in FileExtTypes)
            {
                if (cit.Use)
                    HasExtSelected = true;
            }
            CanStart = Paths.Count > 0 && HasExtSelected;
        }
    }
}
