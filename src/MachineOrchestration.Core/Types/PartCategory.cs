using System.Text.Json.Serialization;
using LanguageExt;

namespace MachineOrchestration.Core.Types;

/// <summary>
/// 零件分类系统（和类型 - Sum Type）
/// 将基础零件库组织为四大类型的分类系统
/// </summary>
[JsonDerivedType(typeof(MotorType), "motortype")]
[JsonDerivedType(typeof(OutputType), "outputtype")]
[JsonDerivedType(typeof(InputType), "inputtype")]
[JsonDerivedType(typeof(StaticType), "statictype")]
public abstract record PartCategory
{
    /// <summary>电机类型（丝杆滑块、旋转座等）</summary>
    public sealed record MotorType : PartCategory
    {
        public MotorType() { }
        public static readonly MotorType Instance = new();
    }
    
    /// <summary>输出类型（指示灯、气缸、夹爪、吸气装置）</summary>
    public sealed record OutputType : PartCategory
    {
        public OutputType() { }
        public static readonly OutputType Instance = new();
    }
    
    /// <summary>输入类型（传感器）</summary>
    public sealed record InputType : PartCategory
    {
        public InputType() { }
        public static readonly InputType Instance = new();
    }
    
    /// <summary>静态类型（轴等结构件）</summary>
    public sealed record StaticType : PartCategory
    {
        public StaticType() { }
        public static readonly StaticType Instance = new();
    }
    
    private PartCategory() { }
}
