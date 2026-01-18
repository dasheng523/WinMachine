using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Machine.Framework.Core.Simulation
{
    public sealed class SimulatorVacuum : ISimulatorVacuum
    {
        private readonly BehaviorSubject<VacuumState> _state;
        private readonly object _lock = new object();
        private IDisposable? _subscription;

        public string VacuumId { get; }

        public IObservable<VacuumState> StateStream => _state.AsObservable();
        public VacuumState CurrentState => _state.Value;

        public SimulatorVacuum(string vacuumId)
        {
            VacuumId = vacuumId;
            _state = new BehaviorSubject<VacuumState>(new VacuumState { IsOn = false, IsChanging = false });
        }

        public void StartSet(bool on, int actionTimeMs)
        {
            lock (_lock)
            {
                _subscription?.Dispose();

                var current = _state.Value;
                if (current.IsOn == on && !current.IsChanging)
                    return;

                _state.OnNext(current with { IsChanging = true });

                var delay = TimeSpan.FromMilliseconds(Math.Max(0, actionTimeMs));
                _subscription = Observable.Timer(delay, TaskPoolScheduler.Default)
                    .Subscribe(_ =>
                    {
                        lock (_lock)
                        {
                            var s = _state.Value;
                            _state.OnNext(s with { IsOn = on, IsChanging = false });
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
                _state.OnNext(current with { IsChanging = false });
            }
        }
    }
}
