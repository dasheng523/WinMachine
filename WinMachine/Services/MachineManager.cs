using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Common.Lifecycle;
using Common.Fsm;
using Devices.Motion.Abstractions;
using LanguageExt;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace WinMachine.Services
{
    /// <summary>
    /// 机器管理服务接口
    /// </summary>
    public interface IMachineService : IDisposable
    {
        IObservable<MachineState> State { get; }
        MachineState CurrentState { get; }
        
        void Connect();
        void Start();
        void Stop();
        void Home();
        void Reset();
    }

    /// <summary>
    /// 机器管理器：将状态机与硬件驱动缝合在一起
    /// </summary>
    public class MachineManager : IMachineService
    {
        private readonly IMotionController<int, int, int> _motion;
        private readonly StateMachine _fsm;
        private readonly IScheduler _scheduler;

        public MachineManager(IMotionController<int, int, int> motion)
            : this(motion, scheduler: null)
        {
        }

        public MachineManager(IMotionController<int, int, int> motion, IScheduler? scheduler)
        {
            _motion = motion;
            _scheduler = scheduler ?? Scheduler.Default;

            // 1. 配置状态机 DSL
            _fsm = new StateMachineBuilder()
                .InitialState(MachineState.Initial)
                
                // 状态转换规则
                .In(MachineState.Initial, s => s.On(MachineTrigger.Connect, MachineState.Initializing))
                .In(MachineState.Initializing, s => s.On(MachineTrigger.Connected, MachineState.WaitHome))
                .In(MachineState.WaitHome, s => s.On(MachineTrigger.Home, MachineState.Homing))
                .In(MachineState.Homing, s => s.On(MachineTrigger.Homed, MachineState.Idle))
                .In(MachineState.Idle, s => s.On(MachineTrigger.Start, MachineState.Running)
                                             .On(MachineTrigger.Home, MachineState.Homing))
                .In(MachineState.Running, s => s.On(MachineTrigger.Stop, MachineState.Stopping)
                                                .On(MachineTrigger.ErrorOccurred, MachineState.Faulted))
                .In(MachineState.Stopping, s => s.On(MachineTrigger.Stopped, MachineState.Idle))
                .In(MachineState.Faulted, s => s.On(MachineTrigger.Reset, MachineState.Initial))

                // 副作用行为 (The "Monad" side effects)
                .WhenEntering(MachineState.Initializing, DoConnect)
                .WhenEntering(MachineState.Homing, DoHome)
                .WhenEntering(MachineState.Running, () => Console.WriteLine("逻辑：启动业务循环"))
                .WhenEntering(MachineState.Stopping, DoStop)
                .Build();
        }

        public IObservable<MachineState> State => _fsm.State;
        public MachineState CurrentState => _fsm.Current;

        // 命令输入
        public void Connect() => _fsm.Fire(MachineTrigger.Connect);
        public void Start() => _fsm.Fire(MachineTrigger.Start);
        public void Stop() => _fsm.Fire(MachineTrigger.Stop);
        public void Home() => _fsm.Fire(MachineTrigger.Home);
        public void Reset() => _fsm.Fire(MachineTrigger.Reset);

        #region 硬件动作实现 (Private Actions)

        private Fin<LUnit> ConnectFlow() =>
            from _ in _motion.Initialization()
            select unit;

        private Fin<LUnit> HomeFlow(int axis) =>
            from _ in _motion.GoBackHome(axis)
            select unit;

        private void DoConnect()
        {
            _ = ConnectFlow().Match(
                Succ: _ =>
                {
                    // 模拟一个异步连接过程
                    Observable.Timer(TimeSpan.FromSeconds(1), _scheduler)
                        .Subscribe(__ => _fsm.Fire(MachineTrigger.Connected));
                    return unit;
                },
                Fail: _ =>
                {
                    _fsm.Fire(MachineTrigger.ErrorOccurred);
                    return unit;
                });
        }

        private void DoHome()
        {
            const int axis = 0;

            _ = HomeFlow(axis).Match(
                Succ: _ =>
                {
                    IDisposable? subscription = null;
                    subscription = Observable.Interval(TimeSpan.FromMilliseconds(500), _scheduler)
                        .Select(__ => _motion.CheckHomeDone(axis))
                        .Subscribe(r =>
                            r.Match(
                                Succ: done =>
                                {
                                    if (done)
                                    {
                                        subscription?.Dispose();
                                        _fsm.Fire(MachineTrigger.Homed);
                                    }

                                    return unit;
                                },
                                Fail: _ =>
                                {
                                    subscription?.Dispose();
                                    _fsm.Fire(MachineTrigger.ErrorOccurred);
                                    return unit;
                                }));

                    return unit;
                },
                Fail: _ =>
                {
                    _fsm.Fire(MachineTrigger.ErrorOccurred);
                    return unit;
                });
        }

        private void DoStop()
        {
            _ = _motion.Stop(0).Match(
                Succ: _ =>
                {
                    _fsm.Fire(MachineTrigger.Stopped);
                    return unit;
                },
                Fail: _ =>
                {
                    _fsm.Fire(MachineTrigger.ErrorOccurred);
                    return unit;
                });
        }

        #endregion

        public void Dispose()
        {
            _fsm?.Dispose();
        }
    }
}
