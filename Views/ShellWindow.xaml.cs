using Intron.LaserMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
