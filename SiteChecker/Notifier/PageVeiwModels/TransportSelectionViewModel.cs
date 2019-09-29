using CredentialHelper;
using CredentialHelper.Interface;
using Prism.Commands;
using System.Windows.Input;

namespace Notifier.PageVeiwModels
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
			if (new WindowsCredentialStorage().TryLoad(out UserInfo userInfo))
				navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, userInfo));
			else
				navigationViewModel.Show(new BusCredentialsViewModel(navigationViewModel));
		}
    }
}
