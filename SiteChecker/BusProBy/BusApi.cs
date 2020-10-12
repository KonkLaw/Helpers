using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WebApiUtils;

namespace StolbcyMinskBy
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

	public class BusApi
    {
		public static readonly ReadOnlyCollection<Station> Stations = new ReadOnlyCollection<Station>(
			new[] { new Station("Минск", "101"), new Station("Столбцы", "102") });

		public static Uri GetSiteUri() => new Uri($"https://stolbcy-minsk.by/");

        public static bool GetSchedule(in SearchParameters requestParamters, out ReadOnlyCollection<BusInfo> schedule)
        {
			string requestBody = $"https://buspro.by/api/trip?s[company_id]=15&s[city_departure_id]={requestParamters.FromStation.Id}&s[city_destination_id]={requestParamters.ToStation.Id}&s[date_departure]={requestParamters.TripDay:yyyy-MM-dd}&actual=1";
            string responce = WebApiHelper.GetRequestGetBody(new Uri(requestBody));
			schedule = new ReadOnlyCollection<BusInfo>(new List<BusInfo>());
			return true;
		}
    }
}
