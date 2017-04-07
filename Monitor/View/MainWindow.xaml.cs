using System;
using System.Diagnostics;
using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Monitor.Model.Messages;
using Monitor.ViewModel;

namespace Monitor.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IMessenger _messenger;

        public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

        public MainWindow()
        {            
            InitializeComponent();

            // TODO: Implement dependency injection for windows
            _messenger = Messenger.Default; 
            _messenger.Register<ShowNewSessionWindowMessage>(this, message => ShowWindowDialog<NewSession.NewSessionWindow>());
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Tell the viewModel we have loaded and we can process data
            ViewModel.Initialize();
        }

        private void ShowWindowDialog<T>() where T : Window
        {
            var window = Activator.CreateInstance<T>();
            window.Owner = this;
            window.ShowDialog();
        }

        private static void OpenLink(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ShowAboutButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowWindowDialog<AboutWindow>();
        }

        private void BrowseLeanGithubMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenLink("https://github.com/QuantConnect/Lean");
        }

        private void BrowseMonitorGithubMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenLink("https://github.com/mirthestam/lean-monitor");
        }

        private void BrowseChartingDocumentationMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            OpenLink("https://www.quantconnect.com/docs#Charting");
        }

        private void MainWindow_OnDrop(object sender, DragEventArgs e)
        {
            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (fileNames != null) ViewModel.HandleDroppedFileName(fileNames[0]);
        }

        private void MainWindow_OnDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (fileNames != null && fileNames.Length == 1)
                {
                    // Drag drop validated.
                    return;
                }
            }

            // Drag drop invalidated.
            e.Effects = DragDropEffects.None;
            e.Handled = true;

        }
    }
}
