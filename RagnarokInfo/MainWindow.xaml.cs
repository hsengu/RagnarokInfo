// Project: MainWindow.xaml.cs
// Description: Interaction logic for the main window of RagnarokInfo
// Coded and owned by: Hok Uy
// Last Source Update: 26 Dec 2021

using System;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

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
        private static String name_initial;

        private static ulong b_actual = 0,
            j_actual = 0,
            b_initial = 0,
            j_initial = 0,
            b_gained = 0,
            j_gained = 0,
            b_lvl_init = 0,
            j_lvl_init = 0,
            b_lvl_req = 0,
            j_lvl_req = 0,
            b_last = 0,
            j_last = 0,
            b_gained_prev = 0,
            j_gained_prev = 0,
            b_rem = 0,
            j_rem = 0,
            max_b = 185,
            max_j = 65;
        private static int acct_id = 0;
        private static double b_exphr = 0, j_exphr = 0, b_percent = 0, j_percent = 0;
        private static Stopwatch stopWatch = new Stopwatch();
        private static double elapsed = 0;
        private static bool startNew = true;
        private static bool firstRun = true;
        private static bool refreshOnNextLog = true;
        private static bool logged_in = false;
        private static double defaultLeftOpacity = 0, defaultRightOpacity = 0, leftOpacityOffset = 0, rightOpacityOffset = 0;
        private static System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
        private static System.Windows.Threading.DispatcherTimer listUpdateTimer = new System.Windows.Threading.DispatcherTimer();
        private static RagnarokInfo.PetInfo petInfo;
        private static RagnarokInfo.Settings settings;
        private static ClientList memAddr;

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
            Process gameProcess = new Process();
            gameProcess.StartInfo.WorkingDirectory = loadedSettings.appSettings.Filepath;
            gameProcess.StartInfo.FileName = "ragexe.exe";
            gameProcess.StartInfo.Arguments = ""; //REDACTED
            gameProcess.Start();
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
            String client;
            ReadInfo(false);
            LeftOpacityOffset = RightOpacityOffset = settings.getOpacity;
            BExp_Percent.Opacity = LeftOpacity + LeftOpacityOffset;
            JExp_Percent.Opacity = RightOpacity + RightOpacityOffset;

            switch(clientSelect)
            {
                default:
                case 0:
                    client = "Renewal";
                    break;
                case 1:
                    client = "Classic";
                    break;
                case 2:
                    client = "Sakray";
                    break;
            }

            if (logged_in)
            {
                this.Title = "ROinfo " + client + "     Lvl: " + b_lvl_init + "/" + j_lvl_init + "     " + getTime();
                Char_Name.Content = name_initial;
                label.Content = b_actual.ToString("N0");
                label4.Content = j_actual.ToString("N0");
                label1.Content = b_lvl_init == max_b ? "Max" : b_rem.ToString("N0");
                label5.Content = j_lvl_init == max_j ? "Max" : j_rem.ToString("N0");
                label2.Content = b_gained.ToString("N0");
                label6.Content = j_gained.ToString("N0");
                label3.Content = b_exphr.ToString("N0");
                label7.Content = j_exphr.ToString("N0");
                BExp_Percent.Content = b_lvl_init == max_b ? "MAX" : b_percent.ToString("0.#") + "%";
                JExp_Percent.Content = j_lvl_init == max_j ? "MAX" : j_percent.ToString("0.#") + "%";
            }
            else
            {
                if (name_initial == "")
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
                    Char_Name.Content = "Last logged in " + name_initial;
                    label.Content = b_actual.ToString("N0");
                    label4.Content = j_actual.ToString("N0");
                    label1.Content = b_lvl_init == max_b ? "Max" : b_rem.ToString("N0");
                    label5.Content = j_lvl_init == max_j ? "Max" : j_rem.ToString("N0");
                    label2.Content = b_gained.ToString("N0");
                    label6.Content = j_gained.ToString("N0");
                    label3.Content = b_exphr.ToString("N0");
                    label7.Content = j_exphr.ToString("N0");
                    BExp_Percent.Content = b_lvl_init == max_b ? "MAX" : b_percent.ToString("0.#") + "%";
                    JExp_Percent.Content = j_lvl_init == max_j ? "MAX" : j_percent.ToString("0.#") + "%";
                }
            }

            if (petInfo != null && petInfo.IsVisible == false)
                checkBox.IsChecked = false;
            timer.Start();
        }

        private void Info_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("If you paid for this, you got scammed!.\n"
                                               + "Please do not redistribute without my permission!\n"
                                               + "ROinfo is coded by Hok Uy\n"
                                               + "© 2022 Build: 20221023");
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
            ulong value = BitConverter.ToUInt64(baseBuff, 0);
            ulong value_j = BitConverter.ToUInt64(jobBuff, 0);
            ulong bLvl = BitConverter.ToUInt32(baseLvl, 0);
            ulong jLvl = BitConverter.ToUInt32(jobLvl, 0);
            ulong bReq = BitConverter.ToUInt64(baseReq, 0);
            ulong jReq = BitConverter.ToUInt64(jobReq, 0);
            int account = BitConverter.ToInt32(acct, 0);
            bool log = (BitConverter.ToInt32(logged, 0) > 0) ? true : false;

            if (!init)
            {
                logged_in = log;
                calcExp(value, value_j, bLvl, jLvl, bReq, jReq, log, account, name);
            }
            else
            {
                resetValues(value, value_j, bLvl, jLvl, bReq, jReq, log, account, name);
                getProcesses();
            }
        }

        private void calcExp(ulong b_current, ulong j_current, ulong b_lvl_curr, ulong j_lvl_curr, ulong b_req, ulong j_req, bool log, int account, String name)
        {
            bool bLeveled = false, jLeveled = false;
            b_actual = b_current;
            j_actual = j_current;
            b_rem = b_req - b_current;
            j_rem = j_req - j_current;
            b_percent = ((double)b_actual / b_req) * 100;
            j_percent = ((double)j_actual / j_req) * 100;

            if (log == false && account != acct_id)
            {
                clearMem();
                ReadInfo(true);
                return;
            }
            else if (log == false)
            {
                if (name == name_initial && name != "")
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
                b_last = b_initial;
                j_last = j_initial;
                startNew = false;
            }
            else if (refreshOnNextLog)
            {
                ReadInfo(true);
                refreshOnNextLog = false;
                return;
            }
            else if (b_gained > 0 || j_gained > 0)
                stopWatch.Start();

            if (b_lvl_curr == (b_lvl_init + 1))
            {
                b_gained += (b_lvl_req - b_initial - b_gained + b_gained_prev + b_current);
                b_last = b_initial = b_current;
                b_lvl_init = b_lvl_curr;
                b_lvl_req = b_req;
                bLeveled = true;
            }

            if (j_lvl_curr == (j_lvl_init + 1))
            {
                j_gained += (j_lvl_req - j_initial - j_gained + j_gained_prev + j_current);
                j_last = j_initial = j_current;
                j_lvl_init = j_lvl_curr;
                j_lvl_req = j_req;
                jLeveled = true;
            }

            if (!bLeveled && (b_current - b_last) != 0)
            {
                b_gained += b_current - b_last;
                b_last = b_current;
            }
            else if (bLeveled)
                b_gained_prev = b_gained;

            if (!jLeveled && (j_current - j_last) != 0)
            {
                j_gained += j_current - j_last;
                j_last = j_current;
            }
            else if (jLeveled)
                j_gained_prev = j_gained;

            if (b_gained != 0 || j_gained != 0)
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
            b_exphr = b_gained * elapsed;
            j_exphr = j_gained * elapsed;
            b_lvl_req = b_req;
            j_lvl_req = j_req;
        }

        private void resetValues(ulong b_current, ulong j_current, ulong b_lvl_curr, ulong j_lvl_curr, ulong b_req, ulong j_req, bool log, int account, String name)
        {
            startNew = true;
            stopWatch.Reset();
            name_initial = name;
            acct_id = account;
            b_actual = b_initial = b_current;
            j_actual = j_initial = j_current;
            b_lvl_init = b_lvl_curr;
            j_lvl_init = j_lvl_curr;
            b_lvl_req = b_req;
            j_lvl_req = j_req;
            b_rem = b_req - b_actual;
            j_rem = j_req - j_actual;
            elapsed = b_last = j_last = b_gained_prev = j_gained_prev = b_gained = j_gained = 0;
            b_percent = j_percent = b_exphr = j_exphr = 0;
            logged_in = log;
        }

        private void readMem(byte[] bBuff, byte[] jBuff, byte[] nBuff, byte[] bLvl, byte[] jLvl, byte[] bReq, byte[] jReq, byte[] logged, byte[] acct, ref int r)
        {
            try
            {
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16), acct, acct.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 23264, nBuff, nBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 24532, logged, logged.Length, out r);

                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 56, bLvl, bLvl.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 24, bBuff, bBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 32, bReq, bReq.Length, out r);

                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 64, jLvl, jLvl.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 48, jBuff, jBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 40, jReq, jReq.Length, out r);
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
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 23264, clear_string, clear_string.Length, out r);

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 56, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 24, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 32, clear, clear.Length, out r);

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 64, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 48, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 40, clear, clear.Length, out r);
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
                    UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(memAddr.clientList[clientSelect].Account, 16) + 23264, charName, charName.Length, out read);
                    Items.Add(System.Text.Encoding.ASCII.GetString(charName).Trim('\0'));
                    if (Items[i].ToString() == name_initial)
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
                    memAddr = (ClientList)serializer.Deserialize(reader);
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
            get { return name_initial; }
        }

        public static bool getLogged
        {
            get { return logged_in; }
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

        public static ClientList mem
        {
            get { return memAddr; }
        }

        public static int sClient
        {
            get { return clientSelect; }
            set { clientSelect = value; }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}