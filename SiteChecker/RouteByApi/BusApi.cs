using System;
using System.Collections.ObjectModel;
using System.Net;
using WebApiUtils;

namespace RouteByApi
{
	public static class BusApi
    {
        public const int MinHours = 6;
        public const int MaxHours = 21;

		public static readonly ReadOnlyCollection<Station> Stations = new ReadOnlyCollection<Station>(
			new [] { new Station("Минск", "1"), new Station("Столбцы", "102") });

		private const string SiteUrl = "https://stolbcy-minsk.by/";
		public static Uri GetSiteUri() => new Uri(SiteUrl);

        public static bool TryGetCachedSession(in SessionData sessionData, out RouteApiSession session)
        {
            var newSession = new RouteApiSession(sessionData);
            var searchParameters = new SearchParameters(Stations[0], Stations[1], DateTime.Now.AddDays(1).Date);
            if (newSession.GetSchedule(in searchParameters, out _, out _))
            {
                session = newSession;
                return true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

		public static bool TryGetNewSession(in LoginData loginData, out RouteApiSession session, out string errorMessage)
        {
            if (SessionCreator.TryCreateSession(loginData, out SessionData sessionData, out string message))
            {
                errorMessage = default;
                session = new RouteApiSession(sessionData);
                return true;
            }
            else
            {
                errorMessage = message;
                session = default;
                return false;
            }
        }
    }

	public class Station
	{
		private readonly string name;
		internal readonly string Id;

		public Station(string name, string id)
		{
			this.name = name;
			Id = id;
		}

		public override string ToString() => name;
	}

    public readonly struct SessionData
    {
        public readonly string PhoneNumber;
        public readonly string PhpSesSid;
        public readonly string Uidh;

        public SessionData(string phoneNumber, string phpSesSid, string uidh)
        {
            PhoneNumber = phoneNumber;
            PhpSesSid = phpSesSid;
            Uidh = uidh;
        }
    }

	public readonly struct LoginData
	{
		public readonly string PhoneNumber;
		public readonly string Pas;

        public LoginData(string phoneNumber, string pas)
        {
            PhoneNumber = phoneNumber;
            Pas = pas;
        }
    }

    public readonly struct BusInfo
	{
		public readonly string Id;
		public readonly int TicketsCount;
		public readonly TimeSpan Time;

		public BusInfo(string id, TimeSpan time, int ticketsCount)
		{
			Id = id;
			TicketsCount = ticketsCount;
			Time = time;
		}
	}

	public class RouteApiSession
	{
		public readonly SessionData SessionData;

		internal RouteApiSession(in SessionData sessionData)
		{
			SessionData = sessionData;
		}

        private WebRequest GetRequest(string requestBody) =>
            RouteByApiHelpers.GetRequest(requestBody, RouteByApiHelpers.GetScheduleRequestHeaders(in SessionData));
        
        public bool GetSchedule(in SearchParameters searchParameters, out ReadOnlyCollection<BusInfo> schedule, out string errorMessage)
        {
            WebRequest scheduleWebRequest = GetRequest(RouteByApiHelpers.GetScheduleRequestBody(searchParameters));
			string responseContent = WebApiHelper.GetResponseString(scheduleWebRequest).DecodeUnicide();
			schedule = BusParser.ParseSchedule(responseContent);
            errorMessage = default;
            return true;
		}

        public void Order(in OrderParameters orderParameters)
        {
            string GetValueByKey(string content, string key)
            {
                const string valueMarker = "value=\\\"";
                int index = content.IndexOf(key);
                index = content.IndexOf(valueMarker, index) + valueMarker.Length;
                int endIndex = content.IndexOf("\\\"", index);
                return content.Substring(index, endIndex - index);
            }

            WebRequest preOrderRequest = GetRequest(RouteByApiHelpers.GerOrderRequestBody(in orderParameters));
            string preOrderResponse = WebApiHelper.GetResponseString(preOrderRequest).DecodeUnicide();

            string GetValue(string key) => GetValueByKey(preOrderResponse, key);

            string part = "%5B" + GetValue("aurb_id_add_parts")[1] + "%5D";
            string orderRequestContent =
                @"type=load_step2_save&" +
                $"aurb_id_add_et={GetValue("aurb_id_add_et")}&" +
                "aurb_id_add_num_space=1&" +
                $"aurb_id_add_tt={GetValue("aurb_id_add_tt")}&" +
                $"aurb_id_add_parts={part}&" +
                "aurb_points_finish[1]=9761&" +
                "aurb_point_start[1]=9716&" +
                $"aurb_id_add_df={GetValue("aurb_id_add_df")}&" +
                $"aurb_id_add_ds={GetValue("aurb_id_add_ds")}&" +
                "aurb_id_add_comment=&" +
                $"aurb_id_add_sl={GetValue("aurb_id_add_sl")}&" +
                $"aurb_id_add_save_points={GetValue("aurb_id_add_save_points")}&" +
                "aurb_id_service=144";

            WebRequest orderRequest = GetRequest(orderRequestContent);
            string orderResponse = WebApiHelper.GetResponseString(orderRequest).DecodeUnicide();
        }
    }

    public readonly struct OrderParameters
    {
        public readonly Station FromStation;
        public readonly Station ToStation;
        public readonly string BusId;

        public OrderParameters(Station fromStation, Station toStation, string busId)
        {
            FromStation = fromStation;
            ToStation = toStation;
            BusId = busId;
        }
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
}
