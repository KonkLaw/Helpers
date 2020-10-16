using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using HtmlAgilityPack;
using Newtonsoft.Json;
using WebApiUtils;

namespace RwByApi
{
	public class TrainsInfoApi
	{
		public static readonly ReadOnlyCollection<Station> ReliableStations
			= new ReadOnlyCollection<Station>(
				new[] { new Station("Минск", "2100000"), new Station("Столбцы", "2100123") });

		public static Uri GetRequestUri(in TrainParameters parameters)
		{
			const string getTrainsRequest = "https://pass.rw.by/ru/route/?from={0}&to={1}&date={2}";
			return new Uri(
				string.Format(
					getTrainsRequest,
					parameters.FromStation,
					parameters.ToStation,
					parameters.Date.ToString("yyyy-MM-dd")));
		}

		public static List<TrainInfo> GetInterRegionalBusinessTrains(in TrainParameters parameters)
			=> GetTrains(in parameters).Where(train => train.IsInterRegionalBusiness).ToList();

		private static List<TrainInfo> GetTrains(in TrainParameters parameters)
		{
			string response = WebApiHelper.GetRequestGetBody(GetRequestUri(parameters));
			return ParseTrainsResponse(response);
		}

		public static bool HasTickets(string trainId, in TrainParameters trainParameters)
		{
			CheckStations(trainParameters.FromStation, trainParameters.ToStation);
			string url = $"https://pass.rw.by/be/ajax/route/car_places/?from={trainParameters.FromStation.Id}&to={trainParameters.ToStation.Id}&date={trainParameters.Date:yyyy-MM-dd}&train_number={trainId}&car_type=2";
			string responce = WebApiHelper.GetRequestGetBody(new Uri(url));
			string correctedJsonBody = "{ \"Document\": " + responce + "}";
			XDocument document = JsonConvert.DeserializeXNode(correctedJsonBody);
            IEnumerable<XElement> tariffs = document.Root.Elements("tariffs");
			foreach (XElement tarif in tariffs)
			{
				XElement carForDisables = tarif.Element("is_car_for_disabled");
				if (carForDisables.Value == "false")
					return true;
			}
			return false;
		}

		private static List<TrainInfo> ParseTrainsResponse(string response)
		{
			var document = new HtmlDocument();
			document.LoadHtml(response);
			HtmlNode[] trainsDivs = document.DocumentNode.SelectNodes("//div[@class='sch-table__body js-sort-body']/div[contains(@class, 'sch-table__row')]").ToArray();
			var result = new List<TrainInfo>();
			foreach (HtmlNode trainsDiv in trainsDivs)
			{
				result.Add(ProcessTrainRow(trainsDiv));
			}
			return result;
		}

		private static TrainInfo ProcessTrainRow(HtmlNode train)
		{
			HtmlNode[] trainNodes = train.ChildNodes[1].ChildNodes.Where(n => n.Name == "div").ToArray();
			string timeString = trainNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].ChildNodes[1].InnerText;
			TimeSpan time = TimeSpan.ParseExact(timeString, "hh\\:mm", CultureInfo.InvariantCulture);
			string trainId = train.ChildNodes[1].ChildNodes[1].ChildNodes[3].ChildNodes[0].InnerText;
			bool isInterRegionalBusiness = train.InnerHtml.Contains("interregional_business");
			bool anyPlaces = !train.InnerHtml.Contains("Мест нет");
			return new TrainInfo(time, trainId, isInterRegionalBusiness, anyPlaces);
		}

        private static void CheckStations(Station from, Station to)
		{
			if (from == to)
				throw new ArgumentException("From equals to start");
			
			if (ReliableStations.IndexOf(from) < 0 || ReliableStations.IndexOf(to) < 0)
				throw new ArgumentException("Some of stations is not recognized");
		}
	}

	public class TrainInfo
	{
		public readonly TimeSpan TrainTime;

		public readonly string TrainId;

		public readonly bool IsInterRegionalBusiness;

		public readonly bool AnyPlaces;

		internal TrainInfo(TimeSpan trainTime, string trainId, bool isInterRegionalBusiness, bool anyPlaces)
		{
			TrainTime = trainTime;
			TrainId = trainId;
			IsInterRegionalBusiness = isInterRegionalBusiness;
			AnyPlaces = anyPlaces;
		}

		public override string ToString() => TrainTime.ToString(@"hh\:mm");
	}

	public class Station
	{
		private readonly string name;
		internal readonly string Id;

		internal Station(string name, string id)
		{
			this.name = name;
			Id = id;
		}

		public override string ToString() => name;
	}
}
