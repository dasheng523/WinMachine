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

    /// <summary>
    /// 获取该连接对应的互斥锁对象，用于保证一条读写序列原子。
    /// </summary>
    object GetLock(ModbusRtuConnectionOptions options);
}
