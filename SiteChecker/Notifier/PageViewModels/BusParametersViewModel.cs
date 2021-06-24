using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CredentialHelper;
using Notifier.Model;
using Notifier.UtilTypes;

namespace Notifier.PageViewModels
{
	class BusParametersViewmodel : BasePageViewModel
	{
		private static readonly WindowsCredentialStorage CredentialHelper = new WindowsCredentialStorage("Bus");
		private readonly NavigationViewModel navigationViewModel;
		private readonly IBaseBusModel busServiceModel;

		public DelegateCommand BackCommand { get; }
		public DelegateCommand NextCommand { get; }
		public DelegateCommand Today { get; }
		public DelegateCommand Tomorrow { get; }

		private bool isFromListOpened = true;

		public bool IsFromListOpened
		{
			get => isFromListOpened;
			set => SetProperty(ref isFromListOpened, value);
		}

		public static IEnumerable<Station> Stations { get; } = Station.Stations;
		public static IEnumerable<int> AvailablePassengersCount { get; } = Enumerable.Range(1, 4);

		private Station? fromStation;

		public Station? FromStation
		{
			get => fromStation;
			set
			{
				if (SetProperty(ref fromStation, value))
				{
					if (fromStation != null)
						ToStation = Station.GetOpposite(fromStation);
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
						FromStation = Station.GetOpposite(toStation);
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
			set => SetProperty(ref shouldBy, value);
		}

		private int passengersCount = 1;

		public int PassengersCount
		{
			get => passengersCount;
			set => SetProperty(ref passengersCount, value);
		}

		public bool CanOrder => busServiceModel.CanOrder;

		public BusParametersViewmodel(NavigationViewModel navigationViewModel, IBaseBusModel busServiceModel)
		{
			this.navigationViewModel = navigationViewModel;
			this.busServiceModel = busServiceModel;
			shouldBy = CanOrder;
			BackCommand = new DelegateCommand(
				() => navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
			NextCommand = new DelegateCommand(NextHandler, GetNextEnabled);
			Today = new DelegateCommand(() => Date = DateTime.Now.Date);
			Tomorrow = new DelegateCommand(() => Date = DateTime.Now.Date.AddDays(1));
		}

		private void NextHandler()
		{
			if (!date.HasValue || fromStation == null || toStation == null)
				return;

			var searchParameters = new BusSearchParameters(fromStation, toStation, date.Value.Date, fromTime, toTime,
				passengersCount);

			Credentials? credentialsToBuy;
			if (ShouldBy)
			{
				if (CredentialHelper.TryLoad(out Credentials credentials))
					credentialsToBuy = credentials;
				else
				{
					navigationViewModel.Show(new BusCredentialsViewModel(navigationViewModel, in searchParameters,
						CredentialHelper, busServiceModel));
					return;
				}
			}
			else
				credentialsToBuy = null;

			navigationViewModel.Show(BusSearchingViewModel.Create(navigationViewModel, in searchParameters,
				credentialsToBuy, busServiceModel));
		}

		private void ValidateNextButtonAllowed() => NextCommand.RaiseCanExecuteChanged();

		private bool GetNextEnabled()
			=> fromStation != null && toStation != null && fromStation != toStation
			   && date.HasValue
			   && toTime - fromTime > new TimeSpan(0, 22, 0);
	}

	readonly struct BusSearchParameters
	{
		public readonly Station FromStation;
		public readonly Station ToStation;
		public readonly DateTime Date;
		public readonly TimeSpan FromTime;
		public readonly TimeSpan ToTime;
		public readonly int PassengersCount;

		public BusSearchParameters(
			Station fromStation, Station toStation, DateTime date, TimeSpan fromTime, TimeSpan toTime,
			int passengersCount)
		{
			FromStation = fromStation;
			ToStation = toStation;
			Date = date;
			FromTime = fromTime;
			ToTime = toTime;
			PassengersCount = passengersCount;
		}

		public string GetDescription(string serviceDescription)
			=> $"Service:    {serviceDescription}" +
			   $"{Environment.NewLine}" +
			   $"Direction:  {FromStation}-{ToStation}" +
			   $"{Environment.NewLine}" +
			   $"Date:       {Date.ToString("yyyy-MM-dd dddd", CultureInfo.GetCultureInfo("ru-ru"))}" +
			   $"{Environment.NewLine}" +
			   $"Time:       {FromTime:hh\\:mm}-{ToTime:hh\\:mm}";
	}

	class Station
	{
		public static Station Stolbtcy { get; } = new Station("Столбцы");

		public static Station Minsk { get; } = new Station("Минск");

		public static Station[] Stations { get; } = {Stolbtcy, Minsk};

		public readonly string Name;

		private Station(string name) => Name = name;

		public override string ToString() => Name;

		public static Station GetOpposite(Station station)
		{
			if (station == Stolbtcy)
				return Minsk;
			if (station == Minsk)
				return Stolbtcy;

			throw new InvalidOperationException();
		}
	}
}