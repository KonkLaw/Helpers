using System.Net;

namespace WebApiUtils
{
	public static class CookiesHelper
	{
		private const string CookiesNameKey = "Set-Cookie";
		private const char CookiesSeparator = ';';

		public static string GetCookies(WebHeaderCollection headers) => headers.Get(CookiesNameKey);

		public static string? ExtractValueFromCookies(string cookies, string cookiesKey)
		{
			int startIndex = cookies.IndexOf(cookiesKey);
			if (startIndex < 0)
				return null;
			return cookies[(startIndex + cookiesKey .Length + 1)..cookies.IndexOf(CookiesSeparator)];
		}
	}
}