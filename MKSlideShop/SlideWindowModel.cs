using NLog;
using SlideStore;
using SlideWalker;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace MKSlideShop
{
    /// <summary>
    /// Viewmodel providing data and actions of the SlideWindow
    /// </summary>
    internal class SlideWindowModel : INotifyPropertyChanged
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

        /// <summary>
        /// Is the 'Run' button active?
        /// </summary>
        public bool CanRun { get { return canRun; } set { canRun = value; OnPropertyChanged(); } }
        private bool canRun = false;


        /// <summary>
        /// Is the 'Pause' button active?
        /// </summary>
        public bool CanPause { get { return canPause; } set { canPause = value; OnPropertyChanged(); } }
        private bool canPause = true;

        public bool CanBack { get { return canBack; } set { canBack = value; OnPropertyChanged(); } }
        private bool canBack = true;

        List<int> JustDone { get; set; } = new List<int>();

        /// <summary>
        /// file name of the current image
        /// </summary>
        public string CurrentFile
        {
            get { return currentFile; }
            set
            {
                currentFile = value;
                OnPropertyChanged();
            }
        }
        private string currentFile = string.Empty;

        /// <summary>
        /// Number of image files found
        /// </summary>
        public string NumFiles
        {
            get { return numFiles.ToString(); }
            set
            {
                if (int.TryParse(value, out int n))
                    numFiles = n;
                OnPropertyChanged();
            }
        }
        private int numFiles;

        
        public string SelImgTip
        {
            get { return selImgTip; }
            set
            {
                selImgTip = value;
                OnPropertyChanged();
            }
        }
        private string selImgTip = string.Empty;

        /// <summary>
        /// Hiding NumFiles by default
        /// </summary>
        public Visibility NumVisible { get; set; } = Visibility.Collapsed;

        /// <summary>
        /// Searchers of the paths ;-)
        /// </summary>
        private PathWalker pathWalk = new PathWalker();
        private SlideWorker FWorker = new SlideWorker();


        /// <summary>
        /// Timer to update the image display with a new image
        /// </summary>
        System.Timers.Timer? FWTimer;

        /// <summary>
        /// Storage of images found
        /// </summary>
        Storage SlideStore = new Storage(); 

        /// <summary>
        /// Randomize display sequence
        /// </summary>
        Random RndIdx = new Random((int) DateTime.Now.Ticks);

        bool isSearchRunning = false;

        /// <summary>
        /// Window to display image details
        /// </summary>
        internal SlideInfoWindow? InfoWindow { get; private set; }

        object lockObject = new object();

        public Image? ImageCtrl { get; internal set; }

        int LastIdx { get; set; } = 0;
        int CurrentIdx { get; set; } = 0;
        public string BrowserPath { get; internal set; } = string.Empty;

        ShowSettings slideSettings = ShowSettings.Default;

        #endregion // Properties
        internal SlideWindowModel(ShowSettings settings)
        {
            slideSettings = settings;
        }

        #region Collect Files

        private void SetRunning(bool running)
        {
            log.Debug($"Set img search running {running}");

            // while running ... allow stopping that ?
            // suppress other activities ?

            isSearchRunning = running;
        }

        /// <summary>
        /// Walk through the configured paths to collect images
        /// </summary>
        internal void SlideWalk()
        {
            log.Debug("Init Slide Walk");

            SetRunning(true);
            
            FWorker.FileWorkerError += FWorker_FileWorkerError;
            FWorker.SlideRetrieved += FWorker_SlideRetrieved;
            FWorker.FileWorkDone += FWorker_Done;

            log.Debug("Start Slideshow Timer");

            int secTicks = slideSettings.ShowTime * 1000;
            FWTimer = new System.Timers.Timer(secTicks);
            FWTimer.AutoReset = true;

            FWTimer.Elapsed += FWTimer_Elapsed;
            FWTimer.Enabled = true;

            string[] paths = new string[slideSettings.LastPaths.Count];
            slideSettings.LastPaths.CopyTo(paths, 0);

            log.Debug("Find slides");
            FWorker.FindSlides(pathWalk, paths);

        }


        private void FWorker_FileWorkerError(object? sender, string e)
        {
            System.Windows.MessageBox.Show($"Error: {e}");
            //EnableMain();
        }

        /// <summary>
        /// Ready searchin images
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="objects"></param>
        private void FWorker_Done(object? sender, object[] objects)
        {
            
            FWorker.Stop();
            SetRunning(false);

            //ShowRandomImage();

        }


        /// <summary>
        /// It's up to display another image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FWTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (sender is System.Timers.Timer timer)
            {
                log.Debug($"FWTimer tick: {e.SignalTime}");
                ShowRandomImage();
            }
        }

        private void FWorker_SlideRetrieved(object? sender, object[] objects)
        {
            if (objects != null && objects.Length == 1)
            {
                if (objects[0] is FileCheckEventArgs slide)
                {
                    if (sender is SlideWorker fw)
                    {
                        log.Trace($"Store slide {fw.NFiles}: {slide.FInfo.FullName}\r\n");
                        NumFiles = fw.NFiles;
                        SlideStore.AddSlide(slide);
                        if(int.TryParse(NumFiles, out int nFiles ) && nFiles == 5)
                            ShowRandomImage();
                    }
                }
            }
        }

        #endregion // Collect Files

        #region Display Image

        void ShowLastImageByIndex()
        {
            lock (lockObject)
            {
                if (SlideStore.Slides.Count > 0)
                {
                    int idx = LastIdx;
                    log.Debug($"Show previous slide ({idx}) of {SlideStore.Slides.Count}");
                    string fName = SlideStore.Slides[idx].FInfo.Name;
                    CurrentFile = SlideStore.Slides[idx].FInfo.FullName;
                    if (File.Exists(CurrentFile))
                    {
                        LastIdx = CurrentIdx;
                        CurrentIdx = idx;
                        UpdateImage();
                    }
                    else
                        log.Debug($"Show Index: Slide {CurrentFile} not found!");
                }
                else
                    log.Debug("Show Random: No slides");
            }
        }

        /// <summary>
        /// Repeat images earliest when half of the stuff is shown ...
        /// </summary>
        /// <returns></returns>
        int GetNextIdx()
        {
            int idx = RndIdx.Next(SlideStore.Slides.Count);
            if (JustDone.Count > SlideStore.Slides.Count / 2)
                JustDone.Clear();
            else
            {
                while(JustDone.Contains(idx))
                    idx = RndIdx.Next(SlideStore.Slides.Count);
                
                JustDone.Add(idx);
            }
            return idx;
        }

        void ShowRandomImage()
        {
            lock (lockObject)
            {
                if (SlideStore.Slides.Count > 0)
                {
                    int idx = GetNextIdx();
                    log.Debug($"Show Random: slide {idx} of {SlideStore.Slides.Count}");

                    string fName = SlideStore.Slides[idx].FInfo.Name;
                    CurrentFile = SlideStore.Slides[idx].FInfo.FullName;
                    if (File.Exists(CurrentFile))
                    {
                        SelImgTip = String.Format($"Image {idx}({NumFiles}): {fName} ({SlideStore.Slides[idx].FInfo.LastWriteTime})");

                        LastIdx = CurrentIdx;
                        CurrentIdx = idx;
                        UpdateImage();
                    }
                    else
                        log.Debug($"Show Random: Slide {CurrentFile} not found!");
                }
                else
                    log.Debug("Show Random: No slides");
            }
        }

        private void UpdateImageCtrl(Image? img, Rotation flip)
        {
            if (img != null)
            {
                BitmapImage bi = new BitmapImage();
                if (bi != null)
                {
                    log.Trace($"\tFlip Img={flip}\r\n");
                    bi.Rotation = flip;

                    // Begin initialization.
                    bi.BeginInit();

                    // Set properties.
                    bi.CacheOption = BitmapCacheOption.OnDemand;
                    bi.CreateOptions = BitmapCreateOptions.DelayCreation;
                    //bi.DecodePixelHeight = 125;
                    //bi.DecodePixelWidth = 125;
                    bi.Rotation = flip;
                    bi.UriSource = new Uri(CurrentFile);
                    // End initialization.

                    bi.EndInit();
                    img.Source = bi;
                }
            }
        }

        /// <summary>
        /// Updates image display for a new image file
        /// </summary>
        private void UpdateImage()
        {
            // rotate if needed
            //
            // main thread : 
            Rotation flip = GetRotation(CurrentFile);

            // need dispatcher thread to avoid thread crashes

            // remove image from parent canvas ?
            // clear
            ImageCtrl?.Dispatcher.BeginInvoke(new Action(() => ImageCtrl.Source = null));

            // set new image
            ImageCtrl?.Dispatcher.BeginInvoke(new Action(() => UpdateImageCtrl(ImageCtrl, flip)));



            // fade in time [s]: min 1, max time interval / 10
            double timSecs = FWTimer!.Interval / 1000d;
            TimeSpan fadeInTime = TimeSpan.FromSeconds(Math.Max(Math.Min(1d, timSecs), timSecs/10d));
            ImageCtrl?.Dispatcher.BeginInvoke(new Action(() => ImageCtrl.BeginAnimation(Image.OpacityProperty, 
                new DoubleAnimation() 
                { 
                    From=0d,
                    To=1d,
                    Duration=new Duration(fadeInTime),
                }
                )));

            // update file info window
            if (InfoWindow != null)
                InfoWindow.SetInfo(CurrentFile);
        }

        /// <summary>
        /// Returns the desired rotation for the image file
        /// </summary>
        /// <param name="currentFile"></param>
        /// <returns></returns>
        private Rotation GetRotation(string currentFile)
        {
            Rotation rotation = Rotation.Rotate0;

            try
            {
                using (var image = System.Drawing.Image.FromFile(CurrentFile))
                {
                    log.Trace($"Get Drawing.Image orientation");
                    foreach (var prop in image.PropertyItems)
                    {
                        if (prop.Id == 0x112 && prop.Value != null)
                        {
                            rotation = OrientationToRotation((short)prop.Value[0]);
                            log.Trace($"\t\tRotate={rotation}\r\n");
                        }
                    }
                }
            }
            catch(OutOfMemoryException ox)
            {
                log.Error(ox);
                Pause(true);
                MessageBox.Show($"Error on file '{currentFile}': {ox.Message}");
            }

            return rotation;
        }

        /// <summary>
        /// Returns the desired rotation, based on the orientation property
        /// </summary>
        /// <param name="orientKey"></param>
        /// <returns></returns>
        private Rotation OrientationToRotation(short orientKey)
        {
            switch(orientKey)
            {
                case 6:
                    return Rotation.Rotate90;
                case 8:
                    return Rotation.Rotate270;
                case 3:
                    return Rotation.Rotate180;
                case 0:
                default:
                    return Rotation.Rotate0;
            }
        }

        #endregion // Display Image

        #region Event handlers

        /// <summary>
        /// Handles the button to open a SlideInfoWindow
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void InfoClicked(object sender, RoutedEventArgs e)
        {
            if(InfoWindow == null)
            {
                InfoWindow = new SlideInfoWindow(CurrentFile, BrowserPath);
                InfoWindow.Show();
                InfoWindow.Closing += InfoWindow_Closing;
            }
            else
            {
                InfoWindow.Close();
            }
        }

        void Pause(bool paused)
        {
            if (FWTimer != null)
            {
                FWTimer.Enabled = !paused;
                log.Debug($"Set UpdateTimer to: {FWTimer.Enabled}");
            }
            CanPause = !paused;
            CanRun = paused;
        }

        internal void BackClicked(object sender, RoutedEventArgs e)
        {
            Pause(true);
            ShowLastImageByIndex();
        }

        internal void PauseClicked(object sender, RoutedEventArgs e)
        {
            Pause(true);
        }

        internal void RunClicked(object sender, RoutedEventArgs e)
        {
            Pause(false);
        }

        internal void ExploreClicked(object sender, RoutedEventArgs e)
        {
            if (File.Exists(CurrentFile))
            {
                Pause(true);
                ImageBrowser.OpenExplorer(CurrentFile, BrowserPath);
            }
        }


        private void InfoWindow_Closing(object sender, CancelEventArgs e)
        {
            InfoWindow = null;
        }

        internal void WindowClosing(object sender, CancelEventArgs e)
        {
            if (FWTimer != null)
            {
                log.Debug($"Stop UpdateTimer");
                FWTimer.Enabled = false;
                FWTimer.Stop();
                FWTimer.Elapsed -= FWTimer_Elapsed;
                FWTimer.Close();
            }

            if(sender is SlideWindow sw)
            {
                log.Debug($"Store Position: {sw.WindowState} L={sw.Left} W={sw.Width} T={sw.Top} H={sw.Height}");
                
                if(slideSettings == null)
                    throw new System.ArgumentNullException(nameof(slideSettings));  

                slideSettings.SlideState = (int) sw.WindowState;
                if (!sw.WindowState.Equals(WindowState.Minimized))
                {
                    slideSettings.SlideLeft = sw.Left;
                    slideSettings.SlideWidth = sw.Width;
                    slideSettings.SlideTop = sw.Top;
                    slideSettings.SlideHeight = sw.Height;

                    slideSettings.Save();
                }
            }
        }

        /// <summary>
        /// SourceInitialized response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal void WindowInitialized(object? sender, EventArgs e)
        {
            if (sender is SlideWindow sw && slideSettings != null)
            {
                log.Debug($"Restore Position: {(WindowState)slideSettings.SlideState} L={slideSettings.SlideLeft} W={slideSettings.SlideWidth} T={slideSettings.SlideTop} H={slideSettings.SlideHeight}");
                if (slideSettings.SlideWidth > 0 && slideSettings.SlideHeight > 0)
                {
                    sw.Left = slideSettings.SlideLeft;
                    sw.Width = slideSettings.SlideWidth;
                    sw.Top = slideSettings.SlideTop;
                    sw.Height = slideSettings.SlideHeight;

                    sw.WindowState = (WindowState)slideSettings.SlideState;
                }
            }
        }

        #endregion // Event handlers
    }
}
