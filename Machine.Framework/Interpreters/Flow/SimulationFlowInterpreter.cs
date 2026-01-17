using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Configuration.Models;

namespace Machine.Framework.Interpreters.Flow
{
    /// <summary>
    /// 仿真流程解释器。
    /// 负责将 DSL 步骤映射到仿真硬设备 (SimulatorAxis, SimulatorCylinder 等) 的行为。
    /// </summary>
    public class SimulationFlowInterpreter : IFlowInterpreter
    {
        public async Task<object?> RunAsync(StepDesc definition, FlowContext context)
        {
            if (definition == null) return null;
            if (context == null) throw new ArgumentNullException(nameof(context));

            // 1. 自动初始化 (如果设备列表为空，尝试根据配置发现)
            EnsureDevicesInitialized(context);

            // 2. 递归执行 AST
            return await ExecuteStepAsync(definition, context);
        }

        private async Task<object?> ExecuteStepAsync(StepDesc step, FlowContext context)
        {
            // 策略：重试逻辑
            int attempts = 0;
            int maxRetries = step.Policy.RetryCount;

            while (true)
            {
                try
                {
                    // 检查取消请求
                    context.CancellationToken.ThrowIfCancellationRequested();

                    return await (step switch
                    {
                        ActionStepDesc action => ExecuteActionAsync(action, context),
                        SequenceStepDesc sequence => ExecuteSequenceAsync(sequence, context),
                        ParallelStepDesc parallel => ExecuteParallelAsync(parallel, context),
                        MapStepDesc map => ExecuteMapAsync(map, context),
                        ScopeStepDesc scope => ExecuteStepAsync(scope.InnerStep, context),
                        _ => throw new NotSupportedException($"Unsupported step type: {step.GetType().Name}")
                    });
                }
                catch (OperationCanceledException)
                {
                    throw; // 直接抛出取消异常，不触发重试
                }
                catch (Exception ex) when (attempts < maxRetries)
                {
                    attempts++;
                    Console.WriteLine($"[Flow] Step '{step.Name}' failed. Retrying ({attempts}/{maxRetries}). Error: {ex.Message}");
                    continue; 
                }
            }
        }

        private async Task<object?> ExecuteActionAsync(ActionStepDesc action, FlowContext context)
        {
            // 特殊系统操作
            if (action.TargetDevice == "System")
            {
                return await ExecuteSystemActionAsync(action);
            }

            // 硬件操作
            return action.Operation switch
            {
                "MoveTo" => await HandleMoveToAsync(action, context),
                "MoveToAndWait" => await HandleMoveToAndWaitAsync(action, context),
                "MoveUntil" => await HandleMoveUntilAsync(action, context),
                "Fire" => await HandleFireAsync(action, context),
                "FireAndWait" => await HandleFireAndWaitAsync(action, context),
                "ReadAnalog" => await HandleReadAnalogAsync(action, context),
                "CheckLevel" => await HandleCheckLevelAsync(action, context),
                _ => throw new NotSupportedException($"Operation '{action.Operation}' not implemented in Simulation.")
            };
        }

        // 非阻塞：仅启动
        private async Task<object?> HandleMoveToAsync(ActionStepDesc action, FlowContext context)
        {
            var axis = context.GetDevice<SimulatorAxis>(action.TargetDevice);
            if (axis == null) throw new InvalidOperationException($"SimulatorAxis '{action.TargetDevice}' not found.");

            double targetPos = Convert.ToDouble(action.Args[0]);
            axis.StartMove(targetPos, axis.MaxSpeed);
            
            // 为了确轴已经切换到 IsMoving = true 状态，稍微等一帧
            await Task.Delay(10);
            return true;
        }

        // 阻塞：启动并等待
        private async Task<object?> HandleMoveToAndWaitAsync(ActionStepDesc action, FlowContext context)
        {
            var axis = context.GetDevice<SimulatorAxis>(action.TargetDevice);
            if (axis == null) throw new InvalidOperationException($"SimulatorAxis '{action.TargetDevice}' not found.");

            double targetPos = Convert.ToDouble(action.Args[0]);
            axis.StartMove(targetPos, axis.MaxSpeed);

            try
            {
                await axis.StateStream
                    .Where(s => !s.IsMoving)
                    .FirstAsync()
                    .ToTask(context.CancellationToken);
            }
            catch (OperationCanceledException)
            {
                axis.Stop();
                throw;
            }

            return true;
        }

