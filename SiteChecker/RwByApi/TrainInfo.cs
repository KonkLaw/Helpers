using System;

namespace RwByApi
{
	public class TrainInfo
	{
		public readonly TimeSpan TrainTime;

		public readonly bool IsBusinessClass;

		public readonly string TrainId;

		public TrainInfo(TimeSpan trainTime, bool isBusinessClass, string trainId)
		{
			TrainTime = trainTime;
			IsBusinessClass = isBusinessClass;
			TrainId = trainId;
		}

		public override string ToString() => TrainTime.ToString(@"hh\:mm");
	}
}
