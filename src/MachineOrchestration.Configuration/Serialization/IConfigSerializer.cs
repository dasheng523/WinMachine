using LanguageExt;
using MachineOrchestration.Configuration.Types;

namespace MachineOrchestration.Configuration.Serialization;

/// <summary>配置序列化器接口（纯函数）</summary>
/// <remarks>
/// 提供配置的序列化和反序列化功能。
/// 所有方法都是纯函数，不包含副作用。
/// 验证：需求 23.1-23.2
/// </remarks>
public interface IConfigSerializer
{
    /// <summary>
    /// 将机器配置序列化为 JSON 字符串
    /// </summary>
    /// <param name="config">要序列化的机器配置</param>
    /// <returns>成功时返回 JSON 字符串，失败时返回序列化错误</returns>
    /// <remarks>
    /// 纯函数：相同的输入总是产生相同的输出，无副作用。
    /// 验证：需求 23.1
    /// </remarks>
    Either<SerializationError, string> Serialize(MachineConfig config);
    
    /// <summary>
    /// 从 JSON 字符串反序列化机器配置
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <returns>成功时返回机器配置，失败时返回反序列化错误</returns>
    /// <remarks>
    /// 纯函数：相同的输入总是产生相同的输出，无副作用。
    /// 验证：需求 23.2, 23.5
    /// </remarks>
    Either<DeserializationError, MachineConfig> Deserialize(string json);
}
