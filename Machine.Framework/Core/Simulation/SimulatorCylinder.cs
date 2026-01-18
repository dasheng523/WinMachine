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
            _state = new BehaviorSubject<CylinderState>(new CylinderState { IsExtended = false, IsMoving = false });
        }

        public void StartSet(bool extended, int actionTimeMs)
        {
            lock (_lock)
            {
                _subscription?.Dispose();

                var current = _state.Value;
                if (current.IsExtended == extended && !current.IsMoving)
                    return;

                _state.OnNext(current with { IsMoving = true });

                var delay = TimeSpan.FromMilliseconds(Math.Max(0, actionTimeMs));
                _subscription = Observable.Timer(delay, TaskPoolScheduler.Default)
                    .Subscribe(_ =>
                    {
                        lock (_lock)
                        {
                            var s = _state.Value;
                            _state.OnNext(s with { IsExtended = extended, IsMoving = false });
                            _subscription?.Dispose();
                            _subscription = null;
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
