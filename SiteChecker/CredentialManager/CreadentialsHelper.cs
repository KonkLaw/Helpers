using System;

namespace CredentialManager
{
	public class CredetiansWriteArg
	{
		public readonly string TargetName;
		public readonly string Comment;
		public readonly string UserName;
		public readonly string Password;

		public CredetiansWriteArg(
			string targetName,
			string comment,
			string userName,
			string password)
		{
			TargetName = targetName;
			Comment = comment;
			UserName = userName;
			Password = password;
		}
	}

	public class CredetiansReadArg
	{
		public readonly string Comment;
		public readonly DateTime LastWriteTimeUtc;
		public readonly string Password;
		public readonly string UserName;

		public CredetiansReadArg(
			string comment,
			DateTime lastWriteTimeUtc,
			string password,
			string userName)
		{
			Comment = comment;
			LastWriteTimeUtc = lastWriteTimeUtc;
			Password = password;
			UserName = userName;
		}
	}

	public static class CreadentialsHelper
    {
		private const int FlagsDefault = 0;
		private const CredentialType CredentialTypeDefault = CredentialType.Generic;
        private const PersistanceType PersistanceTypeDefault = PersistanceType.LocalComputer;

        public static void Wrtire(CredetiansWriteArg credetiansWriteArg)
        {
			if (!CreadentialsManager.TryWrite(new CredentialInput(
				FlagsDefault,
				CredentialTypeDefault,
				credetiansWriteArg.TargetName,
				credetiansWriteArg.Comment,
				credetiansWriteArg.Password,
				PersistanceTypeDefault,
				credetiansWriteArg.UserName)))
				throw new InvalidOperationException("Can't write creadentials.");
        }

		public static bool Load(string targetName, out CredetiansReadArg credetiansReadArg)
		{
			if (!CreadentialsManager.Load(targetName, CredentialTypeDefault, out CredentialOutput credentialOutput))
			{
				credetiansReadArg = default;
				return false;
			}
			credetiansReadArg = new CredetiansReadArg(
				credentialOutput.Comment,
				credentialOutput.LastWriteTimeUtc,
				credentialOutput.Password,
				credentialOutput.UserName);
			return true;
		}

		public static void Delete(string targetName)
			=> CreadentialsManager.Delete(targetName, CredentialTypeDefault);
	}
}
