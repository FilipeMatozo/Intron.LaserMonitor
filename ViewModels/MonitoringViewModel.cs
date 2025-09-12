using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Intron.LaserMonitor.ViewModels
{
    public partial class MonitoringViewModel : ObservableObject
    {
        private readonly ISerialService _serialService;
        private readonly IExcelExportService _excelService;
        private readonly List<Measurement> _allMeasurements = new();

        [ObservableProperty]
        private ObservableCollection<string> _availablePorts = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectBtnEnabled))]
        [NotifyCanExecuteChangedFor(nameof(ConnectDisconnectCommand))]
        private string _selectedPort = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ConnectText))]
        [NotifyPropertyChangedFor(nameof(IsConnectedText))]
        [NotifyCanExecuteChangedFor(nameof(StartMeasurementCommand))]
        [NotifyCanExecuteChangedFor(nameof(StopMeasurementCommand))]
        [NotifyCanExecuteChangedFor(nameof(ZeroOffsetCommand))]
        private bool _isConnected = false;

        [ObservableProperty]
        private string _currentDistance = "N/A";
        public string ConnectText
        {
            get => IsConnected ? "Desconectar" : "Conectar";
        }

        public string IsConnectedText
        {
            get => IsConnected ? "Conectado" : "Desconectado";
        }
        public bool ConnectBtnEnabled
        {
            get => !string.IsNullOrWhiteSpace(SelectedPort);
        }
        public PlotModel PlotModel { get; private set; }
        public List<DataPoint> PlotPoints { get; private set; } = new();

        private double _zeroOffset = 0;

        public MonitoringViewModel(ISerialService serialService, IExcelExportService excelExportService)
        {
            _serialService = serialService;
            _excelService = excelExportService;

            SetupPlotModel();
            LoadComboboxes();
            SubscribeEvents();
        }

        private void LoadComboboxes()
        {
            RefreshPorts();
            SelectedPort = AvailablePorts.FirstOrDefault()!;
        }

        private void UnsubscribeEvents()
        {
            _serialService.DataReceived -= OnDataReceived;
            _serialService.Connected -= OnConnected;
            _serialService.Disconnected -= OnDisconnected;
        }

        private void SubscribeEvents()
        {
            _serialService.DataReceived += OnDataReceived;
            _serialService.Connected += OnConnected;
            _serialService.Disconnected += OnDisconnected;
        }

        [RelayCommand(CanExecute = nameof(CanConnectDisconnect))]
        private void ConnectDisconnect()
        {
            if (!IsConnected)
            {
                if (string.IsNullOrEmpty(SelectedPort))
                    return;
                if (!_serialService.Connect(SelectedPort))
                {
                    MessageBox.Show("Falha ao conectar à porta serial.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                _serialService.Disconnect();
            }
        }
        private bool CanConnectDisconnect()
        {
            if (!IsConnected)
            {
                return !string.IsNullOrWhiteSpace(SelectedPort);
            }
            return true;
        }

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

        [RelayCommand(CanExecute = nameof(CanZeroOffset))]
        private void ZeroOffset()
        {
            _zeroOffset = _allMeasurements.Last().DistanceAbsolute;
            _allMeasurements.Clear();
            PlotPoints.Clear();
        }
        private bool CanZeroOffset() => IsConnected;

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
        private bool CanExport() => _allMeasurements.Count > 0;

        private void SetupPlotModel()
        {
            PlotModel = new PlotModel { Title = "Distância do Laser vs. Tempo" };

            PlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Tempo",
                StringFormat = "HH:mm:ss.fff",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Distância (mm)",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot
            });

            var lineSeries = new LineSeries
            {
                Title = "Distância (mm)",
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

            if (data.StartsWith("D=") && data.EndsWith("m"))
            {
                string distanceStr = data.Replace("D=", "").Replace("m", "").Trim();

                if (double.TryParse(distanceStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var distance))
                {
                    distance = distance * 1000;
                    var measurement = new Measurement
                    {
                        Timestamp = DateTime.Now,
                        Distance = distance - _zeroOffset,
                        DistanceAbsolute = distance
                    };
                    _allMeasurements.Add(measurement);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PlotPoints.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.Timestamp), measurement.Distance));
                        ExportToExcelCommand.NotifyCanExecuteChanged();
                    });
                    PlotModel.InvalidatePlot(true);

                    CurrentDistance = $"Relativa: {measurement.Distance}mm | Absoluta: {measurement.DistanceAbsolute}mm";
                }
            }
            else if (data.StartsWith("E="))
            {
                var measurement = new Measurement
                {
                    Timestamp = DateTime.Now,
                    Distance = 0,
                    DistanceAbsolute = 0
                };
                _allMeasurements.Add(measurement);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    PlotPoints.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.Timestamp), measurement.Distance));
                    ExportToExcelCommand.NotifyCanExecuteChanged();
                });
                PlotModel.InvalidatePlot(true);

                CurrentDistance = "N/A";
            }
        }
    }
}
