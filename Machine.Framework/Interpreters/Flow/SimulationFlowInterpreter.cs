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
using Machine.Framework.Telemetry.Contracts;

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
                        LoopStepDesc loop => ExecuteLoopAsync(loop, context),
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
                return await ExecuteSystemActionAsync(action, context);
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
                "CylinderWaitFor" => await HandleCylinderWaitForAsync(action, context),
                _ => throw new NotSupportedException($"Operation '{action.Operation}' not implemented in Simulation.")
            };
        }

        // 非阻塞：仅启动
        private async Task<object?> HandleMoveToAsync(ActionStepDesc action, FlowContext context)
        {
            var axis = context.GetDevice<SimulatorAxis>(action.TargetDevice);
            if (axis == null) throw new InvalidOperationException($"SimulatorAxis '{action.TargetDevice}' not found.");

            double targetPos;
            if (action.Args[0] is Func<double, double> selector)
            {
                targetPos = selector(axis.CurrentState.Position);
            }
            else
            {
                targetPos = Convert.ToDouble(action.Args[0]);
            }

            axis.StartMove(targetPos, axis.MaxSpeed);
            
            // 为了确轴已经切换到 IsMoving = true 状态，稍微等一帧
            await Task.Delay(10);
            return new Unit();
        }

        // 阻塞：启动并等待
        private async Task<object?> HandleMoveToAndWaitAsync(ActionStepDesc action, FlowContext context)
        {
            var axis = context.GetDevice<SimulatorAxis>(action.TargetDevice);
            if (axis == null) throw new InvalidOperationException($"SimulatorAxis '{action.TargetDevice}' not found.");

            double targetPos;
            if (action.Args[0] is Func<double, double> selector)
            {
                targetPos = selector(axis.CurrentState.Position);
            }
            else
            {
                targetPos = Convert.ToDouble(action.Args[0]);
            }

            axis.StartMove(targetPos, axis.MaxSpeed);

            try
            {
                await axis.StateStream
                    .Where(s => !s.IsMoving)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(30)) // Safety: prevent infinite wait
                    .ToTask(context.CancellationToken);
            }
            catch (TimeoutException)
            {
                Console.WriteLine($"[SimWarning] MoveToAndWait timed out for '{action.TargetDevice}'. Forcing stop.");
                axis.Stop(); // Ensure it stops
            }
            catch (OperationCanceledException)
            {
                axis.Stop();
                throw;
            }

            return new Unit();
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
                        .Timeout(TimeSpan.FromSeconds(30))
                        .ToTask(context.CancellationToken);
                }
                catch (TimeoutException)
                {
                     Console.WriteLine($"[SimWarning] FireAndWait timed out for '{action.TargetDevice}' (State={state}). Continuing...");
                     // Recalibrate state to ensure it doesn't stay stuck
                     cyl.Stop();
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
                        .Timeout(TimeSpan.FromSeconds(30)) 
                        .ToTask(context.CancellationToken);
                }
                catch (TimeoutException) 
                {
                     Console.WriteLine($"[SimWarning] Vacuum Wait timed out for '{action.TargetDevice}'.");
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

        private async Task<object?> HandleCylinderWaitForAsync(ActionStepDesc action, FlowContext context)
        {
            bool state = (bool)(action.Args?[0] ?? false);
            var device = context.GetDevice<object>(action.TargetDevice);
            
            if (device is ISimulatorCylinder cyl) 
            {
                // 如果已经在位，直接返回
                if (!cyl.CurrentState.IsMoving && cyl.CurrentState.IsExtended == state) 
                    return new Unit();

                await cyl.StateStream
                    .Where(s => !s.IsMoving && s.IsExtended == state)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(30)) // 互锁等待通常允许较长时间，或者直到超时报错
                    .ToTask(context.CancellationToken);
            }
            else if (device is ISimulatorVacuum vac)
            {
                if (!vac.CurrentState.IsChanging && vac.CurrentState.IsOn == state) 
                    return new Unit();

                await vac.StateStream
                    .Where(s => !s.IsChanging && s.IsOn == state)
                    .FirstAsync()
                    .Timeout(TimeSpan.FromSeconds(30))
                    .ToTask(context.CancellationToken);
            }
            else 
            {
                // Fallback for non-simulated or unknown devices: just pass.
                // In a real generic interpreter, we might check IO inputs here.
            }

            return new Unit();
        }

        private async Task<object?> ExecuteSystemActionAsync(ActionStepDesc action, FlowContext context)
        {
            if (action.Operation.StartsWith("Material"))
            {
                return await HandleMaterialOpAsync(action, context);
            }

            if (action.Operation == "Delay")
            {
                int ms = Convert.ToInt32(action.Args?[0] ?? 0);
                await Task.Delay(ms, context.CancellationToken);
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

        private Task<object?> HandleMaterialOpAsync(ActionStepDesc action, FlowContext context)
        {
            string op = action.Operation;
            string station = (string)(action.Args?[0] ?? "");
            
            if (op == "MaterialCheckState")
            {
                if (context.MaterialStates.TryGetValue(station, out var info)) return Task.FromResult<object?>(info.Class);
                return Task.FromResult<object?>("Empty");
            }

            if (op == "MaterialSpawn")
            {
                string id = (string)action.Args![1];
                string cls = (string)action.Args![2];
                var info = new MaterialInfo { Id = id, Class = cls };
                
                context.MaterialStates[station] = info;
                context.EventStream.OnNext(new TelemetryEvent 
                { 
                    Type = EventType.MaterialSpawn, 
                    Payload = new MaterialEventPayload { Id = id, AtStation = station, Class = cls } 
                });
                return Task.FromResult<object?>(new Unit());
            }

            if (op == "MaterialTransform")
            {
                string newCls = (string)action.Args![1];
                if (context.MaterialStates.TryGetValue(station, out var info))
                {
                    info.Class = newCls; // Update locally
                    // Emit event
                    context.EventStream.OnNext(new TelemetryEvent
                    {
                        Type = EventType.MaterialTransform,
                        Payload = new MaterialEventPayload { Id = info.Id, ToClass = newCls }
                    });
                }
                return Task.FromResult<object?>(new Unit());
            }

            if (op == "MaterialConsume")
            {
                if (context.MaterialStates.TryRemove(station, out var info))
                {
                    context.EventStream.OnNext(new TelemetryEvent
                    {
                        Type = EventType.MaterialConsume,
                        Payload = new MaterialEventPayload { Id = info.Id }
                    });
                }
                return Task.FromResult<object?>(new Unit());
            }

            if (op == "MaterialBind")
            {
                string id = (string)action.Args![1];
                string cls = (string)action.Args![2];
                context.MaterialStates[station] = new MaterialInfo { Id = id, Class = cls };
                // Bind 只是纯状态同步，不需要发 Event，因为这是“逻辑移动”的结果
                return Task.FromResult<object?>(new Unit());
            }

            if (op == "MaterialUnbind")
            {
                context.MaterialStates.TryRemove(station, out _);
                return Task.FromResult<object?>(new Unit());
            }

            if (op == "MaterialAttach")
            {
                string parent = (string)action.Args![1];
                string child = (string)action.Args![2];
                // Emit Attach Event
                context.EventStream.OnNext(new TelemetryEvent 
                { 
                    Type = EventType.Attach, 
                    Payload = new MaterialEventPayload { AtStation = station, ParentId = parent, ChildId = child } 
                });
                return Task.FromResult<object?>(new Unit());
            }

            if (op == "MaterialDetach")
            {
                // Emit Detach Event
                context.EventStream.OnNext(new TelemetryEvent 
                { 
                    Type = EventType.Detach, 
                    Payload = new MaterialEventPayload { AtStation = station } 
                });
                return Task.FromResult<object?>(new Unit());
            }
            
            return Task.FromResult<object?>(new Unit());
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

        private async Task<object?> ExecuteLoopAsync(LoopStepDesc loop, FlowContext context)
        {
            object? result = null;
            int count = 0;
            
            while (loop.Count == -1 || count < loop.Count)
            {
                result = await ExecuteStepAsync(loop.InnerStep, context);
                count++;
                
                // 给系统一点喘息时间，防止完全阻塞
                await Task.Yield();
            }
            
            return result;
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

                    var axis = new SimulatorAxis(logicId, min, max, 2000);
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
