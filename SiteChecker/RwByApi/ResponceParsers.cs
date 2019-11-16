using System;
using System.Collections.Generic;

namespace RwByApi
{
	class TrainsScheduleResponseParser
	{
		internal static List<TrainInfo> ParseTrainsInfo(string response)
		{
			var result = new List<TrainInfo>();
			int indexOfStart = response.IndexOf("<tbody class=\"schedule_list\">");
			while (true)
			{
				int indexOfEnd = GetIndexOfTrainEnd(response, indexOfStart + 1);
				if (indexOfEnd < 0)
					break;
				result.Add(ProcessTrainPart(response, indexOfStart, indexOfEnd));
				indexOfStart = indexOfEnd;
			}
			return result;
		}

		private static int GetIndexOfTrainEnd(string content, int indexOfStart)
			=> content.IndexOf(@"</tr><!-- // b-train -->", indexOfStart);

		private static TrainInfo ProcessTrainPart(string content, int indexOfStart, int indexOfEnd)
		{
			int indexOfTime = content.IndexOfEnd("<b class=\"train_start-time\">", indexOfStart);
			string timeString = content.Substring(indexOfTime, 5);
			var time = new TimeSpan(int.Parse(timeString.Substring(0, 2)), int.Parse(timeString.Substring(3, 2)), 0);
			return new TrainInfo(
				time,
				content.IndexOf("regional_business", indexOfStart, indexOfEnd - indexOfStart) > 0,
				GetTrainId(content, indexOfStart));
		}

		private static string GetTrainId(string content, int indexOfStart)
		{
			int idStart = content.IndexOfEnd("<small class=\"train_id\">", indexOfStart);
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
