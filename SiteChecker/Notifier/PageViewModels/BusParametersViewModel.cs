using Prism.Commands;
using RouteByApi;
using System;

namespace Notifier.PageViewModels
{
	internal class BusParametersViewmodel : BasePageViewModel
	{
		private readonly NavigationViewModel navigationViewModel;
        private readonly RouteApiSession session;

        public DelegateCommand BackCommand { get; }
		public DelegateCommand NextCommand { get; }

		public string[] Stations { get; } = new string[] { "Минск", "Столбцы" };

		private string from;

		public string From
		{
			get => from;
			set
			{
				if (SetProperty(ref from, value))
				{
					if (from != null)
						To = GetOpposite(from);
					ValidateNextButtonAllowed();
				}
			}
		}

		private string to;
		public string To
		{
			get => to;
			set
			{
				if (SetProperty(ref to, value))
				{
					if (to != null)
						From = GetOpposite(to);
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

		public DateTime StartDate { get; } = DateTime.Now;
		public DateTime EndDate { get; } = DateTime.Now.AddDays(3 - 1);

		private DateTime? fromTime;
		public DateTime? FromTime
		{
			get => fromTime;
			set
			{
				if (SetProperty(ref fromTime, value))
					ValidateNextButtonAllowed();
			}
		}

		private DateTime? toTime;
		public DateTime? ToTime
		{
			get => toTime;
			set
			{
				if (SetProperty(ref toTime, value))
					ValidateNextButtonAllowed();
			}
		}

		public DateTime MinTime { get; } = new DateTime(1, 1, 1, BusApi.MinHours, 0, 0);
		public DateTime MaxTime { get; } = new DateTime(1, 1, 1, BusApi.MaxHours, 0, 0);

        public TimeSpan StartTimeInList { get; } = new TimeSpan(BusApi.MinHours, 0, 0);
		public TimeSpan StopTimeInList { get; } = new TimeSpan(BusApi.MaxHours, 0, 0);

		public BusParametersViewmodel(NavigationViewModel navigationViewModel, RouteApiSession session)
		{
			this.navigationViewModel = navigationViewModel;
            this.session = session;
            BackCommand = new DelegateCommand(
				() => navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
			NextCommand = new DelegateCommand(NextHandler, GetNextEnabled);
		}

		private void NextHandler()
		{
			bool fromMinskToS;
			if (from == Stations[0] && to == Stations[1])
				fromMinskToS = true;
			else if (from == Stations[1] && to == Stations[0])
				fromMinskToS = false;
			else
				throw new Exception();

			var searchParameters = new SearchParameters(fromMinskToS, date.Value.Date, fromTime.Value.TimeOfDay, toTime.Value.TimeOfDay);
			navigationViewModel.Show(BusSearchingViewModel.Create(navigationViewModel, searchParameters, session));
		}

		private void ValidateNextButtonAllowed() => NextCommand.RaiseCanExecuteChanged();

		private bool GetNextEnabled()
			=> from != null && to != null && from != to
			&& date.HasValue && fromTime.HasValue && toTime.HasValue
			&& fromTime.Value.Ticks < toTime.Value.Ticks;

		private string GetOpposite(string station)
		{
			if (station == Stations[0])
				return Stations[1];
			else if (station == Stations[1])
				return Stations[0];
			else
				throw new InvalidOperationException();
		}
	}

	readonly struct SearchParameters
	{
		public readonly bool FromMinskToS;
		public readonly DateTime Date;
		public readonly TimeSpan FromTime;
		public readonly TimeSpan ToTime;

		public SearchParameters(bool fromMinskToS, DateTime date, TimeSpan fromTime, TimeSpan toTime)
		{
			FromMinskToS = fromMinskToS;
			Date = date;
			FromTime = fromTime;
			ToTime = toTime;
		}
	}
}