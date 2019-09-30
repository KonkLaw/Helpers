using CredentialHelper.Interface;
using Prism.Commands;
using RouteByApi;
using System;

namespace Notifier.PageViewModels
{
	internal class BusParametersViewmodel : BasePageViewModel
	{
		private readonly NavigationViewModel navigationViewModel;
		private readonly UserInfo userInfo;

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

		public DateTime MinTime { get; } = DateTime.Now.AddHours(-5);
		public DateTime MaxTime { get; } = DateTime.Now.AddHours(+5);

		public TimeSpan StartTimeInList { get; } = new TimeSpan(5, 0, 0);
		public TimeSpan StopTimeInList { get; } = new TimeSpan(22, 0, 0);

		public BusParametersViewmodel(NavigationViewModel navigationViewModel, UserInfo userInfo)
		{
			this.navigationViewModel = navigationViewModel;
			this.userInfo = userInfo;
			BackCommand = new DelegateCommand(
				() => navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
			NextCommand = new DelegateCommand(NextHandler, GetNextEnabled);
		}

		private void NextHandler()
		{
			bool fromMInskToStolbcy;
			if (from == Stations[0] && to == Stations[1])
				fromMInskToStolbcy = true;
			else if (from == Stations[1] && to == Stations[0])
				fromMInskToStolbcy = false;
			else
				throw new Exception();

			var searchParamters = new SearchParamters(fromMInskToStolbcy, date.Value.Date, fromTime.Value.TimeOfDay, toTime.Value.TimeOfDay);
			navigationViewModel.Show(BusSearchingViewModel.Create(navigationViewModel, searchParamters, new PrivateData()
			{
				Pas = userInfo.Password,
				PhoneNumber = userInfo.PrivateLogin
			}
			));
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

	readonly struct SearchParamters
	{
		public readonly bool FromMinskToStolbcy;
		public readonly DateTime Date;
		public readonly TimeSpan FromTime;
		public readonly TimeSpan ToTime;

		public SearchParamters(bool fromMinskToStolbcy, DateTime date, TimeSpan fromTime, TimeSpan toTime)
		{
			FromMinskToStolbcy = fromMinskToStolbcy;
			Date = date;
			FromTime = fromTime;
			ToTime = toTime;
		}
	}
}