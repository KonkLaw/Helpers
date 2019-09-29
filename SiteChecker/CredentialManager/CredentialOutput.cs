using System;

namespace CredentialManager
{
	public class CredentialOutput
	{
		public readonly CredentialType CredentialType;
		public readonly string TargetName;
		public readonly string Comment;
		public readonly DateTime LastWriteTimeUtc;
		public readonly string Password;
		public readonly PersistanceType PersistanceType;
		public readonly string UserName;

		internal CredentialOutput(
			CredentialType credentialType,
			string targetName,
			string comment,
			DateTime lastWriteTimeUtc,
			string password,
			PersistanceType persistanceType,
			string userName)
		{
			CredentialType = credentialType;
			TargetName = targetName;
			Comment = comment;
			LastWriteTimeUtc = lastWriteTimeUtc;
			Password = password;
			PersistanceType = persistanceType;
			UserName = userName;
		}
	}
}
