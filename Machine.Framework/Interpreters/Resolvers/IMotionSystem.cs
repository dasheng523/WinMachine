using System;
using LanguageExt;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;

namespace Machine.Framework.Interpreters.Resolvers
{
    public interface IMotionSystem : IDisposable
    {
        IMotionController<ushort, ushort, ushort> Primary { get; }
        Fin<IMotionController<ushort, ushort, ushort>> GetBoard(string name);
    }
}
