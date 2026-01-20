using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using Machine.Framework.Core.Flow;
using Machine.Framework.Core.Flow.Dsl;
using Machine.Framework.Core.Simulation;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Visualization;

namespace Machine.Framework.Interpreters.Flow
{
    /// <summary>
    /// 仿真流程解释器。
    /// 负责将 DSL 步骤映射到仿真硬设备 (SimulatorAxis, SimulatorCylinder 等) 的行为。
    /// </summary>
    public class SimulationFlowInterpreter : IVisualFlowInterpreter
    {
        private readonly Subject<ActiveStepUpdate> _trace = new Subject<ActiveStepUpdate>();

        public IObservable<ActiveStepUpdate> TraceStream => _trace.AsObservable();

        public void InitializeDevices(FlowContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            EnsureDevicesInitialized(context);
        }

        public async Task<object?> RunAsync(StepDesc definition, FlowContext context)
        {
            if (definition == null) return null;
            if (context == null) throw new ArgumentNullException(nameof(context));

            // 1. 自动初始化 (如果设备列表为空，尝试根据配置发现)
            EnsureDevicesInitialized(context);

            // 2. 递归执行 AST
            return await ExecuteStepAsync(definition, context);
        }

        private static bool ShouldTrace(StepDesc step) => step is ActionStepDesc or ScopeStepDesc;

        private static string ResolveTraceTargetDevice(StepDesc step)
        {
            if (step is ActionStepDesc action) return action.TargetDevice;
            return "System";
        }

        private void EmitTrace(StepDesc step, StepStatus status)
        {
            if (!ShouldTrace(step)) return;
            _trace.OnNext(new ActiveStepUpdate(ResolveTraceTargetDevice(step), step.Name, status));
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

                    EmitTrace(step, StepStatus.Running);

                    var result = await (step switch
                    {
                        ActionStepDesc action => ExecuteActionAsync(action, context),
                        SequenceStepDesc sequence => ExecuteSequenceAsync(sequence, context),
                        ParallelStepDesc parallel => ExecuteParallelAsync(parallel, context),
                        MapStepDesc map => ExecuteMapAsync(map, context),
                        ScopeStepDesc scope => ExecuteStepAsync(scope.InnerStep, context),
                        _ => throw new NotSupportedException($"Unsupported step type: {step.GetType().Name}")
                    });

                    EmitTrace(step, StepStatus.Completed);
                    return result;
                }
                catch (OperationCanceledException)
                {
                    EmitTrace(step, StepStatus.Error);
                    throw; // 直接抛出取消异常，不触发重试
                }
                catch (Exception ex) when (attempts < maxRetries)
                {
                    attempts++;
                    EmitTrace(step, StepStatus.Error);
                    Console.WriteLine($"[Flow] Step '{step.Name}' failed. Retrying ({attempts}/{maxRetries}). Error: {ex.Message}");
                    continue; 
                }
                catch
                {
                    EmitTrace(step, StepStatus.Error);
                    throw;
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
            bool state = (bool)(action.Args?[0] ?? false);

            var cyl = context.GetDevice<ISimulatorCylinder>(action.TargetDevice);
            if (cyl != null)
            {
                var conf = context.GetDevice<CylinderConfig>(action.TargetDevice);
                var actionTime = conf?.MoveTime ?? 200;
                cyl.StartSet(state, actionTime);
                return Task.FromResult<object?>(new Unit());
            }

            var vac = context.GetDevice<ISimulatorVacuum>(action.TargetDevice);
            if (vac != null)
            {
                var conf = context.GetDevice<CylinderConfig>(action.TargetDevice);
                var actionTime = conf?.MoveTime ?? 50;
                vac.StartSet(state, actionTime);
                return Task.FromResult<object?>(new Unit());
            }

            Console.WriteLine($"[Sim] Device '{action.TargetDevice}' fired with {state}");
            return Task.FromResult<object?>(new Unit());
        }

