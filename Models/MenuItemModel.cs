using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace Intron.LaserMonitor.Models;

public class MenuItemModel : ObservableObject
{
    #region Fields
    private string title = "";
    private bool isVisible = false;
    private int buttonWidth;
    private string imagePath = "";
    private string selectedImagePath = "";
    private Visibility textVisibility = new();
    private RelayCommand actionCommand = null!;
    #endregion

    #region Constructors
    public MenuItemModel(string title, string imagePath, string selectedImagePath, RelayCommand actionCommand)
    {
        Title = title;
        ImagePath = imagePath;
        SelectedImagePath = selectedImagePath;
        ActionCommand = actionCommand;
        IsVisible = false;
        ButtonWidth = 60;
        TextVisibility = Visibility.Collapsed;
    }
    #endregion

    #region Properties
    public string Title { get => title; set { SetProperty(ref title, value); } }
    public string ImagePath { get => imagePath; set { SetProperty(ref imagePath, value); } }
    public string SelectedImagePath { get => selectedImagePath; set { SetProperty(ref selectedImagePath, value); } }
    public bool IsVisible { get => isVisible; set { SetProperty(ref isVisible, value); } }
    public RelayCommand ActionCommand { get => actionCommand; set { SetProperty(ref actionCommand, value); } }
    public Visibility TextVisibility { get => textVisibility; set { SetProperty(ref textVisibility, value); } }
    public int ButtonWidth { get => buttonWidth; set { SetProperty(ref buttonWidth, value); } }
    #endregion

    #region Methods
    public void SwitchOpenCloseMenuItem(bool isOpen)
    {
        if (isOpen)
        {
            OpenMenuItem();
        }
        else
        {
            CloseMenuItem();
        }
    }

    private void OpenMenuItem()
    {
        ButtonWidth = 188;
        TextVisibility = Visibility.Visible;
    }

    private void CloseMenuItem()
    {
        ButtonWidth = 60;
        TextVisibility = Visibility.Collapsed;
    }
    #endregion
}
