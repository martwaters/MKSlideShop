using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Net.Http;

namespace MKSlideShop
{
    /// <summary>
    /// Interaction logic for AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public String AboutText = "Hallo!";

        public AboutDialog()
        {
            InitializeComponent();
            Process p = Process.GetCurrentProcess();
            if (p.MainModule != null && !string.IsNullOrEmpty(p.MainModule.FileName))
            {
                using System.Drawing.Icon? ico = System.Drawing.Icon.ExtractAssociatedIcon(p.MainModule.FileName);
                if (ico != null)
                {
                    image.Source = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
            //AddHandler(Hyperlink.RequestNavigateEvent, new RoutedEventHandler(OnNavigationRequest));
        }       

        public void HLClick(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Hyperlink source)
            {
                try
                {
                    string url = source.NavigateUri.ToString().Replace("&", "^&");
                    Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

    }
}
