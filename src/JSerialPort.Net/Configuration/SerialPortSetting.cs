using System.IO.Ports;

namespace JSerialPort.Net.Configuration
{
    public class SerialPortSetting
    {
        /// <summary>
        /// 串口名称
        /// </summary>
        public string PortName { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// ???
        /// 0 - None
        /// 1 - Odd
        /// 2 - Even
        /// 3 - Mark
        /// 4 - Space
        /// </summary>
        public Parity Parity { get; set; }

        /// <summary>
        /// 数据位数
        /// </summary>
        public int DataBits { get; set; }

        /// <summary>
        /// ???
        /// 0 - None
        /// 1 - One
        /// 2 - Two
        /// 3 - OnePointFive
        /// </summary>
        public StopBits StopBits { get; set; }

        /// <summary>
        /// 握手???
        /// 0 - None
        /// 1 - X On X Off
        /// 2 - Request To Send
        /// 3 - Request To Send X On X Off
        /// </summary>
        public Handshake Handshake { get; set; }

        /// <summary>
        /// 发送超时
        /// </summary>
        public int WriteTimeout { get; set; } = 500;

        /// <summary>
        /// 接收超时
        /// </summary>
        public int ReadTimeout { get; set; } = 500;

        /// <summary>
        /// 命令地址-从机地址
        /// </summary>
        public byte SerialCommandAddress { get; set; }

        public int ReadBufferSize { get; set; } = 2000;

        public int WriteCommandRetryCount { get; set; } = 3;
        
        
        public int ReadResultRetryCount { get; set; } = 5;
    }
    
    
}