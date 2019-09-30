using System;

namespace TrainsApi
{
	public readonly struct TrainParameters
	{
		public readonly DateTime Date;
		public readonly Station FromStation;
		public readonly Station ToStation;

		public TrainParameters(DateTime date, Station outStation, Station inStation)
		{
			Date = date;
			FromStation = outStation;
			ToStation = inStation;
		}
	}
}
