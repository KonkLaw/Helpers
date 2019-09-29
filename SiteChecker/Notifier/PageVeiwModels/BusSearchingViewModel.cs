using RouteByApi;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Notifier.PageVeiwModels
{
	class BusSearchingViewModel : BaseSearingViewModel
	{
		private readonly RouteApiSession session;
		private readonly SearchParamters searchParamters;
		static PrivateData privateData;

		public static BusSearchingViewModel Create(NavigationViewModel navigationViewModel, SearchParamters searchParamters, PrivateData privateData)
		{
			BusSearchingViewModel.privateData = privateData;
			var busSearchingViewmodel = new BusSearchingViewModel(navigationViewModel, searchParamters, privateData);
			Task.Run(busSearchingViewmodel.SearchProcess);
			return busSearchingViewmodel;
		}

		private BusSearchingViewModel(NavigationViewModel navigaionViewModel, SearchParamters searchParamters, PrivateData privateData)
			: base(navigaionViewModel)
		{
			this.searchParamters = searchParamters;
			session = BusApi.GetSession(privateData);
		}

		protected override object GetCancelViewModel()
		{
			// Get back to parameters veiw model
			return new TransportSelectionViewModel(NavigaionViewModel);
		}

		protected override Uri GetLink() => BusApi.GetSiteUri();

		protected override bool TryFind()
		{
			if (CancelationSource.IsCancellationRequested)
				return false;

			ReadOnlyCollection<BusInfo> schedule = session.GetSchedule(searchParamters.FromMinskToStolbcy, searchParamters.Date);
			var session2 = BusApi.GetSession(privateData);
			ReadOnlyCollection<BusInfo> schedule2 = session2.GetSchedule(searchParamters.FromMinskToStolbcy, searchParamters.Date);

			if (CancelationSource.IsCancellationRequested)
				return false;
			List<BusInfo> selected = schedule.Where(
				bus => bus.Time >= searchParamters.FromTime && bus.Time <= searchParamters.ToTime).ToList();

			return selected.Count > 0;
		}

		public void Buy()
		{
			TimeSpan preferedTime = default;
			List<BusInfo> selected = null;
			long worstMark = long.MaxValue;
			List<(int index, long mark)> orderedResult = selected.Select(
				(record, index) => (
					index,
					mark: record.TicketsCount < 1 ? worstMark : (record.Time - preferedTime).Ticks))
				.OrderBy(t => t.mark).ToList();
			session.Buy();
		}
	}
}