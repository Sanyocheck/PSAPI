using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PSAPI
{
    class Program
    {
        #region APIS
        [DllImport("psapi")]
        private static extern bool EnumProcesses([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U4)] [In][Out] IntPtr[] processIds, UInt32 arraySizeBytes, [MarshalAs(UnmanagedType.U4)] out UInt32 bytesCopied);

        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, IntPtr dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr hObject);

        [DllImport("psapi.dll")]
        static extern uint GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);

        [DllImport("psapi.dll", SetLastError = true)]
        public static extern bool EnumProcessModules(IntPtr hProcess,
        [Out] IntPtr lphModule,
        uint cb,
        [MarshalAs(UnmanagedType.U4)] out uint lpcbNeeded);

        [DllImport("psapi.dll")]
        static extern uint GetModuleBaseName(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, [In] [MarshalAs(UnmanagedType.U4)] int nSize);
        #endregion
        #region ENUMS

        [Flags]
        enum ProcessAccessFlags : uint
        {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
        }
        #endregion

        static string PrintProcessName(IntPtr processID)
        {
            string sName = "";
            bool bFound = false;
            IntPtr hProcess = OpenProcess(ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VMRead, false, processID);
            if (hProcess != IntPtr.Zero)
            {
                StringBuilder szProcessName = new StringBuilder(260);
                IntPtr hMod = IntPtr.Zero;
                uint cbNeeded = 0;
                EnumProcessModules(hProcess, hMod, (uint)Marshal.SizeOf(typeof(IntPtr)), out cbNeeded);
                if (GetModuleBaseName(hProcess, hMod, szProcessName, szProcessName.Capacity) > 0)
                {
                    sName = szProcessName.ToString();
                    bFound = true;
                }

                CloseHandle(hProcess);
            }
            if (!bFound)
            {
                sName = "<unknown>";
            }
            return sName;
        }

        public static void GetModules()
        {
            Process myProcess = new Process();
            ProcessStartInfo myProcessStartInfo = new ProcessStartInfo("notepad.exe");
            myProcess.StartInfo = myProcessStartInfo;
            myProcess.Start();
            System.Threading.Thread.Sleep(1000);
            ProcessModule myProcessModule;
            ProcessModuleCollection myProcessModuleCollection = myProcess.Modules;
            Console.WriteLine("Properties of the modules associated with 'notepad' are:");


            for (int i = 0; i < myProcessModuleCollection.Count; i++)
            {
                myProcessModule = myProcessModuleCollection[i];
                Console.WriteLine("The moduleName is " + myProcessModule.ModuleName);
                Console.WriteLine("The " + myProcessModule.ModuleName + "'s File Name is: " + myProcessModule.FileName);
                Console.WriteLine("The " + myProcessModule.ModuleName + "'s base address is: " + myProcessModule.BaseAddress);
                Console.WriteLine("For " + myProcessModule.ModuleName + " Entry point address is: " + myProcessModule.EntryPointAddress);
            }
            myProcessModule = myProcess.MainModule;
            Console.WriteLine("The Main Module associated");
            Console.WriteLine("The process's main modulename is " + myProcessModule.ModuleName);
            Console.WriteLine("The process's main modulename  File Name is: " + myProcessModule.FileName);
            Console.WriteLine("The process's main modulename base address is: " + myProcessModule.BaseAddress);
            Console.WriteLine("The process's main modulename Entry point address is: " + myProcessModule.EntryPointAddress);
            myProcess.CloseMainWindow();
        }



        public static void Testy()
        {
            UInt32 arraySize = 9000;
            UInt32 arrayBytesSize = arraySize * sizeof(UInt32);
            IntPtr[] processIds = new IntPtr[arraySize];
            UInt32 bytesCopied;

            bool success = EnumProcesses(processIds, arrayBytesSize, out bytesCopied);

            Console.WriteLine("success={0}", success);
            Console.WriteLine("bytesCopied={0}", bytesCopied);

            if (!success)
            {
                Console.WriteLine("Boo!");
                return;
            }
            if (0 == bytesCopied)
            {
                Console.WriteLine("Nobody home!");
                return;
            }

            UInt32 numIdsCopied = bytesCopied >> 2; ;

            if (0 != (bytesCopied & 3))
            {
                UInt32 partialDwordBytes = bytesCopied & 3;

                Console.WriteLine("EnumProcesses copied {0} and {1}/4th DWORDS...  Please ask it for the other {2}/4th DWORD",
                    numIdsCopied, partialDwordBytes, 4 - partialDwordBytes);
                return;
            }

            for (UInt32 index = 0; index < numIdsCopied; index++)
            {
                string sName = PrintProcessName(processIds[index]);
                IntPtr PID = processIds[index];
                Console.WriteLine("Name '" + sName + "' PID '" + PID + "'");
            }
        }

        static void Main(string[] args)
        {
            GetModules();

            Console.ReadKey();
        }
    }
}
