using System;
using System.Linq;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Devices.Sensors.Modbus;
using Machine.Framework.Devices.Sensors.Serial;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Core.Configuration.Models;
using static LanguageExt.Prelude;

namespace WinMachine.Services
{
    // Minimal implementation to fix broken file.
    // Assuming SensorOptions is defined in Machine.Framework.Configuration or similar
    // We will stub this to get it to compile.
    
    public class StringSensorResolver : IResolver<ISensor<string>>
    {
         public Fin<ISensor<string>> Resolve(string name)
         {
             return FinFail<ISensor<string>>(Error.New("Not implemented"));
         }
    }

    public class DoubleSensorResolver : IResolver<ISensor<double>>
    {
         public Fin<ISensor<double>> Resolve(string name)
         {
             return FinFail<ISensor<double>>(Error.New("Not implemented"));
         }
    }

    public class LevelSensorResolver : IResolver<ISensor<Level>>
    {
        public Fin<ISensor<Level>> Resolve(string name)
        {
             return FinFail<ISensor<Level>>(Error.New("Not implemented"));
        }
    }
}
