using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;

namespace Machine.Framework.Core.Simulation
{
    public class SimulatorAxis : ISimulatorAxis
    {
        private readonly BehaviorSubject<AxisState> _state;
        private IDisposable? _movementSubscription;
        private readonly object _lock = new object();

        public string AxisId { get; }
        public double TravelMin { get; }
        public double TravelMax { get; }
        public double MaxSpeed { get; }

        public IObservable<AxisState> StateStream => _state.AsObservable();
        public AxisState CurrentState => _state.Value;

        public SimulatorAxis(string axisId, double min, double max, double maxSpeed)
        {
            AxisId = axisId;
            TravelMin = min;
            TravelMax = max;
            MaxSpeed = maxSpeed;

            // 初始状态
            var initial = new AxisState
            {
                Position = 0,
                CommandPos = 0,
                IsMoving = false,
                IsHomed = false,
                Speed = new AxisSpeed { Max = maxSpeed }
            };
            _state = new BehaviorSubject<AxisState>(initial);
        }

        public void SetLogicalPosition(double pos)
        {
            lock (_lock)
            {
                var current = _state.Value;
                _state.OnNext(current with { Position = pos, CommandPos = pos });
            }
        }

        public void StartMove(double targetPos, double speedVal)
        {
            lock (_lock)
            {
                _movementSubscription?.Dispose(); // 停止旧运动

                var current = _state.Value;
                if (Math.Abs(current.Position - targetPos) < 0.001)
                    return;

                double finalSpeed = Math.Min(speedVal, MaxSpeed);
                if (finalSpeed <= 0) finalSpeed = MaxSpeed * 0.1;

                // 更新状态为运动中
                _state.OnNext(current with { 
                    CommandPos = targetPos, 
                    IsMoving = true,
                    Speed = current.Speed // 简化处理
                });
                
                // 使用 Rx Interval 驱动运动
                _movementSubscription = Observable.Interval(TimeSpan.FromMilliseconds(10), TaskPoolScheduler.Default)
                    .Subscribe(_ => 
                    {
                        var s = _state.Value;
                        double step = finalSpeed * 0.01; // 10ms move
                        double dist = targetPos - s.Position;
                        
                        double newPos;
                        bool done = false;

                        if (Math.Abs(dist) <= step)
                        {
                            newPos = targetPos;
                            done = true;
                        }
                        else
                        {
                            newPos = s.Position + Math.Sign(dist) * step;
                        }

                        // 软限位检查
                        if (newPos > TravelMax) { newPos = TravelMax; done = true; }
                        if (newPos < TravelMin) { newPos = TravelMin; done = true; }

                        var newState = s with { 
                            Position = newPos, 
                            IsMoving = !done 
                        };
                        _state.OnNext(newState);

                        if (done)
                        {
                             _movementSubscription?.Dispose();
                        }
                    });
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _movementSubscription?.Dispose();
                var current = _state.Value;
                _state.OnNext(current with { IsMoving = false });
            }
        }
    }
}
