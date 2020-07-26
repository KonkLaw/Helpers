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
		private static readonly Uri GetSessidUri = new Uri("https://bx.atlasbus.by");
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
			// More info
			//"p_s":[
			//	{"name":"автост. Юго-Западная","geo":"53.87187925721996,27.498859558242902","line":[{"id_line":"1600","time":"-15","id":"25480"}]},
			//	{"name":"ост. м. Петровщина","geo":"53.863595133083486,27.484576503446384","line":[{"id_line":"1600","time":"-10","id":"25477"}]},
			//	{"name":"ост. м. Малиновка","geo":"53.84853247484787,27.473953032808673","line":[{"id_line":"1612","time":"0","id":"25600"},{"id_line":"1600","time":"0","id":"25330"}]},
			//	{"name":"Щомыслица","geo":"53.82488228017008,27.44450397784975","line":[{"id_line":"1600","time":"0","id":"28183"}]},
			//	{"name":"Дзедава карчма","geo":"53.781003662994245,27.362171029398947","line":[{"id_line":"1600","time":"10","id":"25333"}]},
			//	{"name":"Чечино","geo":"53.75221331684783,27.325649567537365","line":[{"id_line":"1612","time":"10","id":"25591"},{"id_line":"1600","time":"13","id":"25339"}]},
			//	{"name":"Черкасы","geo":"53.76110679625421,27.337183570216215","line":[{"id_line":"1612","time":"10","id":"25594"},{"id_line":"1600","time":"12","id":"25336"}]},
			//	{"name":"Уса","geo":"53.711673447633444,27.229147141617986","line":[{"id_line":"1612","time":"15","id":"25579"}]},
			//	{"name":"Хомичи","geo":"53.719819651432175,27.247665112656634","line":[{"id_line":"1612","time":"15","id":"25582"}]},
			//	{"name":"Красная горка","geo":"53.72784974754264,27.278177922410016","line":[{"id_line":"1612","time":"15","id":"25585"}]},
			//	{"name":"Вязань","geo":"53.73933347349912,27.309292919397834","line":[{"id_line":"1612","time":"15","id":"25588"},{"id_line":"1600","time":"15","id":"25342"}]},
			//	{"name":"Веста","geo":"53.662528883107996,27.14289114293807","line":[{"id_line":"1600","time":"20","id":"25345"},{"id_line":"1612","time":"20","id":"25576"}]}
			//	],
			//"p_f":[
			//	{"name":"Веста","geo":"53.66221462619769,27.143361448882597","line":[{"id_line":"1612","time":"-30","id":"25630"}]},
			//	{"name":"АЗС Энергетик","geo":"53.596597073575005,27.04303757930961","line":[{"id_line":"1612","time":"-22","id":"25627"}]},
			//	{"name":"306-й км","geo":"53.57416136523581,27.0089756638231","line":[{"id_line":"1612","time":"-20","id":"25624"}]},
			//	{"name":"Кучкуны","geo":"53.52344627424882,26.8425955564203","line":[{"id_line":"1600","time":"-15","id":"25348"},{"id_line":"1612","time":"-11","id":"25621"}]},
			//	{"name":"Яблоновка","geo":"53.5191496780142,26.83120153252503","line":[{"id_line":"1600","time":"-13","id":"25351"},{"id_line":"1612","time":"-11","id":"25618"}]},
			//	{"name":"Слобода","geo":"53.50930006474091,26.777978464847617","line":[{"id_line":"1600","time":"-10","id":"25354"},{"id_line":"1612","time":"-8","id":"25615"}]},
			//	{"name":"Прыпынак","geo":"53.488698555624914,26.740674863051208","line":[{"id_line":"1612","time":"-3","id":"25612"}]},
			//	{"name":"Автовокзал","geo":"53.48315250416034,26.7547430425093","line":[{"id_line":"1612","time":"0","id":"25609"},{"id_line":"1600","time":"0","id":"25357"}]},
			//	{"name":"Парк","geo":"53.48418938555796,26.741833485069204","line":[{"id_line":"1612","time":"3","id":"25606"}]},
			//	{"name":"Кинотеатр","geo":"53.476679049570244,26.73151605972805","line":[{"id_line":"1612","time":"5","id":"25603"}]}],"num_res":27}
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
