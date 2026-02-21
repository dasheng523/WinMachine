using LanguageExt;
using MachineOrchestration.Configuration.Types;

namespace MachineOrchestration.Configuration.Validation;

/// <summary>配置验证器接口（纯函数）</summary>
/// <remarks>
/// 验证机器配置的完整性和正确性。
/// 验证：需求 11.9-11.12, 12.2-12.4
/// </remarks>
public interface IConfigValidator
{
    /// <summary>验证机器配置</summary>
    /// <param name="config">要验证的机器配置</param>
    /// <returns>验证成功返回 Right(Unit)，失败返回 Left(ConfigValidationError)</returns>
    /// <remarks>
    /// 验证内容包括：
    /// - 传感器端口分配
    /// - 控制板兼容性
    /// - 电机配置
    /// - 执行器传感器配置
    /// - 端口冲突检测
    /// </remarks>
    Either<ConfigValidationError, Unit> Validate(MachineConfig config);
}
