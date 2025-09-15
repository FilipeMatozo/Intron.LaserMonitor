using Intron.LaserMonitor.Contracts.Services;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

        public SerialService()
        {
            App.Current.Exit += (s, e) => Dispose();
        }
        
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
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao conectar a {portName} com {baudRate}: {ex}");
                Disconnect();
                MessageBox.Show($"Falha ao conectar à porta serial.\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private bool VerifyDevice()
        {
            _serialPort.DiscardInBuffer();
            _serialPort.DiscardOutBuffer();
            _serialPort.WriteLine("iGET:6");

            try
            {
                string line = _serialPort.ReadLine();
                line = line.Replace("\r", "").Replace("\n", "").Trim();
                return string.Equals(line.Trim(), "ADDRESS=1", StringComparison.Ordinal);
            }
            catch (Exception ex)
            {
                return false;
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

        public async void Dispose()
        {
            await StopMeasurement(new());
            Disconnect();
        }
    }
}
