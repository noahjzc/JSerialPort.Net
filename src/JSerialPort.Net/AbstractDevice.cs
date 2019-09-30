using System;
using System.Collections.Generic;
using System.Threading;
using JSerialPort.Net.Configuration;
using JSerialPort.Net.Exceptions;
using Microsoft.Extensions.Logging;

namespace JSerialPort.Net
{
    public abstract class AbstractDevice
    {
        private readonly ILogger<AbstractDevice> _logger;
        private readonly SerialPortClient _serialPortClient;
        private SerialPortSetting _serialPortSetting;

        protected AbstractDevice(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AbstractDevice>();
            _serialPortClient = new SerialPortClient(loggerFactory);
        }

        protected void InitClient(SerialPortSetting setting)
        {
            _serialPortSetting = setting;

            if (!_serialPortClient.Initialized)
            {
                _serialPortClient.Initialize(_serialPortSetting);
            }
        }

        // _serialPortSetting = serialPortSetting;
        // Initialize(serialPortSetting);

        protected Dictionary<string, string> TestParameters { get; set; }

        protected string DeviceName { get; set; }

        protected abstract byte[] MakeCommand(byte address, byte commandWord, byte[] content);

        protected abstract bool ValidReceivedData(byte[] resultData);

        protected abstract byte[] CheckCommand(byte[] command);

        protected void ExecuteWithoutResponse(byte[] command)
        {
            var sendCount = 0;
            var writeSucceed = false;
            while (true)
            {
                sendCount++;
                if (sendCount >= _serialPortSetting.WriteCommandRetryCount)
                {
                    break;
                }

                if (_serialPortClient.Write(command))
                {
                    writeSucceed = true;
                    break;
                }

                Thread.Sleep(50);
            }

            if (!writeSucceed)
            {
                _logger.LogError($"{DateTime.Now}: {DeviceName} -> write command failed{Environment.NewLine}" +
                                 $"command is {string.Join(",", command)}");
                throw new JSerialPortException($"{DateTime.Now}: {DeviceName} -> write command failed");
            }

            _logger.LogDebug($"{DateTime.Now}: {DeviceName} -> it has been written command successful{Environment.NewLine}" +
                             $"command is {string.Join(",", command)}");
        }

        protected byte[] ExecuteWithResponse(byte[] command, Func<byte[], bool> checkResultDataFunc)
        {
            ExecuteWithoutResponse(command);

            var retryCount = 0;
            var readSucceed = false;
            ResultData result = null;
            while (true)
            {
                retryCount++;
                if (retryCount >= _serialPortSetting.ReadResultRetryCount)
                {
                    break;
                }

                var resultData = _serialPortClient.Read();
                if (resultData == null)
                {
                    Thread.Sleep(500);
                    continue;
                }

                if (!ValidReceivedData(resultData.Data))
                {
                    Thread.Sleep(50);
                    continue;
                }

                if (checkResultDataFunc != null && !checkResultDataFunc.Invoke(resultData.Data))
                {
                    Thread.Sleep(50);
                    continue;
                }

                result = resultData;
                readSucceed = true;
                break;
            }

            if (readSucceed)
            {
                return result.Data;
            }

            throw new JSerialPortException($"{DateTime.Now}: {DeviceName} -> execute command failed");
        }

        protected void ClearResults()
        {
            while (_serialPortClient.Read() != null)
            {
            }
        }
    }
}