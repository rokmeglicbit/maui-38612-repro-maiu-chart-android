using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Repro38612.ViewModels;

namespace Repro38612.Models;

// Podatkovni model (ni View), podoben SmartHouseUIControl
public partial class DemoControl : ObservableObject
{
    public string Name { get; set; } = "";
    public bool IsFullWidth { get; set; }
    public bool IsCamera { get; set; }
    public bool IsChart { get; set; }

    [ObservableProperty]
    private string reading = "";

    // Graf kot dinamicna predloga: podatki + os so na kontroli, SfCartesianChart jih veze prek TemplateHost
    private ObservableCollection<ChartPoint> chartData = [];
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

    // MORA teci na glavni niti. Vrstni red: nova kolekcija, Minimum, Maximum, Interval
    public void ApplyChart(List<ChartPoint> data)
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
    }
}

public class DemoGroup : List<DemoControl>
{
    public string Name { get; set; } = "";
    public bool IsGroupHeaderVisible { get; set; } = true;
}

public partial class DeviceRow : ObservableObject
{
    public bool IsHeader { get; set; }
    public string HeaderText { get; set; } = "";

    [ObservableProperty]
    private ObservableCollection<DemoControl>? left;

    [ObservableProperty]
    private ObservableCollection<DemoControl>? right;

    private ObservableCollection<DemoControl>? rightBackup;

    public bool IsFullWidth => Left is { Count: 1 } && Left[0].IsFullWidth;

    // Right preklaplja med null in vrednostjo (Right == null pomeni prazno vsebino)
    public void FlipRight()
    {
        if (Right is not null)
        {
            rightBackup = Right;
            Right = null;
        }
        else if (rightBackup is not null)
        {
            Right = rightBackup;
        }
    }
}

public static class DemoData
{
    // Vsak klic ustvari NOVE objekte, kot pri pravi aplikaciji
    public static List<DemoGroup> Create()
    {
        List<DemoGroup> groups = [];
        for (int g = 0; g < 6; g++)
        {
            DemoGroup group = new() { Name = $"Skupina {g + 1}" };
            for (int i = 0; i < 7; i++)
            {
                group.Add(new DemoControl
                {
                    Name = $"Naprava {g + 1}.{i + 1}",
                    Reading = DateTime.Now.ToString("HH:mm:ss.fff"),
                    IsFullWidth = i % 5 == 4,
                    IsCamera = i % 5 == 4,
                    IsChart = i % 5 == 1,
                    ChartData = i % 5 == 1 ? new ObservableCollection<ChartPoint>(ChartViewModel.BuildData()) : []
                });
            }
            groups.Add(group);
        }
        return groups;
    }
}
