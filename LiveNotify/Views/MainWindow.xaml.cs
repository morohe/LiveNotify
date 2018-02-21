using System;
using System.Windows;

namespace LiveNotify.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
        : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            DataContext = null;
        }
    }
}
