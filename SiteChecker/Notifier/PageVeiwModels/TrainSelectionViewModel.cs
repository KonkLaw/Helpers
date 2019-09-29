using Prism.Commands;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;
using TrainsApi;

namespace Notifier.PageVeiwModels
{
    class TrainSelectionViewModel : BasePageViewModel
    {
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

        private readonly NavigationViewModel mainViewmodel;
        private readonly TrainParameters trainParameters;

        public TrainSelectionViewModel(TrainsResult trainsResult, NavigationViewModel mainViewmodel)
        {
            trainParameters = trainsResult.TrainParameters;
            Trains = trainsResult.Trains.Select(t => new TrainViewModel(t)).ToList();

            NextCommand = new DelegateCommand(NextHandler, () => trains.Any(t => t.IsSelected));
            BackCommand = new DelegateCommand(() => mainViewmodel.Show(new TrainParametersViewmodel(mainViewmodel)));
            this.mainViewmodel = mainViewmodel;
        }

        private void ValidateNextButton() => NextCommand.RaiseCanExecuteChanged();

        private void NextHandler()
        {
            List<TrainInfo> selectedTrains = trains.Where(t => t.IsSelected).Select(t => t.TrainInfo).ToList();
            mainViewmodel.Show(TrainSearingViewModel.CreateRunSearch(trainParameters, selectedTrains, mainViewmodel));
        }
    }

    public class TrainViewModel : BindableBase
    {
        public TrainInfo TrainInfo { get; private set; }

        private bool isSelected;

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
                RaisePropertyChanged();
            }
        }

        public TrainViewModel(TrainInfo trainInfo) => TrainInfo = trainInfo;
    }
}
