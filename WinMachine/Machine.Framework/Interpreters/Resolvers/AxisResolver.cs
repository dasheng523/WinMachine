using System;
using System.Linq;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;
using LanguageExt;
using LanguageExt.Common;
using Microsoft.Extensions.Options;
using Machine.Framework.Core.Configuration.Models;
using Machine.Framework.Core.Hardware;

using static LanguageExt.Prelude;

namespace Machine.Framework.Interpreters.Resolvers
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

        public Fin<(object Controller, ushort Axis)> Resolve(string axisName)
        {
            if (string.IsNullOrWhiteSpace(axisName))
                return FinFail<(object, ushort)>(Error.New("Axis name cannot be empty"));

            var hit = _options.Axes?.FirstOrDefault(a => a.Name.Equals(axisName, StringComparison.OrdinalIgnoreCase));
            
            if (hit == null)
            {
                 return FinFail<(object, ushort)>(Error.New($"Axis mapping not found for: {axisName}"));
            }

            if (string.IsNullOrWhiteSpace(hit.Board))
            {
                return FinSucc(((object)_motionSystem.Primary, hit.Axis));
            }

            return _motionSystem.GetBoard(hit.Board)
                .Map(ctrl => ((object)ctrl, hit.Axis));
        }

        public Fin<ushort> ResolveOnPrimary(string axisName) =>
            Resolve(axisName).Map(t => t.Axis);
    }
}
