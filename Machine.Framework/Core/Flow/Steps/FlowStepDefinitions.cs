using System;
using Machine.Framework.Core.Flow.Dsl;

namespace Machine.Framework.Core.Flow.Steps
{
    // 模拟具体硬件 DSL 入口
    public static class FlowBuilders
    {
        public static MotionBuilder Motion(string axisId) => new MotionBuilder(axisId);
        public static CylinderBuilder Cylinder(string cylId) => new CylinderBuilder(cylId);
        public static SensorBuilder Sensor(string sensorId) => new SensorBuilder(sensorId);
        public static SystemBuilder SystemStep => new SystemBuilder();
    }
    
    public class MotionBuilder
    {
        private readonly string _id;
        public MotionBuilder(string id) => _id = id;

        public Step<bool> MoveTo(double pos)
        {
            return new Step<bool>(new ActionStepDesc 
            { 
                TargetDevice = _id, 
                Operation = "MoveTo", 
                Args = new object[] { pos } 
            });
        }
    }

    public class CylinderBuilder
    {
        private readonly string _id;
        public CylinderBuilder(string id) => _id = id;

        public Step<Unit> Fire(bool state)
        {
            return new Step<Unit>(new ActionStepDesc 
            { 
                TargetDevice = _id, 
                Operation = "Fire", 
                Args = new object[] { state } 
            });
        }
    }

    public class SensorBuilder
    {
        private readonly string _id;
        public SensorBuilder(string id) => _id = id;

        public Step<bool> CheckLevel(bool expected)
        {
            return new Step<bool>(new ActionStepDesc
            {
                TargetDevice = _id,
                Operation = "CheckLevel",
                Args = new object[] { expected }
            });
        }

        public Step<double> ReadAnalog()
        {
            return new Step<double>(new ActionStepDesc
            {
                TargetDevice = _id,
                Operation = "ReadAnalog",
                Args = Array.Empty<object>()
            });
        }
    }

    public class SystemBuilder
    {
        public Step<Unit> Delay(int ms)
        {
             return new Step<Unit>(new ActionStepDesc 
            { 
                TargetDevice = "System", 
                Operation = "Delay", 
                Args = new object[] { ms } 
            });
        }
    }
}
