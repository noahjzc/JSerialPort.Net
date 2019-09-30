using System;
using System.IO.Ports;
using JSerialPort.Net.Configuration;

namespace JSerialPort.Net.Abstraction
{
    public interface ISerialPortClient : IDisposable
    {
        SerialPort Client { get; }

        bool Initialized { get; }

        bool Opened { get; }

        void Initialize(SerialPortSetting setting);
        
        void Open();

        void Close();

        bool Write(byte[] command);
        
        ResultData Read();
    }
}