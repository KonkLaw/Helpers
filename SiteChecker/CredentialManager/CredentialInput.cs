using System;

namespace CredentialManager
{
	class CredentialInput
	{
		public readonly int Flags;
		public readonly CredentialType CredentialType;
		public readonly string TargetName;
		public readonly string Comment;
		public readonly string Password;
		public readonly PersistanceType PersistanceType;
		public readonly string UserName;

		public CredentialInput(
			int flags,
			CredentialType credentialType,
			string targetName,
			string comment,
			string password,
			PersistanceType persistanceType,
			string userName)
		{
			if (string.IsNullOrEmpty(userName))
				throw new ArgumentException($"{userName} is null or empty");
			Flags = flags;
			CredentialType = credentialType;
			TargetName = targetName;
			Comment = comment;
			Password = password;
			PersistanceType = persistanceType;
			UserName = userName;
		}
	}
}
