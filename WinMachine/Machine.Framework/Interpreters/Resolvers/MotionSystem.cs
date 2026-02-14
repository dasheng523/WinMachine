using System;
using System.Collections.Generic;
using System.Linq;
using LanguageExt;
using LanguageExt.Common;
using Machine.Framework.Core.Hardware.Interfaces;
using Machine.Framework.Core.Hardware.Models;
using static LanguageExt.Prelude;

namespace Machine.Framework.Interpreters.Resolvers
{
    public class MotionBoard
    {
        public required string Name { get; set; }
        public required IMotionController<ushort, ushort, ushort> Controller { get; set; }
    }

    public class MotionSystem : IMotionSystem
    {
        private readonly List<MotionBoard> _boards;

        public MotionSystem(IEnumerable<MotionBoard> boards)
        {
            _boards = boards?.ToList() ?? new List<MotionBoard>();
        }

        public IMotionController<ushort, ushort, ushort> Primary => 
            _boards.FirstOrDefault()?.Controller ?? throw new InvalidOperationException("No boards configured");

        public Fin<IMotionController<ushort, ushort, ushort>> GetBoard(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return FinFail<IMotionController<ushort, ushort, ushort>>(Error.New("Board name cannot be empty"));

            var board = _boards.FirstOrDefault(b => b.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return board != null
                ? FinSucc(board.Controller)
                : FinFail<IMotionController<ushort, ushort, ushort>>(Error.New($"Board '{name}' not found"));
        }

        public void Dispose()
        {
            foreach (var b in _boards)
            {
                b.Controller.Dispose();
            }
        }
    }
}
