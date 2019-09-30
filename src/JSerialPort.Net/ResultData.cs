using System;

namespace JSerialPort.Net
{
    public class ResultData
    {
        public ResultData(byte[] data)
        {
            Data = data;
            Length = data.Length;
            CreateTime = DateTime.Now;
        }

        public byte[] Data { get; }

        public int Length { get; }

        public DateTime CreateTime { get; }
    }
}