using System;
using System.Collections.ObjectModel;
using System.Net;
using WebApiUtils;

namespace AtlasbusByApi
{
	public class Station
	{
		internal readonly string Name;
		internal readonly string Id;

		internal Station(string name, string id)
		{
			Name = name;
			Id = id;
		}

		public override string ToString() => Name;
	}

	public readonly struct SearchParameters
	{
		public readonly Station FromStation;
		public readonly Station ToStation;
		public readonly DateTime TripDay;

		public SearchParameters(Station fromStation, Station toStation, DateTime tripDay)
		{
			FromStation = fromStation;
			ToStation = toStation;
			TripDay = tripDay;
		}
	}

	public readonly struct BusInfo
	{
		public readonly string Id;
		public readonly int TicketsCount;
		public readonly TimeSpan Time;
		public readonly string ServiceId;

		public BusInfo(string id, TimeSpan time, int ticketsCount, string serviceId)
		{
			Id = id;
			TicketsCount = ticketsCount;
			Time = time;
			ServiceId = serviceId;
		}

		public override string ToString() => $"time: {Time}";
	}

	public readonly struct LoginData
	{
		public readonly string PhoneNumber;
		public readonly string Password;

		public LoginData(string phoneNumber, string pas)
		{
			PhoneNumber = phoneNumber;
			Password = pas;
		}
	}

	public class BusApi
	{
		private static readonly Uri GetSessidUri = new Uri("https://atlasbus.by/index.php");
		private static readonly Uri ScheduleRequestUri = new Uri(@"https://atlasbus.by/local/components/route/user.order/templates/.default/ajax.php");
		private static readonly Uri AuthenticateUri = new Uri("https://atlasbus.by/local/components/route/user.auth/templates/.default/ajax.php");

		internal static PostRequestOptions GetPostRequestOptions(RequestHeader[]? headers = null)
		{
			const string contentType = @"application/x-www-form-urlencoded";
			const DecompressionMethods decompressionMethod = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			return new PostRequestOptions(contentType, decompressionMethod, headers);
		}

		public static bool GetSchedule(in SearchParameters searchParameters, out ReadOnlyCollection<BusInfo> schedule)
		{
			string dateString = searchParameters.TripDay.ToString("dd.MM.yyyy");
			string requestBodyTemplate =
				$"type=load_list_order&select_in={searchParameters.FromStation.Id}" +
				$"&select_out={searchParameters.ToStation.Id}" +
				$"&date={dateString}&strGET=";

			string response = WebApiHelper.PostRequestResponseBody(ScheduleRequestUri, requestBodyTemplate, GetPostRequestOptions());
			return ParsingHelper.ParseScheduleIsSessionOk(response, out schedule);
		}

		public static BusApiSession? TryLogin(in LoginData loginData, out string errorMessage)
		{
			// Get sessId
			const string phpsessidKey = "PHPSESSID";
			WebHeaderCollection headers = WebApiHelper.GetRequestGetHeaders(GetSessidUri);
			string? phpsessidOrNot = CookiesHelper.ExtractValueFromCookies(CookiesHelper.GetCookies(headers), phpsessidKey);
			if (phpsessidOrNot == null)
				throw new InvalidOperationException();

			var sessidHeader = new RequestHeader("Cookie", $"PHPSESSID={phpsessidOrNot};");
			// Try authenticate
			var login = loginData.PhoneNumber;

			string authenticationRequestBody =
				"type=auth_login&data=" +
				$"phone%3D{login[0..3]}%2520({login[3..5]})%2520{login[5..8]}-{login[8..10]}-{login[10..12]}%26" +
				$"user_pass%3D{loginData.Password}%26" +
				"g-recaptcha-response%3D%26" +
				"accept_agreement%3Don%26" +
				"sms_registration%3D%26" +
				"user_pass_new%3D%26" +
				"user_pass_new_conf%3D%26" +
				"remember_reg%3Don%26" +
				"sms_recall%3D%26" +
				"new_pass%3D%26" +
				"new_pass_conf%3D";

			var requestHeaders = new RequestHeader[] { sessidHeader };
			string authenticationResponse = WebApiHelper.PostRequestResponseBody(
				AuthenticateUri,
				authenticationRequestBody,
				GetPostRequestOptions(requestHeaders)).DecodeUnicide();
			if (ParsingHelper.ParseAuthenticationRespponcce(authenticationResponse, out errorMessage))
			{
				return new BusApiSession(requestHeaders);
			}
			else
			{
				return null;
			}
		}

