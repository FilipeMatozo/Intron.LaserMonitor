using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intron.LaserMonitor.Models;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace Intron.LaserMonitor.ViewModels;

public partial class ShellViewModel : ObservableRecipient
{
    [ObservableProperty] private Visibility logoCloseMenu = Visibility.Visible;
    [ObservableProperty] private Visibility logoOpenMenu = Visibility.Collapsed;
    [ObservableProperty] private ObservableCollection<MenuItemModel> menuItemModelList = new();
    [ObservableProperty] private bool isMenuItemListOpen;
    [ObservableProperty] Thickness deviceConnectionMargin;
    [ObservableProperty] private object currentUserControl;
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

    private void UpdateMenuItemModelList()
    {
        MenuItemModelList.Add(new MenuItemModel("MonitoringView", "/Assets/Images/icons8-grafico-24.png", "/Assets/Images/icons8-grafico-24-white.png", (RelayCommand)MonitoringViewCommand));

    }

    [RelayCommand]
    private void MonitoringView()
    {
        // Abrir MonitoringVIew UC
    }
}
