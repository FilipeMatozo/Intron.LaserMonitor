using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Intron.LaserMonitor.ViewModels;

public partial class ShellViewModel : ObservableRecipient // TODO: Tornar a classe IDisposable e desinscrever os eventos no dispose.
{
    #region SerialProps
    private readonly ISerialService _serialService;
    [NotifyPropertyChangedFor(nameof(DeviceConnectionText))]
    [NotifyPropertyChangedFor(nameof(DeviceConnectionImage))]
    [NotifyPropertyChangedFor(nameof(ConnectButtonText))]
    [NotifyPropertyChangedFor(nameof(EllipseConnectionColor))]
    [NotifyPropertyChangedFor(nameof(IsSerialDisconnected))]
    [ObservableProperty]
    private bool isSerialConnected = false;

    public bool IsSerialDisconnected
    {
        get => !IsSerialConnected;
    }

    [ObservableProperty] private ObservableCollection<string> _availablePorts = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConnectDisconnectCommand))]
    private string _selectedPort = string.Empty;
    public string ConnectButtonText
    {
        get => IsSerialConnected ? "Desconectar" : "Conectar";
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
    #endregion

    #region OtherProps
    [ObservableProperty] private Visibility logoCloseMenu = Visibility.Visible;
    [ObservableProperty] private Visibility logoOpenMenu = Visibility.Collapsed;
    [ObservableProperty] private ObservableCollection<MenuItemModel> menuItemModelList = new();
    [ObservableProperty] private bool isMenuItemListOpen;
    [ObservableProperty] Thickness deviceConnectionMargin;
    [ObservableProperty] private object currentUserControl;
    #endregion
    public ShellViewModel(ISerialService serialService)
    {
        _serialService = serialService;


        LoadComboboxes();
        SubscribeEvents();
    }
    #region Eventos
    private void UnsubscribeEvents()
    {
        _serialService.Connected -= OnConnected;
        _serialService.Disconnected -= OnDisconnected;
    }

    private void SubscribeEvents()
    {
        _serialService.Connected += OnConnected;
        _serialService.Disconnected += OnDisconnected;
    }

    private void OnDisconnected(object? sender, EventArgs e) => IsSerialConnected = false;

    private void OnConnected(object? sender, EventArgs e) => IsSerialConnected = true;
    #endregion

    #region SerialMethods
    private void LoadComboboxes()
    {
        RefreshPorts();
        SelectedPort = AvailablePorts.FirstOrDefault()!;
    }

    [RelayCommand]
    private void RefreshPorts()
    {
        AvailablePorts.Clear();
        foreach (var port in _serialService.GetAvailableSerialPorts())
        {
            AvailablePorts.Add(port);
        }

        SelectedPort = string.IsNullOrWhiteSpace(SelectedPort)
            ? AvailablePorts.FirstOrDefault()!
            : SelectedPort;
    }
    [RelayCommand(CanExecute = nameof(CanConnectDisconnect))]
    private void ConnectDisconnect()
    {
        if (!IsSerialConnected)
        {
            if (string.IsNullOrEmpty(SelectedPort))
                return;
            if (!_serialService.Connect(SelectedPort))
            {
               
            }
        }
        else
        {
            _serialService.Dispose();
        }
    }
    private bool CanConnectDisconnect()
    {
        if (!IsSerialConnected)
        {
            return !string.IsNullOrWhiteSpace(SelectedPort);
        }
        return true;
    }
    #endregion
    [RelayCommand]
    private void OpenMainWindow()
    {
        //CurrentUserControl = new();
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

    private void UpdateMenuItemModelList()
    {
        MenuItemModelList.Add(new MenuItemModel("Real Time Monitoring", "/Assets/Images/icons8-grafico-24.png", "/Assets/Images/icons8-graph-gif-white.gif", (RelayCommand)MonitoringViewCommand));

    }

    [RelayCommand]
    private void MonitoringView()
    {
        // Abrir MonitoringVIew UC
        if (CurrentUserControl is not MonitoringViewModel)
            CurrentUserControl = App.GetService<MonitoringViewModel>();
    }
}