        private async Task<object?> HandleFireAndWaitAsync(ActionStepDesc action, FlowContext context)
        {
            bool state = (bool)(action.Args?[0] ?? false);

            var cyl = context.GetDevice<ISimulatorCylinder>(action.TargetDevice);
            if (cyl != null)
            {
                var conf = context.GetDevice<CylinderConfig>(action.TargetDevice);
                var actionTime = conf?.MoveTime ?? 200;
                cyl.StartSet(state, actionTime);

                try
                {
                    await cyl.StateStream
                        .Where(s => !s.IsMoving && s.IsExtended == state)
                        .FirstAsync()
                        .ToTask(context.CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    cyl.Stop();
                    throw;
                }

                return new Unit();
            }

            var vac = context.GetDevice<ISimulatorVacuum>(action.TargetDevice);
            if (vac != null)
            {
                var conf = context.GetDevice<CylinderConfig>(action.TargetDevice);
                var actionTime = conf?.MoveTime ?? 50;
                vac.StartSet(state, actionTime);

                try
                {
                    await vac.StateStream
                        .Where(s => !s.IsChanging && s.IsOn == state)
                        .FirstAsync()
                        .ToTask(context.CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    vac.Stop();
                    throw;
                }

                return new Unit();
            }

            await Task.Delay(200, context.CancellationToken);
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

            var registeredAxes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var registeredCylinders = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var board in context.Config.BoardConfigs)
            {
                // 1) 气缸/真空：板级 IO 绑定 -> CylinderConfig + SimulatorCylinder/Vacuum
                foreach (var kv in board.CylinderMappings)
                {
                    var id = kv.Key;
                    var binding = kv.Value;

                    if (!registeredCylinders.Add(id))
                    {
                        throw new InvalidOperationException($"Cylinder '{id}' is mapped on multiple boards.");
                    }

                    var cylCfg = new CylinderConfig(id)
                        .Drive(binding.OutputPort);

                    if (binding.ExtendedSensorPort.HasValue && binding.RetractedSensorPort.HasValue)
                    {
                        cylCfg.WithSensors(binding.ExtendedSensorPort.Value, binding.RetractedSensorPort.Value);
                    }

                    if (context.Config.CylinderConfigs.TryGetValue(id, out var commonCylCfg))
                    {
                        cylCfg.MoveTime = commonCylCfg.MoveTime;
                        cylCfg.DefaultTimeoutMs = commonCylCfg.DefaultTimeoutMs;
                    }

                    context.RegisterDevice(id, cylCfg);

                    if (IsVacuumName(id))
                    {
                        context.RegisterDevice(id, new SimulatorVacuum(id));
                    }
                    else
                    {
                        context.RegisterDevice(id, new SimulatorCylinder(id));
                    }
                }

                // 2) 轴：仅对 Simulator 驱动创建仿真轴对象
                if (board.Driver is not SimulatorDriverConfig simDriver) continue;

                foreach (var pair in board.AxisMappings)
                {
                    string logicId = pair.Key;

                    if (!registeredAxes.Add(logicId))
                    {
                        throw new InvalidOperationException($"Axis '{logicId}' is mapped on multiple boards.");
                    }

                    context.Config.AxisConfigs.TryGetValue(logicId, out var commonAxisConfig);
                    simDriver.Axes.TryGetValue(logicId, out var physical);

                    double min = commonAxisConfig?.SoftLimits?.Min
                        ?? physical?.TravelMin
                        ?? 0;

                    double max = commonAxisConfig?.SoftLimits?.Max
                        ?? physical?.TravelMax
                        ?? 1000;

                    var axis = new SimulatorAxis(logicId, min, max, 200);
                    context.RegisterDevice(logicId, axis);
                }
            }
        }

        private static bool IsVacuumName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            return name.StartsWith("VAC", StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Vac", StringComparison.OrdinalIgnoreCase)
                || name.Contains("Vacuum", StringComparison.OrdinalIgnoreCase);
        }
    }
}
