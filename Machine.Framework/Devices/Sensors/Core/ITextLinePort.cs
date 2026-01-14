using System;

namespace Machine.Framework.Devices.Sensors.Core;

/// <summary>
/// 最小行读取抽象：用于把业务逻辑�?System.IO.Ports.SerialPort 中解耦，便于单元测试�?
/// </summary>
public interface ITextLinePort
{
    int ReadTimeout { get; set; }

    string NewLine { get; set; }

    void DiscardInBuffer();

    void Write(string text);

    string ReadLine();
}


