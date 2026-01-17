using System;
using System.Linq;
using Machine.Framework.Devices.Motion.Abstractions;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Hardware;
using static LanguageExt.Prelude;

namespace Machine.Framework.Platform
{
    public class AxisResolver : IAxisResolver
    {
        private readonly IMotionSystem _motionSystem;
        private readonly SystemOptions _options;

        public AxisResolver(IMotionSystem motionSystem, IOptions<SystemOptions> options)
        {
            _motionSystem = motionSystem;
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public Fin<(IMotionController<ushort, ushort, ushort> Controller, ushort Axis)> Resolve(string axisName)
        {
            if (string.IsNullOrWhiteSpace(axisName))
                return FinFail<(IMotionController<ushort, ushort, ushort>, ushort)>(Error.New("Axis name cannot be empty"));

            var hit = _options.Axes?.FirstOrDefault(a => a.Name.Equals(axisName, StringComparison.OrdinalIgnoreCase));
            
            if (hit == null)
            {
                 return FinFail<(IMotionController<ushort, ushort, ushort>, ushort)>(Error.New($"Axis mapping not found for: {axisName}"));
            }

            if (string.IsNullOrWhiteSpace(hit.Board))
            {
                return FinSucc((_motionSystem.Primary, hit.Axis));
            }

            return _motionSystem.GetBoard(hit.Board)
                .Map(ctrl => (ctrl, hit.Axis));
        }

        public Fin<ushort> ResolveOnPrimary(string axisName) =>
            Resolve(axisName).Map(t => t.Axis);
    }
}
