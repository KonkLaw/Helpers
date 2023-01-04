using System;
using System.Collections.ObjectModel;
using System.Text;
using Notifier.Model;
using Notifier.UtilTypes;
using RwByApi;

namespace Notifier.SearchServices;

public class TrainSearchService : ISearchService
{
    private readonly TrainParameters parameters;
    private readonly ReadOnlyCollection<TrainInfo> selectedTrains;

    public TrainSearchService(in TrainParameters trainParameters, ReadOnlyCollection<TrainInfo> selectedTrains)
    {
        parameters = trainParameters;
        this.selectedTrains = selectedTrains;
    }

    public string GetSearchDescription()
    {
        StringBuilder result = new StringBuilder(parameters.GetDescription());
        result.AppendLine();
        result.Append("Trains {");
        foreach (TrainInfo train in selectedTrains)
        {
            result.Append(train.TrainTime.ToShortString());
            result.Append(" (");
            result.Append(train.TrainId);
            result.Append("), ");
        }
        result.Remove(result.Length - 2, 2);
        result.Append("}");
        return result.ToString();
    }

    public Uri GetFastNavigationLink() => TrainsInfoApi.GetRequestUri(in parameters);

    public bool MakeRequest(out string goodResultMessage)
    {
        goodResultMessage = string.Empty;
        foreach (TrainInfo train in selectedTrains)
        {
            if (TrainsInfoApi.HasTickets(train.TrainId, in parameters))
            {
                goodResultMessage = "Was found at: " + DateTime.Now.ToLongTimeString();
                return true;
            }
        }
        return false;
    }
}