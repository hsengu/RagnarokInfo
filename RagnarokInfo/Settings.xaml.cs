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
            RagexeTextbox.Text = getInstalledPath();
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

        private string getInstalledPath()
        {
            string registryLocation = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\";
            string installPath = "";

            using (Microsoft.Win32.RegistryKey keys = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryLocation))
            {
                foreach(string name in keys.GetSubKeyNames())
                {
                    Console.WriteLine(name);
                    using (Microsoft.Win32.RegistryKey subkey = keys.OpenSubKey(name))
                    {
                        try
                        {
                            if (subkey.GetValue("DisplayName").ToString() == "Ragnarok Online")
                            {
                                installPath = subkey.GetValue("InstallLocation").ToString();
                            }
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }

            return installPath;
        }

        public String getPath()
        {
            return RagexeTextbox.Text;
        }
    }
}
