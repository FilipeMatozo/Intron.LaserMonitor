using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.Models;
using Intron.LaserMonitor.Services;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Intron.LaserMonitor.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ISerialService _serialService;
        private readonly IExcelExportService _excelService;
        private readonly List<Measurement> _allMeasurements;

        [ObservableProperty]
        private ObservableCollection<string> _availablePorts;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
        private string _selectedPort;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConnectCommand))]
        [NotifyCanExecuteChangedFor(nameof(DisconnectCommand))]
        [NotifyCanExecuteChangedFor(nameof(StartMeasurementCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopMeasurementCommand))]
        private bool _isConnected = false;

        [ObservableProperty]
        private string _currentDistance = "N/A";

        public PlotModel PlotModel { get; private set; }
        public ObservableCollection<DataPoint> PlotPoints { get; private set; }
        
        public MainViewModel(ISerialService serialService, IExcelExportService excelExportService)
        {
            _serialService = serialService;
            _excelService = excelExportService;

            SetupPlotModel();

            AvailablePorts = new ObservableCollection<string>(_serialService.GetAvailableSerialPorts());

            _serialService.DataReceived += OnDataReceived;
            _serialService.Connected += OnConnected;
            _serialService.Disconnected += OnDisconnected;
        }


        [RelayCommand(CanExecute = nameof(CanConnect))]
        private void Connect()
        {
            if (!_serialService.Connect(SelectedPort))
            {
                MessageBox.Show("Falha ao conectar à porta serial.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool CanConnect() => !IsConnected && !string.IsNullOrEmpty(SelectedPort);


        [RelayCommand(CanExecute = nameof(CanDisconnect))]
        private void Disconnect()
        {
            _serialService.Disconnect();
        }
        private bool CanDisconnect() => IsConnected;


        [RelayCommand(CanExecute = nameof(CanStartMeasurement))]
        private async void StartMeasurement()
        {
            _allMeasurements.Clear();
            PlotPoints.Clear();
            PlotModel.InvalidatePlot(true);
            CurrentDistance = "Iniciando...";

            await _serialService.StartMeasurement();
        }
        private bool CanStartMeasurement() => IsConnected;


        [RelayCommand(CanExecute = nameof(CanStopMeasurement))]
        private async void StopMeasurement()
        {
            await _serialService.StopMeasurement();
            CurrentDistance = "Medição parada.";
        }
        private bool CanStopMeasurement() => IsConnected;

        [RelayCommand]
        private void RefreshPorts()
        {
            AvailablePorts.Clear();
            var ports = _serialService.GetAvailableSerialPorts();
            if (ports != null)
            {
                foreach (var port in ports)
                {
                    AvailablePorts.Add(port);
                }
            }
        }

        private void OnConnected(object sender, EventArgs e) => IsConnected = true;

        private void OnDisconnected(object sender, EventArgs e) => IsConnected = false;

        [RelayCommand(CanExecute = nameof(CanExport))]
        private void ExportToExcel()
        {
            var sfd = new SaveFileDialog
            {
                Filter = "Arquivo Excel (*.xlsx)|*.xlsx",
                Title = "Salvar medições",
                FileName = $"Medicoes_Laser_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    _excelService.Export(_allMeasurements, sfd.FileName);
                    MessageBox.Show($"Dados exportados com sucesso para:\n{sfd.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ocorreu um erro ao exportar:\n{ex.Message}", "Erro de Exportação", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private bool CanExport() => _allMeasurements.Any();

        private void SetupPlotModel()
        {
            PlotModel = new PlotModel { Title = "Distância do Laser vs. Tempo" };
            
            PlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Tempo",
                StringFormat = "HH:mm:ss",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Distância (m)",
                Minimum = 0,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            var lineSeries = new LineSeries
            {
                Title = "Distância",
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
                ItemsSource = PlotPoints,
                TrackerFormatString = "Tempo: {2:HH:mm:ss}\nDistância: {4:0.000} m",
                CanTrackerInterpolatePoints = false
            };

            PlotModel.Series.Add(lineSeries);
        }

        private void OnDataReceived(object sender, Models.Events.DataReceivedEventArgs dataReceivedEventArgs)
        {
            var data = dataReceivedEventArgs.Data;
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (data.StartsWith("D=") && data.EndsWith("m"))
                {
                    string distanceStr = data.Replace("D=", "").Replace("m", "").Trim();

                    if (double.TryParse(distanceStr, out var distance))
                    {
                        distance = distance / 1000;
                        var measurement = new Measurement
                        {
                            Timestamp = DateTime.Now,
                            Distance = distance
                        };
                        _allMeasurements.Add(measurement);

                        PlotPoints.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.Timestamp), distance));
                        PlotModel.InvalidatePlot(true);

                        CurrentDistance = $"{distance:F3}";
                        ExportToExcelCommand.NotifyCanExecuteChanged();
                    }
                }
                else if (data.StartsWith("E="))
                {
                    var measurement = new Measurement
                    {
                        Timestamp = DateTime.Now,
                        Distance = 0
                    };
                    _allMeasurements.Add(measurement);
                    PlotPoints.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.Timestamp), measurement.Distance));
                    PlotModel.InvalidatePlot(true);
                    CurrentDistance = "N/A";
                }
            });
        }
    }
}