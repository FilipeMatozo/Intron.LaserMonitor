using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Intron.LaserMonitor.Contracts.Services;

namespace Intron.LaserMonitor.Services
{
    public class SerialService : ISerialService
    {
        private SerialPort _serialPort;

        public event EventHandler Connected;   
        public event EventHandler Disconnected;   
        public event EventHandler<Models.Events.DataReceivedEventArgs> DataReceived;   

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
                };
                _serialPort.DataReceived += OnDataReceived;
                _serialPort.Open();

                Connected?.Invoke(this, new());
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao conectar a {portName} com {baudRate}: {ex}");
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
            }
        }

        public async Task StartMeasurement()
        {
            await SendCommandAsync("iFACM");
        }

        public async Task StopMeasurement()
        {
            await SendCommandAsync("iHALT");
        }

        public async Task SendCommandAsync(string command)
        {
            if (_serialPort == null || !_serialPort.IsOpen)
                return;

            try
            {
                string commandToSend = $"{command}\r\n";

                await _serialPort.BaseStream.WriteAsync(
                    System.Text.Encoding.ASCII.GetBytes(commandToSend), 0, commandToSend.Length);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao enviar comando ao: {ex}");
                return;
            }
        }
    }
}