        // 寻碰逻辑：移动直到传感器报错或到达极限
        private async Task<object?> HandleMoveUntilAsync(ActionStepDesc action, FlowContext context)
        {
            var axis = context.GetDevice<SimulatorAxis>(action.TargetDevice);
            if (axis == null) throw new InvalidOperationException($"SimulatorAxis '{action.TargetDevice}' not found.");

            double targetPos = Convert.ToDouble(action.Args[0]);
            string sensorId = (string)action.Args[1];
            double threshold = Convert.ToDouble(action.Args[2]);

            axis.StartMove(targetPos, axis.MaxSpeed * 0.5); // 寻碰通常慢一点

            try
            {
                // 定义一个观察流：监听位置变化，同时注入压力升高仿真逻辑
                var result = await axis.StateStream
                    .Select(s => 
                    {
                        // 仿真压力：假设在 100mm 处接触物料，之后压力随深度直线升高 (系数 0.5)
                        double contactPos = 100.0;
                        double simulatedPressure = s.Position > contactPos ? (s.Position - contactPos) * 0.5 : 0;
                        context.Variables[$"MockValue_{sensorId}"] = simulatedPressure;
                        
                        return new { State = s, Pressure = simulatedPressure };
                    })
                    .Where(x => !x.State.IsMoving || x.Pressure >= threshold)
                    .FirstAsync()
                    .ToTask(context.CancellationToken);

                // 如果是因为压力到达阈值而停止
                if (result.Pressure >= threshold)
                {
                    axis.Stop();
                    Console.WriteLine($"[Sim] MoveUntil reached threshold: {result.Pressure} at pos {result.State.Position}");
                }

                return result.State.Position;
            }
            catch (OperationCanceledException)
            {
                axis.Stop();
                throw;
            }
        }

        private Task<object?> HandleFireAsync(ActionStepDesc action, FlowContext context)
        {
            Console.WriteLine($"[Sim] Device '{action.TargetDevice}' fired with {action.Args?[0] ?? "null"}");
            return Task.FromResult<object?>(new Unit());
        }

        private async Task<object?> HandleFireAndWaitAsync(ActionStepDesc action, FlowContext context)
        {
            bool state = (bool)(action.Args?[0] ?? false);
            Console.WriteLine($"[Sim] Device '{action.TargetDevice}' FireAndWait start: {state}");
            
            await Task.Delay(200, context.CancellationToken);
            
            Console.WriteLine($"[Sim] Device '{action.TargetDevice}' FireAndWait finished.");
            return new Unit();
        }

        private Task<object?> HandleReadAnalogAsync(ActionStepDesc action, FlowContext context)
        {
            string key = $"MockValue_{action.TargetDevice}";
            if (context.Variables.TryGetValue(key, out var val)) return Task.FromResult<object?>(val);
            
            return Task.FromResult<object?>(0.0);
        }

        private Task<object?> HandleCheckLevelAsync(ActionStepDesc action, FlowContext context)
        {
            bool expected = (bool)(action.Args?[0] ?? false);
            return Task.FromResult<object?>(expected);
        }

        private async Task<object?> ExecuteSystemActionAsync(ActionStepDesc action)
        {
            if (action.Operation == "Delay")
            {
                int ms = Convert.ToInt32(action.Args?[0] ?? 0);
                await Task.Delay(ms);
            }
            if (action.Operation == "Throw")
            {
                throw new Exception(action.Args?[0]?.ToString() ?? "Unknown error");
            }
            if (action.Operation == "NoOp")
            {
                return new Unit();
            }
            return new Unit();
        }

        private async Task<object?> ExecuteSequenceAsync(SequenceStepDesc sequence, FlowContext context)
        {
            var firstResult = await ExecuteStepAsync(sequence.First, context);
            var nextStepDef = sequence.NextFactory(firstResult!);
            var secondResult = await ExecuteStepAsync(nextStepDef, context);
            return sequence.ResultSelector(firstResult!, secondResult!);
        }

        private async Task<object?> ExecuteMapAsync(MapStepDesc map, FlowContext context)
        {
            var sourceResult = await ExecuteStepAsync(map.Source, context);
            return map.Mapper(sourceResult!);
        }

        private async Task<object?> ExecuteParallelAsync(ParallelStepDesc parallel, FlowContext context)
        {
            var tasks = parallel.Steps.Select(s => ExecuteStepAsync(s, context));
            return await Task.WhenAll(tasks);
        }

        private void EnsureDevicesInitialized(FlowContext context)
        {
            if (!context.Devices.IsEmpty) return;

            foreach (var cyl in context.Config.CylinderConfigs)
            {
                context.RegisterDevice(cyl.Name, cyl);
            }
            foreach (var board in context.Config.BoardConfigs)
            {
                if (board is SimulatorBoardConfig simBoard)
                {
                    foreach (var pair in simBoard.AxisMappings)
                    {
                        string logicId = pair.Key;
                        simBoard.AxisConfigs.TryGetValue(logicId, out var axisConfig);
                        
                        double min = axisConfig?.SoftLimits?.Min ?? 0;
                        double max = axisConfig?.SoftLimits?.Max ?? 1000;

                        var axis = new SimulatorAxis(logicId, min, max, 200);
                        context.RegisterDevice(logicId, axis);
                    }
                }
            }
        }
    }
}
