// Project: MainWindow.xaml.cs
// Description: Interaction logic for the main window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 06 Feb 2023

using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace RagnarokInfo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
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

        private static int clientSelect;
        private static Process[] ragList;
        private static ObservableCollection<string> Items = new ObservableCollection<string>();
        private static IntPtr hProcess, whProcess;
        private static Character_Info character = new Character_Info();
        private static Stopwatch stopWatch = new Stopwatch();
        private static double elapsed = 0;
        private static bool startNew = true;
        private static bool firstRun = true;
        private static bool refreshOnNextLog = true;
        private static double defaultLeftOpacity = 0, defaultRightOpacity = 0, leftOpacityOffset = 0, rightOpacityOffset = 0;
        private static System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private static System.Windows.Threading.DispatcherTimer listUpdateTimer = new System.Windows.Threading.DispatcherTimer();
        private static RagnarokInfo.PetInfo petInfo;
        private static RagnarokInfo.Settings settings;
        private static ClientInfo client;
        private static Calculator calc = new Calculator();

        public MainWindow(int cSelect)
        {
            InitializeComponent();
            Char_Combo.ItemsSource = Items;
            clientSelect = cSelect;
            getProcesses();
            ReadInfo(true);
            petInfo = new RagnarokInfo.PetInfo();
            settings = new RagnarokInfo.Settings();
            LeftOpacity = BExp_Percent.Opacity;
            RightOpacity = JExp_Percent.Opacity;
            petInfo.Visibility = Visibility.Hidden;
            settings.Visibility = Visibility.Hidden;
            timer.Tick += dispatcherTimer_Tick;
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Start();
            listUpdateTimer.Tick += listUpdate_Tick;
            listUpdateTimer.Interval = TimeSpan.FromSeconds(10);
            listUpdateTimer.Start();
            this.Visibility = Visibility.Visible;
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            petInfo.Visibility = (bool)checkBox.IsChecked ? Visibility.Visible : Visibility.Hidden;
        }

        private void Settings_Button_Click(object sender, RoutedEventArgs e)
        {
            settings.Visibility = settings.IsVisible ? Visibility.Hidden : Visibility.Visible;
        }

        private void New_Button_Click(object sender, RoutedEventArgs e)
        {
            AppSettings loadedSettings = ClientSelect.getUserSettings();
            ClientSelect.gameStart();
        }

        private void Char_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = Char_Combo.SelectedIndex;
            if (Char_Combo.HasItems == true)
            {
                hProcess = UnsafeNativeMethods.OpenProcess(0x0010, false, ragList[Char_Combo.SelectedIndex].Id);
                whProcess = UnsafeNativeMethods.OpenProcess(0x1F0FFF, false, ragList[Char_Combo.SelectedIndex].Id);
                ReadInfo(true);
                Char_Combo.SelectionChanged -= Char_Combo_SelectionChanged;
                Char_Combo.SelectedIndex = index;
                Char_Combo.SelectionChanged += Char_Combo_SelectionChanged;
            }
        }

        private void Refresh_Button_Click(object sender, RoutedEventArgs e)
        {
            ReadInfo(true);
        }

        private void listUpdate_Tick(object sender, EventArgs e)
        {
            getProcesses();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            ReadInfo(false);
            OutputInfo();
            timer.Start();
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("If you paid for this, you got scammed!.\n"
                                               + "Please do not redistribute without my permission!\n"
                                               + "ROinfo is coded by Hok Uy\n"
                                               + "© 2023 Build: 20230203");
        }

        private void ReadInfo(bool init)
        {
            byte[] baseBuff = new byte[sizeof(long)];
            byte[] jobBuff = new byte[sizeof(long)];
            byte[] charName = new byte[24];
            byte[] baseLvl = new byte[sizeof(int)];
            byte[] jobLvl = new byte[sizeof(int)];
            byte[] baseReq = new byte[sizeof(long)];
            byte[] jobReq = new byte[sizeof(long)];
            byte[] logged = new byte[sizeof(int)];
            byte[] acct = new byte[sizeof(int)];
            int read = 0;

            readMem(baseBuff, jobBuff, charName, baseLvl, jobLvl, baseReq, jobReq, logged, acct, ref read);
            String name = System.Text.Encoding.ASCII.GetString(charName).Trim('\0');
            long value = BitConverter.ToInt64(baseBuff, 0);
            long value_j = BitConverter.ToInt64(jobBuff, 0);
            long bLvl = BitConverter.ToInt32(baseLvl, 0);
            long jLvl = BitConverter.ToInt32(jobLvl, 0);
            long bReq = BitConverter.ToInt64(baseReq, 0);
            long jReq = BitConverter.ToInt64(jobReq, 0);
            int account = BitConverter.ToInt32(acct, 0);
            bool log = (BitConverter.ToInt32(logged, 0) > 0) ? true : false;

            if (!init)
            {
                character.Logged_In = log;
                //calcExp(value, value_j, bLvl, jLvl, bReq, jReq, log, account, name);
                calc.calcExp(character, stopWatch, ref elapsed, ref firstRun, ref refreshOnNextLog, ref startNew);
            }
            else
            {
                resetValues(value, value_j, bLvl, jLvl, bReq, jReq, log, account, name);
                getProcesses();
            }
        }

        private void OutputInfo()
        {
            String clientType;

            LeftOpacityOffset = RightOpacityOffset = settings.getOpacity;
            BExp_Percent.Opacity = LeftOpacity + LeftOpacityOffset;
            JExp_Percent.Opacity = RightOpacity + RightOpacityOffset;

            switch (clientSelect)
            {
                default:
                case 0:
                    clientType = "Renewal";
                    break;
                case 1:
                    clientType = "Classic";
                    break;
                case 2:
                    clientType = "Sakray";
                    break;
            }

            if (character.Logged_In)
            {
                this.Title = "ROinfo " + clientType + "     Lvl: " + character.Base.level_initial + "/" + character.Job.level_initial + "     " + getTime();
                Char_Name.Content = character.Name;
                label.Content = character.Base.actual.ToString("N0");
                label4.Content = character.Job.actual.ToString("N0");
                label1.Content = character.Base.is_max() ? "Max" : character.Base.remaining.ToString("N0");
                label5.Content = character.Job.is_max() ? "Max" : character.Job.remaining.ToString("N0");
                label2.Content = character.Base.gained.ToString("N0");
                label6.Content = character.Job.gained.ToString("N0");
                label3.Content = character.Base.hour.ToString("N0");
                label7.Content = character.Job.hour.ToString("N0");
                BExp_Percent.Content = character.Base.is_max() ? "MAX" : character.Base.percent.ToString("0.#") + "%";
                JExp_Percent.Content = character.Job.is_max() ? "MAX" : character.Job.percent.ToString("0.#") + "%";
            }
            else
            {
                if (character.Name == "")
                {
                    Char_Name.Content = "Not logged in";
                    label.Content = "0";
                    label4.Content = "0";
                    label1.Content = "0";
                    label5.Content = "0";
                    label2.Content = "0";
                    label6.Content = "0";
                    label3.Content = "0";
                    label7.Content = "0";
                    BExp_Percent.Content = "0%";
                    JExp_Percent.Content = "0%";
                }
                else
                {
                    Char_Name.Content = "Last logged in " + character.Name;
                    label.Content = character.Base.actual.ToString("N0");
                    label4.Content = character.Job.actual.ToString("N0");
                    label1.Content = character.Base.is_max() ? "Max" : character.Base.remaining.ToString("N0");
                    label5.Content = character.Job.is_max() ? "Max" : character.Job.remaining.ToString("N0");
                    label2.Content = character.Base.gained.ToString("N0");
                    label6.Content = character.Job.gained.ToString("N0");
                    label3.Content = character.Base.hour.ToString("N0");
                    label7.Content = character.Job.hour.ToString("N0");
                    BExp_Percent.Content = character.Base.is_max() ? "MAX" : character.Base.percent.ToString("0.#") + "%";
                    JExp_Percent.Content = character.Job.is_max() ? "MAX" : character.Job.percent.ToString("0.#") + "%";
                }
            }

            if (petInfo != null && petInfo.IsVisible == false)
                checkBox.IsChecked = false;
        }

