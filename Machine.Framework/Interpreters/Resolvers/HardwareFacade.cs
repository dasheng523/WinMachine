using System;
using Machine.Framework.Core.Hardware;
using Machine.Framework.Core.Hardware.Interfaces;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;

using Machine.Framework.Core.Hardware.Models;
using LUnit = LanguageExt.Unit;

namespace Machine.Framework.Interpreters.Resolvers
{
    public interface IHardware
    {
        IResolver<IAxis> Axes { get; }
        IResolver<IDigitalInput> DIs { get; }
        IResolver<IDigitalOutput> DOs { get; }
        IResolver<ISensor<Machine.Framework.Core.Primitives.Level>> LevelSensors { get; }
        IResolver<ISingleSolenoidCylinder> Cylinders { get; }
        IResolver<ISensor<double>> DoubleSensors { get; }
        IResolver<ISensor<string>> StringSensors { get; }
    }

    public sealed class HardwareFacade : IHardware
    {
        public HardwareFacade(
            IAxisResolver axisResolver,
            IMotionSystem motionSystem,
            IIoResolver io,
            ICylinderResolver cylinders,
            IResolver<ISensor<Machine.Framework.Core.Primitives.Level>> levelSensors,
            IResolver<ISensor<double>> doubleSensors,
            IResolver<ISensor<string>> stringSensors)
        {
            Axes = new AxisAsHardwareResolver(axisResolver, motionSystem);
            DIs = new DiAsHardwareResolver(io);
            DOs = new DoAsHardwareResolver(io);
            LevelSensors = levelSensors;
            Cylinders = new CylinderAsHardwareResolver(cylinders);
            DoubleSensors = doubleSensors;
            StringSensors = stringSensors;
        }

        public IResolver<IAxis> Axes { get; }
        public IResolver<IDigitalInput> DIs { get; }
        public IResolver<IDigitalOutput> DOs { get; }
        public IResolver<ISensor<Machine.Framework.Core.Primitives.Level>> LevelSensors { get; }
        public IResolver<ISingleSolenoidCylinder> Cylinders { get; }
        public IResolver<ISensor<double>> DoubleSensors { get; }
        public IResolver<ISensor<string>> StringSensors { get; }

        private sealed class AxisAsHardwareResolver : IResolver<IAxis>
        {
            private readonly IAxisResolver _axes;
            private readonly IMotionSystem _motionSystem;

            public AxisAsHardwareResolver(IAxisResolver axes, IMotionSystem motionSystem)
            {
                _axes = axes;
                _motionSystem = motionSystem;
            }

            public Fin<IAxis> Resolve(string logicalName) =>
                _axes.Resolve(logicalName)
                    .Map(t => (IAxis)new MotionAxis(logicalName, (IMotionController<ushort, ushort, ushort>)t.Controller, t.Axis));

            private sealed class MotionAxis : IAxis
            {
                private readonly IMotionController<ushort, ushort, ushort> _controller;
                private readonly ushort _axis;

                public MotionAxis(string name, IMotionController<ushort, ushort, ushort> controller, ushort axis)
                {
                    Name = name;
                    _controller = controller;
                    _axis = axis;
                }

                public string Name { get; }

                public Fin<double> GetCommandPos() => _controller.GetCommandPos(_axis);
                public Fin<double> GetEncoderPos() => _controller.GetEncoderPos(_axis);
                public Fin<LUnit> MoveAbs(double pos) => _controller.Move_Absolute(_axis, pos);
                public Fin<LUnit> Stop() => _controller.Stop(_axis);
            }
        }

        private sealed class DiAsHardwareResolver : IResolver<IDigitalInput>
        {
            private readonly IIoResolver _io;
            public DiAsHardwareResolver(IIoResolver io) => _io = io;
            public Fin<IDigitalInput> Resolve(string logicalName) => _io.ResolveDi(logicalName);
        }

        private sealed class DoAsHardwareResolver : IResolver<IDigitalOutput>
        {
            private readonly IIoResolver _io;
            public DoAsHardwareResolver(IIoResolver io) => _io = io;
            public Fin<IDigitalOutput> Resolve(string logicalName) => _io.ResolveDo(logicalName);
        }

        private sealed class CylinderAsHardwareResolver : IResolver<ISingleSolenoidCylinder>
        {
            private readonly ICylinderResolver _cylinders;
            public CylinderAsHardwareResolver(ICylinderResolver cylinders) => _cylinders = cylinders;
            public Fin<ISingleSolenoidCylinder> Resolve(string logicalName) => _cylinders.Resolve(logicalName);
        }
    }
}
