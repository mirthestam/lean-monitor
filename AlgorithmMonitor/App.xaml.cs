using System;
using System.Windows;
using System.Windows.Threading;

namespace Monitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Dispatcher.UnhandledException += DispatcherOnUnhandledException;
        }

        private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            // TODO: Show exception in dialog
            Console.WriteLine(dispatcherUnhandledExceptionEventArgs.Exception);
        }
    }
}
