// Project: Start.xaml.cs
// Description: Interaction logic for the PetInfo window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 06 Feb 2023

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

namespace RagnarokInfo
{
    /// <summary>
    /// Interaction logic for Start.xaml
    /// </summary>
    public partial class ClientSelect : Window
    {
        private static RagnarokInfo.Settings settingsWindow;
        private static AppSettings loadedSettings;
        public ClientSelect()
        {
            InitializeComponent();
            loadSettings();
            settingsWindow = new RagnarokInfo.Settings();
            settingsWindow.Visibility = Visibility.Hidden;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Process [] ragList = Process.GetProcessesByName("ragexe");
            if (ragList.Length == 0)
            {
                if(settingsWindow.getPath() == "")
                {
                    System.Windows.MessageBox.Show("Ragnarok Online is not installed, exiting application.");
                    Application.Current.Shutdown();
                }

                gameStart();
            }
            MainWindow mainProg = new MainWindow(0);
        }
        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            settingsWindow.Visibility = settingsWindow.IsVisible ? Visibility.Hidden : Visibility.Visible;
        }

        private void loadSettings()
        {
            try
            {
                using (TextReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\Settings.xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(AppSettings));
                    loadedSettings = (AppSettings)serializer.Deserialize(reader);
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                Application.Current.Shutdown();
            }
        }

        public static AppSettings getUserSettings()
        {
            return loadedSettings;
        }

        public static void gameStart()
        {
            Process gameProcess = new Process();
            gameProcess.StartInfo.WorkingDirectory = settingsWindow.getPath();
            gameProcess.StartInfo.FileName = "ragexe.exe";
            gameProcess.StartInfo.Arguments = "1rag1 -eac-nop-loaded"; //REDACTED
            gameProcess.Start();
        }
    }
}
