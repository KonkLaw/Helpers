using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CredentialManager
{
	internal class CreadentialsManager
    {
        public static bool TryWrite(CredentialInput credentialInput)
        {
			//    unmanagedCodePermission.Demand();

			if (string.IsNullOrEmpty(credentialInput.TargetName))
				throw new InvalidOperationException("Target must be specified.");

			if (credentialInput.Comment != null && credentialInput.Comment.Length > NativeMethods.CRED_MAX_STRING_LENGTH)
                throw new ArgumentOutOfRangeException(
                    $"The comment has exceeded {NativeMethods.CRED_MAX_STRING_LENGTH} characters.");

            if (credentialInput.UserName.Length > NativeMethods.CRED_MAX_USERNAME_LENGTH)
                throw new ArgumentOutOfRangeException(
                    $"The UserName has exceeded {NativeMethods.CRED_MAX_USERNAME_LENGTH} characters.");

            byte[] bytes = Encoding.Unicode.GetBytes(credentialInput.Password);
            if (bytes.Length > NativeMethods.CRED_MAX_CREDENTIAL_BLOB_SIZE)
                throw new ArgumentOutOfRangeException(
                    $"The password has exceeded {NativeMethods.CRED_MAX_CREDENTIAL_BLOB_SIZE} bytes.");

            NativeMethods.CREDENTIAL parameters;
            IntPtr credentialBlob = IntPtr.Zero;
            try
            {
                parameters = new NativeMethods.CREDENTIAL
                {
                    Flags = credentialInput.Flags,
                    Type = (int)credentialInput.CredentialType,
                    TargetName = credentialInput.TargetName,
                    Comment = credentialInput.Comment,
                    // LastWritten = // dont' work on write.
                    CredentialBlobSize = bytes.Length,
                    CredentialBlob = Marshal.StringToCoTaskMemUni(credentialInput.Password),
                    Persist = (int)credentialInput.PersistanceType,
                    //AttributeCount = 0, // attributes is not used.
                    // TargetAlias is not used.
                    UserName = credentialInput.UserName,
                };
                credentialBlob = parameters.CredentialBlob;
                if (!NativeMethods.CredWrite(ref parameters, 0U))
                    return false;
            }
            finally
            {
                Marshal.FreeCoTaskMem(credentialBlob);
            }
            return true;
        }

        public static bool Load(string targetName, CredentialType credentialType, out CredentialOutput credentialOutput)
        {
			//unmanagedCodePermission.Demand();

			if (string.IsNullOrEmpty(targetName))
				throw new InvalidOperationException("Target must be specified.");

            if (!NativeMethods.CredRead(targetName, credentialType, 0, out IntPtr credentialPtr))
            {
                credentialOutput = default;
                return false;
            }
            using (var credentialHandle = new NativeMethods.CriticalCredentialHandle(credentialPtr))
            {
                credentialOutput = LoadInternal(credentialHandle.GetCredential(), targetName);
            }
            return true;
        }

		static CredentialOutput LoadInternal(NativeMethods.CREDENTIAL credential, string targetName)
			=> new CredentialOutput(
				credentialType: (CredentialType)credential.Type,
				targetName: targetName,
				comment: credential.Comment,
				DateTime.FromFileTimeUtc(credential.LastWritten),
				credential.CredentialBlobSize > 0
					? Marshal.PtrToStringUni(credential.CredentialBlob, credential.CredentialBlobSize / 2)
					: null,
				(PersistanceType)credential.Persist,
				credential.UserName
				);

		public static bool Delete(string targetName, CredentialType credentialType)
        {
            //unmanagedCodePermission.Demand();
            if (string.IsNullOrEmpty(targetName))
                throw new InvalidOperationException("Target must be specified.");
            return NativeMethods.CredDelete(
				string.IsNullOrEmpty(targetName) ? new StringBuilder() : new StringBuilder(targetName),
				credentialType,
				0);
        }
    }
}
