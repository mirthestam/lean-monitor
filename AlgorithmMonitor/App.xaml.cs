using System;
using System.Windows;
using System.Windows.Threading;

namespace QuantConnect.Lean.Monitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.Dispatcher.UnhandledException += DispatcherOnUnhandledException;
        }

        private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs dispatcherUnhandledExceptionEventArgs)
        {
            // TODO: Show exception in dialog
            Console.WriteLine(dispatcherUnhandledExceptionEventArgs.Exception);
        }
    }
}
