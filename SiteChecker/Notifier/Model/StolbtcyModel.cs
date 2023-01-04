using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BusStolbtcy;
using CredentialHelper;
using Notifier.PageViewModels;
using Notifier.UtilTypes;
using Station = BusStolbtcy.Station;

namespace Notifier.Model;

class StolbtcyModel : BaseBusModel<BusInfo>
{
    public override string ServiceDescription => "Stolbtcy-Minsk bus";

    public override bool CanOrder { get; } = true;

    public override bool TryFind(BusSearchParameters searchParameters, Credentials? credentialsForOrder, ref string goodResultMessage)
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

        var requestParameters = new SearchParameters(from, to, searchParameters.Date);

        if (!BusApi.GetSchedule(in requestParameters, out ReadOnlyCollection<BusInfo> schedule))
            throw new NotImplementedException();

			
        List<BusInfo> filteredBuses = FilteredBuses(schedule, searchParameters);
        if (filteredBuses.Count == 0)
            return false;

        if (!credentialsForOrder.HasValue)
        {
            goodResultMessage = $"Was found at: {DateTime.Now.ToLongTimeString()}";
            return true;
        }

        Credentials credential = credentialsForOrder.Value;
        BusInfo targetBus = filteredBuses[filteredBuses.Count / 2];
        BusApi.Order(targetBus, from, credential.Login, credential.Password, searchParameters.PassengersCount);
        goodResultMessage = $"Was ordered for {targetBus.Time.ToShortString()} at: {DateTime.Now.ToLongTimeString()}";
        return true;
    }

    public override Uri GetSiteUri(BusSearchParameters searchParameters) => BusApi.GetSiteUri();

    protected override (TimeSpan time, int ticketsCount) GetInfo(BusInfo bus) => (bus.Time, bus.FreePlaces);
}