using CredentialHelper;

namespace Notifier.UtilTypes
{
	public static class StorageHelper
	{
		private static readonly WindowsCredentialStorage Storage = new WindowsCredentialStorage();

		public static void Save(Credentials credentials) => Storage.Save(credentials);

		public static bool TryLoad(out Credentials credentials) => Storage.TryLoad(out credentials);
	}
}