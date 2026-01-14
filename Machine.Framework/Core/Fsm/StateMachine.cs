using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Machine.Framework.Core.Lifecycle;

namespace Machine.Framework.Core.Fsm
{
    public class StateMachine : IDisposable
    {
        private readonly BehaviorSubject<MachineState> _stateSubject;
        private readonly Subject<MachineTrigger> _triggerSubject;
        private readonly Dictionary<(MachineState, MachineTrigger), MachineState> _transitions;
        private readonly Dictionary<MachineState, Action> _entryActions;
        private readonly IDisposable _subscription;

        internal StateMachine(
            MachineState initialState,
            Dictionary<(MachineState, MachineTrigger), MachineState> transitions,
            Dictionary<MachineState, Action> entryActions)
        {
            _stateSubject = new BehaviorSubject<MachineState>(initialState);
            _triggerSubject = new Subject<MachineTrigger>();
            _transitions = transitions;
            _entryActions = entryActions;

            _subscription = _triggerSubject
                .Select(trigger => (Trigger: trigger, Current: _stateSubject.Value))
                .Select(x => _transitions.TryGetValue((x.Current, x.Trigger), out var next) ? next : x.Current)
                .Where(next => next != _stateSubject.Value)
                .Subscribe(next =>
                {
                    _stateSubject.OnNext(next);
                    if (_entryActions.TryGetValue(next, out var action)) action?.Invoke();
                });
        }

        public IObservable<MachineState> State => _stateSubject.AsObservable();
        public MachineState Current => _stateSubject.Value;
        public void Fire(MachineTrigger trigger) => _triggerSubject.OnNext(trigger);

        public void Dispose()
        {
            _subscription?.Dispose();
            _stateSubject?.Dispose();
            _triggerSubject?.Dispose();
        }
    }

    public class StateMachineBuilder
    {
        private MachineState _initialState = MachineState.Initial;
        private readonly Dictionary<(MachineState, MachineTrigger), MachineState> _transitions = new();
        private readonly Dictionary<MachineState, Action> _entryActions = new();

        public StateMachineBuilder InitialState(MachineState state) { _initialState = state; return this; }

        public StateMachineBuilder In(MachineState state, Action<StateConfiguration> config)
        {
            var stateConfig = new StateConfiguration(state, _transitions);
            config(stateConfig);
            return this;
        }

        public StateMachineBuilder WhenEntering(MachineState state, Action action)
        {
            _entryActions[state] = action;
            return this;
        }

        public StateMachine Build() => new StateMachine(_initialState, _transitions, _entryActions);

        public class StateConfiguration
        {
            private readonly MachineState _sourceState;
            private readonly Dictionary<(MachineState, MachineTrigger), MachineState> _transitions;

            public StateConfiguration(MachineState source, Dictionary<(MachineState, MachineTrigger), MachineState> transitions)
            {
                _sourceState = source;
                _transitions = transitions;
            }

            public StateConfiguration On(MachineTrigger trigger, MachineState target)
            {
                _transitions[(_sourceState, trigger)] = target;
                return this;
            }
        }
    }
}


