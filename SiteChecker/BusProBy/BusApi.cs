using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Newtonsoft.Json;
using WebApiUtils;

namespace BusProBy
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

	public class BusInfo
	{
		public string Driver { get; }
		public TimeSpan Time { get; }
		public int FreePlaces { get; }

		public BusInfo(string driver, TimeSpan time, int freePlaces)
		{
			Driver = driver;
			Time = time;
			FreePlaces = freePlaces;
		}

		public override string ToString() => $"{Time}, {FreePlaces}";
	}

	public class BusApi
    {
		public static readonly ReadOnlyCollection<Station> Stations = new ReadOnlyCollection<Station>(
			new[] { new Station("Минск", "101"), new Station("Столбцы", "102") });

		public static Uri GetSiteUri() => new Uri($"https://stolbcy-minsk.by/");

        public static bool GetSchedule(in SearchParameters requestParamters, out ReadOnlyCollection<BusInfo> schedule)
        {
			string requestBody = $"https://buspro.by/api/trip?s[company_id]=15&s[city_departure_id]={requestParamters.FromStation.Id}&s[city_destination_id]={requestParamters.ToStation.Id}&s[date_departure]={requestParamters.TripDay:yyyy-MM-dd}&actual=1";
            string response = WebApiHelper.GetRequestGetBody(new Uri(requestBody));
            string correctedJsonBody = "{ \"Document\": { \"array\" :" + response + "}";
			XDocument document = JsonConvert.DeserializeXNode(correctedJsonBody);
            List<BusInfo> busInfos = document.Root.Elements().Select(ParseOnBus).ToList();
            schedule = new ReadOnlyCollection<BusInfo>(busInfos);
			return true;
		}

        private static BusInfo ParseOnBus(XElement element)
        {
	        XElement driver = element.Element(XName.Get("driver"));
	        XElement timeDeparture = element.Element(XName.Get("timeDeparture"));
	        TimeSpan time = TimeSpan.ParseExact(timeDeparture.Value, "hh\\:mm", CultureInfo.InvariantCulture);
	        XElement freePlaces = element.Element(XName.Get("freePlaces"));
	        return new BusInfo(driver.Value, time, int.Parse(freePlaces.Value, CultureInfo.InvariantCulture));
        }
	}
}
