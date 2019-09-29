namespace SiteChecker
{
	class CookieStorage
	{
		private static string fileName = "SiteCredentials.xml";

		public static bool TryGetData(out Credentials credentials)
		{
			return DataStorage.TryGetDataOrSaveDefautl(out credentials, fileName, null);
		}

		public static void WriteData(Credentials credentials)
		{
			DataStorage.WriteData(credentials, fileName);
		}
	}
}