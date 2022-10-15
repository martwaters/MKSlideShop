using System.Windows;

namespace MKSlideShop
{
    /// <summary>
    /// Interaction logic for SlideInfoWindow.xaml
    /// </summary>
    /// <remarks>SlideInfoWindow displays details about an image</remarks>
    public partial class SlideInfoWindow : Window
    {
        private SlideInfoWindowViewModel viewModel = new SlideInfoWindowViewModel();

        public SlideInfoWindow(string currentFile, string browserPath)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.CurrentFile = currentFile;
            viewModel.BrowserPath = browserPath;
            butExplorer.Click += viewModel.OpenExplorer;
        }

        internal void SetInfo(string currentFile)
        {
            viewModel.CurrentFile = currentFile;
        }
    }
}
