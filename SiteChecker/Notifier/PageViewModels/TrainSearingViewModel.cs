using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainsApi;

namespace Notifier.PageViewModels
{
	class TrainSearingViewModel : BaseSearingViewModel
	{
        private readonly TrainParameters trainParameters;
        private readonly List<TrainInfo> selectedTrains;

        public static TrainSearingViewModel CreateRunSearch(
            in TrainParameters trainParameters, List<TrainInfo> selectedTrains, NavigationViewModel mainViewmodel)
        {
            if (selectedTrains.Count == 0)
                throw new ArgumentException("Empty collection of trains");
            var resultViewModel = new TrainSearingViewModel(trainParameters, selectedTrains, mainViewmodel);
            Task.Run(resultViewModel.SearchProcess);
            return resultViewModel;
        }

        private TrainSearingViewModel(in TrainParameters trainParameters, List<TrainInfo> selectedTrains, NavigationViewModel navigationViewModel)
			: base(navigationViewModel)
        {
            this.trainParameters = trainParameters;
            this.selectedTrains = selectedTrains;
        }

		protected override Uri GetLink() => TrainsInfoApi.GetRequestUri(trainParameters);

		protected override object GetCancelViewModel() => new TrainParametersViewmodel(NavigationViewModel);

		protected override bool TryFind()
		{
			foreach (TrainInfo train in selectedTrains)
			{
				if (CancellationSource.IsCancellationRequested)
					return false;
				bool haveTicketsForNotDisabled = TrainsInfoApi.HaveTicketsForNotDisabled(trainParameters, train);
				if (CancellationSource.IsCancellationRequested)
					return false;
				if (haveTicketsForNotDisabled)
				{
					return true;
				}
			}
			return false;
		}
	}
}