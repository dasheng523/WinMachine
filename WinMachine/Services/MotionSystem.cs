using System;
using System.Collections.Generic;
using System.Linq;
using Common.Core;
using Devices.Motion.Abstractions;
using LanguageExt;
using LanguageExt.Common;
using static LanguageExt.Prelude;
using LUnit = LanguageExt.Unit;

namespace WinMachine.Services;

public record MotionBoard(string Name, IMotionController<ushort, ushort, ushort> Controller);

public interface IMotionSystem : IDisposable
{
    IReadOnlyList<MotionBoard> Boards { get; }

    IMotionController<ushort, ushort, ushort> Primary { get; }

    Fin<LUnit> Initialization();

    Fin<IMotionController<ushort, ushort, ushort>> GetBoard(string name);
}

public sealed class MotionSystem : IMotionSystem
{
    public MotionSystem(IEnumerable<MotionBoard> boards)
    {
        Boards = boards?.ToList() ?? throw new ArgumentNullException(nameof(boards));
        if (Boards.Count == 0)
        {
            throw new ArgumentException("MotionSystem 至少需要一块板卡", nameof(boards));
        }
    }

    public IReadOnlyList<MotionBoard> Boards { get; }

    public IMotionController<ushort, ushort, ushort> Primary => Boards[0].Controller;

    public Fin<LUnit> Initialization() =>
        Boards.Traverse(b => b.Controller.Initialization());

    public Fin<IMotionController<ushort, ushort, ushort>> GetBoard(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return FinFail<IMotionController<ushort, ushort, ushort>>(Error.New("Board name 不能为空"));
        }

        var board = Boards.FirstOrDefault(b => string.Equals(b.Name, name, StringComparison.OrdinalIgnoreCase));
        if (board is null)
        {
            return FinFail<IMotionController<ushort, ushort, ushort>>(Error.New($"未找到板卡: {name}"));
        }

        return FinSucc(board.Controller);
    }

    public void Dispose()
    {
        foreach (var board in Boards)
        {
            board.Controller.Dispose();
        }
    }
}