/*        private void calcExp(long b_current, long j_current, long b_lvl_curr, long j_lvl_curr, long b_req, long j_req, bool log, int account, String name)
        {
            bool bLeveled = false, jLeveled = false;
            character.Base.actual = b_current;
            character.Job.actual = j_current;
            character.Base.remaining = b_req - b_current;
            character.Job.remaining = j_req - j_current;
            character.Base.percent = ((double)character.Base.actual / b_req) * 100;
            character.Job.percent = ((double)character.Job.actual / j_req) * 100;

            if (log == false && account != character.Account)
            {
                clearMem();
                ReadInfo(true);
                return;
            }
            else if (log == false)
            {
                if (name == character.Name && name != "")
                {
                    stopWatch.Stop();
                    return;
                }
                else if (name == "")
                    firstRun = true;
                refreshOnNextLog = true;
                ReadInfo(true);
                return;
            }

            if (startNew)
            {
                character.Base.previous_value = character.Base.initial;
                character.Job.previous_value = character.Job.initial;
                startNew = false;
            }
            else if (refreshOnNextLog)
            {
                ReadInfo(true);
                refreshOnNextLog = false;
                return;
            }
            else if (character.Base.gained > 0 || character.Job.gained > 0)
                stopWatch.Start();

            checkLeveled(ref b_current, ref b_lvl_curr, ref b_req, ref bLeveled, character.Base);
            checkLeveled(ref j_current, ref j_lvl_curr, ref j_req, ref jLeveled, character.Job);

            if (character.Base.gained != 0 || character.Job.gained != 0)
            {
                if (stopWatch.ElapsedMilliseconds == 0)
                {
                    stopWatch.Start();
                    System.Threading.Thread.Sleep(1);
                }
            }
            else
                return;

            double elapsedMilliseconds = Math.Max(0, stopWatch.ElapsedMilliseconds);
            elapsed = 3600000.0 / elapsedMilliseconds;
            character.Base.hour = character.Base.gained * elapsed;
            character.Job.hour = character.Job.gained * elapsed;
            character.Base.level_required = b_req;
            character.Job.level_required = j_req;
        }*/

        private void resetValues(long b_current, long j_current, long b_lvl_curr, long j_lvl_curr, long b_req, long j_req, bool log, int account, String name)
        {
            startNew = true;
            stopWatch.Reset();
            character.set(b_current, j_current, b_lvl_curr, j_lvl_curr, b_req, j_req, log, account, name);
            elapsed = 0;
        }

        private void readMem(byte[] bBuff, byte[] jBuff, byte[] nBuff, byte[] bLvl, byte[] jLvl, byte[] bReq, byte[] jReq, byte[] logged, byte[] acct, ref int r)
        {
            try
            {
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16), acct, acct.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.Name, nBuff, nBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.LoggedIn, logged, logged.Length, out r);

                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseLevel, bLvl, bLvl.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExp, bBuff, bBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExpRequired, bReq, bReq.Length, out r);

                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobLevel, jLvl, jLvl.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExp, jBuff, jBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExpRequired, jReq, jReq.Length, out r);
            }
            catch(Exception e) {
                System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
                Application.Current.Shutdown();
            }
        }

        private void clearMem()
        {
            byte[] clear = new byte[sizeof(int)];
            byte[] clear_string = new byte[24];
            int r;

            try
            {
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.Name, clear_string, clear_string.Length, out r);

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseLevel, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExp, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExpRequired, clear, clear.Length, out r);

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobLevel, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExp, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExpRequired, clear, clear.Length, out r);
            }
            catch(Exception e) {
                System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
                Application.Current.Shutdown();
            }
        }

        private void makeList()
        {
            byte[] charName = new byte[24];
            int read = 0, newIndex = 0;
            IntPtr tempProcess = hProcess;

            Char_Combo.SelectionChanged -= Char_Combo_SelectionChanged;
            Items.Clear();
            try
            {
                for (int i = 0; i < ragList.Length; i++)
                {
                    hProcess = UnsafeNativeMethods.OpenProcess(0x0010, false, ragList[i].Id);
                    UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.Name, charName, charName.Length, out read);
                    Items.Add(System.Text.Encoding.ASCII.GetString(charName).Trim('\0'));
                    if (Items[i].ToString() == character.Name)
                        newIndex = i;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
                Application.Current.Shutdown();
            }

            try { Char_Combo.SelectedIndex = newIndex; Char_Combo.SelectionChanged += Char_Combo_SelectionChanged; }
            catch (Exception e)
            { Char_Combo.SelectedIndex = 0; }
            finally { hProcess = tempProcess; }
        }

        private void getProcesses()
        {
            IntPtr tempProcess, tempWProcess;

            try
            {
                readMemoryAddresses();
                ragList = Process.GetProcessesByName("ragexe");
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message +
                                               "\nProgram will now terminate.");
                Application.Current.Shutdown();
            }
            

            if (firstRun)
            {
                try
                {
                    hProcess = UnsafeNativeMethods.OpenProcess(0x0010, false, ragList[0].Id);
                    whProcess = UnsafeNativeMethods.OpenProcess(0x1F0FFF, false, ragList[0].Id);
                    firstRun = false;
                    makeList();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
                    Application.Current.Shutdown();
                }
            }
            else
            {
                try
                {
                    tempProcess = UnsafeNativeMethods.OpenProcess(0x0010, false, ragList[0].Id);
                    tempWProcess = UnsafeNativeMethods.OpenProcess(0x1F0FFF, false, ragList[0].Id);
                    makeList();
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("An exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
                    Application.Current.Shutdown();
                }
            }
        }

        private void readMemoryAddresses()
        {
            try
            {
                using (TextReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\AddressList.xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ClientList));
                    client = ((ClientList)serializer.Deserialize(reader)).Client;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(e.Message);
                Application.Current.Shutdown();
            }
        }

        private String getTime()
        {
            String time = "";

            time += stopWatch.Elapsed.Hours.ToString("D2") + "h "
                 + stopWatch.Elapsed.Minutes.ToString("D2") + "m "
                 + stopWatch.Elapsed.Seconds.ToString("D2") + "s";

            return time;
        }

