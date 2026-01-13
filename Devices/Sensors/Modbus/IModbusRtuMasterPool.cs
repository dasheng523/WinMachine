using LanguageExt;
using Modbus.Device;

namespace Devices.Sensors.Modbus;

public interface IModbusRtuMasterPool
{
    /// <summary>
    /// 获取（或创建并缓存）对应串口的 Modbus RTU Master。
    /// 返回对象生命周期由 Pool 管理。
    /// </summary>
    Fin<IModbusSerialMaster> GetOrCreate(ModbusRtuConnectionOptions options);
}
