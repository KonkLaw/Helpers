using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using BusProBy;
using CredentialHelper;
using Notifier.UtilTypes;

namespace Notifier.PageViewModels
{
    class BusSearchingViewModel : BaseSearingViewModel
	{
		private readonly BusSearchParameters searchParameters;
        private readonly Credentials? credentialsForOrder;

        public static BusSearchingViewModel Create(NavigationViewModel navigationViewModel, in BusSearchParameters searchParameters, Credentials? credentialsForOrder)
		{
			var busSearchingViewmodel = new BusSearchingViewModel(navigationViewModel, in searchParameters, credentialsForOrder);
			Task.Run(busSearchingViewmodel.SearchProcess);
			return busSearchingViewmodel;
		}

		private BusSearchingViewModel(NavigationViewModel navigationViewModel, in BusSearchParameters searchParameters, Credentials? credentialsForOrder)
			: base(navigationViewModel, searchParameters.GetDescrpiption())
		{
			this.searchParameters = searchParameters;
            this.credentialsForOrder = credentialsForOrder;
        }

		protected override object GetCancelViewModel() => new TransportSelectionViewModel(NavigationViewModel);

		protected override Uri GetLink() => BusApi.GetSiteUri();

		protected override bool TryFind(out string goodResultMessage)
		{
			goodResultMessage = string.Empty;
			if (IsCanceled)
				return false;

			var requestParameters = new SearchParameters(
				searchParameters.FromStation, searchParameters.ToStation, searchParameters.Date);

			if (BusApi.GetSchedule(in requestParameters, out ReadOnlyCollection<BusInfo> schedule))
			{
				if (IsCanceled)
					return false;
				List<BusInfo> filteredBuses = schedule.Where(
					bus =>
						bus.Time >= searchParameters.FromTime
						&& bus.Time <= searchParameters.ToTime
						&& bus.FreePlaces > 0).ToList();

				if (filteredBuses.Count > 0)
				{
					if (credentialsForOrder.HasValue)
					{
						Credentials credential = credentialsForOrder.Value;
						BusInfo targetBus = filteredBuses[filteredBuses.Count / 2];
						BusApi.Order(targetBus, searchParameters.FromStation, credential.Login, credential.Password);
						goodResultMessage = $"Was ordered for {targetBus.Time.ToShortString()} at: {DateTime.Now.ToLongTimeString()}";
						return true;
					}
					else
					{
						goodResultMessage = $"Was found at: {DateTime.Now.ToLongTimeString()}";
						return true;
					}
				}
				else
					return false;
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}