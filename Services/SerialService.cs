using Intron.LaserMonitor.Contracts.Services;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Windows;

namespace Intron.LaserMonitor.Services
{
    public class SerialService : ISerialService
    {
        private SerialPort _serialPort = new();

        public bool IsConnected
        {
            get => _serialPort.IsOpen;
        }

        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<bool> OnMeasurementStateChanged;
        public event EventHandler<Models.Events.DataReceivedEventArgs> DataReceived;
        private CancellationTokenSource? _cts;
        private Task? _checkTask;

        public SerialService()
        {
            App.Current.Exit += (s, e) => Dispose();
        }
        #region CheckConnection
        /// <summary>
        /// Inicia a rotina de monitoramento da conexão serial.
        /// </summary>
        private void StartCheckRoutine()
        {
            if (_cts != null)
            {
                StopCheckRoutine();
            }               

            _cts = new CancellationTokenSource();
            _checkTask = Task.Run(() => CheckConnectionRoutine(_cts.Token));
        }

        /// <summary>
        /// Para a rotina de monitoramento.
        /// </summary>
        private void StopCheckRoutine()
        {
            if (_cts == null)
                return;

            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
            _checkTask = null; // deixamos a Task finalizar naturalmente
        }

        private async Task CheckConnectionRoutine(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (!_serialPort.IsOpen)
                    {
                        RaiseDisconnectEvent();
                        _cts?.Cancel();
                        continue;
                    }

                    await Task.Delay(1000, token); // intervalo de 1s
                }
            }
            catch (TaskCanceledException)
            {
                // esperado no cancelamento
            }
            catch (Exception ex)
            {

            }
        }
        #endregion
        public IEnumerable<string> GetAvailableSerialPorts()
        {
            return SerialPort.GetPortNames();
        }

        public bool Connect(string portName, int baudRate = 115200)
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    StopCheckRoutine();
                    Disconnect();
                }

                _serialPort = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One)
                {
                    NewLine = "\r\n",
                    Encoding = Encoding.ASCII,
                    ReadTimeout = 1000,
                    WriteTimeout = 1000
                };

                _serialPort.Open();

                if (!VerifyDevice())
                {
                    MessageBox.Show("Dispositivo inválido. Por favor conecte o laser.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    Disconnect();
                    return false;
                }

                _serialPort.DataReceived += OnDataReceived;
                Connected?.Invoke(this, new());
                StartCheckRoutine();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao conectar a {portName} com {baudRate}: {ex}");
                StopCheckRoutine();
                Disconnect();
                MessageBox.Show($"Falha ao conectar à porta serial.\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool VerifyDevice()
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.WriteLine("iHALT");

            try
            {
                while (true) 
                {
                    var line = _serialPort.ReadLine().Trim();

                    if (line == "STOP" || line == "OK")
                        return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        private void OnDataReceivedVerify(object sender, SerialDataReceivedEventArgs e)
        {
            if (!_serialPort.IsOpen)
                return;

            try
            {
                string data = _serialPort.ReadLine();
                DataReceived?.Invoke(this, new(data));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao receber os dados: {ex.Message}");
                return;
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!_serialPort.IsOpen)
                return;

            try
            {
                string data = _serialPort.ReadLine();
                DataReceived?.Invoke(this, new(data));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao receber os dados: {ex.Message}");
                return;
            }
        }

        public void Disconnect()
        {
            if (!_serialPort.IsOpen)
                return;

            try
            {
                _serialPort.DataReceived -= OnDataReceived;
                _serialPort.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao desconectar: {ex}");
                return;
            }
            finally
            {
                _serialPort.Dispose();
                Disconnected?.Invoke(this, new());
                OnMeasurementStateChanged(this, false);
            }
        }

        public async Task StartMeasurement(CancellationToken cancellationToken)
        {
            await SendCommandAsync("iFACM", cancellationToken);
            OnMeasurementStateChanged(this, true);
        }

        public async Task StopMeasurement(CancellationToken cancellationToken)
        {
            await SendCommandAsync("iHALT", cancellationToken);
            OnMeasurementStateChanged(this, false);
        }

        public async Task SendCommandAsync(string command, CancellationToken cancellationToken)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            try
            {
                string commandToSend = $"{command}\r\n";

                await _serialPort.BaseStream.WriteAsync(Encoding.ASCII.GetBytes(commandToSend).AsMemory(0, commandToSend.Length), cancellationToken);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao enviar comando ao: {ex}");
                return;
            }
        }
        protected virtual void RaiseDisconnectEvent()
        {
            var handler = Disconnected;
            if (handler is null) return;

            try
            {
                Application.Current.Dispatcher.BeginInvoke(() => handler(this, new()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Handler falhou: {ex}");
            }
        }
        public void Dispose()
        {
            _ = Task.Run(() => StopMeasurement(new()));
            StopCheckRoutine();
            Disconnect();
        }
    }
}
