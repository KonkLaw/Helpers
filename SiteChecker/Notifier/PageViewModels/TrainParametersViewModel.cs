using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainsApi;

namespace Notifier.PageViewModels
{
	class TrainParametersViewmodel : BasePageViewModel
	{
		public string[] Stations => StationsHelpers.ReliableStations;

        public DelegateCommand NextCommand { get; }
        public DelegateCommand BackCommand { get; }

        private string from;
		public string From
		{
			get => from;
			set
			{
				if (SetProperty(ref from, value))
				{
					if (from != null)
						To = StationsHelpers.GetOpposite(from);
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
						From = StationsHelpers.GetOpposite(to);
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

        private readonly Func<TrainsResult> getTrainsFunction;
        private readonly NavigationViewModel navigationViewModel;

        public TrainParametersViewmodel(NavigationViewModel navigationViewModel)
        {
            NextCommand = new DelegateCommand(NextHandler, GetIsNextAllowed);
            BackCommand = new DelegateCommand(
                () => this.navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
            getTrainsFunction = GetTrains;
            this.navigationViewModel = navigationViewModel;
        }

        private TrainsResult GetTrains()
        {
            var trainParameters = new TrainParameters(date.Value, from, to);
            // TODO: possible fancy async here. Pull it down to response reading.
            return new TrainsResult(TrainsInfoApi.GetBusinessClassTrains(trainParameters), trainParameters);
        }

        private async void NextHandler()
        {
            navigationViewModel.IsOnWaiting = true;
            TrainsResult result = await Task.Run(getTrainsFunction);
            navigationViewModel.IsOnWaiting = false;
            navigationViewModel.Show(new TrainSelectionViewModel(result, navigationViewModel));
        }

        private void ValidateNextButtonAllowed() => NextCommand.RaiseCanExecuteChanged();

        public bool GetIsNextAllowed()
		{
            var dateRounded = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            return from != to && date.HasValue && date.Value >= dateRounded;
        }
	}

	readonly struct TrainsResult
    {
        public readonly List<TrainInfo> Trains;
        public readonly TrainParameters TrainParameters;

        public TrainsResult(List<TrainInfo> trains, TrainParameters trainParameters)
        {
            Trains = trains;
            TrainParameters = trainParameters;
        }
    }

	class StationsHelpers
	{
		public static string[] ReliableStations = TrainsInfoApi.GetReliableStations();

		internal static string GetOpposite(string station)
		{
			if (ReliableStations[0] == station)
				return ReliableStations[1];
			else if (ReliableStations[1] == station)
				return ReliableStations[0];
			else throw new InvalidOperationException();
		}
	}
}
