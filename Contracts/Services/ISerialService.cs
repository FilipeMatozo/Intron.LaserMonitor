using System.Diagnostics;

namespace Intron.LaserMonitor.Contracts.Services;

public interface ISerialService 
{
    event EventHandler Connected;
    event EventHandler Disconnected;
    event EventHandler<Models.Events.DataReceivedEventArgs> DataReceived;

    bool Connect(string portName, int baudRate = 115200);
    void Disconnect();
    IEnumerable<string> GetAvailableSerialPorts();
    Task StartMeasurement();
    Task StopMeasurement();
}
