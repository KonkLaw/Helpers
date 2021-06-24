using System;
using System.Threading.Tasks;
using CredentialHelper;
using Notifier.Model;

namespace Notifier.PageViewModels
{
	class BusSearchingViewModel : BaseSearingViewModel
	{
		private readonly BusSearchParameters searchParameters;
		private readonly Credentials? credentialsForOrder;
		private readonly IBaseBusModel busServiceModel;

		public static BusSearchingViewModel Create(
			NavigationViewModel navigationViewModel,
			in BusSearchParameters searchParameters,
			Credentials? credentialsForOrder,
			IBaseBusModel busServiceModel)
		{
			var busSearchingViewmodel = new BusSearchingViewModel(navigationViewModel, in searchParameters,
				credentialsForOrder, busServiceModel);
			Task.Run(busSearchingViewmodel.SearchProcess);
			return busSearchingViewmodel;
		}

		private BusSearchingViewModel(NavigationViewModel navigationViewModel, in BusSearchParameters searchParameters,
			Credentials? credentialsForOrder, IBaseBusModel busServiceModel)
			: base(navigationViewModel, searchParameters.GetDescription(busServiceModel.ServiceDescription))
		{
			this.searchParameters = searchParameters;
			this.credentialsForOrder = credentialsForOrder;
			this.busServiceModel = busServiceModel;
		}

		protected override object GetCancelViewModel() => new TransportSelectionViewModel(NavigationViewModel);

		protected override Uri GetLink() => busServiceModel.GetSiteUri(searchParameters);

		protected override bool TryFind(out string goodResultMessage)
		{
			goodResultMessage = string.Empty;
			if (IsCanceled)
				return false;
			bool result = busServiceModel.TryFind(searchParameters, credentialsForOrder, ref goodResultMessage);
			if (IsCanceled)
				return false;
			return result;
		}
	}
}