using LanguageExt;
using static LanguageExt.Prelude;
using MachineOrchestration.Configuration.Types;
using MachineOrchestration.Core.Types;

namespace MachineOrchestration.Configuration.Validation;

/// <summary>配置验证器实现（纯函数）</summary>
/// <remarks>
/// 使用纯函数验证机器配置的完整性和正确性。
/// 验证：需求 11.9-11.12, 12.2-12.4
/// </remarks>
public sealed class ConfigValidator : IConfigValidator
{
    /// <summary>验证机器配置</summary>
    public Either<ConfigValidationError, Unit> Validate(MachineConfig config)
    {
        // 收集所有验证错误
        var errors = Seq<ConfigValidationError>();
        
        // 1. 验证控制板配置
        errors = errors.Concat(ValidateControlBoardConfig(config.ControlBoard));
        
        // 2. 验证机器实体配置
        errors = errors.Concat(ValidateMachineEntity(config.Machine));
        
        // 3. 验证端口分配（检测冲突）
        errors = errors.Concat(ValidatePortAllocations(config.Machine));
        
        // 如果有错误，返回 Multiple 错误
        return errors.IsEmpty
            ? Right<ConfigValidationError, Unit>(unit)
            : Left<ConfigValidationError, Unit>(new ConfigValidationError.Multiple(errors));
    }
    
    /// <summary>验证控制板配置</summary>
    private Seq<ConfigValidationError> ValidateControlBoardConfig(ControlBoardConfig config)
    {
        return config switch
        {
            ControlBoardConfig.LeiSai(var connection, var parameters) =>
                ValidateLeiSaiConfig(connection, parameters),
            
            ControlBoardConfig.ZhengYunDong(var connection, var parameters) =>
                ValidateZhengYunDongConfig(connection, parameters),
            
            ControlBoardConfig.Simulated(var latency) =>
                ValidateSimulatedConfig(latency),
            
            _ => Seq1((ConfigValidationError)new ConfigValidationError.IncompatibleBoardConfig("未知的控制板类型"))
        };
    }
    
    /// <summary>验证雷赛控制板配置</summary>
    private Seq<ConfigValidationError> ValidateLeiSaiConfig(string connection, LeiSaiParameters parameters)
    {
        var errors = Seq<ConfigValidationError>();
        
        if (string.IsNullOrWhiteSpace(connection))
            errors = errors.Add(new ConfigValidationError.MissingField("LeiSai.Connection"));
        
        if (parameters.MaxAxes <= 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                "LeiSai.Parameters.MaxAxes", 
                "最大轴数必须大于 0"));
        
