using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Machine.Framework.Core.Simulation
{
    public sealed class SimulatorCylinder : ISimulatorCylinder
    {
        private readonly BehaviorSubject<CylinderState> _state;
        private readonly object _lock = new object();
        private IDisposable? _subscription;

        public string CylinderId { get; }

        public IObservable<CylinderState> StateStream => _state.AsObservable();
        public CylinderState CurrentState => _state.Value;

        public SimulatorCylinder(string cylinderId)
        {
            CylinderId = cylinderId;
            _state = new BehaviorSubject<CylinderState>(new CylinderState { IsExtended = false, IsMoving = false, Position = 0.0 });
        }

        public void StartSet(bool extended, int actionTimeMs)
        {
            lock (_lock)
            {
                _subscription?.Dispose();

                var startState = _state.Value;
                if (startState.IsExtended == extended && !startState.IsMoving)
                    return;

                double targetPos = extended ? 1.0 : 0.0;
                double startPos = startState.Position;
                
                if (actionTimeMs <= 0)
                {
                    _state.OnNext(new CylinderState { IsExtended = extended, IsMoving = false, Position = targetPos });
                    return;
                }

                var startTime = DateTime.UtcNow;
                var interval = TimeSpan.FromMilliseconds(20); // 50fps

                _subscription = Observable.Interval(interval, TaskPoolScheduler.Default)
                    .Subscribe(_ =>
                    {
                        var elapsed = (DateTime.UtcNow - startTime).TotalMilliseconds;
                        var progress = Math.Clamp(elapsed / actionTimeMs, 0.0, 1.0);
                        var currentPos = startPos + (targetPos - startPos) * progress;

                        lock (_lock)
                        {
                            if (progress >= 1.0)
                            {
                                _state.OnNext(new CylinderState { IsExtended = extended, IsMoving = false, Position = targetPos });
                                _subscription?.Dispose();
                                _subscription = null;
                            }
                            else
                            {
                                _state.OnNext(new CylinderState { IsExtended = extended, IsMoving = true, Position = currentPos });
                            }
                        }
                    });
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _subscription?.Dispose();
                _subscription = null;

                var current = _state.Value;
                _state.OnNext(current with { IsMoving = false });
            }
        }
    }
}
