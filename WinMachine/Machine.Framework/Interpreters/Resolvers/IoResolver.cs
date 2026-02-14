using System;
using System.Linq;
using Machine.Framework.Core.Primitives;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Core.Configuration.Models;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Interpreters.Resolvers
{
    public class IoResolver : IIoResolver
    {
        private readonly IMotionSystem _motionSystem;
        private readonly SystemOptions _options;

        public IoResolver(IMotionSystem motionSystem, IOptions<SystemOptions> options)
        {
            _motionSystem = motionSystem;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Fin<IDigitalInput> ResolveDi(string name)
        {
             return ResolveIo(name, false)
                .Map(t => (IDigitalInput)new MotionDigitalInput(name, t.Controller, (ushort)t.Channel));
        }

        public Fin<IDigitalOutput> ResolveDo(string name)
        {
             return ResolveIo(name, true)
                .Map(t => (IDigitalOutput)new MotionDigitalOutput(name, t.Controller, (ushort)t.Channel));
        }

        private Fin<(IMotionController<ushort, ushort, ushort> Controller, int Channel)> ResolveIo(string name, bool isOutput)
        {
            if (string.IsNullOrWhiteSpace(name))
                return FinFail<(IMotionController<ushort, ushort, ushort>, int)>(Error.New("IO name cannot be empty"));

            var hit = _options.Ios?.FirstOrDefault(a => a.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && a.IsOutput == isOutput);

            if (hit == null)
            {
                 return FinFail<(IMotionController<ushort, ushort, ushort>, int)>(Error.New($"IO mapping not found for: {name}"));
            }

            if (string.IsNullOrWhiteSpace(hit.Board))
            {
                return FinSucc((_motionSystem.Primary, hit.Channel));
            }

            return _motionSystem.GetBoard(hit.Board)
                .Map(ctrl => (ctrl, hit.Channel));
        }

        private class MotionDigitalInput : IDigitalInput
        {
            private readonly string _name;
            private readonly IMotionController<ushort, ushort, ushort> _controller;
            private readonly ushort _bit;

            public MotionDigitalInput(string name, IMotionController<ushort, ushort, ushort> controller, ushort bit)
            {
                _name = name;
                _controller = controller;
                _bit = bit;
            }

            public Fin<Level> Read() => _controller.GetInput(_bit);
        }

        private class MotionDigitalOutput : IDigitalOutput
        {
            private readonly string _name;
            private readonly IMotionController<ushort, ushort, ushort> _controller;
            private readonly ushort _bit;

            public MotionDigitalOutput(string name, IMotionController<ushort, ushort, ushort> controller, ushort bit)
            {
                _name = name;
                _controller = controller;
                _bit = bit;
            }

            public Fin<LUnit> Write(Level level) => _controller.SetOutput(_bit, level);
        }
    }
}
