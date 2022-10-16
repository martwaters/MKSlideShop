﻿using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        static readonly Logger log = LogManager.GetCurrentClassLogger();
        private ShowSettings settings = ShowSettings.Default;

        /// <summary>
        /// Paths to be scanned for images
        /// </summary>
        private ObservableCollection<string> paths = new ObservableCollection<string>();
        public ObservableCollection<string> Paths
        {
            get { return paths; }
            set
            {
                paths = value;
                OnPropertyChanged();
                CanStart = paths.Count > 0;
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

        #endregion // Properties

        #region Settings operations

        private void SaveSettings()
        {
            settings.LastPaths = new System.Collections.Specialized.StringCollection();
            settings.LastPaths.AddRange(Paths.ToArray());
            settings.ShowTime = Duration;
            settings.BrowserPath = ExplorerPath;
            settings.Save();
        }
        internal void MainClosed(object? sender, EventArgs e)
        {
            SaveSettings();
        }

        internal void LoadSetting()
        {
            ObservableCollection<string> collect = new ObservableCollection<string>();
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
            ExplorerPath = settings.BrowserPath;
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
                    if (listBox.SelectedItem is string path)
                    {
                        ObservableCollection<string> collect = new ObservableCollection<string>(Paths);
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
                    ObservableCollection<string> collect = new ObservableCollection<string>(Paths);
                    collect.Add(dlg.FileName);
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
            log.Debug($"Start Show ({Paths}):");

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

            SlideWindow slides = new SlideWindow(settings.BrowserPath);
            slides.StartShow(settings);
        }
    }
}