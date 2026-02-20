using FsCheck;
using FsCheck.Xunit;
using LanguageExt;
using MachineOrchestration.Core.Types;
using static LanguageExt.Prelude;

namespace MachineOrchestration.Tests.Properties;

/// <summary>
/// 属性测试：传感器配置类型安全
/// Feature: machine-orchestration-system, Property 23: 传感器配置类型安全
/// </summary>
public class SensorConfigProperties
{
    /// <summary>
    /// **Validates: Requirements 1.6-1.9, 11.6-11.8**
    /// 
    /// Property 23: 传感器配置类型安全
    /// 对于任意执行器类型和传感器配置，如果配置与执行器类型不匹配（例如为夹爪配置气缸传感器），
    /// 类型系统应该在编译时拒绝。
    /// 
    /// 此属性验证：
    /// 1. 气缸（Cylinder）使用 CylinderSensorConfig（None、ExtendOnly、Both）
    /// 2. 夹爪（Gripper）使用 Option<GripperSensorConfig>
    /// 3. 吸气装置（Suction）使用 Option<SuctionSensorConfig>
    /// 4. 当配置了传感器时，传感器端口必须被指定
    /// 5. Option<T> 正确表示可选配置
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "machine-orchestration-system")]
    [Trait("Property", "23")]
    public bool ActuatorType_SensorConfig_ShouldBeTypeCorrect()
    {
        // 测试气缸传感器配置
        var cylinderTests = TestCylinderSensorConfigs();
        
        // 测试夹爪传感器配置
        var gripperTests = TestGripperSensorConfigs();
        
        // 测试吸气装置传感器配置
        var suctionTests = TestSuctionSensorConfigs();
        
        return cylinderTests && gripperTests && suctionTests;
    }
    
    /// <summary>
    /// 测试气缸传感器配置的类型安全性
    /// 验证需求 1.8: 气缸应支持三种传感器配置：无传感器、仅伸出传感器、伸出和缩回传感器
    /// 验证需求 1.9: 使用代数数据类型的和类型表示不同的传感器配置组合
    /// </summary>
    private bool TestCylinderSensorConfigs()
    {
        // 测试 1: 无传感器配置
        var cylinderNoSensor = new ActuatorType.Cylinder(
            StrokeLength: 100f,
            SensorConfig: CylinderSensorConfig.None.Instance
        );
        
        // 验证类型正确
        var test1 = cylinderNoSensor.SensorConfig is CylinderSensorConfig.None;
        
        // 测试 2: 仅伸出传感器配置
        var extendPort = new SensorPort(1);
        var cylinderExtendOnly = new ActuatorType.Cylinder(
            StrokeLength: 100f,
            SensorConfig: new CylinderSensorConfig.ExtendOnly(extendPort)
        );
        
        // 验证类型正确且端口已指定
        var test2 = cylinderExtendOnly.SensorConfig is CylinderSensorConfig.ExtendOnly extendOnly
                    && extendOnly.ExtendSensorPort.PortNumber > 0;
        
        // 测试 3: 伸出和缩回传感器配置
        var retractPort = new SensorPort(2);
        var cylinderBoth = new ActuatorType.Cylinder(
            StrokeLength: 100f,
            SensorConfig: new CylinderSensorConfig.Both(extendPort, retractPort)
        );
        
        // 验证类型正确且两个端口都已指定
        var test3 = cylinderBoth.SensorConfig is CylinderSensorConfig.Both both
                    && both.ExtendSensorPort.PortNumber > 0
                    && both.RetractSensorPort.PortNumber > 0;
        
        return test1 && test2 && test3;
    }
    
    /// <summary>
    /// 测试夹爪传感器配置的类型安全性
    /// 验证需求 1.6-1.7: 夹爪使用 Option<T> 表示可选的状态传感器配置
    /// 验证需求 11.6: 输出类型零件允许配置可选的状态传感器
    /// </summary>
    private bool TestGripperSensorConfigs()
    {
        // 测试 1: 无传感器配置（Option.None）
        var gripperNoSensor = new ActuatorType.Gripper(
            MaxOpening: 50f,
            SensorConfig: Option<GripperSensorConfig>.None
        );
        
        // 验证 Option 为 None
        var test1 = gripperNoSensor.SensorConfig.IsNone;
        
        // 测试 2: 有传感器配置（Option.Some）
        var closedPort = new SensorPort(3);
        var openedPort = new SensorPort(4);
        var gripperConfig = new GripperSensorConfig(
            ClosedSensorPort: Some(closedPort),
            OpenedSensorPort: Some(openedPort)
        );
        var gripperWithSensor = new ActuatorType.Gripper(
            MaxOpening: 50f,
            SensorConfig: Some(gripperConfig)
        );
        
        // 验证 Option 为 Some 且配置正确
        var test2 = gripperWithSensor.SensorConfig.IsSome
                    && gripperWithSensor.SensorConfig.Match(
                        Some: config => config.ClosedSensorPort.IsSome && config.OpenedSensorPort.IsSome,
                        None: () => false
                    );
        
        // 测试 3: 部分传感器配置（仅闭合传感器）
        var gripperConfigPartial = new GripperSensorConfig(
            ClosedSensorPort: Some(closedPort),
            OpenedSensorPort: Option<SensorPort>.None
        );
        var gripperPartial = new ActuatorType.Gripper(
            MaxOpening: 50f,
            SensorConfig: Some(gripperConfigPartial)
        );
        
        // 验证部分配置正确
        var test3 = gripperPartial.SensorConfig.IsSome
                    && gripperPartial.SensorConfig.Match(
                        Some: config => config.ClosedSensorPort.IsSome && config.OpenedSensorPort.IsNone,
                        None: () => false
                    );
        
        return test1 && test2 && test3;
    }
    
    /// <summary>
    /// 测试吸气装置传感器配置的类型安全性
    /// 验证需求 1.6-1.7: 吸气装置使用 Option<T> 表示可选的状态传感器配置
    /// 验证需求 11.8: 使用 Option/Maybe 表示可选的传感器端口配置
    /// </summary>
    private bool TestSuctionSensorConfigs()
    {
        // 测试 1: 无传感器配置（Option.None）
        var suctionNoSensor = new ActuatorType.Suction(
            SensorConfig: Option<SuctionSensorConfig>.None
        );
        
        // 验证 Option 为 None
        var test1 = suctionNoSensor.SensorConfig.IsNone;
        
        // 测试 2: 有传感器配置（Option.Some）
        var vacuumPort = new SensorPort(5);
        var suctionConfig = new SuctionSensorConfig(VacuumSensorPort: vacuumPort);
        var suctionWithSensor = new ActuatorType.Suction(
            SensorConfig: Some(suctionConfig)
        );
        
        // 验证 Option 为 Some 且端口已指定
        var test2 = suctionWithSensor.SensorConfig.IsSome
                    && suctionWithSensor.SensorConfig.Match(
                        Some: config => config.VacuumSensorPort.PortNumber > 0,
                        None: () => false
                    );
        
        return test1 && test2;
    }
    
    /// <summary>
    /// 测试传感器端口的有效性
    /// 验证需求 11.7-11.8: 当配置了传感器时，端口必须被指定
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "machine-orchestration-system")]
    [Trait("Property", "23")]
    public bool SensorPort_WhenConfigured_ShouldBeValid(ushort portNumber)
    {
        // 只测试有效的端口号（大于0）
        if (portNumber == 0)
            return true; // 跳过无效端口号
        
        var port = new SensorPort(portNumber);
        
        // 验证端口号被正确存储
        return port.PortNumber == portNumber && port.PortNumber > 0;
    }
    
    /// <summary>
    /// 测试 Option<T> 的正确使用
    /// 验证需求 1.7: 系统应使用 Option/Maybe 类型表示可选的状态传感器配置
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Feature", "machine-orchestration-system")]
    [Trait("Property", "23")]
    public bool Option_ShouldCorrectlyRepresentOptionalConfig()
    {
        // 测试 None 情况
        var noneConfig = Option<GripperSensorConfig>.None;
        var test1 = noneConfig.IsNone && !noneConfig.IsSome;
        
        // 测试 Some 情况
        var someConfig = Some(new GripperSensorConfig(
            ClosedSensorPort: Some(new SensorPort(1)),
            OpenedSensorPort: Option<SensorPort>.None
        ));
        var test2 = someConfig.IsSome && !someConfig.IsNone;
        
        // 测试 Match 方法
        var matchResult = someConfig.Match(
            Some: config => true,
            None: () => false
        );
        var test3 = matchResult;
        
        return test1 && test2 && test3;
    }
}
