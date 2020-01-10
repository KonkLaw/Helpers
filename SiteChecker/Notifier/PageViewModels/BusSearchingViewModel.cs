using AtlasbusByApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Notifier.PageViewModels
{
	class BusSearchingViewModel : BaseSearingViewModel
	{
		private readonly BusSearchParameters searchParameters;

        public static BusSearchingViewModel Create(NavigationViewModel navigationViewModel, in BusSearchParameters searchParameters)
		{
            var busSearchingViewmodel = new BusSearchingViewModel(navigationViewModel, in searchParameters);
			Task.Run(busSearchingViewmodel.SearchProcess);
			return busSearchingViewmodel;
		}

		private BusSearchingViewModel(NavigationViewModel navigationViewModel, in BusSearchParameters searchParameters)
			: base(navigationViewModel)
		{
			this.searchParameters = searchParameters;
            //this.session = session;
        }

		protected override object GetCancelViewModel() => new TransportSelectionViewModel(NavigationViewModel);

		protected override Uri GetLink() => BusApi.GetSiteUri(searchParameters.FromStation, searchParameters.ToStation);

		protected override bool TryFind(out string goodResultMessage)
		{
			goodResultMessage = string.Empty;
			if (IsCanceled)
				return false;

			var requestParamters = new SearchParameters(
				searchParameters.FromStation, searchParameters.ToStation, searchParameters.Date);

			if (BusApi.GetSchedule(in requestParamters, out ReadOnlyCollection<BusInfo> schedule))
			{
				if (IsCanceled)
					return false;
				List<BusInfo> filteredBusses = schedule.Where(
					bus =>
						bus.Time >= searchParameters.FromTime
						&& bus.Time <= searchParameters.ToTime
						&& bus.TicketsCount > 0).ToList();

				if (filteredBusses.Count > 0)
				{
					BusInfo midleBus = filteredBusses[filteredBusses.Count / 2];
					//if (searchParameters.ShouldBy)
					//{
					//	var orderParameters = new OrderParameters(
					//		searchParameters.FromStation, searchParameters.ToStation, midleBus.Id);
					//	session.Order(in orderParameters);
					//	goodResultMessage = $"Was bought at: {DateTime.Now.ToLongTimeString()}. Time: |{midleBus.Time}|";
					//}
					//else
					{
						goodResultMessage = $"Was found at: {DateTime.Now.ToLongTimeString()}";
					}
					return true;
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