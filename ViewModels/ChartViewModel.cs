using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Repro38612.ViewModels;

public record ChartPoint(double X, double Y);

// Podatki za SfCartesianChart: {Binding Chart.ChartData} + os Minimum / Maximum / Interval
// (kot v pravi aplikaciji: nova kolekcija ob vsakem "onConnected", os se izracuna iz podatkov)
public class ChartViewModel : ObservableObject
{
    private ObservableCollection<ChartPoint> chartData = new();
    private double temperatureMinimum = 0;
    private double temperatureMaximum = 30;
    private double temperatureInterval = 5;

    public ObservableCollection<ChartPoint> ChartData
    {
        get => chartData;
        set => SetProperty(ref chartData, value);
    }

    public double TemperatureMinimum
    {
        get => temperatureMinimum;
        set => SetProperty(ref temperatureMinimum, value);
    }

    public double TemperatureMaximum
    {
        get => temperatureMaximum;
        set => SetProperty(ref temperatureMaximum, value);
    }

    public double TemperatureInterval
    {
        get => temperatureInterval;
        set => SetProperty(ref temperatureInterval, value);
    }

    // Sprozi se na glavni niti na koncu Apply: MainPage zamenja graf z novim in odklopi handler starega
    public event Action? RecreateRequested;

    // Lahko se klice z katere koli niti ("pridobitev" podatkov)
    public static List<ChartPoint> BuildData() =>
        Enumerable.Range(0, 200).Select(i => new ChartPoint(i, Random.Shared.NextDouble() * 30)).ToList();

    // MORA teci na glavni niti. Vrstni red nastavitev: Minimum, Maximum, Interval
    public void Apply(List<ChartPoint> data)
    {
        ChaosOptions o = ChaosOptions.Current;
        double min = Math.Floor(data.Min(p => p.Y) / 5) * 5;
        double max = Math.Ceiling(data.Max(p => p.Y) / 5) * 5;

        ChartData = new ObservableCollection<ChartPoint>(data);

        if (o.ChartAxisMinMax)
        {
            TemperatureMinimum = min;
            TemperatureMaximum = max;
        }

        if (o.ChartAxisInterval)
        {
            TemperatureInterval = 5;
        }

        // Stari graf ima zdaj cakajoc Syncfusion CoreScheduler klic (UpdateArea), njegov native view pa gre v dispose
        if (o.ChartRecreate)
        {
            RecreateRequested?.Invoke();
        }
    }
}
