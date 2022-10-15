using NLog;
using System.Windows;

namespace MKSlideShop
{
    /// <summary>
    /// Interaction logic for SlideWindow.xaml
    /// </summary>
    public partial class SlideWindow : Window 
    {
        private SlideWindowModel viewModel = new SlideWindowModel();

        static readonly Logger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// A window to display a single image, information and handlers
        /// </summary>
        public SlideWindow(string browserPath)
        {
            InitializeComponent();
            viewModel.ImageCtrl = selectedImage;
            viewModel.BrowserPath = browserPath;
            //?? viewModel.ImageCtrl.SetImageShader
            DataContext = viewModel;
            InitControls();

            this.Closing += viewModel.WindowClosing;
        }

        private void InitControls()
        {
            butInfo.Click += viewModel.InfoClicked;
            butPause.Click += viewModel.PauseClicked;
            butRun.Click += viewModel.RunClicked;
            butBack.Click += viewModel.BackClicked;
            butBack.Content = "<";
            butExplore.Click += viewModel.ExploreClicked;
        }

        internal void StartShow(ShowSettings settings)
        {
            log.Trace("Start Show");
            Show();

            viewModel.SlideWalk(settings);
        }

    }
}
