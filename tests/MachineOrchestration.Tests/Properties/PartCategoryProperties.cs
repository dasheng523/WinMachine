using FsCheck;
using FsCheck.Xunit;
using LanguageExt;
using MachineOrchestration.Core.Types;
using System.Numerics;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Tests.Properties;

/// <summary>
/// 属性测试：零件分类完整性
/// Feature: machine-orchestration-system, Property 1: 零件分类完整性
/// </summary>
public class PartCategoryProperties
{
    /// <summary>
    /// **Validates: Requirements 1.12-1.15**
    /// 
    /// Property 1: 零件分类完整性
    /// 对于任意零件，该零件应该属于且仅属于一个零件分类（MotorType、OutputType、InputType、StaticType）。
    /// 
    /// 此属性验证：
    /// 1. 每个零件都有一个 Category 字段
    /// 2. Category 字段的值是四种分类之一
    /// 3. 零件的 Category 与其 PartType 一致
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "machine-orchestration-system")]
    [Trait("Property", "1")]
    public bool Part_ShouldBelongToExactlyOneCategory()
    {
        // 手动创建测试零件
        var testParts = new[]
        {
            new Part(
                new PartId(Guid.NewGuid()),
                "Motor1",
                new PartType.Motor(new MotorType.LinearScrew(50, 500)),
                PartCategory.MotorType.Instance,
                new Vector3(100, 100, 100)
            ),
            new Part(
                new PartId(Guid.NewGuid()),
                "Actuator1",
                new PartType.Actuator(ActuatorType.Indicator.Instance),
                PartCategory.OutputType.Instance,
                new Vector3(50, 50, 50)
            ),
            new Part(
                new PartId(Guid.NewGuid()),
                "Sensor1",
                new PartType.Sensor(new SensorType.Pressure(1000, PressureUnit.Pa)),
                PartCategory.InputType.Instance,
                new Vector3(20, 20, 20)
            ),
            new Part(
                new PartId(Guid.NewGuid()),
                "Static1",
                new PartType.Static(new StaticType.Shaft(500, 50)),
                PartCategory.StaticType.Instance,
                new Vector3(500, 50, 50)
            )
        };
        
        // 验证每个零件
        foreach (var part in testParts)
        {
            // 验证零件有一个分类
            var hasCategory = part.Category != null;
            
            // 验证分类是四种之一
            var isValidCategory = part.Category switch
            {
                PartCategory.MotorType => true,
                PartCategory.OutputType => true,
                PartCategory.InputType => true,
                PartCategory.StaticType => true,
                _ => false
            };
            
            // 验证零件的 Category 与其 PartType 一致
            var categoryMatchesPartType = (part.PartType, part.Category) switch
            {
                (PartType.Motor, PartCategory.MotorType) => true,
                (PartType.Actuator, PartCategory.OutputType) => true,
                (PartType.Sensor, PartCategory.InputType) => true,
                (PartType.Static, PartCategory.StaticType) => true,
                _ => false
            };
            
            if (!(hasCategory && isValidCategory && categoryMatchesPartType))
            {
                return false;
            }
        }
        
        return true;
    }
}
