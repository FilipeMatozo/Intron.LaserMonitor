using Intron.LaserMonitor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Intron.LaserMonitor.Views
{
    /// <summary>
    /// Interação lógica para MonitoringView.xam
    /// </summary>
    public partial class MonitoringView : UserControl
    {
        private MonitoringViewModel ViewModel
        {
            get;
        }
        public MonitoringView()
        {
            InitializeComponent();
            ViewModel = App.GetService<MonitoringViewModel>();
            DataContext = ViewModel;

            this.Unloaded += MonitoringView_Unloaded;
        }

        private void MonitoringView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel is IDisposable disposable)
                disposable.Dispose();
        }
    }
}