		public static readonly ReadOnlyCollection<Station> Stations = new ReadOnlyCollection<Station>(
			new[] { BusApiSession. MinskStation, BusApiSession.StolbtcyStation });

		public static Uri GetSiteUri(Station fromStation, Station toStation) => new Uri($"https://atlasbus.ru/Маршруты/{fromStation.Name}/{toStation.Name}");
	}

	public readonly struct OrderParameters
	{
		public readonly Station FromStation;
		public readonly Station ToStation;
		public readonly string BusId;
		internal readonly string ServiceId;

		public OrderParameters(Station fromStation, Station toStation, string busId, string serviceId)
		{
			FromStation = fromStation;
			ToStation = toStation;
			BusId = busId;
			ServiceId = serviceId;
		}
	}

	public class BusApiSession
	{
		private static readonly Uri PreOrderUri = new Uri("https://atlasbus.by/local/components/route/user.order/templates/.default/ajax.php");
		private static readonly Uri OrderUri = new Uri("https://atlasbus.by/local/components/route/user.order/templates/.default/ajax.php");

		internal static readonly Station MinskStation = new Station("Минск", "101");
		internal static readonly Station StolbtcyStation = new Station("Столбцы", "200");

		private readonly RequestHeader[] requestHeaders;

		internal BusApiSession(RequestHeader[] requestHeaders)
		{
			this.requestHeaders = requestHeaders;
		}

		public void Order(in OrderParameters orderParameters)
		{
			string prereqBody = 
				$"type=load_step2&" +
				$"load_in_page=true&" +
				$"id_tt={orderParameters.BusId}&" +
				$"num_selected=1&" +
				$"select_in={orderParameters.FromStation.Id}" +
				$"&select_out={orderParameters.ToStation.Id}&" +
				$"sline=undefined&idtemp=undefined&timer=undefined";
			string preRequestResponse = WebApiHelper.PostRequestResponseBody(
				PreOrderUri,
				prereqBody,
				BusApi.GetPostRequestOptions(requestHeaders)).DecodeUnicide();

			string orderBody = GetOrderRequest(preRequestResponse, in orderParameters);
			string orderAnswer = WebApiHelper.PostRequestResponseBody(OrderUri, orderBody, BusApi.GetPostRequestOptions(requestHeaders));
			string orderAnswerDecoded = orderAnswer.DecodeUnicide();
		}

