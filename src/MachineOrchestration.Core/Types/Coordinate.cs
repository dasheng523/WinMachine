using System.Numerics;

namespace MachineOrchestration.Core.Types;

/// <summary>坐标（位置 + 姿态）</summary>
public readonly record struct Coordinate(
    Vector3 Position,
    Quaternion Rotation)
{
    /// <summary>单位坐标（原点，无旋转）</summary>
    public static readonly Coordinate Identity = new(Vector3.Zero, Quaternion.Identity);
}
