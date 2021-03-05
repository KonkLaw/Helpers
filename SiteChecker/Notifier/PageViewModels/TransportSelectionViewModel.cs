using Notifier.UtilTypes;
using System.Windows.Input;

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
            BusCommand = new DelegateCommand(() => this.navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel)));
        }
    }
}
