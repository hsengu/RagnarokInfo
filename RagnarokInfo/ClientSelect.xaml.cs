// Project: ClientSelect.xaml.cs
// Description: Interaction logic for the PetInfo window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 6 May 2017 at 12:34

using System.Windows;

namespace RagnarokInfo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ClientSelect : Window
    {
        public ClientSelect()
        {
            InitializeComponent();
        }

        private void Renewal_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainProg = new MainWindow(0);
            this.Visibility = Visibility.Hidden;
        }

        private void Classic_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainProg = new MainWindow(1);
            this.Visibility = Visibility.Hidden;
        }

        private void Sakray_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainProg = new MainWindow(2);
            this.Visibility = Visibility.Hidden;
        }
    }
}
