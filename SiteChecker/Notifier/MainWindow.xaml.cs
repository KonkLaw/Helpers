﻿using Notifier.PageViewModels;
using Notifier.PaveViews;
using System.Windows;

namespace Notifier;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = SetupView();

        Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
        Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
    }

    private NavigationViewModel SetupView()
    {
        var mainViewmodel = new NavigationViewModel();

        mainViewmodel.DeclareMapping<TransportSelectionViewModel>(new TransportationSelectionView());
        mainViewmodel.DeclareMapping<SearchViewModel>(new SearchingView());
        mainViewmodel.DeclareMapping<BusCredentialsViewModel>(new BusCredentialsView());
        mainViewmodel.DeclareMapping<TrainParametersViewmodel>(new TrainParametersView());
        mainViewmodel.DeclareMapping<TrainSelectionViewModel>(new TrainSelectionView());
        mainViewmodel.DeclareMapping<BusParametersViewmodel>(new BusParametersView());

        mainViewmodel.Show(new TransportSelectionViewModel(mainViewmodel));
        return mainViewmodel;
    }
}