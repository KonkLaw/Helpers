using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RwByApi;

namespace Notifier.PageViewModels
{
	class TrainParametersViewmodel : BasePageViewModel
	{
		public static IEnumerable<Station> Stations => TrainsInfoApi.ReliableStations;

		public DelegateCommand NextCommand { get; }
		public DelegateCommand BackCommand { get; }
		public DelegateCommand Today { get; }
		public DelegateCommand Tomorrow { get; }

		private bool isFromListOpened = true;
		public bool IsFromListOpened
		{
			get => isFromListOpened;
			set => SetProperty(ref isFromListOpened, value);
		}

		private Station? fromStation;
		public Station? FromStation
		{
			get => fromStation;
			set
			{
				if (SetProperty(ref fromStation, value))
				{
					if (fromStation != null)
						ToStation = StationsHelpers.GetOpposite(fromStation);
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
						FromStation = StationsHelpers.GetOpposite(toStation);
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

		private readonly NavigationViewModel navigationViewModel;

		public TrainParametersViewmodel(NavigationViewModel navigationViewModel)
		{
			NextCommand = new DelegateCommand(NextHandler, GetIsNextAllowed);
			BackCommand = new DelegateCommand(
				() => this.navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
			this.navigationViewModel = navigationViewModel;
			Today = new DelegateCommand(() => Date = DateTime.Now.Date);
			Tomorrow = new DelegateCommand(() => Date = DateTime.Now.Date.AddDays(1));
		}

		private TrainsResult GetTrains()
		{
			var trainParameters = new TrainParameters(date!.Value, fromStation, toStation);
			// TODO: possible fancy async here. Pull it down to response reading.
			return new TrainsResult(TrainsInfoApi.GetInterRegionalBusinessTrains(in trainParameters), in trainParameters);
		}

		private async void NextHandler()
		{
			navigationViewModel.IsOnWaiting = true;
			TrainsResult result = await Task.Run(GetTrains).ConfigureAwait(true);
			navigationViewModel.IsOnWaiting = false;
			navigationViewModel.Show(new TrainSelectionViewModel(in result, navigationViewModel));
		}

		private void ValidateNextButtonAllowed() => NextCommand.RaiseCanExecuteChanged();

		public bool GetIsNextAllowed() => fromStation != toStation && date.HasValue && date.Value >= DateTime.Now.Date;
	}

	readonly struct TrainsResult
    {
        public readonly List<TrainInfo> Trains;
        public readonly TrainParameters TrainParameters;

        public TrainsResult(List<TrainInfo> trains, in TrainParameters trainParameters)
        {
            Trains = trains;
            TrainParameters = trainParameters;
        }
    }

	class StationsHelpers
	{
		internal static Station GetOpposite(Station station)
		{
			if (TrainsInfoApi.ReliableStations[0] == station)
				return TrainsInfoApi.ReliableStations[1];
			else if (TrainsInfoApi.ReliableStations[1] == station)
				return TrainsInfoApi.ReliableStations[0];
			else throw new InvalidOperationException();
		}
	}
}
