using System;

namespace JSerialPort.Net.Exceptions
{
    public class JSerialPortException : Exception
    {
        public JSerialPortException(string message) : base(message)
        {
        }

        public JSerialPortException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}