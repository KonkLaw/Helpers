using Notifier.PageViewModels;
using Notifier.PaveViews;
using System.Windows;

namespace Notifier
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = SetupView();
		}

		private NavigationViewModel SetupView()
		{
			var mainViewmodel = new NavigationViewModel();
            mainViewmodel.DeclareMapping<TransportSelectionViewModel>(new TransportationSelectionView());
			mainViewmodel.DeclareMapping<BusCredentialsViewModel>(new BusCreadentialsView());
			mainViewmodel.DeclareMapping<BusSearchingViewModel>(new SearchingVeiw());

			mainViewmodel.DeclareMapping<TrainParametersViewmodel>(new TrainParametersView());
			mainViewmodel.DeclareMapping<TrainSelectionViewModel>(new TrainSelectionView());
			mainViewmodel.DeclareMapping<TrainSearingViewModel>(new SearchingVeiw());

            mainViewmodel.DeclareMapping<BusParametersViewmodel>(new BusParametersView());

            mainViewmodel.Show(new TransportSelectionViewModel(mainViewmodel));
			return mainViewmodel;
		}
	}
}
