using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intron.LaserMonitor.Contracts.Services;
using Intron.LaserMonitor.CustomControls.MyMessageBox;
using Intron.LaserMonitor.CustomControls.MyMessageBox.Enums;
using Intron.LaserMonitor.Models;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp.Wpf;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsShowingMarkersText))]
        private bool _isShowingMarkers = false;

        private CancellationTokenSource cancellationTokenSource = new();

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ClearGraphCommand))]
        private string _currentDistance = "N/A";
       
        public string StartStopText
        {
            get => !IsMeasuring && IsConnected ? "Iniciar" : "Interromper";
        }

        public string ConnectText
        {
            get => IsConnected ? "Desconectar" : "Conectar";
        }

        public string IsConnectedText
        {
            get => IsConnected ? "Conectado" : "Desconectado";
        }

        public string IsShowingMarkersText
        {
            get => !IsShowingMarkers ? "Mostrar pontos" : "Esconder pontos";
        }

        public string IsShowingMarkersColor
        {
            get => !IsShowingMarkers ? "Mostrar pontos" : "Esconder pontos";
        }

        public PlotModel PlotModel { get; private set; }
        public PlotController PlotController { get; private set; }
        public LineSeries _lineSeries { get; private set; }
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
                    //MessageBox.Show($"Dados exportados com sucesso para:\n{sfd.FileName}", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                    var (result, dontAsk) = MyMessageBox.Show(
                        owner: Application.Current?.MainWindow,
                        options: new MyMessageBoxOptions(
                            Title: "Exportação concluída",
                            Message: "Dados exportados com sucesso!",
                            Detail: sfd.FileName,
                            Buttons: MyMessageBoxButtons.Custom,
                            Icon: MyMessageBoxIcon.Success,
                            PrimaryText: "Abrir arquivo",
                            SecondaryText: "Mostrar na pasta",
                            TertiaryText: "Fechar",
                            AccentBrush: (Brush)Application.Current.Resources["DialogAccentBrush"],
                            ShowCopyButton: false,
                            ShowDoNotAskAgain: false,
                            DefaultButton: MyMessageBoxDefaultButton.First,
                            CancelResult: MyMessageBoxResult.Tertiary
                        )
                    );

                    switch (result)
                    {
                        case MyMessageBoxResult.Primary:
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true });
                            break;
                        case MyMessageBoxResult.Secondary:
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("explorer.exe", $"/select,\"{sfd.FileName}\"") { UseShellExecute = true });
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception ex)
                {
                    _ = MyMessageBox.Show(
                        owner: Application.Current?.MainWindow,
                        options: new MyMessageBoxOptions(
                            Title: "Erro ao exportar",
                            Message: "Ocorreu um erro ao tentar exportar o arquivo:",
                            Detail: ex.Message,
                            Buttons: MyMessageBoxButtons.Ok,
                            Icon: MyMessageBoxIcon.Error,
                            AccentBrush: (Brush)Application.Current.Resources["DialogAccentBrush"],
                            ShowCopyButton: true,
                            ShowDoNotAskAgain: false,
                            DefaultButton: MyMessageBoxDefaultButton.First,
                            CancelResult: MyMessageBoxResult.Tertiary
                        )
                    );
                    //MessageBox.Show($"Ocorreu um erro ao exportar:\n{ex.Message}", "Erro de Exportação", MessageBoxButton.OK, MessageBoxImage.Error);
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
            ExportToExcelCommand.NotifyCanExecuteChanged();
            ShowHideMarkersCommand.NotifyCanExecuteChanged();
        }
        private bool CanClearGraph() => PlotPoints.Count() > 0 || CurrentDistance != "N/A";

        [RelayCommand(CanExecute = nameof(CanShowHideMarkers))]
        private void ShowHideMarkers()
        {
            if (_lineSeries == null) return;

            if (IsShowingMarkers) {

                _lineSeries.MarkerType = MarkerType.None;
                IsShowingMarkers = false;
            }
            else
            {
                _lineSeries.MarkerType = MarkerType.Circle;
                IsShowingMarkers = true;
            }
            PlotModel?.InvalidatePlot(true);
        }
        private bool CanShowHideMarkers() => PlotPoints.Count() > 0;

        private void SetupPlotModel()
        {
            PlotModel = new PlotModel { Title = "Distância do Laser vs. Tempo" };
            PlotController = new PlotController();
            PlotController.UnbindAll();
            PlotController.BindMouseEnter(PlotCommands.HoverSnapTrack);

            var controller = PlotController;

            // --- Scroll normal = Pan horizontal (X) ---
            controller.BindMouseWheel(
                OxyModifierKeys.None,
                new DelegatePlotCommand<OxyMouseWheelEventArgs>(
                    (view, ctl, args) =>
                    {
                        double delta = args.Delta > 0 ? -1 : 1; // up = direita, down = esquerda
                        const double step = 50;
                        foreach (var axis in view.ActualModel.Axes.Where(a => a.Position == AxisPosition.Bottom))
                        {
                            axis.Pan(delta * step);
                        }
                        view.InvalidatePlot(false);
                        args.Handled = true;
                    }));

            // --- Ctrl + Scroll = Zoom no eixo X ---
            controller.BindMouseWheel(
                OxyModifierKeys.Control,
                new DelegatePlotCommand<OxyMouseWheelEventArgs>(
                    (view, ctl, args) =>
                    {
                        double zoomFactor = args.Delta > 0 ? 1.2 : 0.8;
                        foreach (var axis in view.ActualModel.Axes.Where(a => a.Position == AxisPosition.Bottom))
                        {
                            double x = axis.InverseTransform(args.Position.X);
                            axis.ZoomAt(zoomFactor, x);
                        }
                        if(zoomFactor >= 1)
                        {
                            _lineSeries.MarkerType = MarkerType.Circle;
                            IsShowingMarkers = true;
                            ShowHideMarkersCommand.NotifyCanExecuteChanged();
                        }
                        else
                        {
                            _lineSeries.MarkerType = MarkerType.None;
                            IsShowingMarkers = false;
                            ShowHideMarkersCommand.NotifyCanExecuteChanged();
                        }

                        view.InvalidatePlot(false);
                        args.Handled = true;
                    }));

            // --- Shift + Scroll = Pan vertical (Y) ---
            controller.BindMouseWheel(
                OxyModifierKeys.Shift,
                new DelegatePlotCommand<OxyMouseWheelEventArgs>(
                    (view, ctl, args) =>
                    {
                        double delta = args.Delta > 0 ? 1 : -1; // up = cima, down = baixo
                        const double step = 50;
                        foreach (var axis in view.ActualModel.Axes.Where(a => a.Position == AxisPosition.Left))
                        {
                            axis.Pan(delta * step);
                        }
                        view.InvalidatePlot(false);
                        args.Handled = true;
                    }));

            // 🖱 Botão do meio = Pan livre (X e Y ao mesmo tempo)
            controller.BindMouseDown(OxyMouseButton.Middle, PlotCommands.PanAt);

            // Clique esquerdo/direito → resetar todos os eixos
            var resetCommand = new DelegatePlotCommand<OxyMouseDownEventArgs>(
                (view, ctl, args) =>
                {
                    view.ActualModel.ResetAllAxes();
                    view.InvalidatePlot(false);
                    args.Handled = true;
                });

            controller.BindMouseDown(OxyMouseButton.Left, resetCommand);
            controller.BindMouseDown(OxyMouseButton.Right, resetCommand);
            //controller.BindMouseDown(OxyMouseButton.Middle, resetCommand);

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

            _lineSeries = new LineSeries
            {
                Title = "Distância (mm)",
                StrokeThickness = 2,
                MarkerType = MarkerType.None,
                MarkerSize = 5,
                MarkerStroke = OxyColors.Green,
                MarkerFill = OxyColors.White,
                MarkerStrokeThickness = 1.2,
                ItemsSource = PlotPoints,
                TrackerFormatString = "Tempo: {2:HH:mm:ss.fff}\nDistância: {4:0} mm",
                CanTrackerInterpolatePoints = false
            };

            PlotModel.Series.Add(_lineSeries);
        }

        private void OnDataReceived(object sender, Models.Events.DataReceivedEventArgs dataReceivedEventArgs)
        {
            void AdjustXAxis(DateTime now)
            {
                var xAxis = PlotModel.Axes.OfType<DateTimeAxis>().FirstOrDefault();
                if (xAxis is not null)
                {
                    double lastX = DateTimeAxis.ToDouble(now);
                    double width = xAxis.ActualMaximum - xAxis.ActualMinimum;

                    if (double.IsNaN(width) || double.IsInfinity(width) || width <= 0)
                        width = DateTimeAxis.ToDouble(DateTime.Now) - DateTimeAxis.ToDouble(DateTime.Now.AddSeconds(_maxSecsPlotPoint));

                    xAxis.Maximum = lastX;
                    xAxis.Minimum = lastX - width;
                }
            }

            void AdjustYAxis(DateTime now)
            {
                var yAxis = PlotModel.Axes.OfType<LinearAxis>()
                    .FirstOrDefault(a => a.Position == AxisPosition.Left);
                if (yAxis is null) return;

                var start = now - TimeSpan.FromSeconds(_maxSecsPlotPoint);

                double min = double.PositiveInfinity;
                double max = double.NegativeInfinity;

                for (int i = _allMeasurements.Count - 1; i >= 0; i--)
                {
                    var m = _allMeasurements[i];
                    if (m.Timestamp < start) break;

                    if (m.Distance < min) min = m.Distance;
                    if (m.Distance > max) max = m.Distance;
                }

                if (double.IsInfinity(min) || double.IsInfinity(max)) return; 

                if (min == max)
                {
                    double pad = Math.Max(1e-6, Math.Abs(min) * 0.05 + 0.5);
                    yAxis.Minimum = min - pad;
                    yAxis.Maximum = max + pad;
                }
                else
                {
                    double pad = Math.Max(0.5, (max - min) * 0.10); 
                    yAxis.Minimum = min - pad;
                    yAxis.Maximum = max + pad;
                }
            }

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

                            AdjustXAxis(measurement.Timestamp);
                            AdjustYAxis(measurement.Timestamp);


                            ExportToExcelCommand.NotifyCanExecuteChanged();
                            ClearGraphCommand.NotifyCanExecuteChanged();
                            ShowHideMarkersCommand.NotifyCanExecuteChanged();
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
                if (Application.Current is not null)
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PlotPoints.Add(new DataPoint(DateTimeAxis.ToDouble(measurement.Timestamp), measurement.Distance));

                        AdjustXAxis(measurement.Timestamp);
                        AdjustYAxis(measurement.Timestamp);

                        ExportToExcelCommand.NotifyCanExecuteChanged();
                        ClearGraphCommand.NotifyCanExecuteChanged();
                        ShowHideMarkersCommand.NotifyCanExecuteChanged();
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
