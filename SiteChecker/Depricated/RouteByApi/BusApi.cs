﻿using System;
using System.Collections.ObjectModel;
using System.Net;
using WebApiUtils;

namespace RouteByApi
{
	public static class BusApi
    {
        public const int MinHours = 6;
        public const int MaxHours = 21;

		public static readonly ReadOnlyCollection<Station> Stations = new ReadOnlyCollection<Station>(
			new [] { new Station("Минск", "1"), new Station("Столбцы", "102") });

		internal static bool GetIsFromMinskToStolbtcy(in OrderParameters orderParameters)
		{
			bool fromMinskToStolbtcy;
			if (orderParameters.FromStation == BusApi.Stations[0] && orderParameters.ToStation == BusApi.Stations[1])
				fromMinskToStolbtcy = true;
			else if (orderParameters.FromStation == BusApi.Stations[1] && orderParameters.ToStation == BusApi.Stations[0])
				fromMinskToStolbtcy = false;
			else
				throw new ArgumentException("Not valid stations.");
			return fromMinskToStolbtcy;
		}

		private const string SiteUrl = "https://stolbcy-minsk.by/";
		public static Uri GetSiteUri() => new Uri(SiteUrl);

        public static bool TryGetCachedSession(in SessionData sessionData, out RouteApiSession session)
        {
            var newSession = new RouteApiSession(in sessionData);
            var searchParameters = new SearchParameters(Stations[0], Stations[1], DateTime.Now.AddDays(1).Date);
			if (newSession.GetSchedule(in searchParameters, out _))
			{
				session = newSession;
				return true;
			}
			else
			{
				session = default;
				return false;
			}
        }

		public static bool TryGetNewSession(in LoginData loginData, out RouteApiSession session, out string errorMessage)
        {
            if (SessionCreator.TryCreateSession(in loginData, out SessionData sessionData, out string message))
            {
                errorMessage = default;
                session = new RouteApiSession(in sessionData);
                return true;
            }
            else
            {
                errorMessage = message;
                session = default;
                return false;
            }
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

    public readonly struct SessionData
    {
        public readonly string PhoneNumber;
        public readonly string PhpSesSid;
        public readonly string Uidh;

        public SessionData(string phoneNumber, string phpSesSid, string uidh)
        {
            PhoneNumber = phoneNumber;
            PhpSesSid = phpSesSid;
            Uidh = uidh;
        }
    }

	public readonly struct LoginData
	{
		public readonly string PhoneNumber;
		public readonly string Pas;

        public LoginData(string phoneNumber, string pas)
        {
            PhoneNumber = phoneNumber;
            Pas = pas;
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

	public class RouteApiSession
	{
		public readonly SessionData SessionData;

		internal RouteApiSession(in SessionData sessionData)
		{
			SessionData = sessionData;
		}

        private HttpWebRequest GetRequest(string requestBody) =>
            RouteByApiHelpers.GetRequest(requestBody, RouteByApiHelpers.GetScheduleRequestHeaders(in SessionData));
        
        public bool GetSchedule(in SearchParameters searchParameters, out ReadOnlyCollection<BusInfo> schedule)
        {
            HttpWebRequest scheduleWebRequest = GetRequest(RouteByApiHelpers.GetScheduleRequestBody(in searchParameters));
			string responseContent = WebApiHelper.GetResponseString(scheduleWebRequest).DecodeUnicide();
			if (BusParser.ParseScheduleIsSessionOk(responseContent, out schedule))
			{
				return true;
			}
			else
			{
				schedule = default;
				return false;
			}
		}

        public void Order(in OrderParameters orderParameters)
        {
			HttpWebRequest preOrderRequest = GetRequest(RouteByApiHelpers.GerOrderRequestBody(in orderParameters));
            string preOrderResponse = WebApiHelper.GetResponseString(preOrderRequest).DecodeUnicide();
			if (!BusParser.IsGoodResponce(preOrderResponse))
				throw new InvalidOperationException(preOrderResponse);
			HttpWebRequest orderRequest = GetRequest(BusParser.GetOrderRequest(preOrderResponse, BusApi.GetIsFromMinskToStolbtcy(in orderParameters)));
            string orderResponse = WebApiHelper.GetResponseString(orderRequest).DecodeUnicide();
			if (!BusParser.IsGoodResponce(orderResponse) || !orderResponse.Contains("Ваш заказ добавлен"))
				throw new InvalidOperationException(orderResponse);
		}
    }

    public readonly struct OrderParameters
    {
        public readonly Station FromStation;
        public readonly Station ToStation;
        public readonly string BusId;

        public OrderParameters(Station fromStation, Station toStation, string busId)
        {
            FromStation = fromStation;
            ToStation = toStation;
            BusId = busId;
        }
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
}
