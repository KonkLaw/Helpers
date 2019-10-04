using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RouteByApi
{
	internal class BusParser
	{
		private static readonly string[] KnowsErrors =
{
			"Необходимо пройти дополнительную проверку",
			"Неверный пароль",
			"Пользователь не зарегистрирован",
		};

		public static bool ContainsError(string message, out string recognizedError)
        {
            const string goodResponse = "\"error\":0,";

			// {"error":0,"ph":"......."}
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

			// other
			// "Превышено количество попыток входа. Повторите попытку через"
			// "Нeверный номер телефона"
			const string beginError = "\"error_text\":\"";
			int indexOf = message.IndexOf(beginError);
			if (indexOf > 0)
			{
				int endError = message.IndexOf('"', indexOf + beginError.Length);
				recognizedError = message.Substring(indexOf + beginError.Length, endError - indexOf - beginError.Length);
			}
			else
				recognizedError = message;
            return true;
        }

		public static bool ParseScheduleIsSessionOk(string response, out ReadOnlyCollection<BusInfo> result)
		{
			const string goodResponse = "\"error\":0,";

			// {"error":0,"ph":"......."}
			if (!response.Contains(goodResponse))
			{
				throw new InvalidOperationException(response);
			}
			
			const string sessionExpiredMessage = "Ваша сессия закончилась. Обновите страницу.";
			if (response.Contains(sessionExpiredMessage))
			{
				result = default;
				return false;
			}
				
			const string beginRecordSubstring = "\\\" data-parts=\\\"[1]\\\"><div class=\\\"lol_driver_action\\\"><div class=\\\"lol_driver_time\\\">";
			const int idLength = 7;
			const string freeCountSubstring = "<div class=\\\"lol_driver_space_num\\\">";

			var list = new List<BusInfo>();

			int currentIndex = 0;
			while (currentIndex < response.Length)
			{
				int newIndex = response.IndexOf(beginRecordSubstring, currentIndex);
				if (newIndex < 0)
					break;
				string time = response.Substring(newIndex + beginRecordSubstring.Length, 5);
				string id = response.Substring(newIndex - idLength, idLength);

				int countStarIndex = response.IndexOf(freeCountSubstring, newIndex + 1) + freeCountSubstring.Length;
				int countEndIndex = response.IndexOf('<', countStarIndex + 1);

				string count = response.Substring(countStarIndex, countEndIndex - countStarIndex);
				int ticketsCount = int.Parse(count);

				list.Add(new BusInfo(
					id,
					new TimeSpan(
						int.Parse(time.Substring(0, 2)),
						int.Parse(time.Substring(3, 2)),
						0),
					ticketsCount));

				currentIndex = countEndIndex + 1;
			}
			result = new ReadOnlyCollection<BusInfo>(list);
			return true;
		}

		public static string GetOrderRequest(string preOrderResponse)
		{
			string GetValueByKey(string content, string key)
			{
				const string valueMarker = "value=\\\"";
				int index = content.IndexOf(key);
				index = content.IndexOf(valueMarker, index) + valueMarker.Length;
				int endIndex = content.IndexOf("\\\"", index);
				return content.Substring(index, endIndex - index);
			}

			string GetValue(string key) => GetValueByKey(preOrderResponse, key);

			string part = "%5B" + GetValue("aurb_id_add_parts")[1] + "%5D";
			string orderRequestContent =
				@"type=load_step2_save&" +
				$"aurb_id_add_et={GetValue("aurb_id_add_et")}&" +
				"aurb_id_add_num_space=1&" +
				$"aurb_id_add_tt={GetValue("aurb_id_add_tt")}&" +
				$"aurb_id_add_parts={part}&" +
				"aurb_points_finish[1]=9761&" +
				"aurb_point_start[1]=9716&" +
				$"aurb_id_add_df={GetValue("aurb_id_add_df")}&" +
				$"aurb_id_add_ds={GetValue("aurb_id_add_ds")}&" +
				"aurb_id_add_comment=&" +
				$"aurb_id_add_sl={GetValue("aurb_id_add_sl")}&" +
				$"aurb_id_add_save_points={GetValue("aurb_id_add_save_points")}&" +
				"aurb_id_service=144";

			return orderRequestContent;
		}
	}
}
