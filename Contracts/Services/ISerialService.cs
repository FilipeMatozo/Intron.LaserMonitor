using System.Diagnostics;

namespace Intron.LaserMonitor.Contracts.Services;

public interface ISerialService : IDisposable
{
    event EventHandler Connected;
    event EventHandler Disconnected;
    /// <summary>
    /// This event is invoked when measurement is request by the user, or when the measurement is stopped by any reason.
    /// <para>True is when the measurement just started.</para>
    /// <para>False is when the measurement just stopped.</para>
    /// </summary>
    event EventHandler<bool> OnMeasurementStateChanged;
    event EventHandler<Models.Events.DataReceivedEventArgs> DataReceived;

    bool IsConnected { get; }
    bool Connect(string portName, int baudRate = 115200);
    void Disconnect();
    IEnumerable<string> GetAvailableSerialPorts();
    Task StartMeasurement();
    Task StopMeasurement();
}
