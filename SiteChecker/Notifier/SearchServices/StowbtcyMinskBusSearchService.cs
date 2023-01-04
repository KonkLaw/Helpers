using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BusStowbtcy;
using CredentialHelper;
using Notifier.Model;
using Notifier.UtilTypes;
using Station = Notifier.Model.Station;

namespace Notifier.SearchServices;

class StowbtcyMinskBusSearchService : BaseSearchService, IOrderSearchService
{
    private const string ServiceName = "Stowbtcy-Minsk Bus";
    private readonly BusSearchParameters parameters;
    private readonly SearchParameters apiParameters;
    private Credentials? credentialsForOrder;

    public StowbtcyMinskBusSearchService(in BusSearchParameters parameters)
    {
        this.parameters = parameters;
        apiParameters = Convert(parameters);
    }

    private static SearchParameters Convert(BusSearchParameters searchParameters)
    {
        BusStowbtcy.Station from;
        BusStowbtcy.Station to;
        if (searchParameters.FromStation == Station.Minsk)
        {
            from = BusApi.MinskStation;
            to = BusApi.StolbtcyStation;
        }
        else
        {
            from = BusApi.StolbtcyStation;
            to = BusApi.MinskStation;
        }
        return new SearchParameters(from, to, searchParameters.Date);
    }

    public override string GetSearchDescription() => parameters.GetDescription(ServiceName);

    public override Uri GetFastNavigationLink() => BusApi.GetSiteUri();

    public override bool MakeRequest(out string goodResultMessage)
    {
        if (!BusApi.GetSchedule(in apiParameters, out ReadOnlyCollection<BusInfo> schedule))
            throw new Exception("Error on schedule getting");

        IReadOnlyList<BusInfo> filteredBuses =  FilteredBuses(
            schedule, info => (info.Time, info.FreePlaces), parameters);
        if (filteredBuses.Count == 0)
        {
            goodResultMessage = string.Empty;
            return false;
        }

        if (!credentialsForOrder.HasValue)
        {
            goodResultMessage = $"Was found at: {DateTime.Now.ToLongTimeString()}";
            return true;
        }

        Credentials credential = credentialsForOrder.Value;
        BusInfo targetBus = filteredBuses[filteredBuses.Count / 2];
        BusApi.Order(targetBus, apiParameters.FromStation,
            credential.Login, credential.Password, parameters.PassengersCount);

        goodResultMessage = $"Was ordered for {targetBus.Time.ToShortString()} at: {DateTime.Now.ToLongTimeString()}";
        return true;
    }

    public void SetCredentials(Credentials credentials) => credentialsForOrder = credentials;
}

class StowbtcyMinskProvider : ISearchServiceProvider
{
    public bool CanOrder => true;
    public ISearchService CreateBusSearchService(in BusSearchParameters searchParameters) => new StowbtcyMinskBusSearchService(in searchParameters);
}