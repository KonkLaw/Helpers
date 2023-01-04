using Notifier.UtilTypes;
using System.Windows.Input;
using Notifier.SearchServices;

namespace Notifier.PageViewModels;

class TransportSelectionViewModel : BasePageViewModel
{
    public ICommand RailwayCommand { get; }
    public ICommand BusStowbtcyCommand { get; }
    public ICommand BusAtlasCommand { get; }

    public TransportSelectionViewModel(NavigationViewModel navigationViewModel)
    {
        RailwayCommand = new DelegateCommand(() => navigationViewModel.Show(new TrainParametersViewmodel(navigationViewModel)));
        BusStowbtcyCommand = new DelegateCommand(() => navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, new StowbtcyMinskProvider())));
        BusAtlasCommand = new DelegateCommand(() => navigationViewModel.Show(new BusParametersViewmodel(navigationViewModel, new AtlasSearchServiceProvider())));
    }
}