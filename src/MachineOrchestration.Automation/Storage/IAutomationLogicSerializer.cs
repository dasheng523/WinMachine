using LanguageExt;
using MachineOrchestration.Automation.Types;
using MachineOrchestration.Configuration.Serialization;

namespace MachineOrchestration.Automation.Storage;

/// <summary>自动化逻辑序列化器接口（纯函数）</summary>
/// <remarks>
/// 负责自动化逻辑的序列化和反序列化。
/// 所有操作都是纯函数。
/// 验证：需求 14.4-14.5
/// </remarks>
public interface IAutomationLogicSerializer
{
    /// <summary>
    /// 将自动化逻辑序列化为 JSON 字符串
    /// </summary>
    /// <remarks>
    /// 纯函数：捕获所有异常并转换为 Either 类型。
    /// 验证：需求 14.4-14.5, 23.1
    /// </remarks>
    /// <param name="logic">要序列化的自动化逻辑</param>
    /// <returns>
    /// Right: JSON 字符串
    /// Left: 序列化错误
    /// </returns>
    Either<SerializationError, string> Serialize(AutomationLogic logic);
    
    /// <summary>
    /// 从 JSON 字符串反序列化自动化逻辑
    /// </summary>
    /// <remarks>
    /// 纯函数：捕获所有异常并转换为 Either 类型。
    /// 验证：需求 14.4-14.5, 23.2, 23.5
    /// </remarks>
    /// <param name="json">JSON 字符串</param>
    /// <returns>
    /// Right: 自动化逻辑
    /// Left: 反序列化错误
    /// </returns>
    Either<DeserializationError, AutomationLogic> Deserialize(string json);
}
