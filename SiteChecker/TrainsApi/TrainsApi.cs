using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WebApiUtils;

namespace TrainsApi
{
	public class TrainsInfoApi
	{
		public static ReadOnlyCollection<Station> ReliableStations
			= new ReadOnlyCollection<Station>(
				new[] { new Station("Минск", "2100000"), new Station("Столбцы", "2100123") });

		public static Uri GetRequestUri(TrainParameters parameters)
		{
			const string getTrainsRequest = "http://rasp.rw.by/ru/route/?from={0}&to={1}&date={2}";
			return new Uri(
				string.Format(
					getTrainsRequest,
					parameters.FromStation,
					parameters.ToStation,
					parameters.Date.ToString("yyyy-MM-dd")));
		}

		public static List<TrainInfo> GetBusinessClassTrains(TrainParameters parameters)
			=> GetTrains(parameters).Where(train => train.IsBusinessClass).ToList();

		private static List<TrainInfo> GetTrains(TrainParameters parameters)
		{
			string response = WebApiHelper.GetResponseString(GetRequestUri(parameters));
			return TrainsScheduleResponseParser.ParseTrainsInfo(response);
		}

		public static bool HaveTicketsForNotDisabled(TrainParameters parameters, TrainInfo trainInfo)
		{
			string response = WebApiHelper.GetResponseString(GetTicketsReqeust(parameters, trainInfo));
			return TrainsTicketsParser.HaveTicketsForNotDisabled(response);
		}

		private static Uri GetTicketsReqeust(TrainParameters parameters, TrainInfo trainInfo)
		{
			const string checkTicketsRequest = "https://rasp.rw.by/ru/ajax/route/car_places/?from={0}&to={1}&date={2}&train_number={3}&car_type=2";
			string trainNumber = trainInfo.TrainId;
			return new Uri(
				string.Format(
					checkTicketsRequest,
					parameters.FromStation.Id,
					parameters.ToStation.Id,
					parameters.Date.ToString("yyyy-MM-dd"),
					trainNumber));
		}
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
