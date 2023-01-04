using System;

namespace Notifier.Model;

class Station
{
    public static Station Stowbtcy { get; } = new Station("Столбцы");

    public static Station Minsk { get; } = new Station("Минск");

    public static Station[] Stations { get; } = { Stowbtcy, Minsk };

    public readonly string Name;

    private Station(string name) => Name = name;

    public override string ToString() => Name;

    public static Station GetOpposite(Station station)
    {
        if (station == Stowbtcy)
            return Minsk;
        if (station == Minsk)
            return Stowbtcy;

        throw new InvalidOperationException();
    }
}