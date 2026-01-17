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
        private readonly string _id;
        public MotionBuilder(string id) => _id = id;

        // 非阻塞：仅启动运动
        public Step<bool> MoveTo(double pos)
        {
            return new Step<bool>(new ActionStepDesc 
            { 
                Name = $"MoveTo_{_id}_{pos}",
                TargetDevice = _id, 
                Operation = "MoveTo", 
                Args = new object[] { pos } 
            });
        }

        // 阻塞：启动运动并等待完成
        public Step<bool> MoveToAndWait(double pos)
        {
            return new Step<bool>(new ActionStepDesc
            {
                Name = $"MoveToAndWait_{_id}_{pos}",
                TargetDevice = _id,
                Operation = "MoveToAndWait",
                Args = new object[] { pos }
            });
        }

        /// <summary>
        /// 轴移动直到满足特定传感器条件。
        /// </summary>
        /// <param name="targetPos">最大限制位置</param>
        /// <param name="sensorId">关联传感器 ID</param>
        /// <param name="threshold">停止阈值</param>
        public Step<double> MoveUntil(double targetPos, string sensorId, double threshold)
        {
            return new Step<double>(new ActionStepDesc
            {
                Name = $"MoveUntil_{_id}_By_{sensorId}_{threshold}",
                TargetDevice = _id,
                Operation = "MoveUntil",
                Args = new object[] { targetPos, sensorId, threshold }
            });
        }
    }

    public class CylinderBuilder
    {
        private readonly string _id;
        public CylinderBuilder(string id) => _id = id;

        // 非阻塞：仅触发输出
        public Step<Unit> Fire(bool state)
        {
            return new Step<Unit>(new ActionStepDesc 
            { 
                Name = $"Fire_{_id}_{state}",
                TargetDevice = _id, 
                Operation = "Fire", 
                Args = new object[] { state } 
            });
        }

        // 阻塞：触发输出并等待传感器反馈
        public Step<Unit> FireAndWait(bool state)
        {
            return new Step<Unit>(new ActionStepDesc
            {
                Name = $"FireAndWait_{_id}_{state}",
                TargetDevice = _id,
                Operation = "FireAndWait",
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
                Name = $"CheckLevel_{_id}_{expected}",
                TargetDevice = _id,
                Operation = "CheckLevel",
                Args = new object[] { expected }
            });
        }

        public Step<double> ReadAnalog()
        {
            return new Step<double>(new ActionStepDesc
            {
                Name = $"ReadAnalog_{_id}",
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
                Name = $"Delay_{ms}ms",
                TargetDevice = "System", 
                Operation = "Delay", 
                Args = new object[] { ms } 
            });
        }
    }
}
