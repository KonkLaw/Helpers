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
		private readonly SearchParamters searchParameters;

        public static BusSearchingViewModel Create(NavigationViewModel navigationViewModel, SearchParamters searchParameters, PrivateData privateData)
		{
            var busSearchingViewmodel = new BusSearchingViewModel(navigationViewModel, searchParameters, privateData);
			Task.Run(busSearchingViewmodel.SearchProcess);
			return busSearchingViewmodel;
		}

		private BusSearchingViewModel(NavigationViewModel navigationViewModel, SearchParamters searchParameters, PrivateData privateData)
			: base(navigationViewModel)
		{
			this.searchParameters = searchParameters;
			session = BusApi.GetSession(privateData);
		}

		protected override object GetCancelViewModel()
		{
			// Get back to parameters view model
			return new TransportSelectionViewModel(NavigationViewModel);
		}

		protected override Uri GetLink() => BusApi.GetSiteUri();

		protected override bool TryFind()
		{
			if (CancellationSource.IsCancellationRequested)
				return false;

			ReadOnlyCollection<BusInfo> schedule = session.GetSchedule(searchParameters.FromMinskToStolbcy, searchParameters.Date);

            if (CancellationSource.IsCancellationRequested)
				return false;
			List<BusInfo> selected = schedule.Where(
				bus => bus.Time >= searchParameters.FromTime && bus.Time <= searchParameters.ToTime).ToList();

			return selected.Count > 0;
		}

		public void Buy()
		{
			//TimeSpan preferredTime = default;
			//List<BusInfo> selected = null;
			//long worstMark = long.MaxValue;
			//List<(int index, long mark)> orderedResult = selected.Select(
			//	(record, index) => (
			//		index,
			//		mark: record.TicketsCount < 1 ? worstMark : (record.Time - preferredTime).Ticks))
			//	.OrderBy(t => t.mark).ToList();
			//session.Buy();
		}
	}
}