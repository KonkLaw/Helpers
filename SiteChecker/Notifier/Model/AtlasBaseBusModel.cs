
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BusAtlas;
using CredentialHelper;
using Notifier.PageViewModels;
using Station = BusAtlas.Station;

namespace Notifier.Model;

class AtlasBaseBusModel : BaseBusModel<BusInfo>
{
    public override string ServiceDescription => "Atlas Bus";

    public override bool CanOrder => false;

    public override bool TryFind(BusSearchParameters searchParameters, Credentials? credentialsForOrder, ref string goodResultMessage)
    {
        SearchParameters requestParameters = ConvertParameters(searchParameters);
        if (!BusApi.GetSchedule(in requestParameters, out ReadOnlyCollection<BusInfo> schedule))
            throw new NotImplementedException();

        List<BusInfo> filteredBuses = FilteredBuses(schedule, searchParameters);
        if (filteredBuses.Count == 0)
            return false;

        goodResultMessage = $"Was found at: {DateTime.Now.ToLongTimeString()}";
        return true;
    }

    private static SearchParameters ConvertParameters(BusSearchParameters searchParameters)
    {
        Station from;
        Station to;
        if (searchParameters.FromStation == PageViewModels.Station.Minsk)
        {
            from = BusApi.MinskStation;
            to = BusApi.StolbtcyStation;
        }
        else
        {
            from = BusApi.StolbtcyStation;
            to = BusApi.MinskStation;
        }

        var requestParameters = new SearchParameters(@from, to, searchParameters.Date, searchParameters.PassengersCount);
        return requestParameters;
    }

    public override Uri GetSiteUri(BusSearchParameters searchParameters)
    {
        SearchParameters requestParameters = ConvertParameters(searchParameters);
        return BusApi.GetUrl(requestParameters);
    }

    protected override (TimeSpan time, int ticketsCount) GetInfo(BusInfo bus) => (bus.Time, bus.TicketsCount);
}