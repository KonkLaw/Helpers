using System;
using System.Globalization;

namespace Notifier.Model;

readonly struct BusSearchParameters
{
    public readonly Station FromStation;
    public readonly Station ToStation;
    public readonly DateTime Date;
    public readonly TimeSpan FromTime;
    public readonly TimeSpan ToTime;
    public readonly int PassengersCount;

    public BusSearchParameters(
        Station fromStation, Station toStation, DateTime date, TimeSpan fromTime, TimeSpan toTime,
        int passengersCount)
    {
        FromStation = fromStation;
        ToStation = toStation;
        Date = date;
        FromTime = fromTime;
        ToTime = toTime;
        PassengersCount = passengersCount;
    }

    public string GetDescription(string serviceDescription)
        => $"Service:    {serviceDescription}" +
           $"{Environment.NewLine}" +
           $"Direction:  {FromStation}-{ToStation}" +
           $"{Environment.NewLine}" +
           $"Date:       {Date.ToString("yyyy-MM-dd dddd", CultureInfo.GetCultureInfo("ru-ru"))}" +
           $"{Environment.NewLine}" +
           $"Time:       {FromTime:hh\\:mm}-{ToTime:hh\\:mm}" + 
           $"{Environment.NewLine}" +
           $"Passengers  {PassengersCount}";
}