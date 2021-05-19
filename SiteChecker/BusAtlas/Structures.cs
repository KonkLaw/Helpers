using System;

namespace BusAtlas
{
	public readonly struct SearchParameters
	{
		public readonly Station FromStation;
		public readonly Station ToStation;
		public readonly DateTime TripDay;
		public readonly int PassengersCount;

		public SearchParameters(Station fromStation, Station toStation, DateTime tripDay, int passengersCount)
		{
			FromStation = fromStation;
			ToStation = toStation;
			TripDay = tripDay;
			PassengersCount = passengersCount;
		}
	}

	public readonly struct BusInfo
	{
		public readonly int TicketsCount;
		public readonly TimeSpan Time;

		public BusInfo(TimeSpan time, int ticketsCount)
		{
			TicketsCount = ticketsCount;
			Time = time;
		}

		public override string ToString() => $"time: {Time}";
	}

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
}