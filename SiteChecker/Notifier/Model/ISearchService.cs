using System;
using System.Collections.Generic;
using System.Linq;
using CredentialHelper;

namespace Notifier.Model;

interface ISearchService
{
    string GetSearchDescription();
    Uri GetFastNavigationLink();
    bool MakeRequest(out string goodResultMessage);
}

interface IOrderSearchService : ISearchService
{
    void SetCredentials(Credentials credentials);
}

interface ISearchServiceProvider
{
    bool CanOrder { get; }
    ISearchService CreateBusSearchService(in BusSearchParameters searchParameters);
}

abstract class BaseSearchService : ISearchService
{
    protected static IReadOnlyList<TBus> FilteredBuses<TBus>(
        IEnumerable<TBus> buses,
        Func<TBus, (TimeSpan time, int ticketsCount)> getInfo,
        in BusSearchParameters searchParameters)
    {
        BusSearchParameters parameters = searchParameters;
        return buses.Where(bus =>
        {
            (TimeSpan time, int ticketsCount) = getInfo(bus);
            return time >= parameters.FromTime
                   && time <= parameters.ToTime
                   && ticketsCount >= parameters.PassengersCount;
        }).ToList();
    }

    public abstract string GetSearchDescription();
    public abstract Uri GetFastNavigationLink();
    public abstract bool MakeRequest(out string goodResultMessage);
}