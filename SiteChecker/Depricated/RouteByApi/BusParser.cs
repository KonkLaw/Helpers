using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RouteByApi
{
	internal class BusParser
	{
		const string GoodResponse = "\"error\":0,";

		public static bool IsGoodResponce(string response) => response.Contains(GoodResponse);

		private static readonly string[] KnowsErrors =
{
			"Необходимо пройти дополнительную проверку",
			"Неверный пароль",
			"Пользователь не зарегистрирован",
		};

		public static bool ContainsError(string message, out string recognizedError)
        {
			// {"error":0,"ph":"......."}
			if (IsGoodResponce(GoodResponse))
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
			if (!IsGoodResponce(response))
			{
				throw new InvalidOperationException(response);
			}
			
			const string sessionExpiredMessage = "Ваша сессия закончилась. Обновите страницу.";
			if (response.Contains(sessionExpiredMessage))
			{
				result = default;
				return false;
			}

			const string idSubstring = " id=\\\"";
			const int idLength = 8;

			const string beginRecordSubstring = "]\\\"><div class=\\\"lol_driver_action\\\"><div class=\\\"lol_driver_time\\\">";
			
			const string freeCountSubstring = "<div class=\\\"lol_driver_space_num\\\">";

			var list = new List<BusInfo>();

			int currentIndex = 0;
			while (currentIndex < response.Length)
			{
				int newIndex = response.IndexOf(idSubstring, currentIndex);
				if (newIndex < 0)
					break;

				string id = response.Substring(newIndex + idSubstring.Length, idLength);

				newIndex = response.IndexOf(beginRecordSubstring, newIndex);
				string time = response.Substring(newIndex + beginRecordSubstring.Length, 5);
				
				int countStarIndex = response.IndexOf(freeCountSubstring, newIndex + 1) + freeCountSubstring.Length;
				int countEndIndex = response.IndexOf('<', countStarIndex + 1);

				string count = response[countStarIndex..countEndIndex];
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

		public static string GetOrderRequest(string preOrderResponse, bool fromMinskToStolbtcy)
		{
			// MINSK_STOLBTCY
			// START
			//<option data-time-points="-15" value="2394">Юго западная Автостанция</option>
			//<option data-time-points="-10" value="9833">ост. ст.м. Петровщина</option>
			//<option data-time-points="0" value="2307">Малиновка</option>
			//<option data-time-points="10" value="2844">Дзедава Карчма</option>
			//<option data-time-points="10" value="2400">Чечино</option>
			//<option data-time-points="10" value="2403">Черкасы</option><option data-time-points="15" value="2397">Вязань</option>
			//<option data-time-points="20" value="2310">Веста</option> 
			// FINISH
			//<option value="0" data-time-points="0" selected="selected">не выбрано</option> 
			//<option data-time-points="-15" value="2325">Кучкуны</option>
			//<option data-time-points="-15" value="2406">Слабода</option>
			//<option data-time-points="-10" value="2328">Яблоновка</option>
			//<option data-time-points="-5" value="2319">Остановка</option>
			//<option data-time-points="0" value="2322">Автовокзал</option>
			// STOLBTCY_MINSK
			// START
			//<option data-time-points="0" value="2331">Автовокзал</option>
			//<option data-time-points="1" value="9587" selected="selected">Прыпынак</option>
			//<option data-time-points="10" value="2337">Яблоновка</option>
			//<option data-time-points="10" value="2409">Слабода</option>
			//<option data-time-points="12" value="2340">Кучкуны</option>
			//<option data-time-points="20" value="2415">306 киллометр</option>
			//<option data-time-points="25" value="2412">Заправка Энергетик</option>
			//<option data-time-points="30" value="2847">Дзедава Карчма</option>
			//<option data-time-points="32" value="2343">Веста </option> </select> 
			// FINISH
			//<option value="0" data-time-points="0" selected="selected">не выбрано</option> 
			//<option data-time-points="-30" value="2361">Веста</option>
			//<option data-time-points="0" value="2358">Малиновка</option>
			//<option data-time-points="5" value="2355">Петровщина - Берестье</option>
			//<option data-time-points="15" value="2418">Юго западная Автостанция</option> </select> 

			static string GetValueByKey(string content, string key)
			{
				const string valueMarker = "value=\\\"";
				int index = content.IndexOf(key);
				index = content.IndexOf(valueMarker, index) + valueMarker.Length;
				int endIndex = content.IndexOf("\\\"", index);
				return content[index..endIndex];
			}

			string GetValue(string key) => GetValueByKey(preOrderResponse, key);

			string startStation;
			string finishStation;
			if (fromMinskToStolbtcy)
			{
				startStation = "2307";
				finishStation = "0";
			}
			else
			{
				startStation = "9587";
				finishStation = "0";
			}

			string part = "%5B" + GetValue("aurb_id_add_parts")[1] + "%5D";
			string orderRequestContent =
				@"type=load_step2_save&" +
				$"aurb_id_add_et={GetValue("aurb_id_add_et")}&" +
				"aurb_id_add_num_space=1&" +
				$"aurb_id_add_tt={GetValue("aurb_id_add_tt")}&" +
				$"aurb_id_add_parts={part}&" +
				$"aurb_points_finish[1]={finishStation}&" +
				$"aurb_point_start[1]={startStation}&" +
				$"aurb_id_add_df={GetValue("aurb_id_add_df")}&" +
				$"aurb_id_add_ds={GetValue("aurb_id_add_ds")}&" +
				"aurb_id_add_comment=&" +
				$"aurb_id_add_sl={GetValue("aurb_id_add_sl")}&" +
				$"aurb_id_add_save_points={GetValue("aurb_id_add_save_points")}&" +
				"aurb_id_service=144"; // <-- this can be taken from "aurb_id_add_service" m.b.

			return orderRequestContent;
		}
	}
}
