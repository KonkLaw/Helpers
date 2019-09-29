using CredentialHelper.Interface;
using CredentialManager;

namespace CredentialHelper
{
	public class WindowsCredentialStorage : ICredentialStorage
    {
		private const char Separator = ' ';
		private readonly string targetName = "route.by";
		private readonly string userName = "route_user";

		public void Save(UserInfo userInfo)
        {
			string privateData = userInfo.PrivateLogin + Separator + userInfo.Password;
			CreadentialsHelper.Wrtire(new CredetiansWriteArg(targetName, null, userName, privateData));
		}

        public bool TryLoad(out UserInfo userInfo)
        {
			if (!CreadentialsHelper.Load(targetName, out CredetiansReadArg credetiansReadArg))
			{
				userInfo = default;
				return false;
			}
			string privateData = credetiansReadArg.Password;
			int separatorIndex = privateData.IndexOf(Separator);
			userInfo = new UserInfo(
				privateData.Substring(0, separatorIndex),
				privateData.Substring(separatorIndex, privateData.Length - separatorIndex));
			return true;
		}
    }
}
