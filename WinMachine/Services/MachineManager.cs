using System;
using System.Reactive.Linq;
using Common.Lifecycle;
using Common.Fsm;
using Devices.Motion.Abstractions;

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

        public MachineManager(IMotionController<int, int, int> motion)
        {
            _motion = motion;

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

        private void DoConnect()
        {
            try
            {
                _motion.Initialization();
                // 模拟一个异步连接过程
                Observable.Timer(TimeSpan.FromSeconds(1))
                    .Subscribe(_ => _fsm.Fire(MachineTrigger.Connected));
            }
            catch (Exception)
            {
                _fsm.Fire(MachineTrigger.ErrorOccurred);
            }
        }

        private void DoHome()
        {
            try
            {
                // 假设所有轴回原点，这里简化逻辑
                _motion.GoBackHome(0); 
                
                // 使用 Rx 轮询检查回原点是否完成 (Poll Check)
                Observable.Interval(TimeSpan.FromMilliseconds(500))
                    .Where(_ => _motion.CheckHomeDone(0))
                    .FirstAsync()
                    .Subscribe(_ => _fsm.Fire(MachineTrigger.Homed));
            }
            catch
            {
                _fsm.Fire(MachineTrigger.ErrorOccurred);
            }
        }

        private void DoStop()
        {
            _motion.Stop(0);
            _fsm.Fire(MachineTrigger.Stopped);
        }

        #endregion

        public void Dispose()
        {
            _fsm?.Dispose();
        }
    }
}
