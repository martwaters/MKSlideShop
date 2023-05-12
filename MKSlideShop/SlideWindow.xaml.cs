using NLog;
using System.Collections.Generic;
using System.Windows;

namespace MKSlideShop
{
    /// <summary>
    /// Interaction logic for SlideWindow.xaml
    /// </summary>
    public partial class SlideWindow : Window 
    {
        private SlideWindowModel? viewModel;

        static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A window to display a single image, information and handlers
        /// </summary>
        public SlideWindow(ShowSettings settings, List<string> extensions)
        {
            InitializeComponent();
            viewModel = new SlideWindowModel(settings, extensions);
            viewModel.ImageCtrl = selectedImage;
            viewModel.BrowserPath = settings.BrowserPath;
            //?? viewModel.ImageCtrl.SetImageShader

            DataContext = viewModel;
            
            InitControls();

            Closing += viewModel.WindowClosing;
        }

        private void InitControls()
        {
            if (viewModel == null)
                throw new System.ArgumentNullException("ViewModel not allocated!");

            SourceInitialized += viewModel.WindowInitialized;

            butInfo.Click += viewModel.InfoClicked;
            butPause.Click += viewModel.PauseClicked;
            butRun.Click += viewModel.RunClicked;
            butBack.Click += viewModel.BackClicked;
            butExplore.Click += viewModel.ExploreClicked;
        }

        internal void StartShow()
        {
            log.Trace("Start Show");
            Show();

            if (viewModel == null)
                throw new System.ArgumentNullException("ViewModel not allocated!");

            viewModel.SlideWalk();
        }

    }
}
