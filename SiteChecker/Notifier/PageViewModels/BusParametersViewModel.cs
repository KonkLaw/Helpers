using System;
using System.Collections.Generic;
using System.Linq;
using CredentialHelper;
using Notifier.Model;
using Notifier.UtilTypes;

namespace Notifier.PageViewModels;

class BusParametersViewmodel : BasePageViewModel
{
    private static readonly WindowsCredentialStorage CredentialHelper = new WindowsCredentialStorage("Bus");
    private readonly NavigationViewModel navigationViewModel;
    private readonly ISearchServiceProvider searchProvider;

    public DelegateCommand BackCommand { get; }
    public DelegateCommand NextCommand { get; }
    public DelegateCommand Today { get; }
    public DelegateCommand Tomorrow { get; }

    private bool isFromListOpened = true;

    public bool IsFromListOpened
    {
        get => isFromListOpened;
        set => SetProperty(ref isFromListOpened, value);
    }

    public static IEnumerable<Station> Stations => Station.Stations;
    public static IEnumerable<int> AvailablePassengersCount { get; } = Enumerable.Range(1, 4);

    private Station? fromStation;
    public Station? FromStation
    {
        get => fromStation;
        set
        {
            if (SetProperty(ref fromStation, value))
            {
                if (fromStation != null)
                    ToStation = Station.GetOpposite(fromStation);
                ValidateNextButtonAllowed();
            }
        }
    }

    private Station? toStation;
    public Station? ToStation
    {
        get => toStation;
        set
        {
            if (SetProperty(ref toStation, value))
            {
                if (toStation != null)
                    FromStation = Station.GetOpposite(toStation);
                ValidateNextButtonAllowed();
            }
        }
    }

    private DateTime? date;
    public DateTime? Date
    {
        get => date;
        set
        {
            if (SetProperty(ref date, value))
                ValidateNextButtonAllowed();
        }
    }

    private DateTime startDate = DateTime.Now.Date;
    public DateTime StartDate
    {
        get => startDate;
        set
        {
            if (SetProperty(ref startDate, value))
                ValidateNextButtonAllowed();
        }
    }

    private TimeSpan fromTime = new TimeSpan(13, 0, 0);
    public TimeSpan FromTime
    {
        get => fromTime;
        set
        {
            if (SetProperty(ref fromTime, value))
                ValidateNextButtonAllowed();
        }
    }

    private TimeSpan toTime = new TimeSpan(16, 0, 0);
    public TimeSpan ToTime
    {
        get => toTime;
        set
        {
            if (SetProperty(ref toTime, value))
                ValidateNextButtonAllowed();
        }
    }

    private bool shouldBy;
    public bool ShouldBy
    {
        get => shouldBy;
        set => SetProperty(ref shouldBy, value);
    }

    private int passengersCount = 1;
    public int PassengersCount
    {
        get => passengersCount;
        set => SetProperty(ref passengersCount, value);
    }

    public bool CanOrder => searchProvider.CanOrder;

    public BusParametersViewmodel(NavigationViewModel navigationViewModel, ISearchServiceProvider searchProvider)
    {
        this.navigationViewModel = navigationViewModel;
        this.searchProvider = searchProvider;
        shouldBy = CanOrder;
        BackCommand = new DelegateCommand(
            () => navigationViewModel.Show(new TransportSelectionViewModel(navigationViewModel)));
        NextCommand = new DelegateCommand(NextHandler, GetNextEnabled);
        Today = new DelegateCommand(() => Date = DateTime.Now.Date);
        Tomorrow = new DelegateCommand(() => Date = DateTime.Now.Date.AddDays(1));
    }

    private void NextHandler()
    {
        if (!date.HasValue || fromStation == null || toStation == null)
            return;

        var searchParameters = new BusSearchParameters(fromStation, toStation, date.Value.Date, fromTime, toTime, passengersCount);
        ISearchService searchService = searchProvider.CreateBusSearchService(in searchParameters);

        if (ShouldBy)
        {
            var orderSearchService = (IOrderSearchService)searchService;
            if (CredentialHelper.TryLoad(out Credentials credentials))
            {
                
                orderSearchService.SetCredentials(credentials);
            }
            else
            {
                navigationViewModel.Show(new BusCredentialsViewModel(navigationViewModel, CredentialHelper, orderSearchService));
                return;
            }
        }

        var searchViewModel = new SearchViewModel(navigationViewModel, searchService);
        navigationViewModel.Show(searchViewModel);
        searchViewModel.RunSearch();
    }

    private void ValidateNextButtonAllowed() => NextCommand.RaiseCanExecuteChanged();

    private bool GetNextEnabled()
        => fromStation != null && toStation != null && fromStation != toStation
           && date.HasValue
           && toTime - fromTime > new TimeSpan(0, 22, 0);
}