namespace GPSReader.Interfaces;

public interface INMEAInput
{
    void Open();
    void Close();
    bool IsOpen { get; }  
    public event EventHandler<InputReceivedEventArgs> DataReceived;
}
