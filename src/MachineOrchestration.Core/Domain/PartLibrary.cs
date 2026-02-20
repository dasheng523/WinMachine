using System;
using System.Numerics;
using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Core.Domain;

/// <summary>
/// 零件库实现（纯函数，不可变）
/// 使用 Lazy 实现单例模式
/// </summary>
public sealed class PartLibrary : IPartLibrary
{
    private static readonly Lazy<PartLibrary> _instance = new(() => new PartLibrary());
    
    /// <summary>单例实例</summary>
    public static PartLibrary Instance => _instance.Value;
    
    private readonly Seq<Part> _parts;
    
    /// <summary>私有构造函数，初始化零件库</summary>
    private PartLibrary()
    {
        _parts = InitializeParts();
    }
    
    /// <summary>获取所有零件</summary>
    public Seq<Part> GetAllParts() => _parts;
    
    /// <summary>按分类获取零件</summary>
    public Seq<Part> GetPartsByCategory(PartCategory category) =>
        _parts.Filter(part => part.Category.GetType() == category.GetType());
    
    /// <summary>根据 ID 获取零件</summary>
    public Option<Part> GetPartById(PartId id) =>
        _parts.Find(part => part.Id.Equals(id));
    
    /// <summary>初始化零件库数据</summary>
    private static Seq<Part> InitializeParts()
    {
        var parts = Seq<Part>();
        
        // 电机类型零件
        parts = parts.Add(CreateLinearScrewMotor());
        parts = parts.Add(CreateRotaryTableMotor());
        
        // 执行器类型零件（输出类型）
        parts = parts.Add(CreateCylinder());
        parts = parts.Add(CreateGripper());
        parts = parts.Add(CreateSuction());
        parts = parts.Add(CreateIndicator());
        
        // 传感器类型零件（输入类型）
        parts = parts.Add(CreatePressureSensor());
        parts = parts.Add(CreateMicrometerSensor());
        parts = parts.Add(CreateScannerSensor());
        
        // 静态类型零件
        parts = parts.Add(CreateShaft());
        parts = parts.Add(CreateBracket());
        
        return parts;
    }
    
    // 电机零件工厂方法
    
    private static Part CreateLinearScrewMotor() => new(
        Id: new PartId(Guid.Parse("10000000-0000-0000-0000-000000000001")),
        Name: "丝杆滑块电机",
        PartType: new PartType.Motor(new MotorType.LinearScrew(
            MaxSpeed: 100.0f,
            StrokeLength: 500.0f)),
        Category: PartCategory.MotorType.Instance,
        PhysicalDimensions: new Vector3(50, 50, 500));
    
    private static Part CreateRotaryTableMotor() => new(
        Id: new PartId(Guid.Parse("10000000-0000-0000-0000-000000000002")),
        Name: "旋转座电机",
        PartType: new PartType.Motor(new MotorType.RotaryTable(
            MaxSpeed: 180.0f,
            MaxAngle: 360.0f)),
        Category: PartCategory.MotorType.Instance,
        PhysicalDimensions: new Vector3(100, 100, 50));
    
    // 执行器零件工厂方法（输出类型）
    
    private static Part CreateCylinder() => new(
        Id: new PartId(Guid.Parse("20000000-0000-0000-0000-000000000001")),
        Name: "气缸",
        PartType: new PartType.Actuator(new ActuatorType.Cylinder(
            StrokeLength: 100.0f,
            SensorConfig: CylinderSensorConfig.None.Instance)),
        Category: PartCategory.OutputType.Instance,
        PhysicalDimensions: new Vector3(40, 40, 100));
    
    private static Part CreateGripper() => new(
        Id: new PartId(Guid.Parse("20000000-0000-0000-0000-000000000002")),
        Name: "夹爪",
        PartType: new PartType.Actuator(new ActuatorType.Gripper(
            MaxOpening: 50.0f,
            SensorConfig: None)),
        Category: PartCategory.OutputType.Instance,
        PhysicalDimensions: new Vector3(80, 30, 60));
    
    private static Part CreateSuction() => new(
        Id: new PartId(Guid.Parse("20000000-0000-0000-0000-000000000003")),
        Name: "吸气装置",
        PartType: new PartType.Actuator(new ActuatorType.Suction(
            SensorConfig: None)),
        Category: PartCategory.OutputType.Instance,
        PhysicalDimensions: new Vector3(30, 30, 40));
    
    private static Part CreateIndicator() => new(
        Id: new PartId(Guid.Parse("20000000-0000-0000-0000-000000000004")),
        Name: "指示灯",
        PartType: new PartType.Actuator(ActuatorType.Indicator.Instance),
        Category: PartCategory.OutputType.Instance,
        PhysicalDimensions: new Vector3(20, 20, 30));
    
    // 传感器零件工厂方法（输入类型）
    
    private static Part CreatePressureSensor() => new(
        Id: new PartId(Guid.Parse("30000000-0000-0000-0000-000000000001")),
        Name: "压力传感器",
        PartType: new PartType.Sensor(new SensorType.Pressure(
            Range: 10.0f,
            Unit: PressureUnit.Bar)),
        Category: PartCategory.InputType.Instance,
        PhysicalDimensions: new Vector3(25, 25, 40));
    
    private static Part CreateMicrometerSensor() => new(
        Id: new PartId(Guid.Parse("30000000-0000-0000-0000-000000000002")),
        Name: "千分表",
        PartType: new PartType.Sensor(new SensorType.Micrometer(
            Resolution: 0.001f)),
        Category: PartCategory.InputType.Instance,
        PhysicalDimensions: new Vector3(30, 30, 50));
    
    private static Part CreateScannerSensor() => new(
        Id: new PartId(Guid.Parse("30000000-0000-0000-0000-000000000003")),
        Name: "扫码器",
        PartType: new PartType.Sensor(new SensorType.Scanner(
            Protocol: ScannerProtocol.Serial)),
        Category: PartCategory.InputType.Instance,
        PhysicalDimensions: new Vector3(60, 40, 30));
    
    // 静态零件工厂方法
    
    private static Part CreateShaft() => new(
        Id: new PartId(Guid.Parse("40000000-0000-0000-0000-000000000001")),
        Name: "轴",
        PartType: new PartType.Static(new StaticType.Shaft(
            Length: 200.0f,
            Diameter: 20.0f)),
        Category: PartCategory.StaticType.Instance,
        PhysicalDimensions: new Vector3(20, 20, 200));
    
    private static Part CreateBracket() => new(
        Id: new PartId(Guid.Parse("40000000-0000-0000-0000-000000000002")),
        Name: "支架",
        PartType: new PartType.Static(new StaticType.Bracket(
            Dimensions: new Vector3(100, 100, 50))),
        Category: PartCategory.StaticType.Instance,
        PhysicalDimensions: new Vector3(100, 100, 50));
}
