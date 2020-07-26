using AtlasbusByApi;
using CredentialHelper;
using Prism.Commands;
using System;
using System.Collections.Generic;

namespace Notifier.PageViewModels
{
	internal class BusParametersViewmodel : BasePageViewModel
	{
		private static BusApiSession? cachedSession;
		private readonly NavigationViewModel navigationViewModel;
        
        public DelegateCommand BackCommand { get; }
		public DelegateCommand NextCommand { get; }
		public DelegateCommand Today { get; }
		public DelegateCommand Tomorow { get; }

		private bool isFromListOpened = true;
		public bool IsFromListOpened
		{
			get => isFromListOpened;
			set => SetProperty(ref isFromListOpened, value);
		}

		public static IEnumerable<Station> Stations => BusApi.Stations;

		private Station? fromStation;
		public Station? FromStation
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

		private Station? toStation;
		public Station? ToStation
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

		private DateTime startDate = DateTime.Now.Date;
		public DateTime StartDate
		{
			get => startDate;
			set
			{
				if (SetProperty(ref startDate, value))
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
			set
			{
				if (SetProperty(ref shouldBy, value) && value)
				{
					Check();
				}
			}
		}

		private void Check()
		{
			if (cachedSession != null)
				return;

			string messageForLoginWindow = string.Empty;
			if (new WindowsCredentialStorage().TryLoad(out Credentials credentials))
			{
				var loginData = new LoginData(credentials.Login, credentials.Password);
				cachedSession = BusApi.TryLogin(in loginData, out string errorMessage);
                if (cachedSession == null)
                    messageForLoginWindow = "Previous credentials was wrong: " + errorMessage;
                else
                    return;
            }			
			navigationViewModel.Show(new BusCredentialsViewModel(navigationViewModel, this)
			{
				Message = messageForLoginWindow
			});
		}

		public BusParametersViewmodel(NavigationViewModel navigationViewModel)
		{
			this.navigationViewModel = navigationViewModel;
            BackCommand = new DelegateCommand(
				() => navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
			NextCommand = new DelegateCommand(NextHandler, GetNextEnabled);
			Today = new DelegateCommand(() => Date = DateTime.Now.Date);
			Tomorow = new DelegateCommand(() => Date = DateTime.Now.Date.AddDays(1));
		}

		public void SetSession(BusApiSession busApiSession) => cachedSession = busApiSession;

		private void NextHandler()
		{
            if (!date.HasValue || fromStation == null || toStation == null)
                return;
			BusApiSession? sessionForOrder = shouldBy ? cachedSession : null;
            if (shouldBy && sessionForOrder == null)
			{
				throw new InvalidOperationException();
			}
			var searchParameters = new BusSearchParameters(
				fromStation, toStation, date.Value.Date, fromTime, toTime, sessionForOrder);
			navigationViewModel.Show(BusSearchingViewModel.Create(navigationViewModel, in searchParameters));
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
        public readonly BusApiSession? SessionForOrder;

        public BusSearchParameters(
            Station fromStation, Station toStation, DateTime date, TimeSpan fromTime, TimeSpan toTime, BusApiSession? sessionForOrder)
		{
			FromStation = fromStation;
			ToStation = toStation;
			Date = date;
			FromTime = fromTime;
			ToTime = toTime;
			SessionForOrder = sessionForOrder;
        }
	}
}