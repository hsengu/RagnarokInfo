// Project: Settings.xaml.cs
// Description: Interaction logic for the settings window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 5 May 2017 at 16:00

using System.Windows;
using System.ComponentModel;

namespace RagnarokInfo
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>

    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            switch(MainWindow.sClient)
            {
                default:
                case 0: ClientName.Content = "Renewal";
                    break;
                case 1: ClientName.Content = "Classic";
                    break;
                case 2: ClientName.Content = "Sakray";
                    break;
            }
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            OpacityValue.Content = ((int)OpacitySlider.Value).ToString();
        }

        public double getOpacity {
            get { return OpacitySlider.Value / 100.0; }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }
    }
}
