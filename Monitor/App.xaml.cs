using System;
using System.Windows;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Messaging;
using Monitor.Model;
using Monitor.Model.Messages;

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
            Console.WriteLine(dispatcherUnhandledExceptionEventArgs.Exception);

            // Set the exception as handled.
            try
            {
                var messenger = Messenger.Default;
                var ex = dispatcherUnhandledExceptionEventArgs.Exception;
                messenger.Send(new LogEntryReceivedMessage(DateTime.Now, $"Caught unhandled exception: {ex.Message} at {ex.StackTrace }", LogItemType.Monitor));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);                
            }
            dispatcherUnhandledExceptionEventArgs.Handled = true;
        }
    }
}
