using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Newtonsoft.Json;
using WebApiUtils;

namespace BusStolbtcy
{
	public class Station
	{
		internal readonly string Name;
		internal readonly string Id;
		internal readonly string DefaultPickStationId;

        internal Station(string name, string id, string defaultPickStationId)
        {
            Name = name;
            Id = id;
            DefaultPickStationId = defaultPickStationId;
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
		public readonly string TripId;
		public readonly TimeSpan Time;
		public readonly int FreePlaces;

		public BusInfo(string tripId, TimeSpan time, int freePlaces)
		{
			TripId = tripId;
			Time = time;
			FreePlaces = freePlaces;
		}

		public override string ToString() => $"{Time}, {FreePlaces}";
	}

	public class BusApi
    {
		private const string PickUpStationMalinovka = "617";
		private const string PickUpStationPripinak = "625";

		public static Station MinskStation = new Station("Минск", "101", PickUpStationMalinovka);
		public static Station StolbtcyStation = new Station("Столбцы", "102", PickUpStationPripinak);

		private static readonly Uri OrderLink = new Uri("https://buspro.by/api/reservation");

		public static Uri GetSiteUri() => new Uri($"https://stolbcy-minsk.by/");

        public static bool GetSchedule(in SearchParameters requestParamters, out ReadOnlyCollection<BusInfo> schedule)
        {
			string requestBody = $"https://buspro.by/api/trip?s[company_id]=15&s[city_departure_id]={requestParamters.FromStation.Id}&s[city_destination_id]={requestParamters.ToStation.Id}&s[date_departure]={requestParamters.TripDay:yyyy-MM-dd}&actual=1";
            string response = WebApiHelper.GetRequestGetBody(new Uri(requestBody));
            string correctedJsonBody = "{ \"Document\": { \"array\" :" + response + "}";
			XDocument? document = JsonConvert.DeserializeXNode(correctedJsonBody);
			if (document == null)
				throw new InvalidOperationException();
            List<BusInfo> busInfos = document.Root.Elements().Select(ParseOnBus).ToList();
            schedule = new ReadOnlyCollection<BusInfo>(busInfos);
			return true;
		}

        public static void Order(in BusInfo busInfo, Station fromStation, string login, string phone, int passengersCount)
        {
            string phoneNew = "+375 (" + phone.Substring(3, 2) + ") " + phone.Substring(5, 3) + '-' + phone.Substring(8, 2) + '-' + phone.Substring(10, 2);
			string body =
				$"{{\"tripId\":{busInfo.TripId}," +
				$"\"stayId\":{fromStation.DefaultPickStationId}," +
				$"\"finishStayId\":null," +
				$"\"phone\":\"{phoneNew}\"," +
				$"\"name\":\"{login}\"," +
				$"\"seats\":{passengersCount},\"source\":\"web\",\"promocode\":null,\"tariff\":null,\"note\":\"\"}}";

			const DecompressionMethods decompressionMethod = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			PostRequestOptions postRequestOptions = new ("application/json; charset=UTF-8", decompressionMethod);
            string response = WebApiHelper.PostRequestResponseBody(OrderLink, body, postRequestOptions);
			const string successResponseKeyWord = "success";
			if (!response.Contains(successResponseKeyWord))
				throw new InvalidOperationException("Bad responce:" + response);
		}

        private static BusInfo ParseOnBus(XElement element)
        {
	        XElement timeDeparture = element.Element(XName.Get("timeDeparture"));
	        TimeSpan time = TimeSpan.ParseExact(timeDeparture.Value, "hh\\:mm", CultureInfo.InvariantCulture);
	        string freePlaces = element.Element(XName.Get("freePlaces")).Value;
			string id = element.Element(XName.Get("id")).Value;
	        return new BusInfo(id, time, int.Parse(freePlaces, CultureInfo.InvariantCulture));
        }
	}
}
