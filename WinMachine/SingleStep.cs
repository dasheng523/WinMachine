using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Machine.Framework.Core.Core;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Steps;
using LanguageExt;
using Microsoft.Extensions.Options;
using Machine.Framework.Configuration;
using WinMachine.Services;
using Machine.Framework.Runtime;
using static LanguageExt.Prelude;

namespace WinMachine
{
    public partial class SingleStep : Form
    {
        private readonly IAxisResolver _axes;
        private readonly IHardware _hw;
        private readonly IOptionsMonitor<SingleStepOptions> _options;

        public SingleStep(IAxisResolver axes, IHardware hw, IOptionsMonitor<SingleStepOptions> options)
        {
            _axes = axes;
            _hw = hw;
            _options = options;

            InitializeComponent();
            BtnLoadPen1Pick.Click += (_, _) => RunLoadPen1Pick();
        }

        private void RunLoadPen1Pick()
        {
            BtnLoadPen1Pick.Enabled = false;

            var o = _options.CurrentValue.LoadPen1Pick;
            var decisionProvider = new WinFormsDecisionProvider(this);
            var ctx = StepContext.Create(decisionProvider);

            var step = BuildLoadPen1PickStep(o);

            var ui = new SynchronizationContextScheduler(SynchronizationContext.Current ?? new WindowsFormsSynchronizationContext());

            step.Run(ctx)
                .ObserveOn(ui)
                .Finally(() => BtnLoadPen1Pick.Enabled = true)
                .Subscribe(outcome =>
                {
                    _ = outcome.Value.Match(
                        Succ: _ =>
                        {
                            if (outcome.Status == StepStatus.Succeeded)
                            {
                                MessageBox.Show(this, "上料笔1取料：成功", "单步测试");
                            }
                            else if (outcome.Status == StepStatus.Skipped)
                            {
                                MessageBox.Show(this, "上料笔1取料：已跳过（按决策）", "单步测试");
                            }

                            return unit;
                        },
                        Fail: e =>
                        {
                            var title = outcome.Status switch
                            {
                                StepStatus.Aborted => "上料笔1取料：已中止",
                                StepStatus.Failed => "上料笔1取料：失败",
                                _ => "上料笔1取料：失败"
                            };

                            MessageBox.Show(this, e.Message, title);
                            return unit;
                        });
                });
        }

        private Step<Unit> BuildLoadPen1PickStep(LoadPen1PickOptions o)
        {
            var speedXY = o.SpeedXY.ToAxisSpeed();
            var speedZ = o.SpeedZ.ToAxisSpeed();

            return Step.Named("上料笔1取料",
                from _ in MoveAbsAndWait("X", o.XPos, speedXY, o.MoveTimeoutMs, o.PollMs)
                from __ in MoveAbsAndWait("Y2", o.Y2Pos, speedXY, o.MoveTimeoutMs, o.PollMs)
                from ___ in MoveAbsAndWait("Z1", o.Z1DownPos, speedZ, o.MoveTimeoutMs, o.PollMs)
                from ____ in SetDo(o.VacuumDo, Level.On)
                from _____ in MoveAbsAndWait("Z1", o.Z1SafePos, speedZ, o.MoveTimeoutMs, o.PollMs)
                from ______ in EnsureSensorOn(o.PressureOkSensor)
                select unit);
        }

        private Step<Unit> MoveAbsAndWait(string axisName, double pos, Machine.Framework.Devices.Motion.Abstractions.AxisSpeed speed, int timeoutMs, int pollMs)
        {
            return Step.Named($"轴 {axisName} -> {pos}",
                from _ in Effect($"SetSpeed({axisName})", () =>
                    _axes.Resolve(axisName)
                        .Bind(t => t.Controller.SetSpeed(t.Axis, speed)))
                from __ in Effect($"MoveAbs({axisName}, {pos})", () =>
                    _axes.Resolve(axisName)
                        .Bind(t => t.Controller.Move_Absolute(t.Axis, pos)))
                from ___ in Effect($"WaitDone({axisName})", () => WaitAxisDone(axisName, timeoutMs, pollMs))
                select unit);
        }

        private Step<Unit> SetDo(string doName, Level level) =>
            Effect($"DO {doName} = {level}", () =>
                _hw.DOs.Resolve(doName).Bind(@do => @do.Write(level)));

        private Step<Unit> EnsureSensorOn(string sensorName) =>
            Effect($"检查 {sensorName} = ON", () =>
                _hw.LevelSensors.Resolve(sensorName)
                    .Bind(s => s.Read())
                    .Bind(l => l == Level.On
                        ? FinSucc(unit)
                        : FinFail<Unit>(LanguageExt.Machine.Framework.Core.Error.New($"传感器 {sensorName} 未到高电平（当前={l}）"))));

        private Step<Unit> Effect(string name, Func<Fin<Unit>> f) =>
            Step.Effect(name, _ => Observable.Start(f, Scheduler.Default));

        private Fin<Unit> WaitAxisDone(string axisName, int timeoutMs, int pollMs)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                var r = _axes.Resolve(axisName)
                    .Bind(t => t.Controller.CheckDone(t.Axis));

                var fin = r.Match(
                    Succ: done => done ? FinSucc(unit) : FinFail<Unit>(LanguageExt.Machine.Framework.Core.Error.New("NOT_DONE")),
                    Fail: e => FinFail<Unit>(e));

                if (fin.IsSucc)
                {
                    return fin;
                }

                // NOT_DONE 继续等待；其他错误直接返回。
                if (fin.IsFail && fin.Match(Succ: _ => false, Fail: e => e.Message != "NOT_DONE"))
                {
                    return fin;
                }

                Thread.Sleep(pollMs);
            }

            return FinFail<Unit>(LanguageExt.Machine.Framework.Core.Error.New($"等待轴 {axisName} 完成超时（{timeoutMs}ms）"));
        }

        private sealed class WinFormsDecisionProvider : IStepDecisionProvider
        {
            private readonly IWin32Window _owner;

            public WinFormsDecisionProvider(IWin32Window owner) => _owner = owner;

            public IObservable<StepDecision> Decide(StepFailure failure)
            {
                return Observable.Start(() =>
                {
                    var canRetry = failure.OnError.CanRetry;
                    var canSkip = failure.OnError.CanSkip;

                    var msg = $"步骤失败：{failure.Name}\n\n错误：{failure.Error.Message}\n\n第 {failure.Attempt} 次尝试\n\n" +
                              $"可选：{(canRetry ? "重试 " : string.Empty)}{(canSkip ? "跳过 " : string.Empty)}中止";

                    // 这里用最朴素的三选一：Yes=Retry, No=Skip, Cancel=Abort。
                    // 若某个选项不允许，则自动落到 Abort。
                    var buttons = MessageBoxButtons.YesNoCancel;
                    var dr = MessageBox.Show(_owner, msg, "单步测试 - 决策", buttons, MessageBoxIcon.Warning);

                    return dr switch
                    {
                        DialogResult.Yes when canRetry => StepDecision.Retry,
                        DialogResult.No when canSkip => StepDecision.Skip,
                        _ => StepDecision.Abort
                    };
                }, Scheduler.Default);
            }
        }
    }
}


