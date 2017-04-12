using System.Windows;
using GalaSoft.MvvmLight.Messaging;
using Monitor.Model.Messages;

namespace Monitor.View.NewSession
{
    /// <summary>
    /// Interaction logic for NewSessionWindow.xaml
    /// </summary>
    public partial class NewSessionWindow : Window
    {
        private readonly IMessenger _messenger;

        public NewSessionWindow()
        {
            InitializeComponent();

            // TODO: Implement dependency injection for the messenger
            _messenger = Messenger.Default;
            _messenger.Register<SessionOpenedMessage>(this, m => Close());
        }

        private void CloseButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
