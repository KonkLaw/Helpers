using System;
using CredentialHelper.Interface;
using Prism.Commands;
using System.Windows.Input;
using RouteByApi;

namespace Notifier.PageViewModels
{
    class TransportSelectionViewModel : BasePageViewModel
    {
        private readonly NavigationViewModel navigationViewModel;

        public ICommand RailwayCommand { get; }
        public ICommand BusCommand { get; }

        public TransportSelectionViewModel(NavigationViewModel navigationViewModel)
        {
            this.navigationViewModel = navigationViewModel;
            RailwayCommand = new DelegateCommand(() => this.navigationViewModel.Show(new TrainParametersViewmodel(navigationViewModel)));
            BusCommand = new DelegateCommand(BusSelect);
        }

		private void BusSelect()
		{
            if (StorageHelper.TryLoad(out Credentials credentials))
            {
				var sessionData = new SessionData(credentials.Login, credentials.Sessid, credentials.Uidh);
				if (BusApi.TryGetCachedSession(in sessionData, out RouteApiSession session))
                {
                    navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, session));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
				navigationViewModel.Show(new BusCredentialsViewModel(navigationViewModel));
		}
    }
}
