using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

using WebApiUtils;

namespace SiteChecker.RoutieBy
{
	public readonly struct MyTime
	{
		private readonly DateTime dateTime;

		public MyTime(int hours, int minutes)
		{
			dateTime = new DateTime(1, 1, 1, hours, minutes, 0);
		}

		public static bool operator<= (MyTime left, MyTime right) => left.dateTime <= right.dateTime;

		public static bool operator ==(MyTime left, MyTime right) => left.dateTime == right.dateTime;

		public static bool operator !=(MyTime left, MyTime right) => left.dateTime != right.dateTime;

		public static bool operator>= (MyTime left, MyTime right) => left.dateTime >= right.dateTime;

		public static bool operator< (MyTime left, MyTime right) => left.dateTime < right.dateTime;

		public static bool operator> (MyTime left, MyTime right) => left.dateTime > right.dateTime;

		public long GetDist(MyTime time) => Math.Abs(time.dateTime.Ticks - dateTime.Ticks);

		public override bool Equals(object obj)
		{
			if (!(obj is MyTime))
			{
				return false;
			}

			var time = (MyTime)obj;
			return dateTime == time.dateTime;
		}

		public override int GetHashCode() => dateTime.GetHashCode();

		public override string ToString() => $"{dateTime.Hour}:{dateTime.Minute}";
	}

	public static class RoutieByLogicAndParser
	{
		private readonly struct InfoRecord
		{
			public readonly int TicketsCount;
			public readonly string Id;
			public readonly MyTime Time;

			public InfoRecord(string id, in MyTime time, int ticketsCount)
			{
				TicketsCount = ticketsCount;
				Id = id;
				Time = time;
			}

			public override string ToString() => $"{Time}; Count={TicketsCount}; Id={Id}";
		}

		public static void Buy(
            MyTime fromTime, MyTime toTime, MyTime preferedTime, DateTime tripDay,
            bool fromMinskToStol,
            Action idleAction,
            Action<Exception> endedWithException, PrivateData privateData)
		{
			if (fromTime >= toTime && preferedTime > toTime || preferedTime < fromTime)
				throw new Exception("Bad times");

			if (!CookieStorage.TryGetData(out Credentials credentials))
			{
				credentials = RequestDataAndSave(privateData);
			}

			var routieByHelper = new RoutieBySessionHelper(privateData, credentials);


			while (true)
			{
				try
				{
					string scheduleContent = GetSchedule(tripDay, routieByHelper, fromMinskToStol);

					const string badDate = @"Неверно указана дата";
					if (scheduleContent.Contains(badDate))
						throw new Exception("Bad date.");

					const string basSession = @"Ваша сессия закончилась. Обновите страницу.";
					int firstCharsToCheck = Math.Min(200, scheduleContent.Length);
					if (scheduleContent.IndexOf(basSession, 0, firstCharsToCheck) > 0)
					{
						credentials = RequestDataAndSave(privateData);
						routieByHelper = new RoutieBySessionHelper(privateData, credentials);
						scheduleContent = GetSchedule(tripDay, routieByHelper, fromMinskToStol);
					}

					List<InfoRecord> times = GetItems(scheduleContent);
					List<InfoRecord> selected = times.Where(r => r.Time >= fromTime && r.Time <= toTime).ToList();

					long worstMark = long.MaxValue;
					List<(int index, long mark)> orderedResult = selected.Select(
						(record, index) => (
							index,
							mark: record.TicketsCount < 1 ? worstMark : record.Time.GetDist(preferedTime)))
						.OrderBy(t => t.mark).ToList();

					if (orderedResult.Count > 0 && orderedResult[0].mark == worstMark)
					{
                        idleAction();
                        Thread.Sleep(10000);
						continue;
						//return;
					}
					else
					{
						Signal();
						//Order(selected[orderedResult[0].index], routieByHelper, fromMinskToStol);
					}
				}
				catch(Exception ex)
				{
                    endedWithException(ex);
				}
			}
		}

		public static void Signal()
		{
			while (true)
			{
				Console.Beep();
			}
		}

		private static Credentials RequestDataAndSave(PrivateData privateData)
		{
			string GetUIDH(string cookie)
			{
				const string forSearch = "BITRIX_SM_UIDH=";
				int startIndex = cookie.IndexOf(forSearch) + forSearch.Length;
				int endIndex = cookie.IndexOf(';', startIndex);
				return cookie.Substring(startIndex, endIndex - startIndex);
			}

			string GetPhone(string phone)
			{
				return $"%3D375%2B({phone.Substring(3, 2)})%2B{phone.Substring(5, 3)}-{phone.Substring(8, 2)}-{phone.Substring(10, 2)}%26";
			}

			string request = $"type=auth_login&data=phone{GetPhone(privateData.PhoneNumber)}user_pass%3D{privateData.Pas}%26g-recaptcha-response%3D%26remember%3Don%26sms_registration%3D%26user_pass_new%3D%26user_pass_new_conf%3D%26remember_reg%3Don%26sms_recall%3D%26new_pass%3D%26new_pass_conf%3D";

			string phpsessid = GetPhpSessid();
			WebRequest normalRequest = RoutieBySessionHelper.GetAuthentificationRequest(request, phpsessid);
			var qwe = WebApiHelper.GetResponseString(normalRequest, out WebHeaderCollection headers);
			string uihd = GetUIDH(headers.Get("Set-Cookie"));
			var credentials = new Credentials { PhpSesSid = phpsessid, UIDH = uihd };
			CookieStorage.WriteData(credentials);
			return credentials;
		}

