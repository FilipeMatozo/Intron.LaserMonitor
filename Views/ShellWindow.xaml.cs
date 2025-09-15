using Intron.LaserMonitor.ViewModels;
using System.Windows;

namespace Intron.LaserMonitor.Views
{
    /// <summary>
    /// Lógica interna para ShellWindow.xaml
    /// </summary>
    public partial class ShellWindow : Window
    {
        public ShellViewModel ViewModel
        {
            get;
        }
        public ShellWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<ShellViewModel>();
            DataContext = ViewModel;
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ViewModel.RefreshPortsCommand.Execute(this);
        }
    }
}
