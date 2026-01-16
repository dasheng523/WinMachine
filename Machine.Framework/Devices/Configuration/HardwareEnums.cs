namespace Machine.Framework.Devices.Configuration
{
    // 轴映射目标枚举（框架层不定义具体轴，只定义泛型或基类，但为了简单，这里先从 Enum 入手）
    // 实际项目中，用户会传递自己的枚举

    // 硬件型号枚举
    public enum LeadshineModel
    {
        DMC3000,
        DMC3400
    }

    public enum ZMotionModel
    {
        ZMC406
    }
    
    public enum PulseMode
    {
        PulseDir,
        CwCcw
    }

    public enum SerialProtocol
    {
        ModbusRTU,
        ModbusASCII,
        None
    }

    // 传感器特征 (Feature) - 这边先定义成 object 或者泛型更好，但按照 prototypes，我们先定义一个通用的或者让 MapFeature 接受 Enum
    // 为了让 DSL 跑通，我们需要保证 Signature 兼容。
    // 在 Test 中，SensorFeature 是 Test 命名空间下的 Enum。
    // 所以 Framework 中的 MapFeature 应该接受 Enum 或者 object。
}