		private static List<InfoRecord> GetItems(string schedule)
		{
			const string beginRecordSubstring = "\\\" data-parts=\\\"[1]\\\"><div class=\\\"lol_driver_action\\\"><div class=\\\"lol_driver_time\\\">";
			const int idLength = 7;
			const string freeCountSubstring = "<div class=\\\"lol_driver_space_num\\\">";

			var result = new List<InfoRecord>();

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

				result.Add(new InfoRecord(
					id,
					new MyTime(
						int.Parse(time.Substring(0, 2)),
						int.Parse(time.Substring(3, 2))),
					ticketsCount));

				currentIndex = countEndIndex + 1;
			}
			return result;
		}

		private static string GetPhpSessid()
		{
			var guid = Guid.NewGuid();
			string phpsessid = guid.ToString().Replace("-", "");
			return phpsessid;
		}

		private static string GetSchedule(DateTime tripDay, RoutieBySessionHelper routieBySessionHelper, bool fromMinskToStol)
		{
			const string Minsk = "1";
			const string Stolbcy = "102";

			string inBus;
			string outBus;
			if (fromMinskToStol)
			{
				inBus = Minsk;
				outBus = Stolbcy;
			}
			else
			{
				inBus = Stolbcy;
				outBus = Minsk;
			}

			string formatedDay = tripDay.ToString("dd.MM.yyyy");
			string infoRequestBody = $"type=load_list_order&select_in={inBus}&select_out={outBus}&date={formatedDay}&id_service=144&lines=%7B%220%22%3A192%2C%221%22%3A195%2C%222%22%3A323%2C%223%22%3A368%2C%224%22%3A446%2C%225%22%3A488%2C%226%22%3A491%7D";
			WebRequest infoRequest = routieBySessionHelper.Request(infoRequestBody);
			string resp = WebApiHelper.GetResponseString(infoRequest);
			return resp.DecodeUnicide();
		}

		private static void Order(InfoRecord infoRecord, RoutieBySessionHelper routieBySessionHelper, bool fromMinskToStol)
		{
			if (infoRecord.TicketsCount < 1)
				throw new Exception("No tickets");

			string GetValueByKey(string content, string key)
			{
				const string valueMarker = "value=\\\"";
				int index = content.IndexOf(key);
				index = content.IndexOf(valueMarker, index) + valueMarker.Length;
				int endIndex = content.IndexOf("\\\"", index);
				return content.Substring(index, endIndex - index);
			}

			// IN - POSADKA
			// OUT - VISADKA

			const string stobcy = "102";
			const string minsk = "1";
			string inBus;
			string outBus;
			if (fromMinskToStol)
			{
				inBus = minsk;
				outBus = stobcy;
			}
			else
			{
				inBus = stobcy;
				outBus = minsk;
			}


			string preorderRequestContent = $"type=load_step2&load_in_page=true&id_tt={infoRecord.Id}&num_selected=1&select_in={inBus}&select_out={outBus}&sline=undefined&idtemp=undefined&timer=undefined";
			WebRequest preorderRequest = routieBySessionHelper.Request(preorderRequestContent);
			string infoResponse = WebApiHelper.GetResponseString(preorderRequest).DecodeUnicide();

			string GetValue(string key) => GetValueByKey(infoResponse, key);
			string part = "%5B" + GetValueByKey(infoResponse, "aurb_id_add_parts")[1] + "%5D";
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

			WebRequest orderRequest = routieBySessionHelper.Request(orderRequestContent);
			string orderResponce = WebApiHelper.GetResponseString(orderRequest).DecodeUnicide();
		}
	}

	class RoutieBySessionHelper
	{
		private static readonly Uri UriNonAuth = new Uri(@"https://route.by/local/components/route/widget.order/templates/.default/ajax.php");
		private static readonly Uri UriAuth = new Uri(@"https://route.by/local/components/route/widget.auth/templates/.default/ajax.php");

		private readonly PrivateData privateData;
		private readonly Credentials credentials;

		public RoutieBySessionHelper(PrivateData privateData, Credentials credentials)
		{
			this.privateData = privateData;
			this.credentials = credentials;
		}

		public static HttpWebRequest GetAuthentificationRequest(string request, string phpSessid)
			=> GetRoutirApiWebRequest(UriAuth, request, GetNewSesionCookies(phpSessid));

		private static RequestHeader[] GetNewSesionCookies(string phpsessid)
		{
			const string origin = @"https://route.by";

			return new RequestHeader[]
			{
				new RequestHeader("Origin", origin),
				new RequestHeader("Bx-ajax", @"true"),
				WebApiHelper.CreateCookiesString(("PHPSESSID", phpsessid))
			};
		}

		private RequestHeader[] GetRequestCookies()
		{
			const string origin = @"https://route.by";

			return new RequestHeader[]
			{
				new RequestHeader("Origin", origin),
				new RequestHeader("Bx-ajax", @"true"),
				WebApiHelper.CreateCookiesString(
						("PHPSESSID", credentials.PhpSesSid),
						("BITRIX_SM_UIDH", credentials.UIDH),
						("BITRIX_SM_UIDL", privateData.PhoneNumber),
						("BX_USER_ID", "1ab83d800a7f88a92a431a2c51d7036a"),
						("BITRIX_SM_LOGIN", privateData.PhoneNumber))
			};
		}

		private static HttpWebRequest GetRoutirApiWebRequest(Uri uri, string requiest, params RequestHeader[] headers)
		{
			const string contentType = @"application/x-www-form-urlencoded";
			HttpWebRequest request = WebApiHelper.GetPostRequestWithCookies(
				uri, requiest, contentType, headers);
			return request;
		}

		internal WebRequest Request(string infoRequestBody) => GetRoutirApiWebRequest(
			UriNonAuth, infoRequestBody, GetRequestCookies());
	}
}