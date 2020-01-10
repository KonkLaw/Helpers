using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using WebApiUtils;

namespace AtlasbusByApi
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

		public BusInfo(string id, TimeSpan time, int ticketsCount)
		{
			Id = id;
			TicketsCount = ticketsCount;
			Time = time;
		}

		public override string ToString() => $"time: {Time}";
	}

	public class BusApi
	{
		public static readonly ReadOnlyCollection<Station> Stations = new ReadOnlyCollection<Station>(
			new[] { new Station("Минск", "101"), new Station("Столбцы", "200") });

		public static bool GetSchedule(in SearchParameters searchParameters, out ReadOnlyCollection<BusInfo> schedule)
		{
			Uri SchduleRequestUri = new Uri(@"https://atlasbus.by/local/components/route/user.order/templates/.default/ajax.php");
			string dateString = searchParameters.TripDay.ToString("dd.MM.yyyy");
			string requestBody = $"type=load_list_order&select_in={searchParameters.FromStation.Id}&select_out={searchParameters.ToStation.Id}&date={dateString}&strGET=";
			string contentType = @"application/x-www-form-urlencoded";
			System.Net.HttpWebRequest request = WebApiHelper.GetPostRequestWithCookies(
				SchduleRequestUri, requestBody, contentType);
			string answerNotCoded = WebApiHelper.GetResponseString(request);
			ParseScheduleIsSessionOk(answerNotCoded, out ReadOnlyCollection<BusInfo> result);
			schedule = result;
			//string answerDecoded = answerNotCoded.DecodeUnicide();

			//HttpWebRequest scheduleWebRequest = GetRequest(RouteByApiHelpers.GetScheduleRequestBody(in searchParameters));
			//string responseContent = WebApiHelper.GetResponseString(scheduleWebRequest).DecodeUnicide();
			//if (BusParser.ParseScheduleIsSessionOk(responseContent, out schedule))
			//{
			//	return true;
			//}
			//else
			//{
			//	schedule = default;
			//	return false;
			//}
			return true;
		}

		public static bool ParseScheduleIsSessionOk(string response, out ReadOnlyCollection<BusInfo> result)
		{
			const string idSubstring = " id=\\\"";
			const int idLength = 7;
			const string beginTimeSubstring = "tbb_tc_to\\\">";
			const string freeCountSubstringStart = " data-num_sp=\\\"";
			const string freeCountSubstrinEnd = "\\\"";

			var list = new List<BusInfo>();

			int currentIndex = 0;
			while (currentIndex < response.Length)
			{
				int newIndex = response.IndexOf(idSubstring, currentIndex);
				if (newIndex < 0)
					break;

				string id = response.Substring(newIndex + idSubstring.Length, idLength);
				newIndex = response.IndexOf(beginTimeSubstring, newIndex);
				int timeStartIndex = newIndex + beginTimeSubstring.Length;

				int countStarIndex = response.IndexOf(freeCountSubstringStart, newIndex + 1) + freeCountSubstringStart.Length;
				int countEndIndex = response.IndexOf(freeCountSubstrinEnd, countStarIndex + 1);
				int ticketsCount = int.Parse(response[countStarIndex..countEndIndex]);

				list.Add(new BusInfo(
					id,
					new TimeSpan(
						int.Parse(response[timeStartIndex..(timeStartIndex + 2)]),
						int.Parse(response[(timeStartIndex + 3)..(timeStartIndex + 5)]),
						0),
					ticketsCount));

				currentIndex = countEndIndex + 1;
			}
			result = new ReadOnlyCollection<BusInfo>(list);
			return true;
		}

		public static Uri GetSiteUri(Station fromStation, Station toStation) => new Uri($"https://atlasbus.ru/Маршруты/{fromStation.Name}/{toStation.Name}");
	}

	static class StringHelper
	{
		private readonly static StringBuilder Cache = new StringBuilder();

		public static string DecodeUnicide(this string input)
		{
			const string pattern = @"\u";
			const int partLength = 4;

			int currentIndex = 0;
			do
			{
				int newEncodeIndex = input.IndexOf(pattern, currentIndex);
				if (newEncodeIndex < 0)
				{
					Cache.AppendLine(input[currentIndex..]);
					break;
				}
				else
				{
					Cache.Append(input, currentIndex, newEncodeIndex - currentIndex);
				}

				string sign = input.Substring(newEncodeIndex + pattern.Length, partLength);
				bool parsedSuccessfully = int.TryParse(sign,
						NumberStyles.HexNumber,
						CultureInfo.CurrentCulture,
						out int charData);
				Cache.Append((char)charData);

				currentIndex = newEncodeIndex + pattern.Length + partLength;
			} while (currentIndex < input.Length);
			string result = Cache.ToString();
			Cache.Clear();
			return result;
		}
	}
}
