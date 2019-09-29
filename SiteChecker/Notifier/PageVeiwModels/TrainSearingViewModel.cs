using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainsApi;

namespace Notifier.PageVeiwModels
{
	class TrainSearingViewModel : BaseSearingViewModel
	{
        private readonly TrainParameters trainParameters;
        private readonly List<TrainInfo> selectedTrains;

        public static TrainSearingViewModel CreateRunSearch(
            TrainParameters trainParameters, List<TrainInfo> selectedTrains, NavigationViewModel mainViewmodel)
        {
            if (selectedTrains.Count == 0)
                throw new ArgumentException("Empty collection of trains");
            var resultViewModel = new TrainSearingViewModel(trainParameters, selectedTrains, mainViewmodel);
            Task.Run(resultViewModel.SearchProcess);
            return resultViewModel;
        }

        private TrainSearingViewModel(TrainParameters trainParameters, List<TrainInfo> selectedTrains, NavigationViewModel navigaionViewModel)
			: base(navigaionViewModel)
        {
            this.trainParameters = trainParameters;
            this.selectedTrains = selectedTrains;
        }

		protected override Uri GetLink() => TrainsInfoApi.GetRequestUri(trainParameters);

		protected override object GetCancelViewModel() => new TrainParametersViewmodel(NavigaionViewModel);

		protected override bool TryFind()
		{
			foreach (TrainInfo train in selectedTrains)
			{
				if (CancelationSource.IsCancellationRequested)
					return false;
				bool haveTicketsForNotDisabled = TrainsInfoApi.HaveTicketsForNotDisabled(trainParameters, train);
				if (CancelationSource.IsCancellationRequested)
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