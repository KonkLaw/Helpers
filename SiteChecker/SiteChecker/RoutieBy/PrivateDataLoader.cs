using System.Linq;

namespace SiteChecker.RoutieBy
{
	public static class PrivateDataLoader
	{
		private static string fileName = "PrivateData.xml";

		public static bool TryGetData(out PrivateData privateData)
		{
			if (DataStorage.TryGetDataOrSaveDefautl(out privateData, fileName, () => new PrivateData
			{
				PhoneNumber = "375112223344",
				Pas = "qwe"
			}) && IsValid(privateData))
			{
				return true;
			}
			else
			{
				privateData = default;
				return false;
			}
		}

		private static bool IsValid(PrivateData data)
		{
			return
				data.PhoneNumber != null &&
				data.PhoneNumber.Length == 12 &&
				data.PhoneNumber.All(char.IsDigit) &&
				data.PhoneNumber.Substring(0, 3) == "375" &&
				data.Pas != null &&
				data.Pas.Length > 3;
		}
	}
}
