using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Repro38612.Models;

namespace Repro38612.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public DevicesViewModel Devices { get; } = new();
    public ChartViewModel Chart { get; } = new();

    [ObservableProperty] private bool isListVisible = true;
    [ObservableProperty] private bool isRefreshing;
    [ObservableProperty] private string chaosText = "Chaos START";
    [ObservableProperty] private bool connectedToMqtt;

    public MainViewModel() => Devices.BuildRows(DemoData.Create());

    [RelayCommand]
    private void Toggle() => IsListVisible = !IsListVisible;

    [RelayCommand]
    private void Rebuild() => Devices.BuildRows(DemoData.Create());

    [RelayCommand]
    private async Task Refresh()
    {
        await Task.Delay(300);
        Devices.BuildRows(DemoData.Create());
        IsRefreshing = false;
    }

    // Posnema MQTT sporočila: veliko majhnih posodobitev vrednosti na UI niti
    public void TouchRandomControls()
    {
        var rows = Devices.Rows;
        if (rows.Count == 0) return;

        for (int i = 0; i < 20; i++)
        {
            DeviceRow row = rows[Random.Shared.Next(rows.Count)];
            if (row.Left is { Count: > 0 }) row.Left[0].Reading = Random.Shared.Next().ToString();
            if (row.Right is { Count: > 0 }) row.Right[0].Reading = Random.Shared.Next().ToString();
        }

        UpdateCellChartsThrottled();
    }

    private long lastCellChartTick;

    // Graf v celici dobi novo kolekcijo + os, najvec enkrat na 50 ms (MQTT poplava ne sme graditi tisocev tock)
    private void UpdateCellChartsThrottled()
    {
        if (!ChaosOptions.Current.ChartUpdates) return;

        long now = Environment.TickCount64;
        if (now - lastCellChartTick < 50) return;
        lastCellChartTick = now;

        var rows = Devices.Rows;
        for (int i = 0; i < 4 && rows.Count > 0; i++)
        {
            DeviceRow row = rows[Random.Shared.Next(rows.Count)];
            if (row.Left is { Count: > 0 } && row.Left[0].IsChart) row.Left[0].ApplyChart(ChartViewModel.BuildData());
            if (row.Right is { Count: > 0 } && row.Right[0].IsChart) row.Right[0].ApplyChart(ChartViewModel.BuildData());
        }
    }
}
