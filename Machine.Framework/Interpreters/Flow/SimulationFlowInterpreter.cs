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
        public async Task<object> RunAsync(StepDesc definition, FlowContext context)
        {
            if (definition == null) return null;
            if (context == null) throw new ArgumentNullException(nameof(context));

            // 1. 自动初始化 (如果设备列表为空，尝试根据配置发现)
            EnsureDevicesInitialized(context);

            // 2. 递归执行 AST
            return await ExecuteStepAsync(definition, context);
        }

        private async Task<object> ExecuteStepAsync(StepDesc step, FlowContext context)
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
                        MapStepDesc map => ExecuteMapAsync(map, context),
                        ScopeStepDesc scope => ExecuteStepAsync(scope.InnerStep, context),
                        _ => throw new NotSupportedException($"Unsupported step type: {step.GetType().Name}")
                    });
                }
                catch (Exception ex) when (attempts < maxRetries)
                {
                    attempts++;
                    Console.WriteLine($"[Flow] Step '{step.Name}' failed. Retrying ({attempts}/{maxRetries}). Error: {ex.Message}");
                    continue; 
                }
            }
        }

        private async Task<object> ExecuteActionAsync(ActionStepDesc action, FlowContext context)
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
                "Fire" => await HandleFireAsync(action, context),
                "ReadAnalog" => await HandleReadAnalogAsync(action, context),
                "CheckLevel" => await HandleCheckLevelAsync(action, context),
                _ => throw new NotSupportedException($"Operation '{action.Operation}' not implemented in Simulation.")
            };
        }

        private async Task<object> HandleMoveToAsync(ActionStepDesc action, FlowContext context)
        {
            var axis = context.GetDevice<SimulatorAxis>(action.TargetDevice);
            if (axis == null) throw new InvalidOperationException($"SimulatorAxis '{action.TargetDevice}' not found in context.");

            double targetPos = Convert.ToDouble(action.Args[0]);
            
            // 启动运动
            axis.StartMove(targetPos, axis.MaxSpeed);

            // 等待运动完成 (基于 Rx 观察状态流)
            await axis.StateStream
                .Where(s => !s.IsMoving)
                .FirstAsync()
                .ToTask(context.CancellationToken);

            return true;
        }

        private Task<object> HandleFireAsync(ActionStepDesc action, FlowContext context)
        {
            // 仿真气缸或 IO 暂时可以只记录日志或更新 Context 状态
            Console.WriteLine($"[Sim] Device '{action.TargetDevice}' fired with {action.Args[0]}");
            return Task.FromResult<object>(new Unit());
        }

        private Task<object> HandleReadAnalogAsync(ActionStepDesc action, FlowContext context)
        {
            // 如果 Context Variables 中预设了该值，则返回；否则返回默认值
            string key = $"MockValue_{action.TargetDevice}";
            if (context.Variables.TryGetValue(key, out var val)) return Task.FromResult(val);
            
            return Task.FromResult<object>(50.0); // 默认中位值
        }

        private Task<object> HandleCheckLevelAsync(ActionStepDesc action, FlowContext context)
        {
            bool expected = (bool)action.Args[0];
            return Task.FromResult<object>(expected); // 仿真默认满足条件
        }

        private async Task<object> ExecuteSystemActionAsync(ActionStepDesc action)
        {
            if (action.Operation == "Delay")
            {
                int ms = Convert.ToInt32(action.Args[0]);
                await Task.Delay(ms);
            }
            return new Unit();
        }

        private async Task<object> ExecuteSequenceAsync(SequenceStepDesc sequence, FlowContext context)
        {
            var firstResult = await ExecuteStepAsync(sequence.First, context);
            var nextStepDef = sequence.NextFactory(firstResult);
            var secondResult = await ExecuteStepAsync(nextStepDef, context);
            return sequence.ResultSelector(firstResult, secondResult);
        }

        private async Task<object> ExecuteMapAsync(MapStepDesc map, FlowContext context)
        {
            var sourceResult = await ExecuteStepAsync(map.Source, context);
            return map.Mapper(sourceResult);
        }

        private void EnsureDevicesInitialized(FlowContext context)
        {
            // 如果已经有设备，不再初始化
            if (!context.Devices.IsEmpty) return;

            // 遍历所有板卡配置
            foreach (var board in context.Config.BoardConfigs)
            {
                if (board is SimulatorBoardConfig simBoard)
                {
                    foreach (var pair in simBoard.AxisMappings)
                    {
                        string logicId = pair.Key;
                        // 获取轴配置 (如果有)
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
