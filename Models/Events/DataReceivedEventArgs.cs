namespace Intron.LaserMonitor.Models.Events;

public class DataReceivedEventArgs : EventArgs
{
    public string Data { get; }

    public DataReceivedEventArgs(string data)
    {
        Data = data;
    }
}
