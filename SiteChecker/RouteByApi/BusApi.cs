using System;
using System.Collections.ObjectModel;
using System.Net;
using WebApiUtils;

namespace RouteByApi
{
	public static class BusApi
    {
		private const string SiteUrl = "https://stolbcy-minsk.by/";
		public static Uri GetSiteUri() => new Uri(SiteUrl);

        public static bool TryGetCachedSession(SessionData sessionData, out RouteApiSession session)
        {
            var newSession = new RouteApiSession(sessionData);
            if (newSession.GetSchedule(true, DateTime.Now.AddDays(1).Date, out _, out _))
            {
                session = newSession;
                return true;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

		public static bool TryGetNewSession(LoginData loginData, out RouteApiSession session, out string errorMessage)
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

		internal RouteApiSession(SessionData sessionData)
		{
			SessionData = sessionData;
		}

		public bool GetSchedule(bool fromMinskToS, DateTime tripDay, out ReadOnlyCollection<BusInfo> schedule, out string errorMessage)
		{
			WebRequest scheduleWebRequest = RouteByApiHelpers.GetRequest(
				RouteByApiHelpers.GetRequestBody(fromMinskToS, tripDay),
				RouteByApiHelpers.GetScheduleRequestHeaders(SessionData));
			string responseContent = WebApiHelper.GetResponseString(scheduleWebRequest).DecodeUnicide();
			schedule = BusParser.ParseSchedule(responseContent);
            errorMessage = default;
            return true;
		}

		public void Buy()
		{
			BuyHelper.Buy();
		}
	}
}
