using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Xml.Serialization;

namespace RagnarokInfo
{
    class Memory
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

        public IntPtr hProcess { get; set; }
        public IntPtr whProcess { get; set; }

        public Memory(ref bool firstRun, ref Process[] list, int i, ref ClientInfo client)
        {
            getProcesses(ref firstRun, ref list, ref i, ref client);
        }

        public void processChange(ref Process[] list, ref int i)
        {
            if (i >= list.Length)
                i = 0;
            hProcess = UnsafeNativeMethods.OpenProcess(0x0010, false, list[i].Id);
            whProcess = UnsafeNativeMethods.OpenProcess(0x1F0FFF, false, list[i].Id);
        }

        public void getProcesses(ref bool firstRun, ref Process[] list, ref int i, ref ClientInfo client)
        {
            try
            {
                readMemoryAddresses(ref client);
                list = Process.GetProcessesByName("ragexe");
            }
            catch (Exception e)
            {
                throwE(e);
            }

            if (firstRun)
            {
                try
                {
                    processChange(ref list, ref i);
                    firstRun = false;
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Process change failed. Debug code: 001");
                    throwE(e);
                }
            }
            else
            {
                try
                {
                    processChange(ref list, ref i);
                }
                catch (Exception e)
                {
                    System.Windows.MessageBox.Show("Process change failed. Debug code: 002");
                    throwE(e);
                }
            }
        }

        private void readMem(ClientInfo client, byte[] bBuff, byte[] jBuff, byte[] nBuff, byte[] bLvl, byte[] jLvl, byte[] bReq, byte[] jReq, byte[] logged, byte[] acct, ref int r)
        {
            try
            {
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16), acct, acct.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.LoggedIn, logged, logged.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.Name, nBuff, nBuff.Length, out r);

                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExp, bBuff, bBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseLevel, bLvl, bLvl.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExpRequired, bReq, bReq.Length, out r);

                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExp, jBuff, jBuff.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobLevel, jLvl, jLvl.Length, out r);
                UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExpRequired, jReq, jReq.Length, out r);
            }
            catch (Exception e)
            {
                throwE(e);
            }
        }

        public void clearMem(ClientInfo client)
        {
            byte[] clear = new byte[sizeof(int)];
            byte[] clear_string = new byte[24];
            int r;

            try
            {
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.Name, clear_string, clear_string.Length, out r);

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExp, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseLevel, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.BaseExpRequired, clear, clear.Length, out r);

                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExp, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobLevel, clear, clear.Length, out r);
                UnsafeNativeMethods.WriteProcessMemory(whProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.JobExpRequired, clear, clear.Length, out r);
            }
            catch (Exception e)
            {
                throwE(e);
            }
        }

        public void readMemoryAddresses(ref ClientInfo client)
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
                throwE(e);
            }
        }

        public object[] ReadInfo(ClientInfo client)
        {
            byte[] acct = new byte[sizeof(int)];
            byte[] logged = new byte[sizeof(int)];
            byte[] charName = new byte[24];
            byte[] baseBuff = new byte[sizeof(long)];
            byte[] baseLvl = new byte[sizeof(int)];
            byte[] baseReq = new byte[sizeof(long)];
            byte[] jobBuff = new byte[sizeof(long)];
            byte[] jobLvl = new byte[sizeof(int)];
            byte[] jobReq = new byte[sizeof(long)];
            int read = 0;

            readMem(client, baseBuff, jobBuff, charName, baseLvl, jobLvl, baseReq, jobReq, logged, acct, ref read);

            int account = BitConverter.ToInt32(acct, 0);
            bool logged_in = (BitConverter.ToInt32(logged, 0) > 0) ? true : false;
            String name = System.Text.Encoding.ASCII.GetString(charName).Trim('\0');
            long base_exp = BitConverter.ToInt64(baseBuff, 0);
            int base_Lvl = BitConverter.ToInt32(baseLvl, 0);
            long base_required = BitConverter.ToInt64(baseReq, 0);
            long job_exp = BitConverter.ToInt64(jobBuff, 0);
            int job_Lvl = BitConverter.ToInt32(jobLvl, 0);
            long job_required = BitConverter.ToInt64(jobReq, 0);

            object[] objArray = new object[] { account, logged_in, name, base_exp, base_Lvl, base_required, job_exp, job_Lvl, job_required };

            return objArray;
        }

        public int listHelper(ref Process[] list, ClientInfo client, Character_Info character, ObservableCollection<string> Items)
        {
            byte[] charName = new byte[24];
            int read = 0, newIndex = 0;
            IntPtr tempProcess = hProcess;

            try
            {
                for (int i = 0; i < list.Length; i++)
                {
                    hProcess = UnsafeNativeMethods.OpenProcess(0x0010, false, list[i].Id);
                    UnsafeNativeMethods.ReadProcessMemory(hProcess, Convert.ToUInt32(client.Account, 16) + client.Offsets.Character.Name, charName, charName.Length, out read);
                    Items.Add(System.Text.Encoding.ASCII.GetString(charName).Trim('\0'));
                    if (Items[i].ToString() == character.Name)
                        newIndex = i;
                }
            }
            catch (Exception e)
            {
                throwE(e);
            }
            finally
            {
                hProcess = tempProcess;
            }

            return newIndex;
        }

        private void throwE(Exception e)
        {
            System.Windows.MessageBox.Show("Memory exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
            MainWindow.killApp();
        }
    }
}
