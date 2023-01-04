using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BusAtlas;
using Notifier.Model;
using Station = Notifier.Model.Station;

namespace Notifier.SearchServices;

class AtlasBusSearchService : BaseSearchService
{
    private const string ServiceName = "Atlas Bus";
    private readonly BusSearchParameters searchFilter;
    private readonly SearchParameters searchParameters;

    public AtlasBusSearchService(in BusSearchParameters searchFilter)
    {
        this.searchFilter = searchFilter;
        searchParameters = ConvertParameters(searchFilter);
    }

    private static SearchParameters ConvertParameters(BusSearchParameters searchParameters)
    {
        BusAtlas.Station from;
        BusAtlas.Station to;
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

        var requestParameters = new SearchParameters(from, to, searchParameters.Date, searchParameters.PassengersCount);
        return requestParameters;
    }

    public override string GetSearchDescription() => searchFilter.GetDescription(ServiceName);

    public override Uri GetFastNavigationLink() => BusApi.GetUrl(searchParameters);

    public override bool MakeRequest(out string goodResultMessage)
    {
        if (!BusApi.GetSchedule(in searchParameters, out ReadOnlyCollection<BusInfo> schedule))
            throw new Exception("Error on schedule getting.");

        IReadOnlyList<BusInfo> filteredBuses = FilteredBuses(
            schedule,
            info => (info.Time, info.TicketsCount),
            searchFilter);
        if (filteredBuses.Count == 0)
        {
            goodResultMessage = string.Empty;
            return false;
        }

        goodResultMessage = $"Was found at: {DateTime.Now.ToLongTimeString()}";
        return true;
    }
}

class AtlasSearchServiceProvider : ISearchServiceProvider
{
    public bool CanOrder => false;
    public ISearchService CreateBusSearchService(in BusSearchParameters searchParameters) => new AtlasBusSearchService(in searchParameters);
}