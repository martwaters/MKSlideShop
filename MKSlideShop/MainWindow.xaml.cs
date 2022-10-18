using NLog;
using System.Windows;

namespace MKSlideShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// <remarks>MainWindow shows the options and a button to start the show</remarks>
    public partial class MainWindow : Window
    {
        MainWindowModel viewModel = new MainWindowModel();

        static readonly Logger log = LogManager.GetCurrentClassLogger();

        public MainWindow()
        {
            log.Trace($"Start main window");
            InitializeComponent();
            viewModel.LoadSetting();

            InitControls();
        }
        private void InitControls()
        {
            DataContext = viewModel;

            butAddFolder.Click += viewModel.ButAddFolder;
            butStartShow.Click += viewModel.ButStartShow;
            pathListBox.KeyDown += viewModel.KeyDown;

            Closing += viewModel.MainClosing;
            SourceInitialized += viewModel.WindowInitialized;
        }
    }
}
