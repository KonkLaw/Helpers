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
		private static readonly Uri UriNonAuthentication = new Uri(@"https://route.by/local/components/route/widget.order/templates/.default/ajax.php");

		public static RequestHeader[] GetScheduleRequestHeaders(SessionData sessionData)
		{
			const string origin = @"https://route.by";

			return new RequestHeader[]
			{
				new RequestHeader("Origin", origin),
				new RequestHeader("Bx-ajax", @"true"),
				WebApiHelper.CreateCookiesString(
						("PHPSESSID", sessionData.PhpSesSid),
						("BITRIX_SM_UIDH", sessionData.Uidh),
						("BITRIX_SM_UIDL", sessionData.PhoneNumber),
						("BX_USER_ID", "1ab83d800a7f88a92a431a2c51d7036a"),
						("BITRIX_SM_LOGIN", sessionData.PhoneNumber))
			};
		}

		public static string GetRequestBody(SearchParameters searchParameters)
		{
			if (searchParameters.FromStation == searchParameters.ToStation)
				throw new ArgumentException("From station equals to To sattion.");
			if (BusApi.Stations.IndexOf(searchParameters.FromStation) < 0 || BusApi.Stations.IndexOf(searchParameters.ToStation) < 0)
				throw new ArgumentException("Unknown stations.");

			string inBusStation = searchParameters.FromStation.Id;
			string outBusStation = searchParameters.ToStation.Id;
			string dayString = searchParameters.TripDay.ToString("dd.MM.yyyy");

			string infoRequestBody = $"type=load_list_order&select_in={inBusStation}&select_out={outBusStation}&date={dayString}&id_service=144&lines=%7B%220%22%3A192%2C%221%22%3A195%2C%222%22%3A323%2C%223%22%3A368%2C%224%22%3A446%2C%225%22%3A488%2C%226%22%3A491%7D";
			return infoRequestBody;
		}

		public static WebRequest GetRequest(string requestBody, RequestHeader[] headers)
		{
			HttpWebRequest httpRequest = WebApiHelper.GetPostRequestWithCookies(
				UriNonAuthentication, requestBody, CommonHelper.ContentType, headers);
			return httpRequest;
		}
	}

	internal static class SessionCreator
	{
		private static readonly Uri UriAuthentication = new Uri(@"https://route.by/local/components/route/widget.auth/templates/.default/ajax.php");

		private static RequestHeader[] GetNewSessionRequestHeader(string phpSessid)
		{
			const string origin = @"https://route.by";
			return new RequestHeader[]
			{
				new RequestHeader("Origin", origin),
				new RequestHeader("Bx-ajax", @"true"),
				WebApiHelper.CreateCookiesString(("PHPSESSID", phpSessid))
			};
		}

		public static bool TryCreateSession(LoginData loginData, out SessionData sessionData, out string message)
		{
            string GetPhpSessid()
            {
                Guid guid = Guid.NewGuid();
                return guid.ToString().Replace("-", "");
            }

            string GetUidhFromHeader(string cookie)
            {
                const string forSearch = "BITRIX_SM_UIDH=";
                int startIndex = cookie.IndexOf(forSearch) + forSearch.Length;
                int endIndex = cookie.IndexOf(';', startIndex);
                return cookie.Substring(startIndex, endIndex - startIndex);
            }

            string phpSessid = GetPhpSessid();
			string request = $"type=auth_login&data=phone{GetPhone(loginData.PhoneNumber)}user_pass%3D{loginData.Pas}%26g-recaptcha-response%3D%26remember%3Don%26sms_registration%3D%26user_pass_new%3D%26user_pass_new_conf%3D%26remember_reg%3Don%26sms_recall%3D%26new_pass%3D%26new_pass_conf%3D";
			WebRequest normalRequest = GetAuthenticationRequest(request, GetNewSessionRequestHeader(phpSessid));
			string response = WebApiHelper.GetResponseString(normalRequest, out WebHeaderCollection headers).DecodeUnicide();
            if (BusParser.ContainsError(response, out string errorMessage))
            {
                sessionData = default;
                message = errorMessage;
                return false;
            }
            else
            {
                string uihd = GetUidhFromHeader(headers.Get("Set-Cookie"));
                sessionData = new SessionData(loginData.PhoneNumber, phpSessid, uihd);
                message = default;
                //return new SessionInfo("638950b59f3fa9976aa0b447c179467d", "14cb59caff8fe1163817022dc594e1a8", "375CCxxxYYzz");
                return true;
            }
        }

		private static string GetPhone(string phone)
		{
			return $"%3D375%2B({phone.Substring(3, 2)})%2B{phone.Substring(5, 3)}-{phone.Substring(8, 2)}-{phone.Substring(10, 2)}%26";
		}

        private static WebRequest GetAuthenticationRequest(string requestBody, RequestHeader[] headers)
		{
			HttpWebRequest httpRequest = WebApiHelper.GetPostRequestWithCookies(
				UriAuthentication, requestBody, CommonHelper.ContentType, headers);
			return httpRequest;
		}
	}
}
