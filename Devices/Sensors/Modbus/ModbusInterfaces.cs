using LanguageExt;

namespace Devices.Sensors.Modbus;

public interface IResettableSensor
{
    Fin<Unit> Reset();
}

public interface IAlertLimitWritable
{
    Fin<Unit> WriteAlertLimit(double limitKg);
}
