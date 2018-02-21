using System;
using System.Windows;

namespace LiveNotify.Views
{
    /// <summary>
    /// Interaction logic for FirstSetupWindow.xaml
    /// </summary>
    public partial class FirstSetupWindow : Window
    {
        public FirstSetupWindow()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            (DataContext as IDisposable)?.Dispose();
            DataContext = null;
        }

        public string DialogDataResult { get; set; }
    }
}
