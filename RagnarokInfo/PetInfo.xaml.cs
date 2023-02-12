// Project: PetInfo.xaml.cs
// Description: Interaction logic for the PetInfo window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 06 Feb 2023
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
        private static ClientInfo client = MainWindow.getMem;
        private static Homunculus homunculus = MainWindow.getHomunculus;
        private static Pet pet = MainWindow.getPet;
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

            if (pet.Hunger == 100 && setBeepThresholdP == false)
            {
                setBeepThresholdH = true;
                setBeepP();
            }
        }

        private void Homu_HungerUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            homunculus.Beep = (int)Homu_HungerUpDown.Value + 1;
            setBeepThresholdH = true;
            setBeepH();
        }

        private void comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            pet.Beep = (comboBox.SelectedIndex == 0) ? 76 : 26;
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
            byte[] hExp = new byte[sizeof(long)];
            byte[] hExpNeed = new byte[sizeof(long)];
            byte[] hOut = new byte[sizeof(int)];
            byte[] pnBuff = new byte[24];
            byte[] pLoy = new byte[sizeof(int)];
            byte[] pHun = new byte[sizeof(int)];
            byte[] pOut = new byte[sizeof(int)];
            int read = 0;

            readMem(hnBuff, hLoy, hHun, hExp, hExpNeed, hOut, pnBuff, pLoy, pHun, pOut, ref read);
            homunculus.Name = System.Text.Encoding.ASCII.GetString(hnBuff);
            homunculus.Loyalty = BitConverter.ToInt32(hLoy, 0);
            homunculus.Hunger = BitConverter.ToInt32(hHun, 0);
            homunculus.Exp = BitConverter.ToInt64(hExp, 0);
            homunculus.Exp_Required = BitConverter.ToInt64(hExpNeed, 0);
            homunculus.Out = BitConverter.ToInt32(hOut, 0);
            pet.Name = System.Text.Encoding.ASCII.GetString(pnBuff);
            pet.Loyalty = BitConverter.ToInt32(pLoy, 0);
            pet.Hunger = BitConverter.ToInt32(pHun, 0);
            pet.Out = BitConverter.ToInt32(pOut, 0);

            read = homunculus.Name.IndexOf('\0');
            homunculus.Name = homunculus.Name.Substring(0, read).Replace("\t","");
            read = pet.Name.IndexOf('\0');
            pet.Name = pet.Name.Substring(0, read).Replace("\t", "");

            doBeep();
        }

        private void readMem(byte[] hnBuff, byte[] hLoy, byte[] hHun, byte[] hExp, byte[] hExpNeed, byte[] hOutB, byte[] pnBuff, byte[] pLoy, byte[] pHun, byte[] pOutB, ref int r)
        {
            try
            {
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Homunculus.Name, hnBuff, hnBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Homunculus.Out, hOutB, hOutB.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Homunculus.Loyalty, hLoy, hLoy.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Homunculus.Exp, hExp, hExp.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Homunculus.ExpRequired, hExpNeed, hExpNeed.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Homunculus.Hunger, hHun, hHun.Length, out r);

                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) - client.Offsets.Pet.Name, pnBuff, pnBuff.Length, out r);;
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) - client.Offsets.Pet.Out, pOutB, pOutB.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) - client.Offsets.Pet.Loyalty, pLoy, pLoy.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) - client.Offsets.Pet.Hunger, pHun, pHun.Length, out r);

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

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) - client.Offsets.Pet.Out, clearPetOut, clearPetOut.Length, out r);
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

            if (homunculus.Out != 0 && isLoggedIn)
            {
                Homu_Name.Content = homunculus.Name.ToString();
                label.Content = homunculus.Hunger.ToString() + " / 100";
                label1.Content = homunculus.Loyalty.ToString() + " / 1000";
                label2.Content = (homunculus.Exp_Required - homunculus.Exp).ToString("N0");

                if (homunculus.Hunger > 91)
                {
                    Homu_Status.Content = "Stuffed";
                    Homu_Status.Foreground = System.Windows.Media.Brushes.LightPink;
                    label.Foreground = System.Windows.Media.Brushes.Red;
                }
                else if (homunculus.Hunger > 75)
                {
                    Homu_Status.Content = "Satisfied";
                    Homu_Status.Foreground = System.Windows.Media.Brushes.Orange;
                    label.Foreground = System.Windows.Media.Brushes.DarkOrange;
                }
                else if (homunculus.Hunger > 25)
                {
                    Homu_Status.Content = "Neutral";
                    Homu_Status.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                    label.Foreground = System.Windows.Media.Brushes.Blue;
                }
                else if (homunculus.Hunger > 10)
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

            if (pet.Out != -1 && isLoggedIn)
            {
                Pet_Name.Content = pet.Name.ToString();
                label4.Content = pet.Hunger.ToString() + " / 100";
                label5.Content = pet.Loyalty.ToString() + " / 1000";

                if (pet.Hunger > 90)
                {
                    Pet_Status.Content = "Stuffed";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.LightGreen;
                    label4.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (pet.Hunger > 75)
                {
                    Pet_Status.Content = "Satisfied";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.LightGreen;
                    label4.Foreground = System.Windows.Media.Brushes.Green;
                }
                else if (pet.Hunger > 25)
                {
                    Pet_Status.Content = "Neutral";
                    Pet_Status.Foreground = System.Windows.Media.Brushes.LightSkyBlue;
                    label4.Foreground = System.Windows.Media.Brushes.Blue;
                }
                else if (pet.Hunger > 10)
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
            homunculus.Hunger_Initial = homunculus.Beep;
            setBeepThresholdH = false;
        }

        private void setBeepP()
        {
            pet.Hunger_Initial = pet.Beep;
            setBeepThresholdP = false;
        }

        private void doBeep()
        {
            if (this.IsVisible == true)
            {
                if (homunculus.Hunger < homunculus.Hunger_Initial && homunculus.Out != 0 && isLoggedIn)
                {
                    homunculus.Hunger_Initial = homunculus.Hunger;
                    if (checkBox.IsChecked == false)
                        ding.Play();
                }
                else if (homunculus.Hunger >= homunculus.Beep && homunculus.Hunger_Initial != homunculus.Beep)
                    setBeepThresholdH = true;
                else if (homunculus.Hunger > homunculus.Hunger_Initial && homunculus.Hunger < homunculus.Beep)
                    homunculus.Hunger_Initial = homunculus.Hunger;

                if (pet.Hunger < pet.Hunger_Initial && pet.Out != -1 && isLoggedIn)
                {
                    pet.Hunger_Initial = pet.Hunger;
                    if (checkBox.IsChecked == false)
                        ding.Play();
                }
            }
        }

        private void resetHomun()
        {
            homunculus.Name = "";
            homunculus.Loyalty = homunculus.Hunger = homunculus.Hunger_Initial = homunculus.Out = 0;
            homunculus.Exp = homunculus.Exp_Required = 0;
            homunculus.Beep = (int)Homu_HungerUpDown.Value + 1;
            setBeepThresholdH = true;
            setBeepH();
        }

        private void resetPet()
        {
            pet.Name = "";
            pet.Loyalty = pet.Hunger = pet.Hunger_Initial = pet.Out = 0;
            pet.Beep = (comboBox.SelectedIndex == 0) ? 76 : 26;
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