		private static string GetOrderRequest(string preOrderResponse, in OrderParameters orderParameters)
		{
			#region
			// MINSK-STOLBTCY
			// in
			//< option data - time - points =\"-15\" value=\"25480\" >автост. Юго-Западная<\/option>
			//< option data - time - points =\"-10\" value=\"25477\" >ост. м. Петровщина<\/option>
			//< option data - time - points =\"0\" value=\"25330\" selected=\"selected\">ост. м. Малиновка<\/option>
			//< option data - time - points =\"0\" value=\"28183\" >Щомыслица<\/option>
			//< option data - time - points =\"10\" value=\"25333\" >Дзедава карчма<\/option>
			//< option data - time - points =\"12\" value=\"25336\" >Черкасы<\/option>
			//< option data - time - points =\"13\" value=\"25339\" >Чечино<\/option>
			//< option data - time - points =\"15\" value=\"25342\" >Вязань<\/option>
			//< option data - time - points =\"20\" value=\"25345\" >Веста<\/option>\
			// out
			//< option data - time - points =\"0\" value=\"0\">не выбрано<\/option>
			//< option data - time - points =\"-15\" value=\"25348\" >Кучкуны<\/option>
			//< option data - time - points =\"-13\" value=\"25351\" >Яблоновка<\/option>
			//< option data - time - points =\"-10\" value=\"25354\" >Слобода<\/option>
			//< option data - time - points =\"0\" value=\"25357\" selected=\"selected\">Автовокзал<\/option>
			// STOLBTCY-MINSK
			// in
			//< option data - time - points =\"0\" value=\"25360\" >Автовокзал<\/option>
			//< option data - time - points =\"3\" value=\"25363\" selected=\"selected\">Прыпынак<\/option>
			//< option data - time - points =\"10\" value=\"25366\" >Слобода<\/option>
			//< option data - time - points =\"11\" value=\"25369\" >Яблоновка<\/option>
			//< option data - time - points =\"12\" value=\"25372\" >Кучкуны<\/option>
			//< option data - time - points =\"20\" value=\"25375\" >306-й км<\/option>
			//< option data - time - points =\"25\" value=\"25378\" >АЗС Энергетик<\/option>
			//< option data - time - points =\"32\" value=\"25381\" >Веста<\/option>
			//< option data - time - points =\"40\" value=\"25384\" >Дзедава Карчма<\/option>
			// out
			//<option data-time-points=\"0\" value=\"0\">не выбрано<\/option>
			//< option data - time - points =\"0\" value=\"25387\" selected=\"selected\">ост. м. Малиновка<\/option>
			//< option data - time - points =\"10\" value=\"25390\" >ост. м. Петровщина<\/option>
			//< option data - time - points =\"15\" value=\"25393\" >ост. автост. Юго-Западная<\/option>
			#endregion

			static string GetValueByKey(string content, string key)
			{
				const string valueMarker = "value=\\\"";
				int index = content.IndexOf(key);
				index = content.IndexOf(valueMarker, index) + valueMarker.Length;
				int endIndex = content.IndexOf("\\\"", index);
				return content[index..endIndex];
			}

			string GetValue(string key) => GetValueByKey(preOrderResponse, key);

			string startStationId;
			string finishStationId;
			if (orderParameters.FromStation == MinskStation && orderParameters.ToStation == StolbtcyStation)
			{
				startStationId = "25600";
				finishStationId = "25612";
			}
			else if (orderParameters.FromStation == StolbtcyStation && orderParameters.ToStation == MinskStation)
			{
				startStationId = "25612";
				finishStationId = "25600";
			}
			else
				throw new InvalidOperationException();

			string body =
				"type=load_step2_save&" +
				$"aurb_id_add_et={GetValue("aurb_id_add_et")}&" +
				"aurb_id_add_num_space=1&" +
				$"aurb_id_add_tt={GetValue("aurb_id_add_tt")}&" +
				$"aurb_id_add_parts=%5B{GetValue("aurb_id_add_parts")[1]}%5D&" +
				$"aurb_points_finish[1]={finishStationId}&" +
				 $"aurb_point_start[1]={startStationId}&" +
				 $"aurb_id_add_df={GetValue("aurb_id_add_df")}&" +
				 $"aurb_id_add_ds={GetValue("aurb_id_add_ds")}&" +
				 "aurb_id_add_comment=&" +
				 $"aurb_id_add_sl={GetValue("aurb_id_add_sl")}&" +
				 $"aurb_id_add_save_points={GetValue("aurb_id_add_save_points")}&" +
				 $"aurb_id_service={orderParameters.ServiceId}&" +
				 "type_pay_txt=driver&" +
				 "id_card=undefined&&arr_tariff[1]=0";
			return body;
		}
	}
}
