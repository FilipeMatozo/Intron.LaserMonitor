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
        [NotifyPropertyChangedFor(nameof(ConnectText))]
        [NotifyPropertyChangedFor(nameof(IsConnectedText))]
        [NotifyPropertyChangedFor(nameof(StartStopText))]
        [NotifyCanExecuteChangedFor(nameof(StartStopMeasurementCommand))]
        [NotifyCanExecuteChangedFor(nameof(ZeroOffsetCommand))]
        private bool _isConnected = false;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(StartStopText))]
        [NotifyCanExecuteChangedFor(nameof(StartStopMeasurementCommand))]
        [NotifyCanExecuteChangedFor(nameof(ZeroOffsetCommand))]
        private bool _isMeasuring = false;

        private CancellationTokenSource cancellationTokenSource = new();

        public string StartStopText
        {
            get => !IsMeasuring && IsConnected ? "Iniciar" : "Interromper";
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClearGraphCommand))]
        private string _currentDistance = "N/A";
        public string ConnectText
        {
            get => IsConnected ? "Desconectar" : "Conectar";
        }

        public string IsConnectedText
        {
            get => IsConnected ? "Conectado" : "Desconectado";
        }

        public PlotModel PlotModel { get; private set; }
        public PlotController PlotController { get; private set; }
        public List<DataPoint> PlotPoints { get; private set; } = new();

        private int _maxSecsPlotPoint = 10;
        private double _zeroOffset = 0;

        public MonitoringViewModel(ISerialService serialService, IExcelExportService excelExportService)
        {
            _serialService = serialService;
            _excelService = excelExportService;
            IsConnected = _serialService.IsConnected;
            
            SetupPlotModel();
            SubscribeEvents();
        }



        private void UnsubscribeEvents()
        {
            _serialService.DataReceived -= OnDataReceived;
            _serialService.Connected -= OnConnected;
            _serialService.Disconnected -= OnDisconnected;
            _serialService.OnMeasurementStateChanged -= _serialService_OnMeasurementStateChanged;
        }

        private void SubscribeEvents()
        {
            _serialService.DataReceived += OnDataReceived;
            _serialService.Connected += OnConnected;
            _serialService.Disconnected += OnDisconnected;
            _serialService.OnMeasurementStateChanged += _serialService_OnMeasurementStateChanged;
        }

        private void _serialService_OnMeasurementStateChanged(object? sender, bool isMeasuring) => IsMeasuring = isMeasuring;

        [RelayCommand(CanExecute = nameof(CanStartStopMeasurement))]
        private async void StartStopMeasurement()
        {
            if (!IsMeasuring)
            {
                cancellationTokenSource = new();
                _allMeasurements.Clear();
                PlotPoints.Clear();
                PlotModel.InvalidatePlot(true);
                CurrentDistance = "Iniciando...";

                await _serialService.StartMeasurement(cancellationTokenSource.Token);
            }
            else
            {
                await _serialService.StopMeasurement(cancellationTokenSource.Token);
                CurrentDistance = "Medição parada.";
            }
        }
        private bool CanStartStopMeasurement() => IsConnected;


        [RelayCommand(CanExecute = nameof(CanZeroOffset))]
        private void ZeroOffset()
        {
            _zeroOffset = _allMeasurements.Last().DistanceAbsolute;
            _allMeasurements.Clear();
            PlotPoints.Clear();
        }
        private bool CanZeroOffset() => IsConnected && IsMeasuring;

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

        [RelayCommand(CanExecute = nameof(CanClearGraph))]
        private void ClearGraph()
        {
            _allMeasurements.Clear();
            PlotPoints.Clear();
            PlotModel.InvalidatePlot(true);
            _zeroOffset = 0;
            CurrentDistance = "N/A";
        }
        private bool CanClearGraph() => PlotPoints.Count() > 0 || CurrentDistance != "N/A";

        private void SetupPlotModel()
        {
            PlotModel = new PlotModel { Title = "Distância do Laser vs. Tempo" };
            PlotController = new PlotController();
            PlotController.UnbindAll();
            
            var now = DateTime.Now;

            PlotModel.Axes.Add(new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Tempo",
                StringFormat = "HH:mm:ss.fff",
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.Dot,
                Minimum = DateTimeAxis.ToDouble(now),
                Maximum = DateTimeAxis.ToDouble(now.AddSeconds(_maxSecsPlotPoint))
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

                    if (Application.Current is not null)                    
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            PlotPoints.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.Timestamp), measurement.Distance));

                            var xAxis = PlotModel.Axes.OfType<DateTimeAxis>().FirstOrDefault();
                            if (xAxis is not null)
                            {
                                double lastX = DateTimeAxis.ToDouble(measurement.Timestamp);
                                double width = xAxis.ActualMaximum - xAxis.ActualMinimum;

                                if (double.IsNaN(width) || double.IsInfinity(width) || width <= 0)
                                    width = DateTimeAxis.ToDouble(DateTime.Now) - DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(_maxSecsPlotPoint));

                                xAxis.Maximum = lastX;
                                xAxis.Minimum = lastX - width;
                            }

                            ExportToExcelCommand.NotifyCanExecuteChanged();
                            ClearGraphCommand.NotifyCanExecuteChanged();
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

        public void Dispose()
        {
            UnsubscribeEvents();
            _serialService.StopMeasurement(new());
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
        }
    }
}
