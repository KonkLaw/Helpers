using System;
using System.Collections.Generic;
using System.Linq;
using CredentialHelper;
using Notifier.PageViewModels;

namespace Notifier.Model
{
	interface IBaseBusModel
	{
		string ServiceDescription { get; }

		bool CanOrder { get; }

		bool TryFind(
			BusSearchParameters searchParameters,
			Credentials? credentialsForOrder,
			ref string goodResultMessage);

		Uri GetSiteUri(BusSearchParameters searchParameters);
	}

	abstract class BaseBusModel<TBus> : IBaseBusModel
	{
		public abstract string ServiceDescription { get; }

		public abstract bool CanOrder { get; }

		public abstract bool TryFind(
			BusSearchParameters searchParameters,
			Credentials? credentialsForOrder,
			ref string goodResultMessage);

		public abstract Uri GetSiteUri(BusSearchParameters searchParameters);

		protected abstract (TimeSpan time, int ticketsCount) GetInfo(TBus bus);

		protected List<TBus> FilteredBuses(IEnumerable<TBus> buses, in BusSearchParameters searchParameters)
		{
			BusSearchParameters parameters = searchParameters;
			return buses.Where(bus =>
			{
				(TimeSpan time, int ticketsCount) = GetInfo(bus);
				return time >= parameters.FromTime
				       && time <= parameters.ToTime
				       && ticketsCount >= parameters.PassengersCount;
			}).ToList();
		}
	}
}