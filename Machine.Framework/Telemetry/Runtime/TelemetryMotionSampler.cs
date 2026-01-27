using System;
using System.Collections.Generic;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Simulation;

namespace Machine.Framework.Telemetry.Runtime;

public static class TelemetryMotionSampler
{
    public static Dictionary<string, double> Sample(FlowContext context)
    {
        var snapshot = new Dictionary<string, double>(StringComparer.Ordinal);

        foreach (var kv in context.Devices)
        {
            var id = kv.Key;

            var axis = context.GetDevice<SimulatorAxis>(id);
            if (axis != null)
            {
                snapshot[id] = axis.CurrentState.Position;
                continue;
            }

            var cyl = context.GetDevice<ISimulatorCylinder>(id);
            if (cyl != null)
            {
                // v2.1: 返回目标二值状态 (0/1)，由前端根据动作时间播放动画
                snapshot[id] = cyl.CurrentState.IsExtended ? 1.0 : 0.0;
                continue;
            }
        }

        return snapshot;
    }
}
