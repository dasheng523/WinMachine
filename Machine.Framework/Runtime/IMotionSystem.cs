using System;
using LanguageExt;
using Machine.Framework.Devices.Motion.Abstractions;

namespace Machine.Framework.Runtime
{
    public interface IMotionSystem : IDisposable
    {
        IMotionController<ushort, ushort, ushort> Primary { get; }
        Fin<IMotionController<ushort, ushort, ushort>> GetBoard(string name);
    }
}
