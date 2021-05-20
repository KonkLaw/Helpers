using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using Newtonsoft.Json;
using WebApiUtils;

namespace BusAtlas
{
	public class BusApi
	{
		public static readonly Station MinskStation = new Station("Минск", "c625144");
		public static readonly Station StolbtcyStation = new Station("Столбцы", "c621266");

		public static bool GetSchedule(in SearchParameters searchParameters, out ReadOnlyCollection<BusInfo> schedule)
		{
			string dateString = searchParameters.TripDay.ToString("yyyy-MM-dd");
			string requestUrl = "https://atlasbus.by/api/search?from_id=" + $"{searchParameters.FromStation.Id}&to_id={searchParameters.ToStation.Id}&calendar_width=30&date={dateString}&passengers=1";
			string response = WebApiHelper.GetRequestGetBody(new Uri(requestUrl));
			schedule = TryGetBuses(response).AsReadOnly();
			if (schedule.Count == 0) // additional check
			{
				response = WebApiHelper.GetRequestGetBody(new Uri(requestUrl));
				schedule = TryGetBuses(response).AsReadOnly();
			}

			return true;
		}

		private static List<BusInfo> TryGetBuses(string response)
		{
			string correctedJsonBody = "{ \"Document\": { \"array\" :" + response + "}";
			XDocument document = JsonConvert.DeserializeXNode(correctedJsonBody);
			if (document == null)
				throw new InvalidOperationException();
			List<BusInfo> busInfos = document.Root!.Elements().First().Elements("rides").Select(ParseOnBus).ToList();
			return busInfos;
		}

		private static BusInfo ParseOnBus(XElement element)
		{
			int freeSeats = int.Parse(element.Elements("freeSeats").First().Value);
			string dateTime = element.Elements("departure").First().Value;
			int timeBegin = dateTime.IndexOf('T') + 1;
			string timeString = dateTime.Substring(timeBegin, dateTime.Length - timeBegin);
			TimeSpan time = TimeSpan.ParseExact(timeString, "hh\\:mm\\:ss", CultureInfo.InvariantCulture);
			return new BusInfo(time, freeSeats);
		}

		public static Uri GetUrl(SearchParameters parameters)
		{
			string url = $"https://atlasbus.by/Маршруты/{parameters.FromStation}/{parameters.ToStation}?date=2021-05-21&passengers=1";
			return new Uri(url);
		}
	}
}
