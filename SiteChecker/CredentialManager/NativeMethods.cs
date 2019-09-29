using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CredentialManager
{
	internal class NativeMethods
    {
        public const int CRED_MAX_CREDENTIAL_BLOB_SIZE = 512;
        public const int CRED_MAX_STRING_LENGTH = 256;
        public const int CRED_MAX_USERNAME_LENGTH = 513;

      

        [DllImport("Advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredRead(string target, CredentialType type, int reservedFlag, out IntPtr CredentialPtr);

        [DllImport("Advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern bool CredWrite([In] ref NativeMethods.CREDENTIAL userCredential, [In] uint flags);

        [DllImport("Advapi32.dll", SetLastError = true)]
        internal static extern bool CredFree([In] IntPtr cred);

        [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode)]
        internal static extern bool CredDelete(StringBuilder target, CredentialType type, int flags);

        //[DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        //internal static extern bool CredEnumerateW(string filter, int flag, out uint count, out IntPtr pCredentials);

        //[DllImport("credui.dll")]
        //internal static extern NativeMethods.CredUIReturnCodes CredUIPromptForCredentials(ref NativeMethods.CREDUI_INFO creditUR, string targetName, IntPtr reserved1, int iError, StringBuilder userName, int maxUserName, StringBuilder password, int maxPassword, [MarshalAs(UnmanagedType.Bool)] ref bool pfSave, int flags);

        //[DllImport("credui.dll", CharSet = CharSet.Unicode)]
        //internal static extern NativeMethods.CredUIReturnCodes CredUIPromptForWindowsCredentials(ref NativeMethods.CREDUI_INFO notUsedHere, int authError, ref uint authPackage, IntPtr InAuthBuffer, uint InAuthBufferSize, out IntPtr refOutAuthBuffer, out uint refOutAuthBufferSize, ref bool fSave, int flags);

        //[DllImport("ole32.dll")]
        //internal static extern void CoTaskMemFree(IntPtr ptr);

        //[DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        //internal static extern bool CredPackAuthenticationBuffer(int dwFlags, StringBuilder pszUserName, StringBuilder pszPassword, IntPtr pPackedCredentials, ref int pcbPackedCredentials);

        //[DllImport("credui.dll", CharSet = CharSet.Auto)]
        //internal static extern bool CredUnPackAuthenticationBuffer(int dwFlags, IntPtr pAuthBuffer, uint cbAuthBuffer, StringBuilder pszUserName, ref int pcchMaxUserName, StringBuilder pszDomainName, ref int pcchMaxDomainame, StringBuilder pszPassword, ref int pcchMaxPassword);

        internal struct CREDENTIAL
        {
            public int Flags; // default
            public int Type;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetName;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Comment;
            public long LastWritten;
            public int CredentialBlobSize;
            public IntPtr CredentialBlob;
            public int Persist;
            public int AttributeCount;
            public IntPtr Attributes;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string TargetAlias;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string UserName;
        }

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        //public struct CREDUI_INFO
        //{
        //    public int cbSize;
        //    public IntPtr hwndParent;
        //    public string pszMessageText;
        //    public string pszCaptionText;
        //    public IntPtr hbmBanner;
        //}

        //[Flags]
        //internal enum WINXP_CREDUI_FLAGS
        //{
        //    INCORRECT_PASSWORD = 1,
        //    DO_NOT_PERSIST = 2,
        //    REQUEST_ADMINISTRATOR = 4,
        //    EXCLUDE_CERTIFICATES = 8,
        //    REQUIRE_CERTIFICATE = 16,
        //    SHOW_SAVE_CHECK_BOX = 64,
        //    ALWAYS_SHOW_UI = 128,
        //    REQUIRE_SMARTCARD = 256,
        //    PASSWORD_ONLY_OK = 512,
        //    VALIDATE_USERNAME = 1024,
        //    COMPLETE_USERNAME = 2048,
        //    PERSIST = 4096,
        //    SERVER_CREDENTIAL = 16384,
        //    EXPECT_CONFIRMATION = 131072,
        //    GENERIC_CREDENTIALS = 262144,
        //    USERNAME_TARGET_CREDENTIALS = 524288,
        //    KEEP_USERNAME = 1048576,
        //}

        //[Flags]
        //internal enum WINVISTA_CREDUI_FLAGS
        //{
        //    CREDUIWIN_GENERIC = 1,
        //    CREDUIWIN_CHECKBOX = 2,
        //    CREDUIWIN_AUTHPACKAGE_ONLY = 16,
        //    CREDUIWIN_IN_CRED_ONLY = 32,
        //    CREDUIWIN_ENUMERATE_ADMINS = 256,
        //    CREDUIWIN_ENUMERATE_CURRENT_USER = 512,
        //    CREDUIWIN_SECURE_PROMPT = 4096,
        //    CREDUIWIN_PACK_32_WOW = 268435456,
        //}

        //internal enum CredUIReturnCodes
        //{
        //    NO_ERROR = 0,
        //    ERROR_INVALID_PARAMETER = 87,
        //    ERROR_INSUFFICIENT_BUFFER = 122,
        //    ERROR_BAD_ARGUMENTS = 160,
        //    ERROR_INVALID_FLAGS = 1004,
        //    ERROR_NOT_FOUND = 1168,
        //    ERROR_CANCELLED = 1223,
        //    ERROR_NO_SUCH_LOGON_SESSION = 1312,
        //    ERROR_INVALID_ACCOUNT_NAME = 1315,
        //}

        //internal enum CREDErrorCodes
        //{
        //    SCARD_E_NO_SMARTCARD = -2146435060,
        //    SCARD_E_NO_READERS_AVAILABLE = -2146435026,
        //    SCARD_W_REMOVED_CARD = -2146434967,
        //    SCARD_W_WRONG_CHV = -2146434965,
        //    NO_ERROR = 0,
        //    ERROR_INVALID_PARAMETER = 87,
        //    ERROR_INVALID_FLAGS = 1004,
        //    ERROR_NOT_FOUND = 1168,
        //    ERROR_NO_SUCH_LOGON_SESSION = 1312,
        //    ERROR_BAD_USERNAME = 2202,
        //}

        internal sealed class CriticalCredentialHandle : CriticalHandleZeroOrMinusOneIsInvalid
        {
            internal CriticalCredentialHandle(IntPtr preexistingHandle)
            {
                SetHandle(preexistingHandle);
            }

            internal CREDENTIAL GetCredential()
            {
                if (!IsInvalid)
                    return (CREDENTIAL)Marshal.PtrToStructure(handle, typeof(CREDENTIAL));
                throw new InvalidOperationException("Invalid CriticalHandle!");
            }

            protected override bool ReleaseHandle()
            {
                if (IsInvalid)
                    return false;
                CredFree(handle);
                SetHandleAsInvalid();
                return true;
            }
        }
    }
}
