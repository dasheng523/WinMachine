using System;
using System.Linq;
using Machine.Framework.Core.Core;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Devices.Motion.Abstractions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Configuration;
using static LanguageExt.Prelude;

namespace Machine.Framework.Runtime;

public interface IIoResolver
{
    Fin<IDigitalInput> ResolveDi(string name);

    Fin<IDigitalOutput> ResolveDo(string name);
}

public sealed class IoResolver : IIoResolver
{
    private readonly IMotionSystem _motionSystem;
    private readonly IOptions<SystemOptions> _options;

    public IoResolver(IMotionSystem motionSystem, IOptions<SystemOptions> options)
    {
        _motionSystem = motionSystem ?? throw new ArgumentNullException(nameof(motionSystem));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public Fin<IDigitalInput> ResolveDi(string name) =>
        ResolveIo(_options.Value.IoMap?.Di, name)
            .Bind(t => ResolveBoard(t.Board).Map(c => (Controller: c, Bit: t.Bit)))
            .Map(t => (IDigitalInput)new MotionDigitalInput(name, t.Controller, t.Bit));

    public Fin<IDigitalOutput> ResolveDo(string name) =>
        ResolveIo(_options.Value.IoMap?.Do, name)
            .Bind(t => ResolveBoard(t.Board).Map(c => (Controller: c, Bit: t.Bit)))
            .Map(t => (IDigitalOutput)new MotionDigitalOutput(name, t.Controller, t.Bit));

    private Fin<IMotionController<ushort, ushort, ushort>> ResolveBoard(string? board) =>
        string.IsNullOrWhiteSpace(board)
            ? FinSucc(_motionSystem.Primary)
            : _motionSystem.GetBoard(board);

    private static Fin<(string? Board, ushort Bit)> ResolveIo(
        System.Collections.Generic.Dictionary<string, IoRefOptions>? map,
        string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return FinFail<(string?, ushort)>(Error.New("IO 名称不能为空"));
        }

        if (map is null || map.Count == 0)
        {
            return FinFail<(string?, ushort)>(Error.New("未配�?System.IoMap"));
        }

        if (map.TryGetValue(name, out var hit))
        {
            return FinSucc((hit.Board, hit.Bit));
        }

        var v = map.FirstOrDefault(kv => string.Equals(kv.Key, name, StringComparison.OrdinalIgnoreCase)).Value;
        if (v is null)
        {
            return FinFail<(string?, ushort)>(Error.New($"未找�?IO 映射: {name}"));
        }

        return FinSucc((v.Board, v.Bit));
    }

    private sealed class MotionDigitalInput : IDigitalInput
    {
        private readonly IMotionController<ushort, ushort, ushort> _controller;
        private readonly ushort _bit;

        public MotionDigitalInput(string name, IMotionController<ushort, ushort, ushort> controller, ushort bit)
        {
            Name = name;
            _controller = controller;
            _bit = bit;
        }

        public string Name { get; }

        public Fin<Level> Read() => _controller.GetInput(_bit);
    }

    private sealed class MotionDigitalOutput : IDigitalOutput
    {
        private readonly IMotionController<ushort, ushort, ushort> _controller;
        private readonly ushort _bit;

        public MotionDigitalOutput(string name, IMotionController<ushort, ushort, ushort> controller, ushort bit)
        {
            Name = name;
            _controller = controller;
            _bit = bit;
        }

        public string Name { get; }

        public Fin<Unit> Write(Level level) => _controller.SetOutput(_bit, level);
    }
}