/*        private void checkLeveled(ref long current, ref long level_current, ref long exp_required, ref bool leveled, Character_Info.Exp_template exp)
        {
            if (level_current == (exp.level_initial + 1))
            {
                exp.gained += (exp.remaining - exp.initial - exp.gained + exp.previous_gained + current);
                exp.previous_value = exp.initial = current;
                exp.level_initial = level_current;
                exp.remaining = exp_required;
                leveled = true;
            }

            if (!leveled && (current - exp.previous_value) != 0)
            {
                exp.gained += current - exp.previous_value;
                exp.previous_value = current;
            }
            else if (leveled)
            {
                exp.previous_gained = exp.gained;
            }
        }*/

        public static IntPtr getProcess
        {
            get { return hProcess; }
        }

        public static IntPtr getWriteProcess
        {
            get { return whProcess; }
        }
        public static String getCharName
        {
            get { return character.Name; }
        }

        public static bool getLogged
        {
            get { return character.Logged_In; }
        }

        public static double LeftOpacity
        {
            get { return defaultLeftOpacity; }
            set { defaultLeftOpacity = value; }
        }
        public static double RightOpacity
        {
            get { return defaultRightOpacity; }
            set { defaultRightOpacity = value; }
        }
        public static double LeftOpacityOffset
        {
            get { return leftOpacityOffset; }
            set { leftOpacityOffset = value; }
        }
        public static double RightOpacityOffset
        {
            get { return rightOpacityOffset; }
            set { rightOpacityOffset = value; }
        }

        public static ClientInfo mem
        {
            get { return client; }
        }

        public static int sClient
        {
            get { return clientSelect; }
            set { clientSelect = value; }
        }

        public static Homunculus getHomunculus
        {
            get { return character.homunculus; }
        }

        public static Pet getPet
        {
            get { return character.pet; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}