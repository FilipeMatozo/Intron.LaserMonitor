using System.Configuration;
using System.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.Services;
using Intron.LaserMonitor.ViewModels;

namespace Intron.LaserMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IHost Host
        {
            get;
        }
        public static T GetService<T>() where T : class
        {
            if ((App.Current as App)!.Host.Services.GetService(typeof(T)) is not T service)
            {
                throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
            }

            return service;
        }
        private Window MainWindow { get; }

        public App()
        {
            Host = Microsoft.Extensions.Hosting.Host.
            CreateDefaultBuilder().
            UseContentRoot(AppContext.BaseDirectory).
            ConfigureServices((context, services) =>
            {
                // Configure services
                services.AddSingleton<ISerialService, SerialService>();
                services.AddSingleton<IExcelExportService, ExcelExportService>();

                // Configure views and viewModels
                services.AddTransient<MainViewModel>();
                services.AddTransient<MainWindow>();
            })
            .Build();

            MainWindow = GetService<MainWindow>();
            MainWindow.Show();
            MainWindow.Activate();
        }
    }

}