        if (parameters.DefaultSpeed <= 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                "LeiSai.Parameters.DefaultSpeed", 
                "默认速度必须大于 0"));
        
        return errors;
    }
    
    /// <summary>验证正运动控制板配置</summary>
    private Seq<ConfigValidationError> ValidateZhengYunDongConfig(
        string connection, 
        ZhengYunDongParameters parameters)
    {
        var errors = Seq<ConfigValidationError>();
        
        if (string.IsNullOrWhiteSpace(connection))
            errors = errors.Add(new ConfigValidationError.MissingField("ZhengYunDong.Connection"));
        
        if (parameters.MaxAxes <= 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                "ZhengYunDong.Parameters.MaxAxes", 
                "最大轴数必须大于 0"));
        
        if (parameters.DefaultSpeed <= 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                "ZhengYunDong.Parameters.DefaultSpeed", 
                "默认速度必须大于 0"));
        
        return errors;
    }
    
    /// <summary>验证模拟控制板配置</summary>
    private Seq<ConfigValidationError> ValidateSimulatedConfig(long latencyMs)
    {
        var errors = Seq<ConfigValidationError>();
        
        if (latencyMs < 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                "Simulated.LatencyMs", 
                "延迟必须大于或等于 0"));
        
        return errors;
    }
    
    /// <summary>递归验证机器实体配置</summary>
    private Seq<ConfigValidationError> ValidateMachineEntity(ComposableEntity entity)
    {
        return entity switch
        {
            ComposableEntity.Part part => ValidatePartConfig(part),
            ComposableEntity.Composite composite => ValidateCompositeConfig(composite),
            _ => Seq<ConfigValidationError>()
        };
    }
    
    /// <summary>验证零件配置</summary>
    private Seq<ConfigValidationError> ValidatePartConfig(ComposableEntity.Part part)
    {
        return part.Config switch
        {
            PartConfig.Motor motorConfig => ValidateMotorConfig(part.Id, motorConfig.Config),
            PartConfig.Actuator actuatorConfig => ValidateActuatorConfig(part.Id, actuatorConfig.Config),
            PartConfig.Sensor sensorConfig => ValidateSensorConfig(part.Id, sensorConfig.Config),
            PartConfig.Static => Seq<ConfigValidationError>(),
            _ => Seq<ConfigValidationError>()
        };
    }
    
    /// <summary>验证组合实体配置</summary>
    private Seq<ConfigValidationError> ValidateCompositeConfig(ComposableEntity.Composite composite)
    {
        // 递归验证所有子实体
        return composite.Children.Bind(child => ValidateMachineEntity(child.Entity));
    }
    
    /// <summary>验证电机配置</summary>
    private Seq<ConfigValidationError> ValidateMotorConfig(EntityId motorId, MotorConfig config)
    {
        var errors = Seq<ConfigValidationError>();
        
        // 验证工作速度
        if (config.WorkingSpeed <= 0)
            errors = errors.Add(new ConfigValidationError.InvalidMotorConfig(
                motorId, 
                "工作速度必须大于 0"));
        
        // 验证轴号
        if (config.BoardConnection.AxisNumber < 0)
            errors = errors.Add(new ConfigValidationError.InvalidMotorConfig(
                motorId, 
                "轴号必须大于或等于 0"));
        
        return errors;
    }
    
    /// <summary>验证执行器配置</summary>
    private Seq<ConfigValidationError> ValidateActuatorConfig(EntityId actuatorId, ActuatorConfig config)
    {
        var errors = Seq<ConfigValidationError>();
        
        // 验证状态传感器配置
        config.StateSensorPorts.Match(
            Some: sensorPorts => errors = errors.Append(
                ValidateStateSensorPorts(actuatorId, sensorPorts)),
            None: () => { });
        
        return errors;
    }
    
    /// <summary>验证状态传感器端口配置</summary>
    private Seq<ConfigValidationError> ValidateStateSensorPorts(
        EntityId actuatorId, 
        StateSensorPorts sensorPorts)
    {
        return sensorPorts switch
        {
            StateSensorPorts.Cylinder cylinderConfig => 
                ValidateCylinderSensorConfig(actuatorId, cylinderConfig.Config),
            
            StateSensorPorts.Gripper gripperConfig => 
                ValidateGripperSensorConfig(actuatorId, gripperConfig.Config),
            
            StateSensorPorts.Suction suctionConfig => 
                ValidateSuctionSensorConfig(actuatorId, suctionConfig.Config),
            
            _ => Seq<ConfigValidationError>()
        };
    }
    
    /// <summary>验证气缸传感器配置</summary>
    private Seq<ConfigValidationError> ValidateCylinderSensorConfig(
        EntityId actuatorId, 
        CylinderSensorConfig config)
    {
        // 气缸传感器配置总是有效的（None、ExtendOnly、Both 都是合法的）
        return Seq<ConfigValidationError>();
    }
    
    /// <summary>验证夹爪传感器配置</summary>
    private Seq<ConfigValidationError> ValidateGripperSensorConfig(
        EntityId actuatorId, 
        GripperSensorConfig config)
    {
        // 夹爪传感器配置使用 Option，总是有效的
        return Seq<ConfigValidationError>();
    }
    
    /// <summary>验证吸气装置传感器配置</summary>
    private Seq<ConfigValidationError> ValidateSuctionSensorConfig(
        EntityId actuatorId, 
        SuctionSensorConfig config)
    {
        // 吸气装置传感器配置总是有效的
        return Seq<ConfigValidationError>();
    }
    
    /// <summary>验证传感器配置</summary>
    private Seq<ConfigValidationError> ValidateSensorConfig(EntityId sensorId, SensorConfig config)
    {
        return config.Connection switch
        {
            SensorConnection.SerialSingle(var port, var baudRate) =>
                ValidateSerialConnection(sensorId, port, baudRate),
            
            SensorConnection.SerialMultiple(var port, var baudRate, var address) =>
                ValidateSerialConnection(sensorId, port, baudRate),
            
            SensorConnection.Usb(var vendorId, var productId) =>
                ValidateUsbConnection(sensorId, vendorId, productId),
            
            _ => Seq<ConfigValidationError>()
        };
    }
    
    /// <summary>验证串口连接配置</summary>
    private Seq<ConfigValidationError> ValidateSerialConnection(
        EntityId sensorId, 
        string port, 
        uint baudRate)
    {
        var errors = Seq<ConfigValidationError>();
        
        if (string.IsNullOrWhiteSpace(port))
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                $"Sensor[{sensorId.Value}].Connection.Port", 
                "串口名称不能为空"));
        
        if (baudRate == 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                $"Sensor[{sensorId.Value}].Connection.BaudRate", 
                "波特率必须大于 0"));
        
        return errors;
    }
    
    /// <summary>验证 USB 连接配置</summary>
    private Seq<ConfigValidationError> ValidateUsbConnection(
        EntityId sensorId, 
        ushort vendorId, 
        ushort productId)
    {
        var errors = Seq<ConfigValidationError>();
        
        if (vendorId == 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                $"Sensor[{sensorId.Value}].Connection.VendorId", 
                "供应商 ID 不能为 0"));
        
        if (productId == 0)
            errors = errors.Add(new ConfigValidationError.InvalidValue(
                $"Sensor[{sensorId.Value}].Connection.ProductId", 
                "产品 ID 不能为 0"));
        
        return errors;
    }
    
    /// <summary>验证端口分配（检测冲突）</summary>
    private Seq<ConfigValidationError> ValidatePortAllocations(ComposableEntity entity)
    {
        // 收集所有使用的端口
        var portUsage = CollectPortUsage(entity);
        
        // 检测冲突
        return portUsage
            .Where(kvp => kvp.Value.Count > 1)
            .Map(kvp => (ConfigValidationError)new ConfigValidationError.PortConflict(kvp.Key, kvp.Value))
            .ToSeq();
    }
    
    /// <summary>收集所有端口使用情况</summary>
    private HashMap<ushort, Seq<EntityId>> CollectPortUsage(ComposableEntity entity)
    {
        return entity switch
        {
            ComposableEntity.Part part => CollectPartPortUsage(part),
            ComposableEntity.Composite composite => composite.Children
                .Map(child => CollectPortUsage(child.Entity))
                .Fold(HashMap<ushort, Seq<EntityId>>(), (acc, usage) => MergePortUsage(acc, usage)),
            _ => HashMap<ushort, Seq<EntityId>>()
        };
    }
    
    /// <summary>收集零件端口使用情况</summary>
    private HashMap<ushort, Seq<EntityId>> CollectPartPortUsage(ComposableEntity.Part part)
    {
        var usage = HashMap<ushort, Seq<EntityId>>();
        
        if (part.Config is PartConfig.Actuator actuatorConfig)
        {
            // 记录输出端口
            usage = AddPortUsage(usage, actuatorConfig.Config.OutputPort, part.Id);
            
            // 记录状态传感器端口
            actuatorConfig.Config.StateSensorPorts.Match(
                Some: sensorPorts => usage = MergePortUsage(
                    usage, 
                    CollectSensorPortUsage(part.Id, sensorPorts)),
                None: () => { });
        }
        
        return usage;
    }
    
    /// <summary>收集传感器端口使用情况</summary>
    private HashMap<ushort, Seq<EntityId>> CollectSensorPortUsage(
        EntityId entityId, 
        StateSensorPorts sensorPorts)
    {
        var usage = HashMap<ushort, Seq<EntityId>>();
        
        switch (sensorPorts)
        {
            case StateSensorPorts.Cylinder(var config):
                switch (config)
                {
                    case CylinderSensorConfig.ExtendOnly(var extendPort):
                        usage = AddPortUsage(usage, extendPort.PortNumber, entityId);
                        break;
                    case CylinderSensorConfig.Both(var extendPort, var retractPort):
                        usage = AddPortUsage(usage, extendPort.PortNumber, entityId);
                        usage = AddPortUsage(usage, retractPort.PortNumber, entityId);
                        break;
                }
                break;
            
            case StateSensorPorts.Gripper(var config):
                config.ClosedSensorPort.Match(
                    Some: port => usage = AddPortUsage(usage, port.PortNumber, entityId),
                    None: () => { });
                config.OpenedSensorPort.Match(
                    Some: port => usage = AddPortUsage(usage, port.PortNumber, entityId),
                    None: () => { });
                break;
            
            case StateSensorPorts.Suction(var config):
                usage = AddPortUsage(usage, config.VacuumSensorPort.PortNumber, entityId);
                break;
        }
        
        return usage;
    }
    
    /// <summary>添加端口使用记录</summary>
    private HashMap<ushort, Seq<EntityId>> AddPortUsage(
        HashMap<ushort, Seq<EntityId>> usage, 
        ushort port, 
        EntityId entityId)
    {
        if (usage.ContainsKey(port))
        {
            var existing = usage[port];
            return usage.SetItem(port, existing.Add(entityId));
        }
        else
        {
            return usage.Add(port, Seq1(entityId));
        }
    }
    
    /// <summary>合并端口使用记录</summary>
    private HashMap<ushort, Seq<EntityId>> MergePortUsage(
        HashMap<ushort, Seq<EntityId>> usage1, 
        HashMap<ushort, Seq<EntityId>> usage2)
    {
        return usage2.Fold(usage1, (acc, kvp) =>
        {
            if (acc.ContainsKey(kvp.Key))
            {
                var existing = acc[kvp.Key];
                return acc.SetItem(kvp.Key, existing.Concat(kvp.Value));
            }
            else
            {
                return acc.Add(kvp.Key, kvp.Value);
            }
        });
    }
}
