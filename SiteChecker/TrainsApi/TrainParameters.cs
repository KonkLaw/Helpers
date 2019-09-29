using System;

namespace TrainsApi
{
	public struct TrainParameters
	{
		public readonly DateTime Date;
		public readonly string FromStation;
		public readonly string ToStation;

		public TrainParameters(DateTime date, string outStation, string inStation)
		{
			Date = date;
			FromStation = outStation;
			ToStation = inStation;
		}
	}
}
