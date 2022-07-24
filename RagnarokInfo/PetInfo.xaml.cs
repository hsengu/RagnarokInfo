// Project: PetInfo.xaml.cs
// Description: Interaction logic for the PetInfo window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 11 Jun 2021
using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Media;

namespace RagnarokInfo
{
    /// <summary>
    /// Interaction logic for PetInfo.xaml
    /// </summary>
    public partial class PetInfo : Window
    {
        #region Winapi
        internal static class UnsafeNativeMethods
        {
            [DllImport("Kernel32.dll")]
            public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle,
                int dwProcessId);

            [DllImport("Kernel32.dll")]
            public static extern bool ReadProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer,
                int nSize, out int lpNumberOfBytesRead);

            [DllImport("Kernel32.dll")]
            public static extern bool WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer,
                int nSize, out int lpNumberOfBytesRead);
        }
        #endregion Winapi

        public class Sound : IDisposable
        {
            public SoundPlayer dingding;

            public Sound()
            {
                dingding = new SoundPlayer(RagnarokInfo.Properties.Resources.dingding);
            }

            public void Play()
            {
                dingding.Play();
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                    dingding.Dispose();
            }
        }

        private static IntPtr hProcess = MainWindow.getProcess;
        private static IntPtr whProcess = MainWindow.getWriteProcess;
        private static IntPtr initProcess = MainWindow.getProcess;
        private static bool isLoggedIn = MainWindow.getLogged;
        private static ClientList mem = MainWindow.mem;
        private static int sClient = MainWindow.sClient;
        private static String homu_name, pet_name;
        private static int homu_loy = 0, pet_loy = 0,
                           homu_hun = 0, pet_hun = 0,
                           homu_exp = 0, homu_need = 0,
                           homu_hun_init = 0, pet_hun_init = 0,
                           homu_out = 0, pet_out = 0;
        private static int homu_beep_when = 12, pet_beep_when = 76;
        private static bool setBeepThresholdH = true, setBeepThresholdP = true;
        private static System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private static Sound ding = new Sound();
        private static String char_name_initial = MainWindow.getCharName;

        public PetInfo()
        {
            InitializeComponent();
            comboBox.SelectedIndex = 0;
            Homu_HungerUpDown.ValueChanged += Homu_HungerUpDown_ValueChanged;
            getPetInfo();
            setBeepH();
            setBeepP();
            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            isLoggedIn = MainWindow.getLogged;
            hProcess = MainWindow.getProcess;
            whProcess = MainWindow.getWriteProcess;

            if (hProcess != initProcess)
            {
                initProcess = hProcess;
                getPetInfo();
                resetHomun();
                resetPet();
            }
            getPetInfo();
            outputToForm();

            if (setBeepThresholdH)
            {
                setBeepH();
            }

            if (pet_hun == 100 && setBeepThresholdP == false)
            {
                setBeepThresholdH = true;
                setBeepP();
            }
        }

        private void Homu_HungerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            homu_beep_when = (int)Homu_HungerUpDown.Value + 1;
            setBeepThresholdH = true;
            setBeepH();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pet_beep_when = (comboBox.SelectedIndex == 0) ? 76 : 26;
            setBeepThresholdP = true;
            setBeepP();
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void getPetInfo()
        {
            byte[] hnBuff = new byte[24];
            byte[] hLoy = new byte[sizeof(int)];
            byte[] hHun = new byte[sizeof(int)];
            byte[] hExp = new byte[sizeof(int)];
            byte[] hExpNeed = new byte[sizeof(int)];
            byte[] hOut = new byte[sizeof(int)];
            byte[] pnBuff = new byte[24];
            byte[] pLoy = new byte[sizeof(int)];
            byte[] pHun = new byte[sizeof(int)];
            byte[] pOut = new byte[sizeof(int)];
            int read = 0;

            readMem(hnBuff, hLoy, hHun, hExp, hExpNeed, hOut, pnBuff, pLoy, pHun, pOut, ref read);
            homu_name = System.Text.Encoding.ASCII.GetString(hnBuff);
            homu_loy = BitConverter.ToInt32(hLoy, 0);
            homu_hun = BitConverter.ToInt32(hHun, 0);
            homu_exp = BitConverter.ToInt32(hExp, 0);
            homu_need = BitConverter.ToInt32(hExpNeed, 0);
            homu_out = BitConverter.ToInt32(hOut, 0);
            pet_name = System.Text.Encoding.ASCII.GetString(pnBuff);
            pet_loy = BitConverter.ToInt32(pLoy, 0);
            pet_hun = BitConverter.ToInt32(pHun, 0);
            pet_out = BitConverter.ToInt32(pOut, 0);

            read = homu_name.IndexOf('\0');
            homu_name = homu_name.Substring(0, read).Replace("\t","");
            read = pet_name.IndexOf('\0');
            pet_name = pet_name.Substring(0, read).Replace("\t", "");

            doBeep();
        }

        private void readMem(byte[] hnBuff, byte[] hLoy, byte[] hHun, byte[] hExp, byte[] hExpNeed, byte[] hOutB, byte[] pnBuff, byte[] pLoy, byte[] pHun, byte[] pOutB, ref int r)
        {
            try
            {
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) + 14140, hnBuff, hnBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) + 14228, hLoy, hLoy.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) + 14244, hHun, hHun.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) + 14236, hExp, hExp.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) + 14240, hExpNeed, hExpNeed.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) + 14280, hOutB, hOutB.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) - 1520, pnBuff, pnBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) - 1468, pLoy, pLoy.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) - 1476, pHun, pHun.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) - 1484, pOutB, pOutB.Length, out r);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
                Application.Current.Shutdown();
            }
        }

        private void resetOutStatus()
        {
            try
            {
                byte[] clearPetOut = new byte[sizeof(int)];
                clearPetOut = BitConverter.GetBytes(-1);
                int r;

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(mem.clientList[sClient].Account, 16) - 1596, clearPetOut, clearPetOut.Length, out r);
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
                Application.Current.Shutdown();
            }
        }

        private void outputToForm()
        {
            Homu_Status.Opacity = MainWindow.LeftOpacity + MainWindow.LeftOpacityOffset;
            Pet_Status.Opacity = MainWindow.RightOpacity + MainWindow.RightOpacityOffset;
            this.Title = "PetInfo for " + MainWindow.getCharName;

            if (MainWindow.getCharName != char_name_initial)
            {
                resetOutStatus();
                char_name_initial = MainWindow.getCharName;
            }

            if (homu_out != 0 && isLoggedIn)
            {
                Homu_Name.Content = homu_name.ToString();
                label.Content = homu_hun.ToString() + " / 100";
                label1.Content = homu_loy.ToString() + " / 1000";
                label2.Content = (homu_need - homu_exp).ToString("N0");

                if (homu_hun > 91)
                {
                    Homu_Status.Content = "Stuffed";
                    Homu_Status.Foreground = System.Windows.Media.Brushes.LightPink;
                    label.Foreground = System.Windows.Media.Brushes.Red;
                }
                else if (homu_hun > 75)
                {
                    Homu_Status.Content = "Satisfied";
                    Homu_Status.Foreground = System.Windows.Media.Brushes.Orange;
                    label.Foreground = System.Windows.Media.Brushes.DarkOrange;
                }
                else if (homu_hun > 25)
                {
                    Homu_Status.Content = "Neutral";
                    Homu_Status.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                    label.Foreground = System.Windows.Media.Brushes.Blue;
                }
                else if (homu_hun > 10)
                {
                    Homu_Status.Content = "Hungry";
                    Homu_Status.Foreground = System.Windows.Media.Brushes.LightGreen;
                    label.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    Homu_Status.Content = "Starving";
                    Homu_Status.Foreground = label.Foreground = System.Windows.Media.Brushes.LightPink;
                    label.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                Homu_Name.Content = "N/A";
                label.Content = "0 / 100";
                label1.Content = "0 / 1000";
                label2.Content = "0 exp";
                Homu_Status.Content = "N/A";
                Homu_Status.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                label.Foreground = System.Windows.Media.Brushes.Black;
                resetHomun();
            }

            if (pet_out != -1 && isLoggedIn)
            {
                Pet_Name.Content = pet_name.ToString();
                label4.Content = pet_hun.ToString() + " / 100";
                label5.Content = pet_loy.ToString() + " / 1000";

                if (pet_hun > 90)
                {
                    Pet_Status.Content = "Stuffed";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.LightGreen;
                    label4.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (pet_hun > 75)
                {
                    Pet_Status.Content = "Satisfied";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.LightGreen;
                    label4.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (pet_hun > 25)
                {
                    Pet_Status.Content = "Neutral";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                    label4.Foreground = System.Windows.Media.Brushes.Blue;
                }
                else if (pet_hun > 10)
                {
                    Pet_Status.Content = "Hungry";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.Orange;
                    label4.Foreground = System.Windows.Media.Brushes.DarkOrange;
                }
                else
                {
                    Pet_Status.Content = "Very Hungry";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.LightPink;
                    label4.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            else
            {
                Pet_Name.Content = "N/A";
                label4.Content = "0 / 100";
                label5.Content = "0 / 1000";
                Pet_Status.Content = "N/A";
                Pet_Status.Foreground = System.Windows.Media.Brushes.BlueViolet;
                label4.Foreground = System.Windows.Media.Brushes.Black;
                resetPet();
            }
        }

        private void setBeepH()
        {
            homu_hun_init = homu_beep_when;
            setBeepThresholdH = false;
        }

        private void setBeepP()
        {
            pet_hun_init = pet_beep_when;
            setBeepThresholdP = false;
        }

        private void doBeep()
        {
            if (this.IsVisible == true)
            {
                if (homu_hun < homu_hun_init && homu_out != 0 && isLoggedIn)
                {
                    homu_hun_init = homu_hun;
                    if (checkBox.IsChecked == false)
                        ding.Play();
                }
                else if (homu_hun >= homu_beep_when && homu_hun_init != homu_beep_when)
                    setBeepThresholdH = true;
                else if (homu_hun > homu_hun_init && homu_hun < homu_beep_when)
                    homu_hun_init = homu_hun;

                if (pet_hun < pet_hun_init && pet_out != -1 && isLoggedIn)
                {
                    pet_hun_init = pet_hun;
                    if (checkBox.IsChecked == false)
                        ding.Play();
                }
            }
        }

        private void resetHomun()
        {
            homu_name = "";
            homu_loy = homu_hun = homu_hun_init = homu_exp = homu_need = homu_out = 0;
            homu_beep_when = (int)Homu_HungerUpDown.Value + 1;
            setBeepThresholdH = true;
            setBeepH();
        }

        private void resetPet()
        {
            pet_name = "";
            pet_loy = pet_hun = pet_hun_init = pet_out = 0;
            pet_beep_when = (comboBox.SelectedIndex == 0) ? 76 : 26;
            setBeepThresholdP = true;
            setBeepP();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            ding.Dispose();
            this.Visibility = Visibility.Hidden;
        }
    }
}
