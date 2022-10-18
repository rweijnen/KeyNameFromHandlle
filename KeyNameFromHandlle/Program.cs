using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Windows;
using Microsoft.Win32;

namespace KeyNameFromHandlle
{
    class Program
    {


        static void Main(string[] args)
        {
            // open a reg key for testing, HKCU\SOFTWARE should always exist...
            using (RegistryKey rk = Registry.CurrentUser)
            {
                using (var subKey = rk.OpenSubKey("SOFTWARE"))
                {
                    // get the key name from the Handle...
                    string keyName = Winternl.GetKeyNameFromHandle(subKey.Handle.DangerousGetHandle());
                    Console.WriteLine("KeyName: {0}", keyName);
                }
            }

        }        
    }
}
