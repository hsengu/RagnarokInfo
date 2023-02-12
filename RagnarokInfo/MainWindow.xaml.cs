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
        private static int clientSelect;
        private static Process[] ragList;
        private static ObservableCollection<string> Items = new ObservableCollection<string>();
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
        private static Memory mem = new Memory(ref ragList, 0, ref client);

        public MainWindow(int cSelect)
        {
            InitializeComponent();
            Char_Combo.ItemsSource = Items;
            clientSelect = cSelect;

            getProcesses();
            mem.readMemoryAddresses(ref client);
            makeList();
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

        public void Char_Combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = Char_Combo.SelectedIndex;
            if (Char_Combo.HasItems == true)
            {
                mem.processChange(ragList, Char_Combo.SelectedIndex);
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

        public void ReadInfo(bool init)
        {
            object[] valuesArray = mem.ReadInfo(client);

            calc.setLevels(valuesArray);

            if (!init)
            {
                character.Logged_In = (bool)valuesArray[1];
                calcExp(stopWatch, ref elapsed, ref firstRun, ref refreshOnNextLog, ref startNew);
            }
            else
            {
                resetValues(valuesArray);
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

        public void calcExp(Stopwatch stopWatch, ref double elapsed, ref bool firstRun, ref bool refreshOnNextLog, ref bool startNew)
        {
            calc.setCharacterValues(ref character, calc);

            if (character.Logged_In == false && calc.account != character.Account)
            {
                mem.clearMem(client);
                ReadInfo(true);
                return;
            }
            else if (character.Logged_In == false)
            {
                if (calc.name == character.Name && calc.name != "")
                {
                    stopWatch.Stop();
                    return;
                }
                else if (calc.name == "")
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

            calc.checkLeveled(character.Base, calc.base_level);
            calc.checkLeveled(character.Job, calc.job_level);

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

            calc.time(character.Base, stopWatch, elapsed);
            calc.time(character.Job, stopWatch, elapsed);
        }

        private void resetValues(object[] valuesArray)
        {
            startNew = true;
            stopWatch.Reset();
            character.set(valuesArray);
            elapsed = 0;
        }

        private void makeList()
        {
            int newIndex = 0;
            Char_Combo.SelectionChanged -= Char_Combo_SelectionChanged;
            Items.Clear();
            newIndex = mem.listHelper(ref ragList, client, character, Items);

            try { Char_Combo.SelectedIndex = newIndex; Char_Combo.SelectionChanged += Char_Combo_SelectionChanged; }
            catch (Exception e)
            { Char_Combo.SelectedIndex = 0; }
        }

        private void getProcesses()
        {
            mem.getProcesses(firstRun, ref ragList, ref client);
            makeList();
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
            get { return mem.hProcess; }
        }

        public static IntPtr getWriteProcess
        {
            get { return mem.whProcess; }
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

        public static ClientInfo getMem
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

        public static void callClear()
        {
            mem.clearMem(client);
        }

        public static void killApp()
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Application.Current.Shutdown();
        }
    }
}