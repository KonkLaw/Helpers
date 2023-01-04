using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Notifier.SearchServices;
using Notifier.UtilTypes;
using RwByApi;

namespace Notifier.PageViewModels;

class TrainSelectionViewModel : BasePageViewModel
{
    private readonly NavigationViewModel navigationViewModel;
    private readonly TrainParameters trainParameters;

    public DelegateCommand NextCommand { get; }
    public DelegateCommand BackCommand { get; }

    private List<TrainViewModel> trains;
    public List<TrainViewModel> Trains
    {
        get => trains;
        set => SetProperty(ref trains, value);
    }

    private int selectedIndex;
    public int SelectedIndex
    {
        get => selectedIndex;
        set
        {
            if (SetProperty(ref selectedIndex, value))
                ValidateNextButton();
        }
    }

    public TrainSelectionViewModel(in TrainsResult trainsResult, NavigationViewModel navigationViewModel)
    {
        this.navigationViewModel = navigationViewModel;
        trainParameters = trainsResult.TrainParameters;
        trains = trainsResult.Trains.Select(t => new TrainViewModel(t)).ToList();

        NextCommand = new DelegateCommand(NextHandler, () => trains.Any(t => t.IsSelected));
        BackCommand = new DelegateCommand(() => navigationViewModel.Show(new TrainParametersViewmodel(navigationViewModel)));
    }

    private void ValidateNextButton() => NextCommand.RaiseCanExecuteChanged();

    private void NextHandler()
    {
        ReadOnlyCollection<TrainInfo> selectedTrains =
            trains.Where(t => t.IsSelected).Select(t => t.TrainInfo).ToList().AsReadOnly();
        var searchViewModel = new SearchViewModel(
            navigationViewModel, new TrainSearchService(in trainParameters, selectedTrains));
        navigationViewModel.Show(searchViewModel);
        searchViewModel.RunSearch();
    }
}

public class TrainViewModel : BindableBase
{
    public TrainInfo TrainInfo { get; }

    private bool isSelected;

    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            RaisePropertyChanged();
        }
    }

    public TrainViewModel(TrainInfo trainInfo) => TrainInfo = trainInfo;
}