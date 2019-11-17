using Prism.Commands;
using RouteByApi;
using System;
using System.Collections.Generic;

namespace Notifier.PageViewModels
{
	internal class BusParametersViewmodel : BasePageViewModel
	{
		private readonly NavigationViewModel navigationViewModel;
        private readonly RouteApiSession session;

        public DelegateCommand BackCommand { get; }
		public DelegateCommand NextCommand { get; }
		public DelegateCommand Today { get; }
		public DelegateCommand Tomorow { get; }

		public static IEnumerable<Station> Stations => BusApi.Stations;

		private Station fromStation;
		public Station FromStation
		{
			get => fromStation;
			set
			{
				if (SetProperty(ref fromStation, value))
				{
					if (fromStation != null)
						ToStation = GetOpposite(fromStation);
					ValidateNextButtonAllowed();
				}
			}
		}

		private Station toStation;
		public Station ToStation
		{
			get => toStation;
			set
			{
				if (SetProperty(ref toStation, value))
				{
					if (toStation != null)
						FromStation = GetOpposite(toStation);
					ValidateNextButtonAllowed();
				}
			}
		}

		private DateTime? date;
		public DateTime? Date
		{
			get => date;
			set
			{
				if (SetProperty(ref date, value))
					ValidateNextButtonAllowed();
			}
		}

		private TimeSpan fromTime = new TimeSpan(13, 0, 0);
		public TimeSpan FromTime
		{
			get => fromTime;
			set
			{
				if (SetProperty(ref fromTime, value))
					ValidateNextButtonAllowed();
			}
		}

		private TimeSpan toTime = new TimeSpan(16, 0, 0);
        public TimeSpan ToTime
		{
			get => toTime;
			set
			{
				if (SetProperty(ref toTime, value))
					ValidateNextButtonAllowed();
			}
		}

        private bool shouldBy;
        public bool ShouldBy
        {
            get => shouldBy;
            set => SetProperty(ref shouldBy, value);
        }

        public BusParametersViewmodel(NavigationViewModel navigationViewModel, RouteApiSession session)
		{
			this.navigationViewModel = navigationViewModel;
            this.session = session;
            BackCommand = new DelegateCommand(
				() => navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
			NextCommand = new DelegateCommand(NextHandler, GetNextEnabled);
			Today = new DelegateCommand(() => Date = DateTime.Now.Date);
			Tomorow = new DelegateCommand(() => Date = DateTime.Now.Date.AddDays(1));
		}

		private void NextHandler()
		{
            if (!date.HasValue)
                return;
			var searchParameters = new BusSearchParameters(
				fromStation, toStation, date.Value.Date, fromTime, toTime, shouldBy);
			navigationViewModel.Show(BusSearchingViewModel.Create(navigationViewModel, in searchParameters, session));
		}

		private void ValidateNextButtonAllowed() => NextCommand.RaiseCanExecuteChanged();

		private bool GetNextEnabled()
			=> fromStation != null && toStation != null && fromStation != toStation
			&& date.HasValue
			&& (toTime - fromTime) > new TimeSpan(0, 22, 0);

		private static Station GetOpposite(Station station)
		{
			if (station == BusApi.Stations[0])
				return BusApi.Stations[1];
			else if (station == BusApi.Stations[1])
				return BusApi.Stations[0];
			else
				throw new InvalidOperationException();
		}
	}

	readonly struct BusSearchParameters
	{
		public readonly Station FromStation;
		public readonly Station ToStation;
		public readonly DateTime Date;
		public readonly TimeSpan FromTime;
		public readonly TimeSpan ToTime;
        public readonly bool ShouldBy;

        public BusSearchParameters(
            Station fromStation, Station toStation, DateTime date, TimeSpan fromTime, TimeSpan toTime, bool shouldBy)
		{
			FromStation = fromStation;
			ToStation = toStation;
			Date = date;
			FromTime = fromTime;
			ToTime = toTime;
            ShouldBy = shouldBy;
        }
	}
}