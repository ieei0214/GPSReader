using System.IO.Ports;
using GPSReader;
using GPSReader.EventArgs;
using GPSReader.Interfaces;

namespace ConsoleApp;

public class SerialInput : INMEAInput
{
    private string comPortName = "COM12";
    private int baudRate = 115200;
    private SerialPort? serialPort;

    public event EventHandler<InputReceivedEventArgs>? DataReceived;

    public void Open()
    {
        serialPort = new SerialPort(comPortName, baudRate);
        serialPort.Open();
        serialPort.DataReceived += SerialPort_DataReceived;
    }

    public void Close()
    {
        serialPort?.Close();
    }

    public bool IsOpen { get; private set; }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string data = serialPort!.ReadExisting();
        DataReceived?.Invoke(this, new InputReceivedEventArgs(data));
    }

}