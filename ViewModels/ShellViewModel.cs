using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Intron.LaserMonitor.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    private readonly ISerialService _serialService;

    [ObservableProperty] private Visibility logoCloseMenu = Visibility.Visible;
    [ObservableProperty] private Visibility logoOpenMenu = Visibility.Collapsed;
    [ObservableProperty] private ObservableCollection<MenuItemModel> menuItemModelList = new();
    [ObservableProperty] private bool isMenuItemListOpen;
    [ObservableProperty] Thickness deviceConnectionMargin;
    [ObservableProperty] private object currentUserControl;
    [ObservableProperty] bool isMonitoring = false;
    public string StartStopText
    {
        get => IsMonitoring ? "Stop" : "Start";
    }
    public string ConnectButtonText
    {
        get => IsSerialConnected ? "Disconnect" : "Connect";
    }

    public SolidColorBrush EllipseConnectionColor
    {
        get => IsSerialConnected
            ? new(Colors.Green)
            : new(Colors.Red);
    }
    public string DeviceConnectionImage
    {
        get => IsSerialConnected 
            ? "/Assets/Images/icons8-conectado-50.png" 
            : " /Assets/Images/icons8-desconectado-50.png";
    }
    public string DeviceConnectionText
    {
        get => IsSerialConnected ? "Dispositivo Conectado" : "Dispositivo Desconectado";
    }

    [NotifyPropertyChangedFor(nameof(DeviceConnectionText))]
    [NotifyPropertyChangedFor(nameof(DeviceConnectionImage))]
    [NotifyPropertyChangedFor(nameof(ConnectButtonText))]
    [NotifyPropertyChangedFor(nameof(EllipseConnectionColor))]
    [ObservableProperty] private bool isSerialConnected = false;

    public ShellViewModel(ISerialService serialService)
    {
        _serialService = serialService;
    }

    private void OpenMainWindowCommand()
    {

    }

    [RelayCommand]
    private void Loaded()
    {
        UpdateMenuItemModelList();
        CurrentUserControl = App.GetService<MonitoringViewModel>();
    }
    [RelayCommand]
    private void OpenMenu()
    {
        IsMenuItemListOpen = !IsMenuItemListOpen;

        foreach (var m in MenuItemModelList)
        {
            m.SwitchOpenCloseMenuItem(IsMenuItemListOpen);
        }

        if (IsMenuItemListOpen)
        {
            DeviceConnectionMargin = new Thickness(110, 0, 0, 0);
            LogoCloseMenu = Visibility.Collapsed;
            LogoOpenMenu = Visibility.Visible;
        }
        else
        {
            DeviceConnectionMargin = new Thickness(6, 0, 0, 0);
            LogoCloseMenu = Visibility.Visible;
            LogoOpenMenu = Visibility.Collapsed;
        }
    }
    [RelayCommand]
    private void Connect()
    {

    }
    [RelayCommand]
    private void StartStop()
    {

    }
    [RelayCommand]
    private void SetZero()
    {

    }
    private void UpdateMenuItemModelList()
    {
        MenuItemModelList.Add(new MenuItemModel("Real Time Monitoring", "/Assets/Images/icons8-grafico-24.png", "/Assets/Images/icons8-grafico-24-white.png", (RelayCommand)MonitoringViewCommand));

    }

    [RelayCommand]
    private void MonitoringView()
    {
        // Abrir MonitoringVIew UC
        if (CurrentUserControl is not MonitoringViewModel)
            CurrentUserControl = App.GetService<MonitoringViewModel>();
    }
}
