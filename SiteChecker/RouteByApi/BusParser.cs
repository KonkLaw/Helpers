using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RouteByApi
{
	class BusParser
	{
		internal static ReadOnlyCollection<BusInfo> ParseSchedule(string schedule)
		{
			const string beginRecordSubstring = "\\\" data-parts=\\\"[1]\\\"><div class=\\\"lol_driver_action\\\"><div class=\\\"lol_driver_time\\\">";
			const int idLength = 7;
			const string freeCountSubstring = "<div class=\\\"lol_driver_space_num\\\">";

			var result = new List<BusInfo>();

			int currentIndex = 0;
			while (currentIndex < schedule.Length)
			{
				int newIndex = schedule.IndexOf(beginRecordSubstring, currentIndex);
				if (newIndex < 0)
					break;
				string time = schedule.Substring(newIndex + beginRecordSubstring.Length, 5);
				string id = schedule.Substring(newIndex - idLength, idLength);

				int countStarIndex = schedule.IndexOf(freeCountSubstring, newIndex + 1) + freeCountSubstring.Length;
				int countEndIndex = schedule.IndexOf('<', countStarIndex + 1);

				string count = schedule.Substring(countStarIndex, countEndIndex - countStarIndex);
				int ticketsCount = int.Parse(count);

				result.Add(new BusInfo(
					id,
					new TimeSpan(
						int.Parse(time.Substring(0, 2)),
						int.Parse(time.Substring(3, 2)),
						0),
					ticketsCount));

				currentIndex = countEndIndex + 1;
			}
			return new ReadOnlyCollection<BusInfo>(result);
		}

        private static readonly string[] KnowsErrors =
        {
            "Необходимо пройти дополнительную проверку",
            "Неверный пароль"
        };

        internal static bool ContainsError(string message, out string recognizedError)
        {
            const string goodResponse = "\"error\":0,";

            if (message.Contains(goodResponse))
            {
                recognizedError = default;
                return false;
            }

            foreach (string knowsError in KnowsErrors)
            {
                if (message.Contains(knowsError))
                {
                    recognizedError = knowsError;
                    return true;
                }
            }
            recognizedError = message;
            return true;
        }
    }
}
