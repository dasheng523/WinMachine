using System;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Primitives;

namespace Machine.Framework.Core.Flow.Steps
{
    // 模拟具体硬件 DSL 入口
    public static class FlowBuilders
    {
        public static MotionBuilder Motion(AxisID axis) => new MotionBuilder(axis);
        public static CylinderBuilder Cylinder(CylinderID cylinder) => new CylinderBuilder(cylinder);
        public static SensorBuilder Sensor(SensorID sensor) => new SensorBuilder(sensor);
        public static SystemBuilder SystemStep => new SystemBuilder();

        // 命名包装器，用于在 DSL 中增加可读性
        public static Step<Unit> Name(string name)
        {
            return new Step<Unit>(new ActionStepDesc
            {
                Name = name,
                TargetDevice = "System",
                Operation = "NoOp",
                Args = Array.Empty<object>()
            });
        }

        // 作用域包装器，用于逻辑分组
        public static Step<T> Scope<T>(string name, Step<T> inner)
        {
            return new Step<T>(new ScopeStepDesc
            {
                Name = name,
                InnerStep = inner.Definition
            });
        }
    }
    
    public class MotionBuilder
    {
        private readonly AxisID _axis;
        public MotionBuilder(AxisID axis) => _axis = axis;

        // 非阻塞：仅启动运动
        public Step<Unit> MoveTo(double pos)
        {
            return new Step<Unit>(new ActionStepDesc 
            { 
                Name = $"MoveTo_{_axis}_{pos}",
                TargetDevice = _axis.Name, 
                Operation = "MoveTo", 
                Args = new object[] { pos } 
            });
        }

        // 阻塞：启动运动并等待完成
        public Step<Unit> MoveToAndWait(double pos)
        {
            return new Step<Unit>(new ActionStepDesc
            {
                Name = $"MoveToAndWait_{_axis}_{pos}",
                TargetDevice = _axis.Name,
                Operation = "MoveToAndWait",
                Args = new object[] { pos }
            });
        }

        /// <summary>
        /// 轴移动直到满足特定传感器条件。
        /// </summary>
        /// <param name="targetPos">最大限制位置</param>
        /// <param name="sensor">关联传感器 ID</param>
        /// <param name="threshold">停止阈值</param>
        public Step<double> MoveUntil(double targetPos, SensorID sensor, double threshold)
        {
            return new Step<double>(new ActionStepDesc
            {
                Name = $"MoveUntil_{_axis}_By_{sensor}_{threshold}",
                TargetDevice = _axis.Name,
                Operation = "MoveUntil",
                Args = new object[] { targetPos, sensor.Name, threshold }
            });
        }
    }

    public class CylinderBuilder
    {
        private readonly CylinderID _cylinder;
        public CylinderBuilder(CylinderID cylinder) => _cylinder = cylinder;

        // 非阻塞：仅触发输出
        public Step<Unit> Fire(bool state)
        {
            return new Step<Unit>(new ActionStepDesc 
            { 
                Name = $"Fire_{_cylinder}_{state}",
                TargetDevice = _cylinder.Name, 
                Operation = "Fire", 
                Args = new object[] { state } 
            });
        }

        // 阻塞：触发输出并等待传感器反馈
        public Step<Unit> FireAndWait(bool state)
        {
            return new Step<Unit>(new ActionStepDesc
            {
                Name = $"FireAndWait_{_cylinder}_{state}",
                TargetDevice = _cylinder.Name,
                Operation = "FireAndWait",
                Args = new object[] { state }
            });
        }
    }

    public class SensorBuilder
    {
        private readonly SensorID _sensor;
        public SensorBuilder(SensorID sensor) => _sensor = sensor;

        public Step<bool> CheckLevel(bool expected)
        {
            return new Step<bool>(new ActionStepDesc
            {
                Name = $"CheckLevel_{_sensor}_{expected}",
                TargetDevice = _sensor.Name,
                Operation = "CheckLevel",
                Args = new object[] { expected }
            });
        }

        public Step<double> ReadAnalog()
        {
            return new Step<double>(new ActionStepDesc
            {
                Name = $"ReadAnalog_{_sensor}",
                TargetDevice = _sensor.Name,
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
                Name = $"Delay_{ms}ms",
                TargetDevice = "System", 
                Operation = "Delay", 
                Args = new object[] { ms } 
            });
        }
    }
}
