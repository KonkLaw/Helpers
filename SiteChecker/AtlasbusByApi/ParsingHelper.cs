using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AtlasbusByApi
{
	public static class ParsingHelper
	{
		public static bool ParseScheduleIsSessionOk(string response, out ReadOnlyCollection<BusInfo> result)
		{
			const string idSubstringStart = " id=\\\"";
			const string idSubstringEnd = "\\\"";
			const string serviceIdSubstringStart = "data-id-service=\\\"";
			const string serviceIdSubstringEnd = "\\\"";
			const string beginTimeSubstring = "tbb_tc_to\\\">";
			const string freeCountSubstringStart = " data-num_sp=\\\"";
			const string freeCountSubstrinEnd = "\\\"";

			var list = new List<BusInfo>();

			int inDocumentIndex = 0;
			while (inDocumentIndex < response.Length)
			{
				// Id
				int q = response.IndexOf(idSubstringStart, inDocumentIndex);
				if (q < 0)
					break;
				int inBusStartIndex = q + idSubstringStart.Length;
				
				int inBusStopIndex = response.IndexOf(idSubstringEnd, inBusStartIndex);
				string id = response[inBusStartIndex..inBusStopIndex];
				// serviceId
				int serviceStartIndex = response.IndexOf(serviceIdSubstringStart, inBusStartIndex) + serviceIdSubstringStart.Length;
				int serviceStopIndex = response.IndexOf(serviceIdSubstringEnd, serviceStartIndex);
				string serviceId = response[serviceStartIndex..serviceStopIndex];
				// time
				int timeStartIndex = response.IndexOf(beginTimeSubstring, inBusStartIndex) + beginTimeSubstring.Length;
				// count
				int countStarIndex = response.IndexOf(freeCountSubstringStart, inBusStartIndex + 1) + freeCountSubstringStart.Length;
				int countEndIndex = response.IndexOf(freeCountSubstrinEnd, countStarIndex + 1);
				int ticketsCount = int.Parse(response[countStarIndex..countEndIndex]);

				list.Add(new BusInfo(
					id,
					new TimeSpan(
						int.Parse(response[timeStartIndex..(timeStartIndex + 2)]),
						int.Parse(response[(timeStartIndex + 3)..(timeStartIndex + 5)]),
						0),
					ticketsCount,
					serviceId));

				inDocumentIndex = countEndIndex + 1;
			}
			result = new ReadOnlyCollection<BusInfo>(list);
			return true;
		}

		public static bool ParseAuthenticationRespponcce(string responceDecoded, out string erroeMessage)
		{
			string? valueCode = responceDecoded.ExtractValue("error");
			if (valueCode == null)
				throw new InvalidOperationException(responceDecoded);
			if (valueCode == "0")
			{
				erroeMessage = string.Empty;
				return true;
			}
			string? errorText = responceDecoded.ExtractValue("error_text");
			if (errorText == null)
				throw new InvalidOperationException(responceDecoded);
			erroeMessage = errorText[1..^1];
			return false;
		}
	}
}
