using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RwByApi;

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
			var resultViewModel = new TrainSearingViewModel(in trainParameters, selectedTrains, mainViewmodel);
            Task.Run(resultViewModel.SearchProcess);
            return resultViewModel;
        }

        private TrainSearingViewModel(in TrainParameters trainParameters, List<TrainInfo> selectedTrains, NavigationViewModel navigationViewModel)
			: base(navigationViewModel)
        {
            this.trainParameters = trainParameters;
            this.selectedTrains = selectedTrains;
        }

		protected override Uri GetLink() => TrainsInfoApi.GetRequestUri(in trainParameters);

		protected override object GetCancelViewModel() => new TrainParametersViewmodel(NavigationViewModel);

		protected override bool TryFind(out string goodResultMessage)
		{
			goodResultMessage = string.Empty;
			List<TrainInfo> schedule  = TrainsInfoApi.GetInterRegionalBusinessTrains(in trainParameters);
			foreach (TrainInfo train in selectedTrains)
			{
				if (IsCanceled)
					return false;
				TrainInfo trainInSchedule = schedule.First(t => train.TrainTime == t.TrainTime);
				if (IsCanceled)
					return false;
				if (trainInSchedule.AnyPlaces)
				{
					goodResultMessage = "Was found at: " + DateTime.Now.ToLongTimeString();
					return true;
				}
			}
			return false;
		}
	}
}