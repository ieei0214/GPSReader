using System;
using System.IO.Ports;
using System.Collections.Generic;
using GPSReader.Exceptions;
using GPSReader.Interfaces;
using GPSReader.EventArgs;

namespace GPSReader
{
    public class GPSReader
    {
        private string comPortName;
        private int baudRate;
        private SerialPort serialPort;
        private List<INMEAParser> parsers;

        public event EventHandler<LocationEventArgs> LocationUpdated;

        public GPSReader(string comPortName, int baudRate, IEnumerable<INMEAParser> parsers)
        {
            this.comPortName = comPortName;
            this.baudRate = baudRate;
            this.parsers = new List<INMEAParser>(parsers);
            serialPort = new SerialPort(comPortName, baudRate);
        }

        public void StartReading()
        {
            try
            {
                serialPort.Open();
                serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            }
            catch (Exception ex)
            {
                throw new GPSException("Failed to start reading: " + ex.Message);
            }
        }

        public void StopReading()
        {
            if (serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }

        private void OnLocationUpdated(string latitude, string longitude)
        {
            LocationUpdated?.Invoke(this, new LocationEventArgs(latitude, longitude));
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string data = sp.ReadExisting();
            string[] sentences = data.Split('\n');

            foreach (string sentence in sentences)
            {
                foreach (var parser in parsers)
                {
                    if (parser.TryParse(sentence, out var location))
                    {
                        OnLocationUpdated(location.Latitude, location.Longitude);
                    }
                }
            }
        }
    }
}
