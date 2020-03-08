﻿using CredentialManager;

namespace CredentialHelper
{
	public class WindowsCredentialStorage : ICredentialStorage
    {
        private readonly string targetName = "Notifier.Bus";
		private readonly string userName = "Notifier.Bus.User";

		public void Save(Credentials credentials) =>
            CreadentialsHelper.Wrtire(new CredetiansWriteArg(
                targetName, null, userName, SerializationHelper<Credentials>.Serialize(credentials)));

        public bool TryLoad(out Credentials credentials)
        {
			if (!CreadentialsHelper.Load(targetName, out CredetiansReadArg credentialsReadArg))
			{
                credentials = default;
				return false;
			}
            credentials = SerializationHelper<Credentials>.Deserialize(credentialsReadArg.Password); ;
			return true;
		}
    }
}
