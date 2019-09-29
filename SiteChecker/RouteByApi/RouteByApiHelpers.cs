using System;
using System.Net;
using WebApiUtils;

namespace RouteByApi
{
	internal static class CommonHelper
	{
		public const string ContentType = @"application/x-www-form-urlencoded";
	}

	internal static class RouteByApiHelpers
	{
		private static readonly Uri UriNonAuthentification = new Uri(@"https://route.by/local/components/route/widget.order/templates/.default/ajax.php");

		public static RequestHeader[] GetScheduleRequestHeaders(SessionInfo sessionInfo)
		{
			const string origin = @"https://route.by";

			return new RequestHeader[]
			{
				new RequestHeader("Origin", origin),
				new RequestHeader("Bx-ajax", @"true"),
				WebApiHelper.CreateCookiesString(
						("PHPSESSID", sessionInfo.PhpSesSid),
						("BITRIX_SM_UIDH", sessionInfo.UIDH),
						("BITRIX_SM_UIDL", sessionInfo.PhoneNumber),
						("BX_USER_ID", "1ab83d800a7f88a92a431a2c51d7036a"),
						("BITRIX_SM_LOGIN", sessionInfo.PhoneNumber))
			};
		}

		public static string GetRequestBody(bool fromMinskToStolbtcy, DateTime tripDay)
		{
			const string Minsk = "1";
			const string Stolbcy = "102";

			string inBus;
			string outBus;
			if (fromMinskToStolbtcy)
			{
				inBus = Minsk;
				outBus = Stolbcy;
			}
			else
			{
				inBus = Stolbcy;
				outBus = Minsk;
			}

			string formatedDay = tripDay.ToString("dd.MM.yyyy");
			string infoRequestBody = $"type=load_list_order&select_in={inBus}&select_out={outBus}&date={formatedDay}&id_service=144&lines=%7B%220%22%3A192%2C%221%22%3A195%2C%222%22%3A323%2C%223%22%3A368%2C%224%22%3A446%2C%225%22%3A488%2C%226%22%3A491%7D";
			return infoRequestBody;
		}

		public static WebRequest GetRequest(string requestBody, RequestHeader[] headers)
		{
			HttpWebRequest httpRequest = WebApiHelper.GetPostRequestWithCookies(
				UriNonAuthentification, requestBody, CommonHelper.ContentType, headers);
			return httpRequest;
		}
	}

	internal static class SessionCreator
	{
		private static readonly Uri UriAuthentification = new Uri(@"https://route.by/local/components/route/widget.auth/templates/.default/ajax.php");

		private static string GetPhpSessid()
		{
			var guid = Guid.NewGuid();
			string phpsessid = guid.ToString().Replace("-", "");
			return phpsessid;
		}

		private static RequestHeader[] GetNewSesionRequestHeader(string phpSessid)
		{
			const string origin = @"https://route.by";
			return new RequestHeader[]
			{
				new RequestHeader("Origin", origin),
				new RequestHeader("Bx-ajax", @"true"),
				WebApiHelper.CreateCookiesString(("PHPSESSID", phpSessid))
			};
		}

		public static SessionInfo CreateSession(PrivateData privateData)
		{
			string phpSessid = GetPhpSessid();
			string request = $"type=auth_login&data=phone{GetPhone(privateData.PhoneNumber)}user_pass%3D{privateData.Pas}%26g-recaptcha-response%3D%26remember%3Don%26sms_registration%3D%26user_pass_new%3D%26user_pass_new_conf%3D%26remember_reg%3Don%26sms_recall%3D%26new_pass%3D%26new_pass_conf%3D";
			WebRequest normalRequest = GetAuthentificationRequest(request, GetNewSesionRequestHeader(phpSessid));
			string responce = WebApiHelper.GetResponseString(normalRequest, out WebHeaderCollection headers).DecodeUnicide();
			//if (responce.Contains("Необходимо пройти дополнительную проверку")) "Неверный пароль"
			//	!!!;
			string uihd = GetUIDH(headers.Get("Set-Cookie"));
			return new SessionInfo(phpSessid, uihd, privateData.PhoneNumber);

			//return new SessionInfo("638950b59f3fa9976aa0b447c179467d", "14cb59caff8fe1163817022dc594e1a8", "375CCxxxYYzz");
		}

		private static string GetPhone(string phone)
		{
			return $"%3D375%2B({phone.Substring(3, 2)})%2B{phone.Substring(5, 3)}-{phone.Substring(8, 2)}-{phone.Substring(10, 2)}%26";
		}

		private static string GetUIDH(string cookie)
		{
			const string forSearch = "BITRIX_SM_UIDH=";
			int startIndex = cookie.IndexOf(forSearch) + forSearch.Length;
			int endIndex = cookie.IndexOf(';', startIndex);
			return cookie.Substring(startIndex, endIndex - startIndex);
		}

		private static WebRequest GetAuthentificationRequest(string requestBody, RequestHeader[] headers)
		{
			HttpWebRequest httpRequest = WebApiHelper.GetPostRequestWithCookies(
				UriAuthentification, requestBody, CommonHelper.ContentType, headers);
			return httpRequest;
		}
	}
}
