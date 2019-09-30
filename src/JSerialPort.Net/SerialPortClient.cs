using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO.Ports;
using JSerialPort.Net.Abstraction;
using JSerialPort.Net.Configuration;
using Microsoft.Extensions.Logging;

namespace JSerialPort.Net
{
    public class SerialPortClient : ISerialPortClient
    {
        private readonly ILogger<SerialPortClient> _logger;
        private SerialPortSetting _serialPortSetting;

        public SerialPortClient(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SerialPortClient>();
        }

        private readonly ConcurrentQueue<ResultData> _resultDataQueue = new ConcurrentQueue<ResultData>();

        private readonly List<byte> _receivedBytes = new List<byte>();

        public SerialPort Client { get; private set; }
        public bool Initialized { get; private set; }

        public bool Opened => Client.IsOpen;

        public Func<SerialData, IEnumerable<byte>, ResultData> ResultExtractor;
        public Action<SerialPort, SerialError> ErrorHandler;
        public Action<SerialPort, SerialPinChange> PinChangedHandler;

        public void Initialize(SerialPortSetting setting)
        {
            _serialPortSetting = setting;
            
            Client = new SerialPort
            {
                PortName = setting.PortName,
                BaudRate = setting.BaudRate,
                Parity = setting.Parity,
                DataBits = setting.DataBits,
                Handshake = setting.Handshake,
                ReadTimeout = setting.ReadTimeout,
                WriteTimeout = setting.WriteTimeout,
                ReadBufferSize = setting.ReadBufferSize
            };
            Client.DataReceived += ClientOnDataReceived;
            Client.PinChanged += ClientOnPinChanged;
            Client.ErrorReceived += ClientOnErrorReceived;

            Initialized = true;
        }

        private void ClientOnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            _logger.LogDebug($"{DateTime.Now}: SerialPort-{Client.PortName} -> It has received an error event. Event type is {e.EventType}");
            ErrorHandler?.Invoke(Client, e.EventType);
        }

        private void ClientOnPinChanged(object sender, SerialPinChangedEventArgs e)
        {
            _logger.LogDebug($"{DateTime.Now}: SerialPort-{Client.PortName} -> It has received a pin changed event. Event type is {e.EventType}");
            PinChangedHandler?.Invoke(Client, e.EventType);
        }

        private void ClientOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var buffer = new byte[Client.BytesToRead];
            Client.Read(buffer, 0, buffer.Length);
            _receivedBytes.AddRange(buffer);
            Client.DiscardInBuffer();
            _logger.LogDebug($"{DateTime.Now}: SerialPort-{Client.PortName} -> It has received any datas. Event type is {e.EventType} {Environment.NewLine}" +
                             $"current received datas are ${string.Join(",", buffer)} {Environment.NewLine}" +
                             $"cumulative data is ${string.Join(",", _receivedBytes)}");
            var result = ResultExtractor?.Invoke(e.EventType, _receivedBytes);
            if (result != null && result.Length > 0)
            {
                _resultDataQueue.Enqueue(result);
                _receivedBytes.RemoveRange(0, result.Length);

                _logger.LogDebug($"{DateTime.Now}: SerialPort-{Client.PortName} -> The result was successfully extracted.{Environment.NewLine}" +
                                 $"result data is ${string.Join(",", result.Data)}{Environment.NewLine}" +
                                 $"cumulative data is ${string.Join(",", _receivedBytes)}");
            }

            if (_receivedBytes.Count > _serialPortSetting.ReadBufferSize)
            {
                _receivedBytes.Clear();
                _logger.LogDebug($"{DateTime.Now}: SerialPort-{Client.PortName} -> The data pool is full, clearing all data!");
            }
        }


        public void Open()
        {
            try
            {
                Client.Open();
                // todo: BreakState or CtsHolding or DtrEnable or RtsEnable can be used?
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{DateTime.Now}: SerialPort-{Client.PortName} -> throw an exception when clien will be opened.");
                throw;
            }
        }

        public void Close()
        {
            try
            {
                Client.Close();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"{DateTime.Now}: SerialPort-{Client.PortName} -> throw an exception when clien will be closed.");
                throw;
            }
        }

        public bool Write(byte[] command)
        {
            if (!Client.CtsHolding)
                return false;
            Client.Write(command, 0, command.Length);
            _logger.LogDebug($"{DateTime.Now}: SerialPort-{Client.PortName} -> command has been written.{Environment.NewLine}" +
                             $"command is {string.Join(",", command)}");
            return true;
        }

        public ResultData Read()
        {
            if (_resultDataQueue.TryDequeue(out var resultData))
            {
                return resultData;
            }

            return null;
        }


        public void Dispose()
        {
            if (Client != null && Client.IsOpen)
            {
                Client.Close();
            }

            Client?.Dispose();
        }
    }
}