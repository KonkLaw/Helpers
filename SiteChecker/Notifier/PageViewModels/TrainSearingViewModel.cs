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
			: base(navigationViewModel, GetDescription(in trainParameters, selectedTrains))
        {
            this.trainParameters = trainParameters;
            this.selectedTrains = selectedTrains;
        }

		private static string GetDescription(in TrainParameters parameters, List<TrainInfo> selectedTrains)
        {
			string times = parameters.GetDescription() + Environment.NewLine + "Trains {";
            foreach (TrainInfo train in selectedTrains)
            {
				times +=  train.TrainTime.ToShortString() + " (" + train.TrainId + "), ";
			}
			times = times.Remove(times.Length - 2);
			times += "}";
			return times;
		}

		protected override Uri GetLink() => TrainsInfoApi.GetRequestUri(in trainParameters);

		protected override object GetCancelViewModel() => new TrainParametersViewmodel(NavigationViewModel);

		protected override bool TryFind(out string goodResultMessage)
		{
			goodResultMessage = string.Empty;
			foreach (TrainInfo train in selectedTrains)
			{
				if (IsCanceled)
					return false;
				if (TrainsInfoApi.HasTickets(train.TrainId, in trainParameters))
				{
					goodResultMessage = "Was found at: " + DateTime.Now.ToLongTimeString();
					return true;
				}
			}
			return false;
		}
	}
}