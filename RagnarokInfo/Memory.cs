using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        private static IntPtr hProcess { get; set; }
        private static IntPtr whProcess { get; set; }

        public Memory(Process[] ragList)
        {
            hProcess = UnsafeNativeMethods.OpenProcess(0x0010, false, ragList[Char_Combo.SelectedIndex].Id);
            whProcess = UnsafeNativeMethods.OpenProcess(0x1F0FFF, false, ragList[Char_Combo.SelectedIndex].Id);
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
                throwE(e);
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
                    throwE(e);
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
            catch (Exception e)
            {
                throwE(e);
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
            catch (Exception e)
            {
                throwE(e);
            }
        }

        private void readMemoryAddresses()
        {
            try
            {
                using (TextReader reader = new StreamReader(Directory.GetCurrentDirectory() + "\\AddressList.xml"))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(ClientList));
                    // Set event handlers for unknown nodes/attributes
                    serializer.UnknownNode += new XmlNodeEventHandler(serializer_UnknownNode);
                    serializer.UnknownAttribute += new XmlAttributeEventHandler(serializer_UnknownAttribute);
                    client = ((ClientList)serializer.Deserialize(reader)).Client;
                    /*account = memAddr;*/
                }
            }
            catch (Exception e)
            {
                throwE(e);
            }
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

        private void throwE(Exception e)
        {
            System.Windows.MessageBox.Show("Memory exception was thrown because:\n" + e.Message + "\nProgram will now terminate.");
        }
    }
}
