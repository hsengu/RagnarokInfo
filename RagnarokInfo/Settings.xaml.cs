// Project: Settings.xaml.cs
// Description: Interaction logic for the settings window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 27 May 2021

using System;
using System.Windows;
using System.ComponentModel;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

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
            RagexeTextbox.IsReadOnly = true;
            RagexeTextbox.Text = ClientSelect.getUserSettings().appSettings.Filepath;
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

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = ClientSelect.getUserSettings().appSettings.Filepath;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                RagexeTextbox.Text = dialog.FileName;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            /*
            UserSettings temp = new UserSettings(ClientSelect.getUserSettings());
            System.Xml.Serialization.XmlSerializer writer =
                new System.Xml.Serialization.XmlSerializer(typeof(UserSettings));
            var path = Directory.GetCurrentDirectory() + "//SettingsTest.xml";
            System.IO.FileStream file = System.IO.File.Create(path);
            writer.Serialize(file, temp);
            file.Close();
            */
            System.Windows.MessageBox.Show("This isn't working yet.\nSet your path manually in Settings.xml");
        }

        public String getPath()
        {
            return RagexeTextbox.Text;
        }
    }
}
