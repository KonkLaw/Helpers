using Notifier.UtilTypes;
using System.Windows.Input;
using Notifier.Model;

namespace Notifier.PageViewModels;

class TransportSelectionViewModel : BasePageViewModel
{
    public ICommand RailwayCommand { get; }
    public ICommand BusStolbtcyCommand { get; }
    public ICommand BusAtlasCommand { get; }

    public TransportSelectionViewModel(NavigationViewModel navigationViewModel)
    {
        RailwayCommand = new DelegateCommand(() => navigationViewModel.Show(new TrainParametersViewmodel(navigationViewModel)));
        BusStolbtcyCommand = new DelegateCommand(() => navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, new StolbtcyModel())));
        BusAtlasCommand = new DelegateCommand(() => navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, new AtlasBaseBusModel())));
    }
}