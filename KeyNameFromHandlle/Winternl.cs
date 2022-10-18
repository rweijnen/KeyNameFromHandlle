using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Windows
{
    public static class Winternl
    {
        private const UInt32 STATUS_SUCCESS = 0;
        private const UInt32 STATUS_BUFFER_OVERFLOW = 0x80000005;
        private const UInt32 STATUS_BUFFER_TOO_SMALL = 0xC0000023;

        public static string GetKeyNameFromHandle(IntPtr KeyHandle)
        {           
            UInt32 nts;
            UInt32 resLen = 0;
            IntPtr buffer = IntPtr.Zero;
            string result = String.Empty;
            nts = NtQueryKey(KeyHandle, KEY_INFORMATION_CLASS.KeyNameInformation, IntPtr.Zero, 0, ref resLen);
            if (nts == STATUS_BUFFER_TOO_SMALL | nts == STATUS_BUFFER_OVERFLOW)
            {
                // allocate memory for the buffer
                buffer = Marshal.AllocHGlobal((int)resLen);
                try
                {
                    nts = NtQueryKey(KeyHandle, KEY_INFORMATION_CLASS.KeyNameInformation, buffer, resLen, ref resLen);
                    if (nts == STATUS_SUCCESS)
                    {
                        // read name length from the buffer
                        int nameLen = Marshal.ReadInt32(buffer);
                        
                        // name is an c-style "ANYSIZE" char array, as we don't know the size in advance we cannot use
                        // PtrToStructure, so we get an IntPtr pointing to the char array
                        IntPtr name = new IntPtr(buffer.ToInt64() + sizeof(UInt32));
                        
                        // and Marshal it to string
                        result = Marshal.PtrToStringUni(name, nameLen / sizeof(char));
                        if (result.Contains("\\REGISTRY\\MACHINE"))
                        {
                            result = result.Replace("\\REGISTRY\\MACHINE", "HKLM");
                        }
                        else
                        {
                            using (WindowsIdentity wi = WindowsIdentity.GetCurrent())
                            {
                                if (result.Contains(wi.User.Value))
                                {
                                    result = result.Replace("\\REGISTRY\\USER\\" + wi.User.Value, "HKCU");
                                }
                            }
                        }
                    }
                }
                
                // using a finally to make sure we always free our buffer
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
            
            return result;
        }

        private enum KEY_INFORMATION_CLASS
        {
            KeyBasicInformation,
            KeyNodeInformation,
            KeyFullInformation,
            KeyNameInformation,
            KeyCachedInformation,
            KeyFlagsInformation,
            KeyVirtualizationInformation,
            KeyHandleTagsInformation,
            KeyTrustInformation,
            KeyLayerInformation,
            MaxKeyInfoClass  // MaxKeyInfoClass should always be the last enum
        }

        private struct KEY_NAME_INFORMATION
        {
            private UInt32 NameLength;
            private char Name;
        }

        [DllImport("ntdll.dll", EntryPoint="NtQueryKey", SetLastError=false)]
        private static extern UInt32 NtQueryKey(IntPtr KeyHandle, KEY_INFORMATION_CLASS KeyInformationClass, IntPtr KeyInformation, UInt32 Length, ref UInt32 ResultLength);


    }
}
