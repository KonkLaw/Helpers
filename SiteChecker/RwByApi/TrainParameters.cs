using System;
using System.Globalization;

namespace RwByApi
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

		public string GetDescription() =>
            $"From: {FromStation}, To: {ToStation} {Environment.NewLine}" +
            $"Date: {Date.ToString("yyyy-MM-dd dddd", CultureInfo.GetCultureInfo("ru-ru"))}";
	}
}
