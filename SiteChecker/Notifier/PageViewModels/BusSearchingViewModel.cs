﻿using RouteByApi;
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
		private readonly SearchParameters searchParameters;

        public static BusSearchingViewModel Create(NavigationViewModel navigationViewModel, SearchParameters searchParameters, RouteApiSession session)
		{
            var busSearchingViewmodel = new BusSearchingViewModel(navigationViewModel, searchParameters, session);
			Task.Run(busSearchingViewmodel.SearchProcess);
			return busSearchingViewmodel;
		}

		private BusSearchingViewModel(NavigationViewModel navigationViewModel, SearchParameters searchParameters, RouteApiSession session)
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

		protected override bool TryFind()
		{
			if (CancellationSource.IsCancellationRequested)
				return false;

            if (session.GetSchedule(searchParameters.FromMinskToS, searchParameters.Date,
                out ReadOnlyCollection<BusInfo> schedule, out _))
            {

                if (CancellationSource.IsCancellationRequested)
                    return false;
                List<BusInfo> selected = schedule.Where(
                    bus => bus.Time >= searchParameters.FromTime && bus.Time <= searchParameters.ToTime).ToList();

                return selected.Count > 0;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
	}
}