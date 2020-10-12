using System;
using System.Collections.Generic;

namespace RwByApi
{
	class TrainsScheduleResponseParser
	{
		private const string TrainTag = "<div class=\"sch-table__row-wrap js-row";

		internal static List<TrainInfo> ParseTrainsInfo(string response)
		{
			var result = new List<TrainInfo>();
			int indexOfStart = response.IndexOf("<div class=\"sch-table__body js-sort");
			indexOfStart = GetIndexOfTrainStart(response, indexOfStart);
			bool shouldContinue = true;
			do
			{
				int indexOfEnd = GetIndexOfTrainStart(response, indexOfStart + 1);
				if (indexOfEnd < 0)
				{
					shouldContinue = false;
					indexOfEnd = response.Length;
				}
				result.Add(ProcessTrainPart(response, indexOfStart, indexOfEnd));
				indexOfStart = indexOfEnd;
			} while (shouldContinue);
			return result;
		}

		private static int GetIndexOfTrainStart(string content, int indexOfStart)
			=> content.IndexOf(TrainTag, indexOfStart);

		private static TrainInfo ProcessTrainPart(string content, int trainIndexOfStart, int indexOfEnd)
		{
			const string timeStartTag = "<div class=\"sch-table__time train-from-time\"";
			int indexOfStart = content.IndexOf(timeStartTag, trainIndexOfStart);
			indexOfStart = content.IndexOf('>', indexOfStart) + 1;
			string timeString = content.Substring(indexOfStart, 5);
			var time = new TimeSpan(int.Parse(timeString.Substring(0, 2)), int.Parse(timeString.Substring(3, 2)), 0);
			return new TrainInfo(
				time,
				content.IndexOf("interregional_business", trainIndexOfStart, indexOfEnd - trainIndexOfStart) > 0,
				GetTrainId(content, trainIndexOfStart));
		}

		private static string GetTrainId(string content, int indexOfStart)
		{
			int idStart = content.IndexOfEnd("<span class=\"train-number\">", indexOfStart);
			int idStop = content.IndexOf("<", idStart + 1);
			return content.Substring(idStart, idStop - idStart);
		}
	}

	class TrainsTicketsParser
	{
		internal static bool HaveTicketsForNotDisabled(string response)
		{
			string markerForTicketsNotDisabled = "\"is_car_for_disabled\":false";
			return response.Contains(markerForTicketsNotDisabled);
		}
	}
}
