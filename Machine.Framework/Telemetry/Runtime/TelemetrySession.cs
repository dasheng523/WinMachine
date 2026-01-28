using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Machine.Framework.Core.Flow;
using Machine.Framework.Telemetry.Contracts;
using Machine.Framework.Visualization;

namespace Machine.Framework.Telemetry.Runtime;

public sealed class TelemetrySession : IDisposable
{
    private static readonly string[] InternalStepPrefixes =
    [
        "Delay_"
    ];

    private static readonly HashSet<string> InternalStepNames = new(StringComparer.Ordinal)
    {
        "Start",
        "WherePass",
        "ThrowException"
    };

    private readonly FlowContext _context;
    private readonly MonotonicUnixClock _clock;
    private readonly TelemetryDeltaFilter _delta;
    private readonly BehaviorSubject<TelemetryPacket> _subject;
    private readonly ConcurrentQueue<TelemetryEvent> _events = new();
    private readonly IDisposable _traceSub;
    private readonly IDisposable _timerSub;
    private readonly IDisposable _eventSub;

    private volatile string _currentStep = "";
    private string _lastSentStep = "";
    private volatile bool _forceSnapshot = true;

    public TelemetrySession(FlowContext context, IObservable<ActiveStepUpdate> traceStream, TimeSpan interval, double epsilon = 0.001)
    {
        _context = context;
        _clock = new MonotonicUnixClock();
        _delta = new TelemetryDeltaFilter(epsilon);

        _subject = new BehaviorSubject<TelemetryPacket>(new TelemetryPacket { Tick = _clock.Now(), Step = "" });

        _traceSub = traceStream.Subscribe(OnTrace);
        _eventSub = context.EventStream.Subscribe(Enqueue);
        _timerSub = Observable.Interval(interval).Subscribe(_ => PublishFrame());
    }

    public IObservable<TelemetryPacket> Stream => _subject.AsObservable();

    public void Enqueue(TelemetryEvent telemetryEvent) => _events.Enqueue(telemetryEvent);

    public void ForceSnapshot() => _forceSnapshot = true;

    private void OnTrace(ActiveStepUpdate update)
    {
        if (update.Status != StepStatus.Running) return;
        
        // Remove the filter that ignores non-System devices. 
        // We want to see hardware actions (MoveTo, Fire, etc) in the UI step display.
        // if (!string.Equals(update.TargetDevice, "System", StringComparison.OrdinalIgnoreCase)) return;

        if (InternalStepNames.Contains(update.Name)) return;
        foreach (var p in InternalStepPrefixes)
        {
            if (update.Name.StartsWith(p, StringComparison.Ordinal)) return;
        }

        _currentStep = update.Name;
    }

    private void PublishFrame()
    {
        var tick = _clock.Now();
        var step = _currentStep ?? "";

        var snapshot = TelemetryMotionSampler.Sample(_context);
        Dictionary<string, double>? motions;

        if (_forceSnapshot)
        {
            motions = _delta.Snapshot(snapshot);
            _forceSnapshot = false;
        }
        else
        {
            motions = _delta.Delta(snapshot);
        }

        if (motions.Count == 0) motions = null;

        List<TelemetryEvent>? evts = null;
        while (_events.TryDequeue(out var e))
        {
            evts ??= new List<TelemetryEvent>();
            evts.Add(e);
        }

        // 如果没有运动变化、没有事件、且步骤名称未改变，则跳过推送（除非是强制快照）
        if (!_forceSnapshot && motions == null && evts == null && step == _lastSentStep)
        {
            return;
        }

        var packet = new TelemetryPacket
        {
            Tick = tick,
            Step = step,
            Motions = motions,
            Materials = _context.MaterialStates.IsEmpty ? null : new Dictionary<string, MaterialInfo>(_context.MaterialStates),
            Events = evts
        };

        _lastSentStep = step;
        _subject.OnNext(packet);
    }

    public void Dispose()
    {
        _timerSub.Dispose();
        _timerSub.Dispose();
        _traceSub.Dispose();
        _eventSub.Dispose();
        _subject.Dispose();
    }
}
