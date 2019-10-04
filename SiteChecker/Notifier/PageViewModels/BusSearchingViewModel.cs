using RouteByApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Notifier.PageViewModels
{
	class BusSearchingViewModel : BaseSearingViewModel
	{
		private readonly RouteApiSession session;
		private readonly BusSearchParameters searchParameters;

        public static BusSearchingViewModel Create(NavigationViewModel navigationViewModel, in BusSearchParameters searchParameters, RouteApiSession session)
		{
            var busSearchingViewmodel = new BusSearchingViewModel(navigationViewModel, in searchParameters, session);
			Task.Run(busSearchingViewmodel.SearchProcess);
			return busSearchingViewmodel;
		}

		private BusSearchingViewModel(NavigationViewModel navigationViewModel, in BusSearchParameters searchParameters, RouteApiSession session)
			: base(navigationViewModel)
		{
			this.searchParameters = searchParameters;
            this.session = session;
        }

		protected override object GetCancelViewModel()
		{
			// Get back to parameters view model
			return new TransportSelectionViewModel(NavigationViewModel);
		}

		protected override Uri GetLink() => BusApi.GetSiteUri();

		protected override bool TryFind(out string goodResultMessage)
		{
			goodResultMessage = default;
			if (CancellationSource.IsCancellationRequested)
				return false;

			var requestParamters = new SearchParameters(
				searchParameters.FromStation, searchParameters.ToStation, searchParameters.Date);

			if (session.GetSchedule(in requestParamters, out ReadOnlyCollection<BusInfo> schedule))
			{
                if (CancellationSource.IsCancellationRequested)
                    return false;
                List<BusInfo> selected = schedule.Where(
                    bus =>
						bus.Time >= searchParameters.FromTime
						&& bus.Time <= searchParameters.ToTime
						&& bus.TicketsCount > 0).ToList();
				
                if (selected.Count > 0)
                {
					if (searchParameters.ShouldBy)
					{
						var orderParameters = new OrderParameters(
							searchParameters.FromStation, searchParameters.ToStation, selected.First().Id);
						session.Order(in orderParameters);
						goodResultMessage = "Was bought at: " + DateTime.Now.ToLongTimeString();
					}
					else
					{
						goodResultMessage = "Was found at: " + DateTime.Now.ToLongTimeString();
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