using System.Windows;
using System.Windows.Controls;

namespace Monitor.View.Panels
{
    /// <summary>
    /// Interaction logic for LogPanel.xaml
    /// </summary>
    public partial class LogPanel : UserControl
    {
        public LogPanel()
        {
            InitializeComponent();
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            ((TextBox)sender).ScrollToEnd();
        }

        private void UIElement_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var textBox = (TextBox) sender;
            if (textBox.Visibility == Visibility.Visible)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}
