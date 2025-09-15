using Intron.LaserMonitor.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Intron.LaserMonitor
{
    public partial class MainWindow : Window
    {
        public MainViewModel ViewModel
        {
            get;
        }
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = App.GetService<MainViewModel>();
            DataContext = ViewModel;
        }

        private void ComboBox_DropDownOpened(object sender, EventArgs e)
        {
            ViewModel.RefreshPortsCommand.Execute(this);
        }
    }
}