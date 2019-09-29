using System;
using System.Collections.Generic;
using System.Linq;
using WebApiUtils;

namespace TrainsApi
{
	public class TrainsInfoApi
	{
		private static string[] reliableStations = new[] { "Минск", "Столбцы" };
		private static string[] stationIds = new[] { "2100000", "2100123" };

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

		public static string[] GetReliableStations() => (string[]) reliableStations.Clone();

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
			string fromId = parameters.FromStation == reliableStations[0] ? stationIds[0] : stationIds[1];
			string toId = parameters.ToStation == reliableStations[0] ? stationIds[0] : stationIds[1]; ;
			string trainNumber = trainInfo.TrainId;
			return new Uri(
				string.Format(
					checkTicketsRequest,
					fromId,
					toId,
					parameters.Date.ToString("yyyy-MM-dd"),
					trainNumber));
		}
	}
}
