using NLog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Application = System.Windows.Application;

namespace MKSlideShop
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// <remarks>Logs essential events</remarks>
    public partial class App : Application
    {
        readonly Logger log = LogManager.GetLogger("MKSlideShow");

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.DispatcherUnhandledException += DispatchUnhandledException;

            log.Info("########### User {0} starting {1} ###########", Environment.UserName, AppDomain.CurrentDomain.FriendlyName);

        }

        private void DispatchUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Error("App.Unhandled: {0}", e.Exception.ToString());
            System.Windows.MessageBox.Show("App_DispatcherUnhandledException\r\n" + e.Exception, "MK Slide Show");

        }

        protected override void OnExit(ExitEventArgs e)
        {
            LogManager.GetCurrentClassLogger().Info("### Terminating {0} with code {1} ###", AppDomain.CurrentDomain.FriendlyName, e.ApplicationExitCode);

            this.DispatcherUnhandledException -= DispatchUnhandledException;
            base.OnExit(e);
        }

    }
}
