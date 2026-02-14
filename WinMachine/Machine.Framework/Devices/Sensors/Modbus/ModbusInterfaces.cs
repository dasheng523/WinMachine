using LanguageExt;

namespace Machine.Framework.Devices.Sensors.Modbus;

public interface IResettableSensor
{
    Fin<Unit> Reset();
}

public interface IAlertLimitWritable
{
    Fin<Unit> WriteAlertLimit(double limitKg);
}


