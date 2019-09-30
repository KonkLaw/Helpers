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

		public static RouteApiSession GetSession(PrivateData privateData)
			=> new RouteApiSession(SessionCreator.CreateSession(privateData));
	}

	public class PrivateData
	{
		public string PhoneNumber;
		public string Pas;
	}

	class SessionInfo
	{
		public string PhpSesSid { get; }
		public string Uidh { get; }
		public string PhoneNumber { get; }

		public SessionInfo(string phpSesSid, string uidh, string phoneNumber)
		{
			PhpSesSid = phpSesSid;
			Uidh = uidh;
			PhoneNumber = phoneNumber;
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
		private readonly SessionInfo sessionInfo;

		internal RouteApiSession(SessionInfo sessionInfo)
		{
			this.sessionInfo = sessionInfo;
		}

		public ReadOnlyCollection<BusInfo> GetSchedule(bool fromMinskToStolbcy, DateTime tripDay)
		{
			WebRequest scheduleWebRequest = RouteByApiHelpers.GetRequest(
				RouteByApiHelpers.GetRequestBody(fromMinskToStolbcy, tripDay),
				RouteByApiHelpers.GetScheduleRequestHeaders(sessionInfo));
			string responseContent = WebApiHelper.GetResponseString(scheduleWebRequest).DecodeUnicide();
			ReadOnlyCollection<BusInfo> schedule = BusParser.ParseSchedule(responseContent);
			return schedule;
		}

		public void Buy()
		{
			BuyHelper.Buy();
		}
	}
}
