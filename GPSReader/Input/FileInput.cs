using GPSReader.EventArgs;
using GPSReader.Interfaces;

public class FileInput : INMEAInput
{
    private StreamReader? _stream;
    private Thread? _thread;
    private CancellationTokenSource? _cancellationTokenSource;
    public event EventHandler<InputReceivedEventArgs>? DataReceived;
    
    public FileInput(string filePath)
    {
        _stream = new StreamReader(filePath);
    }

    public void Open()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _thread = new Thread(() => PeriodReadAndInvoke(_cancellationTokenSource.Token));
        _thread.Start();
    }

    public void Close()
    {
        _cancellationTokenSource?.Cancel();
    }

    public bool IsOpen { get; private set; }

    private void PeriodReadAndInvoke(CancellationToken cancellationToken)
    {
        string? lines = "";
        string? data;
        while ((data = _stream?.ReadLine()) != null && !cancellationToken.IsCancellationRequested)
        {
            if ((lines.Contains("$GPGLL")) && string.IsNullOrEmpty(data))
            {
                DataReceived?.Invoke(this, new InputReceivedEventArgs(lines));
                lines = "";
                System.Threading.Thread.Sleep(1000);
            }
            else
            {
                lines += data + "\n";
            }
        }
    }
}