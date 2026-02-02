using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;

namespace Machine.Framework.Core.Simulation
{
    public class SimulatorAxis : ISimulatorAxis
    {
        private readonly BehaviorSubject<AxisState> _state;
        private CancellationTokenSource? _movementCts;
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
                _movementCts?.Cancel(); 
                _movementCts = new CancellationTokenSource();
                var token = _movementCts.Token;

                var current = _state.Value;
                if (Math.Abs(current.Position - targetPos) < 0.001)
                {
                    _state.OnNext(current with { IsMoving = false, Position = targetPos, CommandPos = targetPos });
                    return;
                }

                double finalSpeed = Math.Min(speedVal, MaxSpeed);
                if (finalSpeed <= 0) finalSpeed = MaxSpeed * 0.1;

                _state.OnNext(current with { 
                    CommandPos = targetPos, 
                    IsMoving = true
                });

                Task.Run(async () => 
                {
                    try 
                    {
                        while (!token.IsCancellationRequested)
                        {
                            await Task.Delay(10, token);
                            
                            lock (_lock)
                            {
                                var s = _state.Value;
                                double step = finalSpeed * 0.01; 
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

                                if (newPos > TravelMax) { newPos = TravelMax; done = true; }
                                if (newPos < TravelMin) { newPos = TravelMin; done = true; }

                                var newState = s with { 
                                    Position = newPos, 
                                    IsMoving = !done 
                                };
                                _state.OnNext(newState);

                                if (done) break;
                            }
                        }
                    }
                    catch (OperationCanceledException) { }
                }, token);
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _movementCts?.Cancel();
                var current = _state.Value;
                _state.OnNext(current with { IsMoving = false });
            }
        }
    }
}
